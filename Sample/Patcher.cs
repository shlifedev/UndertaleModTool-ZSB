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
using static System.Net.Mime.MediaTypeNames;

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

    private Dictionary<string, UndertaleTexturePageItem> _fontTextureMap = new Dictionary<string, UndertaleTexturePageItem>();
    private Dictionary<string, JObject> _fontYYInfoMap = new Dictionary<string, JObject>();
     
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
    /// <summary>
    /// 번역데이터를 외부로 내보낼때 스트링이 순수한 번역데이터인지 확인한다.
    /// </summary> 
    bool IsMaybePureString(UndertaleString str)
    {

        return (
            Data.AudioGroups.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Backgrounds.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Sounds.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Shaders.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Sprites.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.EmbeddedTextures.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.TextureGroupInfo.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.TexturePageItems.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Fonts.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Timelines.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.GameObjects.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Paths.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Code.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Functions.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.EmbeddedAudio.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.CodeLocals.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Functions.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Rooms.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Variables.Where(x => x.Name == str).FirstOrDefault() == null &&
            Data.Extensions.Where(x => x.Name == str).FirstOrDefault() == null &&
            str.Content.StartsWith("#define") == false &&
            str.Content.Contains("#define") == false &&
            str.Content.Contains("obj_map") == false &&
            str.Content.Contains("tilemap_set") == false &&
            str.Content.StartsWith("precision mediump") == false &&
            str.Content.StartsWith("void main()") == false &&
            str.Content.StartsWith("scr_") == false &&
            str.Content.StartsWith("ga_") == false &&
            str.Content.StartsWith("gm_") == false &&
            str.Content.StartsWith("bool") == false &&
            str.Content.StartsWith("float4") == false
        );
    }

    public Patcher ExportStrings(string savePath)
    {
        List<GameString> strings = new List<GameString>();
        foreach (var str in Data.Strings)
        {
            if (IsMaybePureString(str))
            {
                var data = new GameString(CreateMD5(str.Content), str.Content, str.Content, str.Content);
                strings.Add(data);
            }
        }
        var content = JsonConvert.SerializeObject(strings);
        System.IO.File.WriteAllText(savePath, content);
        return this;
    }

    /// <summary>
    /// 패쳐 생성
    /// </summary>
    /// <param name="dataFilePath">원본 data.win 절대 경로</param>
    /// <param name="translateFilePath">번역 파일 절대 경로</param>
    /// <param name="fontPath">폰트 파일의 절대 경로 (폴더 위치)</param>
    /// <exception cref="Exception"></exception>
    public Patcher(string dataFilePath, string translateFilePath, string fontPath)
    {
        Console.WriteLine("원본파일 읽는중...");
        this.Data = ReadDataFile(new FileInfo(dataFilePath));
        hashedLocalDatas = new Dictionary<string, List<UndertaleString>>();

        Console.WriteLine("번역파일 분석중..");
        var content = System.IO.File.ReadAllText(translateFilePath);
        var loadedGameStrings = JsonConvert.DeserializeObject<List<GameString>>(content);
        translateData = new Dictionary<string, GameString>();
        if (loadedGameStrings != null)
        {
            foreach (var str in loadedGameStrings)
            {
                if (str.hash != null && !translateData.ContainsKey(str.hash))
                {
                    translateData.Add(str.hash, str);
                }
            }
        }
        if (translateData.Count == 0) throw new Exception("번역 데이터 로드실패 (code 2)");
        if (loadedGameStrings == null) throw new Exception("번역 데이터 로드실패");

        foreach (var internalString in Data.Strings)
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
        foreach (var localString in Data.Strings)
        {
            // 로컬 스트링의 해시를 읽는다.
            var localHash = CreateMD5(localString.Content); 
            // 게임 로컬스트링이 번역파일 해시에 존재하는경우 스트링을 갈아끼운다.
            if (translateData.ContainsKey(localHash))
            {
                // 게임 실제 패치플로우
                string content = "";
                content = translateData[localHash].ko;  
                Console.WriteLine($"[Load Localization] {localString.Content} Change To {content}");
                localString.Content = content;
            }
        }
        return this;
    }


    private UndertaleTexturePageItem LoadFontTexture(string fontName)
    {
        if (_fontTextureMap.ContainsKey(fontName)) return _fontTextureMap[fontName];
        // ./localization/font
        var fontDir = new DirectoryInfo(FontPath);
        var png = new FileInfo(Path.Combine(FontPath, fontName+".png"));
        // create embeded texture
        Bitmap bitmap = new Bitmap(png.FullName);
        UndertaleEmbeddedTexture texture = new UndertaleEmbeddedTexture();
        texture.Name = Data.Strings.MakeString("Texture " + Data.EmbeddedTextures.Count); // ???
        texture.TextureData.TextureBlob = File.ReadAllBytes(png.FullName);
        Data.EmbeddedTextures.Add(texture);

        // and register texture page
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
        _fontTextureMap.Add(fontName, texturePageItem);

        return texturePageItem;
    }
    private JObject LoadFontData(string fontName)
    {
        if (_fontYYInfoMap.ContainsKey(fontName))
        {
            return _fontYYInfoMap[fontName];
        }
        var yy = new FileInfo(Path.Combine(FontPath, fontName+".yy"));
        JObject fontData = null;
        using (StreamReader file = File.OpenText((string)yy.FullName))
        {
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                fontData = (JObject)JToken.ReadFrom(reader);
            }
        }
        if (fontData == null) throw new Exception($"{FontPath}/{fontName}의 font data를 읽을 수 없습니다.");
        _fontYYInfoMap.Add(fontName, fontData);

        return fontData;
    }

    /// <summary>
    /// 기존 폰트명과 바꿀 폰트를 입력한다.
    /// </summary>
    /// <param name="originalFontName"></param>
    /// <param name="fontName"></param>
    private void ChangeFont(string originalFontName, string fontName)
    {
        var font = Data.Fonts.Where(x => x.Name.Content == originalFontName).FirstOrDefault();

        var fontData = LoadFontData(fontName);
        var texture = LoadFontTexture(fontName);

        font.Texture = texture;
        font.Glyphs.Clear();
        font.DisplayName = Data.Strings.MakeString((string)fontData["fontName"]);
        font.EmSize = (uint)fontData["size"];
        font.Bold = (bool)fontData["bold"];
        font.Italic = (bool)fontData["italic"];
        font.Charset = (byte)fontData["charset"];
        font.AntiAliasing = (byte)fontData["AntiAlias"];

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
    public Patcher ApplyFont()
    { 

        foreach(var font in Data.Fonts) 
            ChangeFont(font.Name.Content, "Font1"); 


        return this;
    }
    public Patcher End() => this;

    public Patcher Save(string path)
    {
        Console.WriteLine("새로운 바이너리로 저장중..");
        var fi = new FileInfo(path);
        if (fi.Exists) fi.Delete();

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
