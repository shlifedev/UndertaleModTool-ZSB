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
    public static string GameExePath => "./ZERO Sievert.exe";
    public static string GamePath => "./";
    public static string DataPath => "./data.win";
    public static string GameArgs => "-game ./localized.win";
    public static string LocalePath => "./localization/data.json";
    public static string LocaleFontPath => "./localization/font";
    static void Main(string[] args)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [제로 시버트 한글패치 CLI] [v.001]");
            Console.WriteLine("  오류수정/버그문의 : shlifedev@gmail.com ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" 아래의 숫자를 입력해주세요.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" 1 -  입력시 게임 한글패치 진행 (30초-1분 정도 시간 소요)\n");
            Console.WriteLine(" 2 -  입력시 한글패치 적용된 게임실행 (1번과정 완료시 생략가능)\n");
            Console.WriteLine(" 0 -  [개발용] 모든 한글 출력\n");
            Console.Write("숫자를 입력해주세요 :");
            Console.ForegroundColor = ConsoleColor.White;
            var input = Console.ReadLine();
            Console.Clear();
            if (input == "1")
            {
                Console.Clear();
                var patcher = new Patcher(DataPath, LocalePath, LocaleFontPath);
                patcher.ApplyTranslate();
                patcher.ApplyFont();
                patcher.Save("./localized.win");
                Process.Start(GameExePath, GameArgs);
            }
            if (input == "2")
            {
                Process.Start(GameExePath, GameArgs);
                break;
            }
            if (input == "0")
            {
                var patcher = new Patcher(DataPath, LocalePath, LocaleFontPath);
                patcher.ExportStrings("./localization/debug/en.json");
                break;
            }
            Console.Clear();
        }
    }

 
     
     
}
