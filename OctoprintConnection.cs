using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OctoprintClient

{
    public enum httpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }
    public class OctoprintConnection
    {
        public string endPoint { get; set; }
        public string apiKey { get; set; }
        private string GCodeString { get; set; }
        private float xpos { get; set; }
        private float ypos { get; set; }
        private float zpos { get; set; }
        private int gcodePos { get; set; }
        public httpVerb httpMethod { get; set; }
        public OctoprintConnection()
        {
            endPoint = string.Empty;
            apiKey = string.Empty;
            httpMethod = httpVerb.GET;
        }
        public string makeRequest()
        {
            string strResponseValue = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = httpMethod.ToString();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()){
                if (response.StatusCode != HttpStatusCode.OK){
                    throw new ApplicationException("Response Status Code:"+response.StatusCode.ToString());
                }
                using (Stream responseStream = response.GetResponseStream()){
                    if(responseStream!=null){
                        using (StreamReader reader = new StreamReader(responseStream)){
                            strResponseValue = reader.ReadToEnd();
                        }
                    }
                }
            }
            return strResponseValue;
        }
        public string makeRequest(string location)
        {
            string strResponseValue = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint+ location + "?apikey=" + apiKey);
            request.Method = httpMethod.ToString();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException("Response Status Code:" + response.StatusCode.ToString());
                }
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            strResponseValue = reader.ReadToEnd();
                        }
                    }
                }
            }
            return strResponseValue;
        }
        public float[] getCurrentPos()
        {
            float[] coordinateResponseValue = { 0, 0, 0 };
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint + "/api/job X-Api-Key:" + apiKey);
            request.Method = httpMethod.ToString();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()){
                if (response.StatusCode != HttpStatusCode.OK){
                    throw new ApplicationException("Response Status Code:" + response.StatusCode.ToString());
                }
                using (Stream responseStream = response.GetResponseStream()){
                    if(responseStream!=null){
                        using (StreamReader reader = new StreamReader(responseStream)){
                            string strResponseValue = reader.ReadToEnd();
                            JObject data = JsonConvert.DeserializeObject<JObject>(strResponseValue);
                            if(GCodeString==null){
                                string filelocation = "/api/files" + data["job"]["file"]["origin"] + "/" + data["job"]["file"]["name"];
                                GCodeString=makeRequest(filelocation);
                                string[] preloadString = GCodeString.Substring(0, (int)data["progress"]["filepos"] - 1).Split(new[] { '\r', '\n' });
                                foreach (string line in preloadString){
                                    ReadLine(line);
                                }
                                gcodePos = (int)data["progress"]["filepos"]-1;

                            }
                            if (gcodePos != (int)data["progress"]["filepos"])
                            {
                                string currline = GCodeString.Substring((int)data["progress"]["filepos"]).Split(new[] { '\r', '\n' })[0];
                                ReadLine(currline);
                                gcodePos = (int)data["progress"]["filepos"];
                                coordinateResponseValue[0] = xpos;
                                coordinateResponseValue[1] = ypos;
                                coordinateResponseValue[2] = zpos;
                            }
                        }
                    }
                }
            }
            return coordinateResponseValue;
        }
        private void ReadLine(string currline)
        {
            if (currline.StartsWith("G1 X") || currline.StartsWith("G1 Y") || currline.StartsWith("G1 Z"))
            {
                if (currline.Split(' ')[1][0] == 'X')
                {
                    xpos += (float)currline.Split(' ')[1][1];
                }
                if (currline.Split(' ')[1][0] == 'Y')
                {
                    ypos += (float)currline.Split(' ')[1][1];
                }
                if (currline.Split(' ')[1][0] == 'Z')
                {
                    zpos += (float)currline.Split(' ')[1][1];
                }
                if (currline.Split(' ').Length > 2 && currline.Split(' ')[2][0] == 'Y')
                {
                    ypos += (float)currline.Split(' ')[2][1];
                }
                if (currline.Split(' ').Length > 2 && currline.Split(' ')[2][0] == 'Z')
                {
                    zpos += (float)currline.Split(' ')[2][1];
                }
                if (currline.Split(' ').Length > 3 && currline.Split(' ')[3][0] == 'Z')
                {
                    zpos += currline.Split(' ')[3][1];
                }
            }
        }
    }
}
