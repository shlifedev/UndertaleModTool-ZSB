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
    static void Main(string[] args)
    {
        GameRootPath = ".";

        if(!new FileInfo(GameExePath).Exists)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[!] {GameRootPath}에서 ZERO Sievert.exe 를 찾을 수 없습니다. 게임 경로(폴더)를 직접 입력하세요");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("경로 입력:");
            Console.ForegroundColor = ConsoleColor.White;
            GameRootPath = Console.ReadLine(); 
        }

        if (args.Length == 0)
        { 
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" [제로 시버트 한글패치 CLI] [v.0.0.2]"); 

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" 아래의 숫자를 입력해주세요.");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" 1 -  입력시 게임 한글패치 진행 (30초-1분 정도 시간 소요)\n");
                Console.WriteLine(" 2 -  입력시 한글패치 적용된 게임실행 (1번과정 완료시 생략가능)\n");  
                Console.ForegroundColor = ConsoleColor.White;
                var input = Console.ReadLine();
                Console.Clear();
                if (input == "1")
                {
                    Console.Clear();
                    var patcher = new Patcher(OriginalDataPath, LocalePath, LocaleFontDirectoryPath);
                    patcher.ApplyTranslate()
                        .ApplyFont()
                        .Save(MutatedDataPath);

                    Process.Start(GameExePath, GameArgs);
                }
                if (input == "2")
                {
                    Process.Start(GameExePath, GameArgs); 
                } 
                Console.Clear(); 
        }
    }




}
