using System.Net;


namespace GMSLocalization.Utils
{
    public static class HttpUtils
    {
        public static async Task<string> Get(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            var response = await request.GetResponseAsync();

            string responseText = string.Empty;
            Stream respStream = response.GetResponseStream();

            using (StreamReader sr = new StreamReader(respStream))
                responseText = sr.ReadToEnd();
            return responseText;
        }
    }
}