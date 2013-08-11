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
        public static List<TrainResponse> RealProblems;
        public static List<string> RealSolvedIDs;

        static void Main(string[] args)
        {
           RealSolvedIDs = File.ReadAllLines("attempted.txt").ToList();
           
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

            int[] cacheSizes = new[] { 0 };

            int wins = 0, losses = 0;
            int haltID = 0;            
            for (;;)
            {
                File.Delete("allguesses.txt");
                //Problem p = GetTrainProblem(12);
                //Problem p = GetLastProblem();

                Problem p;

                
                RealSolvedIDs = File.ReadAllLines("attempted.txt").ToList();

                for (; ; )
                {
                    try
                    {
                        p = GetRealProblem(a => !a.Operators.Contains("bonus") && a.Operators.Contains("tfold"));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Too many requests!");
                        Thread.Sleep(4000);
                        continue;
                    }

                    break;
                }

                RealProblems.RemoveAll(a => a.Id == p.ID);
                File.AppendAllText("attempted.txt", p.ID + "\r\n");
                RealSolvedIDs.Add(p.ID);

                Console.WriteLine("\r\nGot problem: " + p.ToString());
               

                for (int i=0; i < cacheSizes.Length; i++)
                {
                    Searcher.UseCacheOfSize = cacheSizes[i];
                    FuncCache.S.ReadCache(p);                                    

                    int currHaltID = haltID;
                    Task haltTask = new Task(() => { Thread.Sleep(TimeSpan.FromSeconds(120)); if (currHaltID != haltID) return; Dzugaru.Search.Solver.ShouldHaltSearch = true; });
                    //haltTask.Start();

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


                        bool submitted;

                        for (; ; )
                        {
                            try
                            {
                                submitted = SubmitGuess(p.ID, result);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Too many requests!");
                                Thread.Sleep(4000);
                                continue;
                            }

                            break;
                        }

                        if (submitted)
                        {
                            Console.WriteLine("Guess accepted!!");
                            wins++;                            
                        }
                        else
                        {
                            Console.WriteLine("Mismatch!");
                            //Let's try again
                            i--;
                            continue;
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
                //Thread.Sleep(20000);
                //Console.WriteLine("Ready next");
                //Console.ReadLine();
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

        static Problem GetProblemFromResp(TrainResponse resp)
        {
            Problem pr = new Problem()
            {
                ID = resp.Id,
                Size = resp.Size,
                Evals = new ulong[512][],
                Solution = resp.Program
            };

            pr.SetAllowedOperators(resp.Operators);

            ulong[] results = new ulong[512];
            for (int i = 0; i < 2; i++)
            {
                var evalJObject = new JObject(
                    new JProperty("id", resp.Id),
                    new JProperty("arguments", new JArray(EvalArgs.Skip(256 * i).Take(256).Select(a => a.ToString("X")).ToArray())));
                JsonReader reader = MakeRequest(RequestType.eval, evalJObject);

                JObject evalResp = (JObject)JObject.ReadFrom(reader);

                for (int j = 0; j < 256; j++)
                {
                    pr.Evals[i * 256 + j] = new ulong[2];
                    pr.Evals[i * 256 + j][0] = EvalArgs[i * 256 + j];
                    pr.Evals[i * 256 + j][1] = ulong.Parse(evalResp["outputs"][j].Value<string>().Substring(2), System.Globalization.NumberStyles.HexNumber);                  
                }
            }

            return pr;
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

            return GetProblemFromResp(resp);
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
            JsonReader reader = MakeRequest(RequestType.train, new JObject(new JProperty("size", size)/*, new JProperty("operators", new JArray()))*/));
            return GetProblemFromReader(reader, true);
        }

        static Problem GetRealProblem(Func<TrainResponse, bool> predicate)
        {
            Console.WriteLine("Problems of this type left: " + RealProblems.Count(a => predicate(a)));
            return GetProblemFromResp(RealProblems.First(a => predicate(a) && !RealSolvedIDs.Contains(a.Id)));
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

            RealProblems = new List<TrainResponse>();
            foreach (var p in probs)
            {
                TrainResponse resp = ser.Deserialize<TrainResponse>(((JObject)p).CreateReader());
                if (!RealSolvedIDs.Contains(resp.Id))
                {
                    RealProblems.Add(resp);
                }
            }

           

            int k = RealProblems.Count(a => a.Operators.Contains("tfold"));
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
