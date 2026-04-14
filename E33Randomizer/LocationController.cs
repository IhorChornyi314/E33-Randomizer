using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

public class LocationController: Controller<LocationData>
{
    private LocationGraph _locationGraph = new LocationGraph();
    private Dictionary<string, string> _destinationChanges = new();
    private List<string> _currentConstraints = new();
    public override void Initialize()
    {
        ReadObjectsData($"{RandomizerLogic.DataDirectory}/location_data.json");

        _destinationChanges = ObjectsData.Select(o => new KeyValuePair<string, string>(o.CodeName, o.CodeName)).ToDictionary();
        _locationGraph.Init();
        ViewModel.ContainerName = "Original Destination";
        ViewModel.ObjectName = "New Destination";
        _cleanSnapshot = ConvertToTxt();
        UpdateViewModel();
        ResetRandomObjectPool();
    }

    public override void Randomize()
    {
        foreach (var originalLocation in _destinationChanges.Keys)
        {
            _destinationChanges[originalLocation] = RandomizerLogic.CustomLocationPlacement.Replace(originalLocation);
        }
        _locationGraph.ApplyDestinationChanges(_destinationChanges);
        var criticalPathChanges = _locationGraph.ConstructGoldenPath(_currentConstraints);
        foreach (var (originalDestination, newDestination) in _destinationChanges)
        {
            if (criticalPathChanges.TryGetValue(newDestination, out var criticalPathChange))
            {
                _destinationChanges[originalDestination] = criticalPathChange;
            }
        }
    }

    public void ReadConstraintFile()
    {
        using (StreamReader r = new StreamReader($"{RandomizerLogic.DataDirectory}/critical_path.json"))
        {
            string json = r.ReadToEnd();
            _currentConstraints = JsonConvert.DeserializeObject<List<string>>(json);
        }
    }

    public override void AddObjectToContainer(string objectCodeName, string containerCodeName)
    {
        throw new NotSupportedException("A location can only have one corresponding replacement.");
    }

    public override void RemoveObjectFromContainer(int objectIndex, string containerCodeName)
    {
        throw new NotSupportedException("A location can only have one corresponding replacement.");
    }

    public override void InitFromTxt(string text)
    {
        text = text.ReplaceLineEndings("\n");
        _destinationChanges = text.Split('\n').Select(l => new KeyValuePair<string, string>(l.Split('|')[0], l.Split('|')[1])).ToDictionary();
    }

    public override void ApplyViewModel()
    {
        foreach (var categoryViewModel in ViewModel.Categories)
        {
            foreach (var originalNodeContainer in categoryViewModel.Containers)
            {
                var originalNodeCodeName = originalNodeContainer.CodeName;
                var newNodeCodeName = originalNodeContainer.Objects[0].CodeName;
                _destinationChanges[originalNodeCodeName] = originalNodeContainer.Objects[0].BoolProperty ? newNodeCodeName : originalNodeCodeName;
            }
        }
    }

    public override void UpdateViewModel()
    {
        ViewModel.FilteredCategories.Clear();
        ViewModel.Categories.Clear();
        if (ViewModel.AllObjects.Count == 0)
        {
            ViewModel.AllObjects = new ObservableCollection<ObjectViewModel>(ObjectsData.Select(i => new ObjectViewModel(i)));
            foreach (var objectViewModel in ViewModel.AllObjects)
            {
                objectViewModel.BoolProperty = true;
            }
        }
        
        Dictionary<string, CategoryViewModel> categoryViewModels = new();
        
        foreach (var (originalDestination, newDestination) in _destinationChanges)
        {
            var originalNode = GetObject(originalDestination);
            var newNode = GetObject(newDestination);
            
            var categoryName = originalNode.CustomName.Split(" - ")[0];
            if (!categoryViewModels.ContainsKey(categoryName))
            {
                categoryViewModels[categoryName] = new CategoryViewModel();
                categoryViewModels[categoryName].CategoryName = originalNode.CustomName.Split(" - ")[0];
                categoryViewModels[categoryName].Containers = new ObservableCollection<ContainerViewModel>();
                ViewModel.Categories.Add(categoryViewModels[categoryName]);
            }
            var newContainer = new ContainerViewModel(originalNode.CodeName, originalNode.CustomName);
            newContainer.Objects = new ObservableCollection<ObjectViewModel>();
            newContainer.Objects.Add(new ObjectViewModel(newNode));
            newContainer.Objects[0].CanDelete = false;
            newContainer.Objects[0].Index = 0;
                
            // Whether the change is active
            newContainer.Objects[0].BoolProperty = true;
            newContainer.Objects[0].HasBoolPropertyControl = true;
                
            newContainer.CanAddObjects = false;
                
            categoryViewModels[categoryName].Containers.Add(newContainer);
            if (ViewModel.CurrentContainer != null && originalNode.CodeName == ViewModel.CurrentContainer.CodeName)
            { 
                ViewModel.CurrentContainer = newContainer;
                ViewModel.UpdateDisplayedObjects();
            }
        }
        ViewModel.UpdateFilteredCategories();
    }

    public override string ConvertToTxt()
    {
        return string.Join('\n', _destinationChanges.Select(kvp => $"{kvp.Key}|{kvp.Value}"));
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void WriteAssets()
    {
        throw new NotImplementedException();
    }
}