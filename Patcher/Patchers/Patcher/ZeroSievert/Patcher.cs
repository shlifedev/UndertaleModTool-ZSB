using GMSLocalization.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
namespace GMSLocalization.Game.ZeroSievert
{
    public class Patcher : PatcherBase
    {
        public override string GameExeFileName => "Zero Sievert.exe";

        /// <summary>
        /// 제로 시버트용 스트링 검출로직 추가
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public override bool IsPureString(string str)
        {
            return base.IsPureString(str) &&
            // 임시 처리, 사실상 소문자 스트링은 번역에서 걸러내도 될 것 같음
            str.StartsWith("#define") == false &&
            str.Contains("#define") == false &&
            str.Contains("obj_map") == false &&
            str.Contains("tilemap_set") == false &&
            str.StartsWith("precision mediump") == false &&
            str.StartsWith("void main()") == false &&
            str.StartsWith("scr_") == false &&
            str.StartsWith("ga_") == false &&
            str.StartsWith("gm_") == false &&
            str.StartsWith("bool") == false &&
            str.StartsWith("float4") == false &&
            // Inventory는 번역시 세이브가 날아간다. 하드코딩 이슈같은데 이 스트링은 무시해야함.
            str.Equals("Inventory") == false && 
            // INI, 커스텀 제외목록등에 포함되는경우
            ignoreStringSet.Contains(str) == false;

        }

        /// <summary>
        /// 제로 시버트용 백업처리
        /// </summary>
        void SaveBackUp()
        {
            var savePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/ZERO_Sievert";
            var di = new DirectoryInfo(savePath);
            var target = "./localization/debug/backup/" + DateTime.Now.ToString("yyyy-M-dd-hh-mm-ss");

            if (Directory.Exists(target) == false)
                Directory.CreateDirectory(target);

           FileUtils.CopyFilesRecursively(di.FullName, target);
            var localization = new FileInfo("./localization/data.json");
            if (localization.Exists)
                localization.CopyTo(Path.Combine(target, localization.Name));
        }

        public override void Run()
        {
            SaveBackUp();
            base.Run();
        }
    }
}
