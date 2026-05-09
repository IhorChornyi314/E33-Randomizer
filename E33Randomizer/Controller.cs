using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace E33Randomizer;

public abstract class Controller<T>: BaseController where T: ObjectData, new()
{
    public T DefaultObject = new ();
    public List<T> ObjectsData = new();
    public ObjectPool<T> RandomObjectPool;
    public Dictionary<string, T> ObjectsByName = new();
    protected string _cleanSnapshot;

    public void ResetRandomObjectPool(List<T> excluded=null)
    {
        RandomObjectPool = new ObjectPool<T>(ObjectsData, excluded);
    }
    
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
            ObjectsData = DefaultObject switch
            {
                CharacterData => JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ListCharacterData)?.Cast<T>().ToList() ?? [],
                CheckData => JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ListCheckData)?.Cast<T>().ToList() ?? [],
                EnemyData => JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ListEnemyData)?.Cast<T>().ToList() ?? [],
                ItemData => JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ListItemData)?.Cast<T>().ToList() ?? [],
                LocationData => JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ListLocationData)?.Cast<T>().ToList() ?? [],
                SkillData => JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ListSkillData)?.Cast<T>().ToList() ?? [],
                SpawnPointData => JsonSerializer.Deserialize(json, JsonSourceGenerationContext.Default.ListSpawnPointData)?.Cast<T>().ToList() ?? [],
                _ => throw new NotImplementedException()
            };
        }

        ObjectsByName = ObjectsData.Select(o => new KeyValuePair<string, T>(o.CodeName, o)).ToDictionary();
    }

    public bool IsObject(string codeName)
    {
        return ObjectsByName.ContainsKey(codeName);
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
        if (RandomObjectPool == null)
        {
            ResetRandomObjectPool();
        }
        return RandomObjectPool.GetObject();
    }
    
    public abstract void WriteAssets();
    
}