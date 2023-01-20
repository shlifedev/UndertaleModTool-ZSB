using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Models;

public class Patcher
{ 
    ///// <summary>
    ///// 중복 스트링이 있을 수 있으므로 리스트로 관리한다.
    ///// </summary>
    //private Dictionary<string, List<UndertaleString>> hashedLocalDatas;

    public UndertaleData Data { get; set; }
    public string TranslateFilePath { get; }
    public string FontPath { get; }

    /// <summary>
    /// qjsdurepdlxj
    /// </summary>
    private Dictionary<string, GameString> _loadedLocale;
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
            // 임시 처리, 사실상 소문자 스트링은 번역에서 걸러내도 될 것 같음
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
            str.Content.StartsWith("float4") == false &&
            // Inventory는 번역시 세이브가 날아간다. 하드코딩 이슈같은데 이 스트링은 무시해야함.
            str.Content.Equals("Inventory") == false
        );
    }
     
    /// <summary>
    /// 게임 스트링 모두 출력(저장)
    /// </summary>  
    /// <param name="savePath"></param>
    /// <param name="migrationLocalePath"></param>
    /// <returns></returns>
    public Patcher ExportStrings(string savePath, string migrationLocalePath = null)
    {

        HashSet<string> hash = new HashSet<string>();
        List<GameString> strings = new List<GameString>(); 
        var fi = new System.IO.FileInfo(migrationLocalePath);
        if (fi.Exists)
        {
            var localeContent = System.IO.File.ReadAllText(fi.FullName);
            var list = JsonConvert.DeserializeObject<List<GameString>>(localeContent);
            if (list != null)
            {
                Console.WriteLine("마이그레이션 Locale 불러옴");
                strings.AddRange(list);

                list.Select(x => x.hash)
                    .ToList()
                    .ForEach(x => hash.Add(x));
            }
        }

        foreach (var str in Data.Strings)
        {
            var createHash = CreateMD5(str.Content);

            if (IsMaybePureString(str) && !hash.Contains(createHash))
            {
                var data = new GameString(createHash, str.Content, str.Content, str.Content);
                strings.Add(data);
            }
        }

        var content = JsonConvert.SerializeObject(strings, Formatting.Indented); 
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
        var data = InIParser.GetAllString();
        foreach (var d in data)
            Console.WriteLine($"{d} {CreateMD5(d)}");
        Console.WriteLine("원본파일 읽는중...");
         
        TranslateFilePath = translateFilePath;
        FontPath = fontPath;

        // 데이터 불러오기
        this.Data = ReadDataFile(new FileInfo(dataFilePath));  
        //Console.WriteLine("게임 내 폰트 이름과 폰트 사이즈를 출력합니다.");
        //Data.Fonts.ToList().ForEach(x =>
        //{
        //    Console.WriteLine(x.Name + "," + x.EmSize);
        //});
         

        Console.WriteLine("번역파일을 불러오고 있습니다."); 
        // 번역 파일 로드 (data.json)
        var content = System.IO.File.ReadAllText(translateFilePath);
        var loadedGameStrings = JsonConvert.DeserializeObject<List<GameString>>(content);
        _loadedLocale = new Dictionary<string, GameString>();

        // 로드된 번역파일 데이터에 추가 
        if (loadedGameStrings != null) 
            foreach (var str in loadedGameStrings) 
                if (str.hash != null && !_loadedLocale.ContainsKey(str.hash)) 
                    _loadedLocale.Add(str.hash, str);  


        // 몇 가지 예외 체크
        if (_loadedLocale.Count == 0) throw new Exception("번역 데이터 로드실패 (code 2)");
        if (loadedGameStrings == null) throw new Exception("번역 데이터 로드실패"); 
    }
 

    public Patcher ApplyTranslate()
    {
        Console.WriteLine("불러온 번역데이터를 게임컨텐츠에 적용중");
        foreach (var localString in Data.Strings)
        {
            // 로컬 스트링의 해시를 읽는다.
            var localHash = CreateMD5(localString.Content);
            // 게임 로컬스트링이 번역파일 해시에 존재하는경우 스트링을 갈아끼운다.
            // 이렇게 하면 게임 버전이 바뀌어도 스트링이 동일하면 대부분 대응이 가능함.
            if (_loadedLocale.ContainsKey(localHash))
            {
                // 게임 실제 패치플로우
                string content = "";
                content = _loadedLocale[localHash].ko;

                // 불러오는데 시간이 좀 더 걸려도 그렇게 느리진 않을거같고 번역시트상 데이터 자체가 잘못되어 있을 수 있으니
                // 순수 스트링 검사를 한번 수행하는게 좋을 듯.
                if (IsMaybePureString(localString))
                {
                    localString.Content = content;
                }
            }
        }
        return this;
    }


    private UndertaleTexturePageItem LoadFontTexture(string fontName)
    {
        if (_fontTextureMap.ContainsKey(fontName)) return _fontTextureMap[fontName];
        // ./localization/font
        var fontDir = new DirectoryInfo(FontPath);
        var png = new FileInfo(Path.Combine(FontPath, fontName + ".png"));
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
        var yy = new FileInfo(Path.Combine(FontPath, fontName + ".yy"));
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
        if (font == null) throw new Exception(originalFontName + " 게임내에서 폰트를 찾을 수 없음.");
        var fontData = LoadFontData(fontName);
        var texture  = LoadFontTexture(fontName);

        font.Texture = texture;
        font.Glyphs.Clear();
        font.DisplayName = Data.Strings.MakeString((string)fontData["fontName"]); 
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
        var fontDi = new System.IO.DirectoryInfo(FontPath);
        var fonts = fontDi.GetFiles("*.yy");
        foreach (var font in Data.Fonts)
        {
            var convertableFont = fonts.Where(x => x.Name.Contains(font.Name.Content)).FirstOrDefault();
            if (convertableFont != null)
            {
                Console.WriteLine("폰트를 불러오는데 성공했습니다. =>" + convertableFont.Name);
                ChangeFont(font.Name.Content, convertableFont.Name.Split('.')[0]);
            }
            else
            {
                //Console.WriteLine($"{font.Name.Content} 폰트가 없으므로 기본 나눔고딕 폰트 불러옴");
                ChangeFont(font.Name.Content, "default_nanumgothic");
            }
        } 
        return this;
    }
    public Patcher End() => this;

    public Patcher Save(string path)
    {
        Console.WriteLine("번역 적용을 위해 저장중..");
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
