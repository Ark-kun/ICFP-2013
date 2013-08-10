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
            //JsonReader reader = MakeRequest(RequestType.train, new JObject(new JProperty("size", 4)));


            //JsonSerializer ser = new JsonSerializer();
            //TrainResponse resp = ser.Deserialize<TrainResponse>(reader);

            Searcher s = new Searcher(3);
            s.Find();

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
    }
}
