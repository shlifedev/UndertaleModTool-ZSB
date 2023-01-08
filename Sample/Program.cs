using static UndertaleModLib.UndertaleReader;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.IO;
using static UndertaleModLib.Compiler.Compiler.AssemblyWriter; 

public static class Program
{
    public static UndertaleData data;

    /// <summary>
    /// 게임 실행 경로
    /// </summary>
    public static string GameRootPath { get; set; }
    /// <summary>
    /// 데이터파일 경로
    /// 임베드 데이터로드가 가능하려면 exe와 같은폴더에 있어야함
    /// </summary>
    public static string GameArgs => $"-game {MutatedDataPath}";


    public static string GameExePath => $"{GameRootPath}/ZERO Sievert.exe";
    public static string OriginalDataPath => $"{GameRootPath}./data.win";
    public static string MutatedDataPath => $"{GameRootPath}/localized.win";
    public static string LocalePath => $"./localization/data.json";
    public static string LocaleFontDirectoryPath => $"./localization/font";

    private static bool InDevelopment = false;
    static void Main(string[] args)
    {
        GameRootPath = ".";

        if (InDevelopment)
        {
            Console.WriteLine("[개발자 모드] 구글시트로부터 최신 번역 파일 다운로드 중"); 
            var task = Updator.GetUpdatedText();  
            System.IO.File.WriteAllText(LocalePath, task.Result);

            Console.WriteLine("게임을 실행하면 구글시트의 최신 작업내용이 반영되어 있어야 합니다.\n");
            System.Threading.Thread.Sleep(500);
        }  
         

        while (!new FileInfo(GameExePath).Exists)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[!] {GameRootPath}에서 ZERO Sievert.exe  를 찾을 수 없습니다. 이 파일은 제로시버트 설치경로에 포함시켜야 합니다. 게임 경로(폴더)를 직접 입력하세요");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("경로 입력:");
            Console.ForegroundColor = ConsoleColor.White;
            GameRootPath = Console.ReadLine();
        }
        Console.Clear();
        var patcher = new Patcher(OriginalDataPath, LocalePath, LocaleFontDirectoryPath);
        patcher.ApplyTranslate()
            .ApplyFont()
            .Save(MutatedDataPath);

        Process.Start(GameExePath, GameArgs);
    }




}
