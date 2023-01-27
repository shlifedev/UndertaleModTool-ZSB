using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GMSLocalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Models;

namespace GMSLocalization
{
    public abstract class PatcherBase
    {

        public UndertaleData Data { get; set; } 
        private Dictionary<string, GameString> _loadedLocale;
        private Dictionary<string, UndertaleTexturePageItem> _fontTextureMap = new Dictionary<string, UndertaleTexturePageItem>();
        private Dictionary<string, JObject> _fontYYInfoMap = new Dictionary<string, JObject>();
        protected HashSet<string> ignoreStringSet = new HashSet<string>();

        /// <summary>
        /// 게임 exe 파일명
        /// </summary>
        public abstract string GameExeFileName { get; }


        /// <summary>
        /// 게임 실행 경로
        /// </summary>
        protected string GameRootPath => AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 데이터파일 경로
        /// 임베드 데이터로드가 가능하려면 exe와 같은폴더에 있어야함
        /// </summary> 
        protected virtual string GameArgs => $"-game localized.win";
        /// <summary>
        /// 실행파일 경로
        /// </summary>
        public virtual string GameExePath => Path.Combine(GameRootPath, GameExeFileName);

        /// <summary>
        /// 데이터 파일 위치
        /// </summary>
        public virtual string OriginalDataPath => Path.Combine(GameRootPath, "data.win");
        /// <summary>
        /// 수정된 data.win 위치
        /// </summary>
        public virtual string NewDataPath => Path.Combine(GameRootPath, "localized.win");
        /// <summary>
        /// 번역데이터 경로
        /// </summary>
        public virtual string LocalePath => Path.Combine(AppContext.BaseDirectory, "localization", "data.json");
        /// <summary>
        /// 번역데이터 폴더 경로
        /// </summary>
        public virtual string LocaleFontDirectoryPath => Path.Combine(AppContext.BaseDirectory, "localization", "font");
        /// <summary>
        /// 중복 적용 방지용 파일해시
        /// </summary>
        public string FileHashPath => Path.Combine(AppContext.BaseDirectory, "localization", "checksum.bin");



       

        public bool Validation()
        {
            if (new System.IO.FileInfo(NewDataPath).Exists == false)
            {
                Console.WriteLine($"{NewDataPath} not found");
                return false;

            } 

            var checkSumData = new FileInfo(FileHashPath);
            if (checkSumData.Exists == false)
            {
                System.IO.File.WriteAllText(FileHashPath, GMSLocalization.Utils.FileUtils.SHA256CheckSum(LocalePath), Encoding.UTF8);
                return false;
            }
            else
            {
                var saved = System.IO.File.ReadAllText(checkSumData.FullName, Encoding.UTF8);
                var current = GMSLocalization.Utils.FileUtils.SHA256CheckSum(LocalePath);
                if (saved == current) return true;
            }
            System.IO.File.WriteAllText(FileHashPath, GMSLocalization.Utils.FileUtils.SHA256CheckSum(LocalePath), Encoding.UTF8);
            return false;
        }

        public void Patch()
        {
            Console.WriteLine("원본파일 읽는중..."); 
            // 데이터 불러오기
            this.Data = ReadDataFile(new FileInfo(OriginalDataPath)); 
            Console.WriteLine("번역파일을 불러오고 있습니다.");
            // 번역 파일 로드 (data.json)
            var content = System.IO.File.ReadAllText(LocalePath);
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

            this.ApplyTranslate();
            this.ApplyFont();
            this.Save();
        }

        public virtual void Run()
        {
            Process.Start(GameExePath, GameArgs);
        }

        /// <summary>
        /// 원격 cdn or api에서 다운로드
        /// </summary>
        public virtual async Task<string> DownloadLatestData()
        {
#if !RELEASE
            try
            {
                Console.WriteLine("[개발자 모드] 구글시트로부터 최신 번역 파일 다운로드 중, 언제든지 X를 눌러 취소가능");
                var task = await Utils.HttpUtils.Get("https://script.google.com/macros/s/AKfycbxRVBLdp0-fhQEcSaAH0ZzA7MpNSBXNJaIUFqRg22aL2LHW3tDCzLm9lAo5-GZYrZT39Q/exec");
                if (task != null)
                {
                    System.IO.File.WriteAllText(LocalePath, task);
                    Console.WriteLine("다운로드 완료\n");
                    return task;
                }
                else
                {
                    throw new Exception();
                    return null;
                }
            }
            catch
            {
                Console.WriteLine("최신 데이터 다운로드 실패");
            }
#endif

            return null;
        }
         
 

        /// <summary>
        /// 번역데이터를 외부로 내보낼때 스트링이 순수한 번역데이터인지 확인한다.
        /// </summary> 
        public virtual bool IsPureString(string str)
        {

            return (
                Data.AudioGroups.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Backgrounds.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Sounds.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Shaders.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Sprites.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.EmbeddedTextures.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.TextureGroupInfo.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.TexturePageItems.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Fonts.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Timelines.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.GameObjects.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Paths.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Code.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Functions.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.EmbeddedAudio.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.CodeLocals.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Functions.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Rooms.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Variables.Where(x => x.Name.Content == str).FirstOrDefault() == null &&
                Data.Extensions.Where(x => x.Name.Content == str).FirstOrDefault() == null
            );
        }

