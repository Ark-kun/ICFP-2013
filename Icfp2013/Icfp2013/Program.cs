using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Icfp2013
{
    class Program
    {
        enum RequestType
        {
            myproblems,
            train,
            eval,
            guess
        }

        static string UserID = File.ReadAllText("key.txt");
        static string RequestPath = "http://icfpc2013.cloudapp.net/{0}?auth={1}";

        static void Main(string[] args)
        {
            Dzugaru.Search.Solver.IterativeDeepeningStep += () => { Searcher.AllEvals.Clear(); };
            Problem p = GetTrainProblem(12);

            Console.WriteLine("Got problem: " + p.ToString());
            File.AppendAllText("problems.txt", p.ID + "\r\n" + p.ToString());


            Searcher s = new Searcher();
            FunctionTreeNode result = s.Find(p);
            if (result != null)
            {
                Console.WriteLine("Solution found! " + result + ", nodes expanded: " + Dzugaru.Search.Solver.NodesExpanded);

                if (SubmitGuess(p.ID, result))
                {
                    Console.WriteLine("Guess accepted!!");
                }

            }

            //Console.WriteLine(s.VariantsCount);

            //EvaluationContext ctx = new EvaluationContext();
            //FunctionTreeNode checkFold = new FunctionTreeNode(ctx)
            //{
            //    Operator = new Operators.Fold()
            //};

            //checkFold.Children.Add(new FunctionTreeNode(ctx, checkFold) { Operator = new Operators.Arg(0) });
            //checkFold.Children.Add(new FunctionTreeNode(ctx, checkFold) { Operator = new Operators.Zero() });

            //FunctionTreeNode foldExpr = new FunctionTreeNode(ctx, checkFold) { Operator = new Operators.Or() };
            //checkFold.Children.Add(foldExpr);
            //foldExpr.Children.Add(new FunctionTreeNode(ctx, checkFold) { Operator = new Operators.Arg(1) });
            //foldExpr.Children.Add(new FunctionTreeNode(ctx, checkFold) { Operator = new Operators.Arg(2) });

            //Console.WriteLine("Checking fold: " + checkFold);
            //if (CheckEvalProgram(checkFold))
            //{
            //    Console.WriteLine("Check success");
            //}

            Console.ReadLine();
        }

        static JsonReader MakeRequest(RequestType type, JObject parameters)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(string.Format(RequestPath, type.ToString(), UserID));
            req.Method = "POST";            
           
            if(parameters != null)
            {
                req.ContentType = "application/json";
                Stream reqStream = req.GetRequestStream();
                JsonWriter writer = new JsonTextWriter(new StreamWriter(reqStream));               
                parameters.WriteTo(writer);
                writer.Flush();
                reqStream.Close();
                reqStream.Dispose();
            }

            using (WebResponse response = req.GetResponse())
            {
                MemoryStream stream = new MemoryStream();
                response.GetResponseStream().CopyTo(stream);
                stream.Position = 0;
                return new JsonTextReader(new StreamReader(stream));
            }
        }

        static Problem GetTrainProblem(int size)
        {
            JsonReader reader = MakeRequest(RequestType.train, new JObject(new JProperty("size", size + 1), new JProperty("operators", new JArray("fold"))));
            JsonSerializer ser = new JsonSerializer();
            TrainResponse resp = ser.Deserialize<TrainResponse>(reader);

            Random rng = new Random();
            ulong[] args = Enumerable.Range(0, 256).Select(a => GetRandomUlong(rng)).ToArray();
            //ulong[] args = GetUlongsForEval(64);

            var evalJObject =  new JObject(
                new JProperty("id", resp.Id),
                new JProperty("arguments", new JArray(args.Select(a => a.ToString("X")).ToArray())));
            reader = MakeRequest(RequestType.eval, evalJObject);

            JObject evalResp = (JObject)JObject.ReadFrom(reader);
            ulong[] results = evalResp["outputs"].Value<JArray>().Select(a => ulong.Parse(a.Value<string>().Substring(2), System.Globalization.NumberStyles.HexNumber)).ToArray();

            Problem pr = new Problem()
                {
                    ID = resp.Id,
                    Size = size,
                    Evals = new ulong[results.Length][],
                    Solution = resp.Program
                };

            pr.SetAllowedOperators(resp.Operators);

            for (int i = 0; i < results.Length; i++)
            {
                pr.Evals[i] = new[] { args[i], results[i] };
            }

            return pr;
        }

        static ulong[] GetUlongsForEval(int num)
        {
            ulong[] res = new ulong[num];
            ulong c = 1;
            for (int i = 0; i < num; i++)
            {
                res[i] = c;
                c = c << 1;
            }

            return res;
        }

        static ulong GetRandomUlong(Random rng)
        {
            byte[] buf = new byte[8];
            rng.NextBytes(buf);
            return BitConverter.ToUInt64(buf, 0);
        }

        static bool SubmitGuess(string id, FunctionTreeNode node)
        {
            JsonReader reader = MakeRequest(RequestType.guess, 
                new JObject(new JProperty("id", id), 
                new JProperty("program", node.ToString())));

            JObject result = (JObject)JObject.ReadFrom(reader);

            return result["status"].Value<string>() == "win";
        }

        static bool CheckEvalProgram(FunctionTreeNode func)
        {
            Random rng = new Random();
            ulong[] args = Enumerable.Range(0, 256).Select(a => GetRandomUlong(rng)).ToArray();

            var evalJObject = new JObject(
               new JProperty("program", func.ToString()),
               new JProperty("arguments", new JArray(args.Select(a => a.ToString("X")).ToArray())));
            JsonReader reader = MakeRequest(RequestType.eval, evalJObject);

            JObject evalResp = (JObject)JObject.ReadFrom(reader);
            ulong[] results = evalResp["outputs"].Value<JArray>().Select(a => ulong.Parse(a.Value<string>().Substring(2), System.Globalization.NumberStyles.HexNumber)).ToArray();

            for (int i = 0; i < results.Length; i++)
            {
                func.Context.Arg = args[i];
                ulong funcEval = func.Eval();
                if (funcEval != results[i]) 
                    return false;
            }

            return true;
        }
    }
}
