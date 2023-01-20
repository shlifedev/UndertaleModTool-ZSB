using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UndertaleModLib;
using static UndertaleModLib.Compiler.Compiler.AssemblyWriter;

public static class Program
{
    /// <summary>
    /// 게임 실행 경로
    /// </summary>
    public static string GameRootPath => AppDomain.CurrentDomain.BaseDirectory;
    /// <summary>
    /// 데이터파일 경로
    /// 임베드 데이터로드가 가능하려면 exe와 같은폴더에 있어야함
    /// </summary> 
    public static string GameArgs => $"-game localized.win -debugoutput ./localization/log.txt";
    /// <summary>
    /// 실행파일 경로
    /// </summary>
    public static string GameExePath => Path.Combine(GameRootPath, "Zero Sievert.exe");
    /// <summary>
    /// 데이터 파일 위치
    /// </summary>
    public static string OriginalDataPath => Path.Combine(GameRootPath, "data.win");
    /// <summary>
    /// 수정된 data.win 위치
    /// </summary>
    public static string MutatedDataPath => Path.Combine(GameRootPath, "localized.win");
    /// <summary>
    /// 번역데이터 경로
    /// </summary>
    public static string LocalePath => Path.Combine(AppContext.BaseDirectory, "localization", "data.json");
    /// <summary>
    /// 번역데이터 폴더 경로
    /// </summary>
    public static string LocaleFontDirectoryPath => Path.Combine(AppContext.BaseDirectory, "localization", "font");


    /// <summary>
    /// 번역데이터 폴더 경로
    /// </summary>
    public static string HashSumPath => Path.Combine(AppContext.BaseDirectory, "localization", "checksum.bin");



    /// <summary>
    /// 원격 cdn or api에서 다운로드
    /// </summary>
    private static async Task<string> DownloadLatestData()
    {
#if !RELEASE
        try
        {
            Console.WriteLine("[개발자 모드] 구글시트로부터 최신 번역 파일 다운로드 중, 언제든지 X를 눌러 취소가능");
            var task = await HttpUtils.GetLatestFromSpreadSheet();
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

    static bool CheckSum()
    {
        var checkSumData = new FileInfo(HashSumPath);
        if (checkSumData.Exists == false)
        {
            System.IO.File.WriteAllText(HashSumPath, SHA256CheckSum(LocalePath), Encoding.UTF8);
            return false;
        }
        else
        {
            var saved = System.IO.File.ReadAllText(checkSumData.FullName, Encoding.UTF8);
            var current = SHA256CheckSum(LocalePath);
            if (saved == current) return true;
        }
        System.IO.File.WriteAllText(HashSumPath, SHA256CheckSum(LocalePath), Encoding.UTF8);
        return false;
    }

    static string SHA256CheckSum(string filePath)
    {
        using (SHA256 SHA256 = SHA256Managed.Create())
        {
            using (FileStream fileStream = File.OpenRead(filePath))
                return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
        }
    }


    static void Main(string[] args)
    { 
        var task = Task.Run(async () => {
             await DownloadLatestData();
         });

        task.Wait();
        if (CheckSum() == false)
        {
            Console.WriteLine("번역파일 갱신에따른 데이터 생성필요");
            var patcher = new Patcher(OriginalDataPath, LocalePath, LocaleFontDirectoryPath);

            patcher.ApplyIgnoreINI() 
                .ApplyFont()
                .ApplyTranslate()
                .Save(MutatedDataPath);

            Process.Start(GameExePath, GameArgs);
        }
        else
        {
//#if !RELEASE
//            var patcher = new Patcher(OriginalDataPath, LocalePath, LocaleFontDirectoryPath);
//            patcher.ExportStrings("./localization/debug/migration.json", LocalePath);
//#endif

            Console.WriteLine("갱신 할 번역데이터가 없습니다. 게임을 실행합니다.");
            Process.Start(GameExePath, GameArgs);
        }
    }
}