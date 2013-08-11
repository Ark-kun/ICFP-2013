using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ark.Icfp2013.Net {
    public class Server {
        static string UserID = File.ReadAllText("key.txt");
        static string RequestPath = "http://icfpc2013.cloudapp.net/{0}?auth={1}";
        static JsonSerializer _serializer = new JsonSerializer();


        static JsonReader MakeRequest(RequestType type, JObject parameters) {
            while (true) {
                try {
                    HttpWebRequest req = HttpWebRequest.CreateHttp(string.Format(RequestPath, type.ToString().ToLowerInvariant(), UserID));
                    req.Method = "POST";

                    if (parameters != null) {
                        req.ContentType = "application/json";
                        Stream reqStream = req.GetRequestStream();
                        JsonWriter writer = new JsonTextWriter(new StreamWriter(reqStream));
                        parameters.WriteTo(writer);
                        writer.Flush();
                        reqStream.Close();
                        reqStream.Dispose();
                    }

                    using (WebResponse response = req.GetResponse()) {
                        MemoryStream stream = new MemoryStream();
                        response.GetResponseStream().CopyTo(stream);
                        stream.Position = 0;
                        return new JsonTextReader(new StreamReader(stream));
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex);
                    Thread.Sleep(1000);
                }
            }
        }

        public static Problem GetTrainProblem(ProblemType problemType, int problemSize) {
            int size = problemSize + 1;
            JArray operators = new JArray();
            switch (problemType) {
                case ProblemType.Normal:
                    break;
                case ProblemType.TFold:
                    operators = new JArray("tfold");
                    break;
                case ProblemType.Fold:
                    operators = new JArray("fold");
                    break;
                case ProblemType.Bonus:
                    size = 42;
                    break;
                case ProblemType.Bonus2:
                    size = 137;
                    break;
            }

            JsonReader reader = MakeRequest(RequestType.train, new JObject(new JProperty("size", size), new JProperty("operators", operators)));

            TrainResponse response = _serializer.Deserialize<TrainResponse>(reader);

            Problem problem = new Problem() {
                ID = response.Id,
                Size = size,
                Solution = response.Program
            };

            foreach(string opStr in response.Operators){
                OperatorType opId = OperatorType.None;
                Enum.TryParse(opStr, true, out opId);
                problem.UsedOperators |= opId;
            }

            return problem;
        }

        public static Dictionary<ulong, ulong> EvalProblemOnAllInputs(string id, ulong[] inputs) {
            const int maxInputs = 256;

            var results = new Dictionary<ulong, ulong>();

            int offset = 0;
            int inputsLeft = inputs.Length;

            while (inputsLeft > 0) {
                int length = inputsLeft > maxInputs ? maxInputs : inputsLeft;
                var segment = new ArraySegment<ulong>(inputs, offset, length);
                var outputs = EvalProblem(id, segment);
                for (int i = 0; i < length; i++) {
                    results[inputs[offset + i]] = outputs[i];
                }
                offset += length;
                inputsLeft -= length;
            }
            return results;
        }

        static ulong[] EvalProblem(string id, IEnumerable<ulong> inputs) {
            var evalJObject = new JObject(
                new JProperty("id", id),
                new JProperty("arguments", new JArray(inputs.Select(a => a.ToString("X")).ToArray())));
            var reader = MakeRequest(RequestType.eval, evalJObject);

            JObject evalResp = (JObject)JObject.ReadFrom(reader);
            ulong[] results = evalResp["outputs"].Value<JArray>().Select(a => ulong.Parse(a.Value<string>().Substring(2), NumberStyles.HexNumber)).ToArray();

            return results;
        }

        public static bool SubmitGuess(string id, RootLambda node) {
            JsonReader reader = MakeRequest(RequestType.guess,
                new JObject(new JProperty("id", id),
                new JProperty("program", node.ToString())));

            JObject result = (JObject)JObject.ReadFrom(reader);

            return result["status"].Value<string>() == "win";
        }
    }
}
