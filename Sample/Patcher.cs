using UndertaleModLib;
using UndertaleModLib.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static UndertaleModLib.Compiler.Compiler.Lexer;
using static UndertaleModLib.Models.UndertaleSequence;
using System.Drawing;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class Patcher
{
    private Dictionary<string, GameString> translateData;
    /// <summary>
    /// 중복 스트링이 있을 수 있으므로 리스트로 관리한다.
    /// </summary>
    private Dictionary<string, List<UndertaleString>> hashedLocalDatas; 

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
        var loadedGameStrings = JsonConvert.DeserializeObject<List<GameString>>(content);
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

        for(int i = 0;  i< Data.Fonts.Count; i++)
        {
            JObject fontData = null;
            var font = Data.Fonts[i];

            var yy = fontYYFiles[0];
            var tex = fontTextureFiles[0];

            using (StreamReader file = File.OpenText(yy.FullName))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    fontData = (JObject)JToken.ReadFrom(reader);
                }
            }
             

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
          
            font.Texture = texturePageItem;
            font.Glyphs.Clear();
            font.DisplayName = Data.Strings.MakeString((string)fontData["fontName"]);
            font.EmSize = (uint)fontData["size"];
            font.Bold = (bool)fontData["bold"];
            font.Italic = (bool)fontData["italic"];
            font.Charset = (byte)fontData["charset"];
            font.AntiAliasing = (byte)fontData["AntiAlias"];

            font.ScaleX = 1;
            font.ScaleY = 1;
            if (fontData.ContainsKey("ascender"))
                font.Ascender = (uint)fontData["ascender"];
            if (fontData.ContainsKey("ascenderOffset"))
                font.AscenderOffset = (int)fontData["ascenderOffset"];

            font.RangeStart = 0;
            font.RangeEnd = 0;

            foreach (JObject range in fontData["ranges"].Values<JObject>())
            {
                var rangeStart = (ushort)range["lower"];
                var rangeEnd = (uint)range["upper"];
                if (font.RangeStart > rangeStart)
                    font.RangeStart = rangeStart;
                if (font.RangeEnd > rangeEnd)
                    font.RangeEnd = rangeEnd;
            }

            foreach (KeyValuePair<string, JToken> glyphMeta in (JObject)fontData["glyphs"])
            {
                var glyph = (JObject)glyphMeta.Value;
                font.Glyphs.Add(new UndertaleFont.Glyph()
                {
                    Character = (ushort)glyph["character"],
                    SourceX = (ushort)glyph["x"],
                    SourceY = (ushort)glyph["y"],
                    SourceWidth = (ushort)glyph["w"],
                    SourceHeight = (ushort)glyph["h"],
                    Shift = (short)glyph["shift"],
                    Offset = (short)glyph["offset"],
                });
            }

            List<UndertaleFont.Glyph> glyphs = font.Glyphs.ToList();

            // I'm literally going to LINQ 100000 times
            // and you can't stop me
            foreach (JObject kerningPair in fontData["kerningPairs"].Values<JObject>())
            {
                var first = (ushort)kerningPair["first"];
                var glyph = glyphs.Find(x => x.Character == first);
                glyph.Kerning.Add(new UndertaleFont.Glyph.GlyphKerning()
                {
                    Other = (short)kerningPair["second"],
                    Amount = (short)kerningPair["amount"],
                });
            }
            // Sort glyphs like in UndertaleFontEditor to be safe
            glyphs.Sort((x, y) => x.Character.CompareTo(y.Character));
            font.Glyphs.Clear();

            foreach (UndertaleFont.Glyph glyph in glyphs)
                font.Glyphs.Add(glyph);
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
