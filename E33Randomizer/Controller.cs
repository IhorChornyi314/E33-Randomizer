using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace E33Randomizer;

public abstract class Controller<T>: BaseController where T: ObjectData, new()
{
    public T DefaultObject = new ();
    public List<T> ObjectsData = new();
    public Dictionary<string, T> ObjectsByName = new();

    public void AddObjectToContainerVM(ObjectData objectData, string containerName)
    {
        var containerVms = ViewModel.Categories
            .SelectMany(c => c.Containers)
            .Where(c => c.CodeName == containerName);
        foreach (var containerVm in containerVms)
        {
            var newObjectVm = new ObjectViewModel(objectData);
            newObjectVm.InitComboBox(ViewModel.AllObjects);
            containerVm.Objects.Add(newObjectVm);

            if (ViewModel.CurrentContainer == containerVm)
            {
                ViewModel.DisplayedObjects.Add(newObjectVm);
            }
        }
    }

    public void RemoveObjectFromContainerVM(int objectIndex, string containerName)
    {
        var containerVm = ViewModel.Categories
            .SelectMany(c => c.Containers)
            .FirstOrDefault(c => c.CodeName == containerName);

        if (containerVm != null && objectIndex >= 0 && objectIndex < containerVm.Objects.Count)
        {
            var removedVm = containerVm.Objects[objectIndex];
            containerVm.Objects.RemoveAt(objectIndex);

            if (ViewModel.CurrentContainer == containerVm)
            {
                ViewModel.DisplayedObjects.Remove(removedVm);
            }
        }
    }
    
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

    public List<T> GetObjects(IEnumerable<string> objectCodeNames)
    {
        return objectCodeNames.Select(GetObject).ToList();
    }

    public T GetRandomObject()
    {
        return Utils.Pick(ObjectsData);
    }
    
    public abstract void WriteAssets();
    
}