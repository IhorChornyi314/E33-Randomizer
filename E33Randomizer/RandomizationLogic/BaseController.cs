using System.Collections;
using System.Text;

namespace E33Randomizer.RandomizationLogic;

public abstract class BaseController
{
    public EditIndividualObjectsWindowViewModel ViewModel = new();
    public abstract void Initialize();
    public abstract void Randomize();
    public abstract void AddObjectToContainer(string objectCodeName, string containerCodeName);
    public abstract void RemoveObjectFromContainer(int objectIndex, string containerCodeName);
    public abstract void InitFromTxt(string text);
    public abstract void ApplyViewModel();
    public abstract void UpdateViewModel();
    public abstract string ConvertToTxt();
    public abstract void Reset();
    
    private static readonly IComparer Comparer = new CaseInsensitiveComparer();
    
    public void ReadTxt(string filename)
    {
        InitFromTxt(File.ReadAllText(filename));
    }

    public void WriteTxt(string filename)
    {
        var result = ConvertToTxt();
        File.WriteAllText(filename, result, Encoding.UTF8);
    }
   
    protected static string[] GetFilesSorted(string path)
    {
        var files = Directory.GetFiles(path);
        files.Sort(Comparer.Compare);
        return files;
    }
}