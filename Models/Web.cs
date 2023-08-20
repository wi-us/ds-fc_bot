using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Faceit_Bot.Models
{
    public class WebRequests
    {
        public static dynamic GetJSONResponseFromWebsite(string strURL, string strToken, bool doDeserialize = true)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Authorization", strToken);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string jsonResponse = reader.ReadToEnd();

                    if (doDeserialize)
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                    }
                    else
                    {
                        return jsonResponse;
                    }
                }
            }
        }
    }
}
