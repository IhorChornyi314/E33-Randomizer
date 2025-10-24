using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

public abstract class Controller<T>
{
    public List<T> ObjectsData = new();
    public Dictionary<string, T> ObjectsByName = new();

    public void ReadObjectsData(string path)
    {
        using (StreamReader r = new StreamReader($"{RandomizerLogic.DataDirectory}/skill_data.json"))
        {
            string json = r.ReadToEnd();
            ObjectsData = JsonConvert.DeserializeObject<List<T>>(json);
        }

        ObjectsByName = ObjectsData.Select(o => new KeyValuePair<string, T>((o as ObjectData).CodeName, o)).ToDictionary();
    }
    
    public abstract void Initialize();

    public T GetObject(string objectCodeName)
    {
        return ObjectsByName.TryGetValue(objectCodeName, out var obj) ? obj : default;
    }
}