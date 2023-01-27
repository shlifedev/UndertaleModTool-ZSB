using GMSLocalization;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UndertaleModLib;
using static UndertaleModLib.Compiler.Compiler.AssemblyWriter;

public static class Program
{
    public static Loader loader = null;
    /// <summary>
    /// 현재는 제로시버트만 실행
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    { 
        loader = new Loader(new GMSLocalization.Game.ZeroSievert.Patcher());
        loader.Run(); 
    }
}