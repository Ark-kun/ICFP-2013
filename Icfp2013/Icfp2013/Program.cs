using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

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

        public static ulong[] EvalArgs;

        static void Main(string[] args)
        {
            MyProblems();



            if (File.Exists("randoms.txt"))
            {
                EvalArgs = File.ReadAllLines("randoms.txt").Select(a => ulong.Parse(a, System.Globalization.NumberStyles.HexNumber)).ToArray();
            }
            else
            {
                Random rng = new Random(3137);
                EvalArgs = (new[] { (ulong)0 }).Concat(GetUlongsForEval(128)).Concat(Enumerable.Range(0, 1024 - 128 - 1).Select(a => GetRandomUlong(rng))).ToArray();
                File.WriteAllLines("randoms.txt", EvalArgs.Select(a => a.ToString("X")));
            }

            new FuncCache();



            //string line = File.ReadAllLines("cache3.txt")[12];
            //string[] split = line.Split('\t');
            //ulong[] cacheResult = split[2].Split(',').Select(a => ulong.Parse(a, System.Globalization.NumberStyles.HexNumber)).ToArray();

            //bool reslt = CheckEvalProgram(split[0], cacheResult);

            //Searcher sg = new Searcher();
            //sg.GenerateCache();


            Dzugaru.Search.Solver.IterativeDeepeningStep += () => { Searcher.AllEvals.Clear(); System.IO.File.AppendAllText("allguesses.txt", "--------------------\r\n"); };

            int[] cacheSizes = new[] { 5 };

            int wins = 0, losses = 0;
            int haltID = 0;            
            for (;;)
            {
                File.Delete("allguesses.txt");
                //Problem p = GetTrainProblem(12);
                Problem p = GetLastProblem();
                

                Console.WriteLine("\r\nGot problem: " + p.ToString());

               

                for (int i=0; i < cacheSizes.Length; i++)
                {
                    Searcher.UseCacheOfSize = cacheSizes[i];
                    FuncCache.S.ReadCache(p);                                    

                    int currHaltID = haltID;
                    Task haltTask = new Task(() => { Thread.Sleep(TimeSpan.FromSeconds(15)); if (currHaltID != haltID) return; Dzugaru.Search.Solver.ShouldHaltSearch = true; });
                    haltTask.Start();

                    Searcher s = new Searcher();
                    FunctionTreeNode result = s.Find(p);
                    if (result != null)
                    {
                        haltID++;
                        Console.WriteLine("Solution found! " + result + ", nodes expanded: " + Dzugaru.Search.Solver.NodesExpanded);

                        if (p.IsTFoldProblem)
                        {
                            var tfoldRoot = new FunctionTreeNode(result.Context) { Operator = new Operators.Fold() };
                            tfoldRoot.Children.Add(new FunctionTreeNode(result.Context) { Operator = new Operators.Arg(0) });
                            tfoldRoot.Children.Add(new FunctionTreeNode(result.Context) { Operator = new Operators.Zero() });
                            tfoldRoot.Children.Add(result);

                            result = tfoldRoot;
                        }

                        if (SubmitGuess(p.ID, result))
                        {
                            Console.WriteLine("Guess accepted!!");
                            wins++;                            
                        }
                        else
                        {
                            Console.WriteLine("Mismatch!");
                            losses++;                            
                        }
                        
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Cant find in time!");
                        Dzugaru.Search.Solver.ShouldHaltSearch = false;
                        Searcher.UseCacheOfSize = 0; //try without cache
                    }
                }

                Console.WriteLine("Score: " + wins + "/" + losses);
                Thread.Sleep(20000);
            }

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

        static Problem GetProblemFromReader(JsonReader reader, bool saveResp)
        {
            JsonSerializer ser = new JsonSerializer();
            JObject obj = JObject.Load(reader);
            TrainResponse resp = ser.Deserialize<TrainResponse>(obj.CreateReader());

            if (saveResp)
            {
                File.WriteAllText("lastProblem.txt", obj.ToString());
            }

            var evalJObject = new JObject(
                new JProperty("id", resp.Id),
                new JProperty("arguments", new JArray(EvalArgs.Take(256).Select(a => a.ToString("X")).ToArray())));
            reader = MakeRequest(RequestType.eval, evalJObject);

            JObject evalResp = (JObject)JObject.ReadFrom(reader);
            ulong[] results = evalResp["outputs"].Value<JArray>().Select(a => ulong.Parse(a.Value<string>().Substring(2), System.Globalization.NumberStyles.HexNumber)).ToArray();

            Problem pr = new Problem()
            {
                ID = resp.Id,
                Size = resp.Size,
                Evals = new ulong[results.Length][],
                Solution = resp.Program
            };

            pr.SetAllowedOperators(resp.Operators);

            for (int i = 0; i < results.Length; i++)
            {
                pr.Evals[i] = new[] { EvalArgs[i], results[i] };
            }

            return pr;
        }

        static Problem GetLastProblem()
        {
            using(var stream = File.OpenRead("lastProblem.txt"))
            {
                MemoryStream ss = new MemoryStream();
                stream.CopyTo(ss);
                ss.Position = 0;
                JsonReader reader = new JsonTextReader(new StreamReader(ss));
                return GetProblemFromReader(reader, false);
            }
        }

        static Problem GetTrainProblem(int size)
        {
            JsonReader reader = MakeRequest(RequestType.train, new JObject(new JProperty("size", size), new JProperty("operators", new JArray())));
            return GetProblemFromReader(reader, true);
        }

        static ulong[] GetUlongsForEval(int num)
        {
            ulong[] res = new ulong[num];
            ulong c = 1;
            for (int i = 0; i < num; i+=2)
            {
                res[i] = c;
                res[i + 1] = ~c;
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

        static bool CheckEvalProgram(string program, ulong[] cacheResults)
        {        
            var evalJObject = new JObject(
               new JProperty("program", program),
               new JProperty("arguments", new JArray(EvalArgs.Take(256).Select(a => a.ToString("X")).ToArray())));
            JsonReader reader = MakeRequest(RequestType.eval, evalJObject);

            JObject evalResp = (JObject)JObject.ReadFrom(reader);
            ulong[] results = evalResp["outputs"].Value<JArray>().Select(a => ulong.Parse(a.Value<string>().Substring(2), System.Globalization.NumberStyles.HexNumber)).ToArray();

            for (int i = 0; i < results.Length; i++)
            {
                if (cacheResults[i] != results[i])
                    return false;
            }

            return true;
        }

        static void MyProblems()
        {
            string s = File.ReadAllText("myproblems.txt");
            JArray probs = JArray.Parse(s);
            JsonSerializer ser = new JsonSerializer();

            List<TrainResponse> allProbs = new List<TrainResponse>();
            foreach (var p in probs)
            {
                TrainResponse resp = ser.Deserialize<TrainResponse>(((JObject)p).CreateReader());
                allProbs.Add(resp);
            }

            int k = allProbs.Count(a => a.Operators.Contains("tfold"));
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
    }
}
