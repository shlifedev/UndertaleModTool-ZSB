using static UndertaleModLib.UndertaleReader;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

public static class Program
{
    public static UndertaleData data;
    public static string GameExePath => "./Game/ZERO Sievert.exe";
    public static string GamePath => "./Game/";
    public static string GameArgs => "-game ./Game/Test.bin"; 

    
    static void Main(string[] args)
    {
        var skipCreatePatch = args.Where(x => x == "--skip").FirstOrDefault() != null;
        Console.WriteLine("원본 파일 로드중..");
        data = ReadDataFile(new FileInfo(@"C:\Users\jsh\UndertaleModTool-ZSB\Sample\bin\Debug\net6.0-windows\Game\data.win"));

        if (!skipCreatePatch)
        { 
            data = ReadDataFile(new FileInfo(@"C:\Users\jsh\UndertaleModTool-ZSB\Sample\bin\Debug\net6.0-windows\Game\data.win"));
            Console.WriteLine("한글패치 적용 임시 모드파일 생성중..");
            var patcher = new Patcher(data, "translated.json");
            patcher.Patch();
            patcher.Save("./Game/Test.bin");
        }
        else
        {
            Console.WriteLine("패치 생성이 스킵되었습니다.");
        }


        Console.WriteLine("게임 실행..");
        StartGame();
    }

 
    static void StartGame()
    {
        Process.Start(GameExePath, GameArgs);
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
