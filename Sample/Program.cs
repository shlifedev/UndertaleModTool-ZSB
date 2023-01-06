using static UndertaleModLib.UndertaleReader;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class GameString
{ 
    public string original = "";
    public string translate = "";

    public GameString(string original, string translate)
    {
        this.original = original;
        this.translate = translate;
    }
}
 
public static class Program
{
    public static UndertaleData data;
    public static string GameExePath => "./Game/ZERO Sievert.exe";
    public static string GamePath => "./Game/";
    public static string GameArgs => "-game ./Game/data.win";

    private static Regex regexVersionCheck = new Regex("\\d+(?:\\.\\d+)+");
    static void Main(string[] args)
    {
        var skipCreatePatch = args.Where(x => x == "--skip").FirstOrDefault() != null;


        if (!skipCreatePatch)
        {
            Console.WriteLine("원본 파일 로드중..");
            LoadData();
            Console.WriteLine("한글패치 적용 임시 모드파일 생성중..");
            // 적용플로우
            CreateModdedData();
        }
        else
        {
            Console.WriteLine("패치 생성이 스킵되었습니다.");
        }

        
        Console.WriteLine("게임 실행..");
        StartGame();
    }
    static void LoadData()
    { 
        data = ReadDataFile(new FileInfo(@"C:\Users\jsh\UndertaleModTool-ZSB\Sample\bin\Debug\net6.0-windows\Game\data.win"));
    }
    static void CreateModdedData()
    { 
        using FileStream fs = new FileInfo("./Game/Korean.win").OpenWrite(); 
        UndertaleIO.Write(fs, data);
    }
    static void StartGame()
    {
        Process.Start(GameExePath, GameArgs);
    }
    static void ExportString()
    {
        List<GameString> pureStrings = new List<GameString>();
        List<GameString> maybeScripts = new List<GameString>();

        foreach (var str in data.Strings)
        {
            if (IsMaybePureString(str))
            {
                pureStrings.Add(new GameString(str.Content, str.Content)); 
            }
        }
        var sorthByString = pureStrings.OrderBy(x => x.original);
        string x = null;
        string t2 = null;
        foreach (var item in sorthByString)
        {
            x += item.original + "\n";
            if (item.original.Length > 1)
                if (item.original[item.original.Length - 1] == '.')
                    t2 += item.original + "\n";
        }
        System.IO.File.WriteAllText("export.txt", x);
        System.IO.File.WriteAllText("sc.txt", t2);
    }

    static void ImportString()
    {
        List<GameString> pureStrings = new List<GameString>();
        foreach(var data in pureStrings)
        {

        }
    }

    public static bool IsMaybePureString(UndertaleString str)
    { 

        return (
            data.AudioGroups.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Backgrounds.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Sounds.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Shaders.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Sprites.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.EmbeddedTextures.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.TextureGroupInfo.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.TexturePageItems.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Fonts.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Timelines.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.GameObjects.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Paths.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Code.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Functions.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.EmbeddedAudio.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.CodeLocals.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Functions.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Rooms.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Variables.Where(x => x.Name == str).FirstOrDefault() == null &&
            data.Extensions.Where(x => x.Name == str).FirstOrDefault() == null &&
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

    static UndertaleData ReadDataFile(FileInfo datafile)
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
