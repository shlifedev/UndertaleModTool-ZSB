using System.Diagnostics;
using UndertaleModLib;

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
    /// 원격 cdn or api에서 다운로드
    /// </summary>
    private static void DownloadLatestData()
    {
#if !RELEASE
        try
        {
            Console.WriteLine("[개발자 모드] 구글시트로부터 최신 번역 파일 다운로드 중");
            var task = Updator.GetLatestFromSpreadSheet();
            if (task.Result != null)
            {
                System.IO.File.WriteAllText(LocalePath, task.Result);
                Console.WriteLine("다운로드 완료\n");
            }
            else
            {
                throw new Exception();
            }
        }
        catch
        {
            Console.WriteLine("최신 데이터 다운로드 실패");
        }
#endif
    }
    static void Main(string[] args)
    { 
        GameRootPath = AppDomain.CurrentDomain.BaseDirectory;
        DownloadLatestData();  
        var patcher = new Patcher(OriginalDataPath, LocalePath, LocaleFontDirectoryPath);
        patcher.ApplyTranslate()
            .ApplyFont()
            .Save(MutatedDataPath); 
        Process.Start(GameExePath, GameArgs);
    } 

}
