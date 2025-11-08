using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace E33Randomizer;

public abstract class Controller<T>: BaseController where T: ObjectData, new()
{
    public T DefaultObject = new ();
    public List<T> ObjectsData = new();
    public Dictionary<string, T> ObjectsByName = new();
    public CustomPlacement CustomPlacement;

    public void ReadObjectsData(string path)
    {
        using (StreamReader r = new StreamReader(path))
        {
            string json = r.ReadToEnd();
            ObjectsData = JsonConvert.DeserializeObject<List<T>>(json);
        }

        ObjectsByName = ObjectsData.Select(o => new KeyValuePair<string, T>(o.CodeName, o)).ToDictionary();
    }
    
    public T GetObject(string objectCodeName)
    {
        return ObjectsByName.TryGetValue(objectCodeName, out var obj) ? obj : DefaultObject;
    }

    public List<T> GetObjects(List<string> objectCodeNames)
    {
        return objectCodeNames.Select(GetObject).ToList();
    }

    public T GetRandomObject()
    {
        return Utils.Pick(ObjectsData);
    }
    
}