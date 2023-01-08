using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


public static class Updator
{
    private static string jsonURL = "https://script.google.com/macros/s/AKfycbxRVBLdp0-fhQEcSaAH0ZzA7MpNSBXNJaIUFqRg22aL2LHW3tDCzLm9lAo5-GZYrZT39Q/exec";
    public static async Task<string> GetUpdatedText()
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(jsonURL);
        request.Method = "GET"; 
        var response = await request.GetResponseAsync();
        string responseText = string.Empty;
         
        Stream respStream = response.GetResponseStream();

        using (StreamReader sr = new StreamReader(respStream))
        {
            responseText = sr.ReadToEnd();
        }

        return responseText;
    }
}
