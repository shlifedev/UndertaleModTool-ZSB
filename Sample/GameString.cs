
[System.Serializable]
public class GameString
{
    public GameString(string hash, string origin, string autoTranslate, string ko)
    {
        this.hash = hash;
        this.origin = origin;
        this.autoTranslate = autoTranslate;
        this.ko = ko;
    }

    public string hash { get; set; }
    public string origin { get; set; }
    public string autoTranslate { get; set; }
    public string ko { get; set; }

  
} 