        /// <summary>
        /// 게임 스트링 모두 출력(저장)
        /// </summary>  
        /// <param name="savePath"></param>
        /// <param name="migrationLocalePath"></param>
        /// <returns></returns>
        public void ExportStrings(string savePath, string migrationLocalePath = null)
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
                    list.ForEach(x =>
                        {
                            if (!hash.Contains(x.hash) && IsPureString(x.origin))
                            {
                                hash.Add(x.hash);
                                strings.Add(x);
                            }
                        });
                }
            }

            foreach (var str in Data.Strings)
            {
                var createHash = CreateMD5(str.Content);

                if (IsPureString(str.Content) && !hash.Contains(createHash))
                {
                    var data = new GameString(createHash, str.Content, null, str.Content);
                    strings.Add(data);
                }
            }

            var content = JsonConvert.SerializeObject(strings, Formatting.Indented);
            System.IO.File.WriteAllText(savePath, content);
        }

 
        private void ApplyTranslate()
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
                    if (IsPureString(localString.Content))
                    {
                        localString.Content = content;
                    }
                }
            } 
        }


        /// <summary>
        /// 폰트 텍스쳐 불러오기
        /// </summary>
        /// <param name="fontName"></param>
        /// <returns></returns>
        private UndertaleTexturePageItem LoadFontTexture(string fontName)
        {
            if (_fontTextureMap.ContainsKey(fontName)) return _fontTextureMap[fontName];
            var png = new FileInfo(Path.Combine(LocaleFontDirectoryPath, fontName + ".png"));
            // create embeded texture
            Bitmap bitmap = new Bitmap(png.FullName);
            UndertaleEmbeddedTexture texture = new UndertaleEmbeddedTexture();
            texture.Name = Data.Strings.MakeString("Texture " + Data.EmbeddedTextures.Count); // ???
            texture.TextureData.TextureBlob = File.ReadAllBytes(png.FullName);
            Data.EmbeddedTextures.Add(texture);

            // texture page 등록
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
            var yy = new FileInfo(Path.Combine(LocaleFontDirectoryPath, fontName + ".yy"));
            JObject fontData = null;
            using (StreamReader file = File.OpenText((string)yy.FullName))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    fontData = (JObject)JToken.ReadFrom(reader);
                }
            }
            if (fontData == null) throw new Exception($"{LocaleFontDirectoryPath}/{fontName}의 font data를 읽을 수 없습니다.");
            _fontYYInfoMap.Add(fontName, fontData);

            return fontData;
        }

        /// <summary>
        /// 폭트 텍스쳐 변경, 기존 폰트명과 바꿀 폰트를 입력한다.
        /// </summary>
        /// <param name="originalFontName"></param>
        /// <param name="fontName"></param>
        private void ReplaceFontTexture(string originalFontName, string fontName)
        {
            var font = Data.Fonts.Where(x => x.Name.Content == originalFontName).FirstOrDefault();
            if (font == null) throw new Exception(originalFontName + " 게임내에서 폰트를 찾을 수 없음.");
            var fontData = LoadFontData(fontName);
            var texture = LoadFontTexture(fontName);

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
            glyphs.Sort((x, y) => x.Character.CompareTo(y.Character));
            font.Glyphs.Clear();

            foreach (UndertaleFont.Glyph glyph in glyphs)
                font.Glyphs.Add(glyph);
        }

        /// <summary>
        /// 폰트 적용
        /// </summary>
        private void ApplyFont()
        {
            var fontDi = new System.IO.DirectoryInfo(LocaleFontDirectoryPath);
            var fonts = fontDi.GetFiles("*.yy");
            foreach (var font in Data.Fonts)
            {
                var convertableFont = fonts.Where(x => x.Name.Contains(font.Name.Content)).FirstOrDefault();
                if (convertableFont != null)
                {
                    Console.WriteLine("폰트를 불러오는데 성공했습니다. =>" + convertableFont.Name);
                    ReplaceFontTexture(font.Name.Content, convertableFont.Name.Split('.')[0]);
                }
                else
                {
                    //Console.WriteLine($"{font.Name.Content} 폰트가 없으므로 기본 나눔고딕 폰트 불러옴");
                    ReplaceFontTexture(font.Name.Content, "default_nanumgothic");
                }
            }
        }

        private PatcherBase Save()
        {
            Console.WriteLine("번역 적용을 위해 저장중..");
            var fi = new FileInfo(NewDataPath);
            if (fi.Exists) fi.Delete();

            using FileStream fs = new FileInfo(NewDataPath).OpenWrite();
            UndertaleIO.Write(fs, Data);

            return this;
        }



        private string CreateMD5(string input)
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

        private UndertaleData ReadDataFile(FileInfo datafile)
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


    }
}