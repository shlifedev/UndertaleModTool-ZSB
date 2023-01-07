using UndertaleModLib;
using UndertaleModLib.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static UndertaleModLib.Compiler.Compiler.Lexer;
using static UndertaleModLib.Models.UndertaleSequence;
using System.Drawing;

public class Patcher
{
    private Dictionary<string, GameString> translateData;
    /// <summary>
    /// 중복 스트링이 있을 수 있으므로 리스트로 관리한다.
    /// </summary>
    private Dictionary<string, List<UndertaleString>> hashedLocalDatas;
    private string v;

    public UndertaleData Data { get; set; }
    public string TranslateFilePath { get; }
    public string FontPath { get; }

    UndertaleData ReadDataFile(FileInfo datafile)
    {
        try
        {
            using FileStream fs = datafile.OpenRead();
            UndertaleData gmData = UndertaleIO.Read(fs);
            return gmData;
        }
        catch (FileNotFoundException e)
        {
            throw new FileNotFoundException($"Data file '{e.FileName}' does not exist");
        }
    }
    public Patcher(string dataFilePath, string translateFilePath, string fontPath)
    {
        this.Data = ReadDataFile(new FileInfo(dataFilePath));
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

        foreach(var internalString in Data.Strings)
        {
            var hash = CreateMD5(internalString.Content);
            if (!hashedLocalDatas.ContainsKey(hash))
                hashedLocalDatas.Add(hash, new List<UndertaleString>()); 
            hashedLocalDatas[hash].Add(internalString);
        }
        TranslateFilePath = translateFilePath;
        FontPath = fontPath;
    }
     

    public Patcher ApplyTranslate()
    {
        Console.WriteLine("번역데이터를 게임컨텐츠에 수정중..");
        foreach (var gameString in Data.Strings)
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
        return this;
    }


    public Patcher ApplyFont()
    { 
        var fontDir = new DirectoryInfo(FontPath);
        var fontYYFiles = fontDir.GetFiles("*.yy");
        var fontTextureFiles = fontDir.GetFiles("*.png");

        for(int i = 0;  i< fontYYFiles.Count(); i++)
        { 
            JsonObject yyData = null;
            var yy = fontYYFiles[i];
            var tex = fontTextureFiles[i];

            //텍스쳐 생성
            Bitmap bitmap = new Bitmap(tex.FullName);
            UndertaleEmbeddedTexture texture = new UndertaleEmbeddedTexture();
            texture.Name = Data.Strings.MakeString("Texture " + Data.EmbeddedTextures.Count); // ???
            texture.TextureData.TextureBlob = File.ReadAllBytes(tex.FullName);
            Data.EmbeddedTextures.Add(texture);


            UndertaleTexturePageItem texturePageItem = new UndertaleTexturePageItem();
            texturePageItem.Name = Data.Strings.MakeString("PageItem " + Data.TexturePageItems.Count); // ???
            texturePageItem.TexturePage = texture;
            texturePageItem.SourceX = 0;
            texturePageItem.SourceY = 0;
            texturePageItem.SourceWidth = (ushort)bitmap.Width;
            texturePageItem.SourceHeight = (ushort)bitmap.Height;
            texturePageItem.TargetX = 0;
            texturePageItem.TargetY = 0;
            texturePageItem.TargetWidth = (ushort)bitmap.Width;
            texturePageItem.TargetHeight = (ushort)bitmap.Height;
            texturePageItem.BoundingWidth = (ushort)bitmap.Width;
            texturePageItem.BoundingHeight = (ushort)bitmap.Height;
            Data.TexturePageItems.Add(texturePageItem);


        }

         
        return this;
    }
    public Patcher End() => this;
        
    public Patcher Save(string path)
    {
        Console.WriteLine("새로운 바이너리로 저장중..");
        using FileStream fs = new FileInfo(path).OpenWrite();
        UndertaleIO.Write(fs, Data); 
        return this;
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
