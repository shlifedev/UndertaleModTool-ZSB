using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

/// <summary>
/// 시버트 세이브파일 파싱용
/// </summary>
public class InI
{
    public List<InIGroup> groups;
    public InI(string path)
    {
        groups = new List<InIGroup>();
        var data = System.IO.File.ReadAllText(path);
        var lines = data.Split("\n").Where(x => x.Length > 1).ToList();
        InIGroup current = null;
        foreach (var m in lines)
        {
            string output = new string(m.Where(c => !char.IsControl(c)).ToArray());
            var IsGroup = (output[0] == '[' && output[output.Length - 1] == ']');
            if (IsGroup)
            {
                var name = output.Substring(1, output.Length - 2);
                groups.Add(new InIGroup(name));
                current = groups[groups.Count - 1];
            }
            else
            {
                if (current != null)
                {
                    var k = m.Split('=')[0];
                    var v = m.Split('=')[1];
                    v = new string(v.Where(c => !char.IsControl(c)).ToArray());

                    var pureValue = v.Substring(1, v.Length - 2); // "" 제거
                    current.Datas.Add(new InIData(k, v));
                }
            }
        }
    }
}
public class InIGroup
{
    public InIGroup(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
    public List<InIData> Datas = new List<InIData>();
}
public class InIData
{
    public string key;
    public string value;

    public InIData(string key, string value)
    {
        this.key = key;
        this.value = value;
    }
}
public static class InIParser
{
    public static HashSet<string> GetAllSettingKeyValues()
    {
        var savePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/ZERO_Sievert";
        var iniDi = new System.IO.DirectoryInfo(savePath);
        HashSet<string> hashs = new HashSet<string>();

        if (iniDi == null || iniDi.Exists == false)
            return hashs;

        iniDi.GetFiles("*.ini").ToList().ForEach(file =>
        {
            var ini = new InI(file.FullName);
            var gKeys = ini.groups.Select(x => x.Name);
            var datas = ini.groups.SelectMany(b => b.Datas).Distinct();
            foreach (var v in gKeys)
                if (!hashs.Contains(v))
                    hashs.Add(v);
            foreach (var v in datas)
                if (!hashs.Contains(v.key))
                    hashs.Add(v.key);
        });

        return hashs;
    }
}
