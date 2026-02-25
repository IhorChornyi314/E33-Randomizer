namespace E33Randomizer;

public class ImportData(string cp, string cn, string on)
{
    public string ClassPackage = cp;
    public string ClassName = cn;
    public int OuterIndex = 0;
    public string ObjectName = on;

    public ImportData(string on) : this("/Script/CoreUObject", "Package", on)
    {
        ObjectName = on;
    }
}