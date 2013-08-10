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
            Problem p = GetTrainProblem(5);

            Console.WriteLine("Got problem: " + p.ToString());


            Searcher s = new Searcher();
            s.Find(p);

            //Console.WriteLine(s.VariantsCount);

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
            }

            WebResponse response = req.GetResponse();
            
            return new JsonTextReader(new StreamReader(response.GetResponseStream()));
        }

        static Problem GetTrainProblem(int size)
        {
            JsonReader reader = MakeRequest(RequestType.train, new JObject(new JProperty("size", size + 1)));
            JsonSerializer ser = new JsonSerializer();
            TrainResponse resp = ser.Deserialize<TrainResponse>(reader);

            Random rng = new Random();
            ulong[] args = Enumerable.Range(0, 10).Select(a => GetRandomUlong(rng)).ToArray();

            var evalJObject =  new JObject(
                new JProperty("id", resp.Id),
                new JProperty("arguments", new JArray(args.Select(a => a.ToString("X")).ToArray())));
            reader = MakeRequest(RequestType.eval, evalJObject);

            JObject evalResp = (JObject)JObject.ReadFrom(reader);
            ulong[] results = evalResp["outputs"].Value<JArray>().Select(a => ulong.Parse(a.Value<string>().Substring(2), System.Globalization.NumberStyles.HexNumber)).ToArray();

            Problem pr = new Problem()
                {
                    Size = size,
                    Evals = new ulong[results.Length][],
                    Solution = resp.Program
                };

            for (int i = 0; i < results.Length; i++)
            {
                pr.Evals[i] = new[] { args[i], results[i] };
            }

            return pr;
        }

        static ulong GetRandomUlong(Random rng)
        {
            byte[] buf = new byte[8];
            rng.NextBytes(buf);
            return BitConverter.ToUInt64(buf, 0);
        }
    }
}
