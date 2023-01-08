using System.Net;


public static class Updator
{
    private static string remoteJsonUrl = "https://script.google.com/macros/s/AKfycbxRVBLdp0-fhQEcSaAH0ZzA7MpNSBXNJaIUFqRg22aL2LHW3tDCzLm9lAo5-GZYrZT39Q/exec";


    public static async Task<string> GetLatestFromSpreadSheet()
    {
        return await Get(remoteJsonUrl);
    }

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
