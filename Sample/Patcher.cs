using UndertaleModLib;
using UndertaleModLib.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class Patcher
{
    private Dictionary<string, GameString> translateData;
    /// <summary>
    /// 중복 스트링이 있을 수 있으므로 리스트로 관리한다.
    /// </summary>
    private Dictionary<string, List<UndertaleString>> hashedLocalDatas;
    private UndertaleData data;
    public Patcher(UndertaleData data, string translateFilePath)
    {
        this.data = data; 
        hashedLocalDatas = new Dictionary<string, List<UndertaleString>>();

        Console.WriteLine("translated.json 분석중..");
        var content = System.IO.File.ReadAllText(translateFilePath);
        var loadedGameStrings = JsonSerializer.Deserialize<List<GameString>>(content);
        translateData = new Dictionary<string, GameString>();
        if (loadedGameStrings != null)
        { 
            foreach(var str in loadedGameStrings)
            {
                if(str.hash != null && !translateData.ContainsKey(str.hash))
                {
                    translateData.Add(str.hash, str); 
                }
            }
        }
        if(translateData.Count == 0) throw new Exception("번역 데이터 로드실패 (code 2)");
        if (loadedGameStrings == null) throw new Exception("번역 데이터 로드실패");

        foreach(var internalString in data.Strings)
        {
            var hash = CreateMD5(internalString.Content);
            if (!hashedLocalDatas.ContainsKey(hash))
                hashedLocalDatas.Add(hash, new List<UndertaleString>()); 
            hashedLocalDatas[hash].Add(internalString);
        } 
    } 

    public void Patch()
    {
        Console.WriteLine("번역데이터를 게임컨텐츠에 수정중..");
        foreach (var gameString in data.Strings)
        {
            var localHash = CreateMD5(gameString.Content);
            if (translateData.ContainsKey(localHash))
            {
                // 게임 실제 패치플로우
                string content = null;
                content = translateData[localHash].ko;
                Console.WriteLine("기존" + gameString.Content + "를 " +content  + "로 수정중");
                gameString.Content = content;
            }
        }
    }
        
    public void Save(string path)
    {
        Console.WriteLine("새로운 바이너리로 저장중..");
        using FileStream fs = new FileInfo(path).OpenWrite();
        UndertaleIO.Write(fs, data);
    }


    string CreateMD5(string input)
    {
        var md5 = MD5.Create();
        byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        string r = null;
        foreach (var d in data)
        {
            var newByte = (d + 256) % 256;
            var hex = newByte.ToString("x2");
            var paded = hex.PadLeft(2, '0');
            r += paded;
        }
        return r.ToUpper();
    }
}
