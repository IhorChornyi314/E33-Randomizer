using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using E33Randomizer.ItemSources;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;


public static class ItemsController
{
    public static List<ItemData> ItemsData = new();
    public static List<ItemData> AccountedItemsData = new();
    public static Dictionary<string, ItemData> ItemsByName = new();
    public static List<ItemSource> ItemsSources = new();
    public static List<string> ItemCodeNames = new();
    public static EditIndividualObjectsWindowViewModel ViewModel = new();

    public static Dictionary<string, List<CheckData>> CheckTypes = new();
    
    private static UAsset asset;
    private static UDataTable itemsCompositeTable;

    public static ItemData GetRandomItem()
    {
        var r = RandomizerLogic.rand.Next(ItemsData.Count);
        return ItemsData[r];
    }
    
    public static ItemData GetItemData(string itemCodeName)
    {
        return ItemsByName.TryGetValue(itemCodeName, out var itemData) ? itemData : new ItemData();
    }

    public static string GetItemCategory(string itemCodeName)
    {
        return RandomizerLogic.CustomItemPlacement.ItemCategories.GetValueOrDefault(itemCodeName, "Invalid");
    }

    public static bool IsItem(string itemCodeName)
    {
        return ItemCodeNames.Contains(itemCodeName);
    }
    
    public static void ProcessFile(string fileName)
    {
        if (fileName.Contains("BP_GameAction") || fileName.Contains("BP_PDT_GameAction") || fileName.Contains("S_ItemOperationData"))
        {
            return;
        }
        var asset = new UAsset(fileName, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        ItemSource newSource;
        string checkType;
        if (fileName.Contains("DT_Merchant"))
        {
            newSource = new MerchantInventoryItemSource();
            checkType = "Merchant inventories";
        }
        else if (fileName.Contains("GA_"))
        {
            newSource = new GameActionItemSource();
            checkType = "Cutscene rewards";
        }
        else if (fileName.Contains("DT_ChestsContent"))
        {
            newSource = new ChestsContentItemSource();
            checkType = "Map pickups";
        }
        else if (fileName.Contains("DT_jRPG_Enemies"))
        {
            newSource = new EnemyLootDropsItemSource();
            checkType = "Enemy drops";
        }
        else if (fileName.Contains("DT_BattleTowerStages"))
        {
            newSource = new BattleTowerItemSource();
            checkType = "Endless tower rewards";
        }
        else
        {
            newSource = new GenericItemSource();
            checkType = "Dialogue rewards";
        }
        newSource.LoadFromAsset(asset);
        ItemsSources.Add(newSource);
        AccountedItemsData.AddRange(newSource.Items);

        if (!CheckTypes.ContainsKey(checkType))
        {
            CheckTypes[checkType] = [];
        }
        
        CheckTypes[checkType].AddRange(newSource.Checks);
    }
    
    public static void BuildItemSources(string filesDirectory)
    {
        if(!Directory.Exists(filesDirectory))
        {
            throw new DirectoryNotFoundException($"Items data directory {filesDirectory} not found");
        }
        ItemsSources.Clear();
        CheckTypes.Clear();
        var fileEntries = new List<string> (Directory.GetFiles(filesDirectory));
        fileEntries.AddRange(Directory.GetFiles(filesDirectory + "/DialoguesData"));
        fileEntries.AddRange(Directory.GetFiles(filesDirectory + "/GameActionsData"));
        fileEntries.AddRange(Directory.GetFiles(filesDirectory + "/MerchantsData"));
        fileEntries = fileEntries.Where(x => Path.GetExtension(x) == ".uasset").ToList();
        foreach(string fileName in fileEntries)
            ProcessFile(fileName);
        UpdateViewModel();
        // foreach (var itemData in ItemsData)
        // {
        //     if (itemData.Type != "Lovely Foot" && itemData.Type != "Journal" && itemData.Type != "Pictos" && !AccountedItemsData.Contains(itemData))
        //     {
        //         Console.WriteLine(itemData);
        //     }
        // }
    }

    public static void ReadTableAsset(string assetPath)
    {
        asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        itemsCompositeTable = (asset.Exports[0] as DataTableExport).Table;

        foreach (StructPropertyData itemData in itemsCompositeTable.Data)
        {
            (itemData.Value[18] as BoolPropertyData).Value = false;
            (itemData.Value[19] as BoolPropertyData).Value = false;
        }
        
        ItemsData = itemsCompositeTable.Data.Select(e => new ItemData(e)).ToList();
        ItemsData = ItemsData.Where(e => !e.IsBroken).ToList();
        ItemsData = ItemsData.OrderBy(e => e.CustomName).ToList();
        ItemsByName = ItemsData.Select(e => new KeyValuePair<string, ItemData>(e.CodeName, e)).ToDictionary();
        ItemCodeNames = ItemsData.Select(e => e.CodeName).ToList();
    }

    public static void WriteTableAsset()
    {
        var filePath = asset.FolderName.Value.Replace("/Game/", "randomizer/Sandfall/Content/") + ".uasset";
        string? directoryPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        asset.Write(filePath);
    }

    public static void WriteItemAssets()
    {
        foreach (var itemsSource in ItemsSources)
        {
            var itemsSourceAsset = itemsSource.SaveToAsset();
            var filePath = itemsSourceAsset.FolderName.Value.Replace("/Game/", "randomizer/Sandfall/Content/") + ".uasset";
            string? directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            itemsSourceAsset.Write(filePath);
        }
    }

    public static void GenerateNewItemChecks()
    {
        SpecialRules.Reset();
        BuildItemSources($"{RandomizerLogic.DataDirectory}/ItemData");
        RandomizerLogic.CustomItemPlacement.Update();
        ItemsSources.ForEach(i => i.Randomize());
        UpdateViewModel();
    }

    public static void Init()
    {
        ReadTableAsset($"{RandomizerLogic.DataDirectory}/Originals/DT_jRPG_Items_Composite.uasset");
        BuildItemSources($"{RandomizerLogic.DataDirectory}/ItemData");
    }
    
    public static void AddItemToCheck(string itemCodeName, string checkCodeName)
    {
        var itemData = GetItemData(itemCodeName);
        var itemSource = ItemsSources.FirstOrDefault(i => i.Checks.Any(c => c.CodeName == checkCodeName));
        if (itemSource == null)
            return;
        var check = itemSource.Checks.FirstOrDefault(c => c.CodeName == checkCodeName);
        itemSource.AddItem(check.Key, itemData);
        UpdateViewModel();
    }
    
    public static void RemoveItemFromCheck(string itemCodeName, string checkCodeName)
    {
        var itemData = GetItemData(itemCodeName);
        var itemSource = ItemsSources.FirstOrDefault(i => i.Checks.Any(c => c.CodeName == checkCodeName));
        if (itemSource == null)
            return;
        var check = itemSource.Checks.FirstOrDefault(c => c.CodeName == checkCodeName);
        itemSource.RemoveItem(check.Key, itemData);
        UpdateViewModel();
    }
    
    public static void UpdateViewModel()
    {
        ViewModel.FilteredCategories.Clear();
        ViewModel.Categories.Clear();
        
        if (ViewModel.AllObjects.Count == 0)
        {
            ViewModel.AllObjects = new ObservableCollection<ObjectViewModel>(ItemsData.Select(i => new ObjectViewModel(i)));
        }

        foreach (var checkCategory in CheckTypes)
        {
            var newTypeViewModel = new CategoryViewModel();
            newTypeViewModel.CategoryName = checkCategory.Key;
            newTypeViewModel.Containers = new ObservableCollection<ContainerViewModel>();

            foreach (var check in checkCategory.Value)
            {
                var newContainer = new ContainerViewModel(check.CodeName, check.CustomName);
                var items = check.IsPartialCheck ? check.ItemSource.GetCheckItems(check.CodeName) : check.ItemSource.Items;
                newContainer.Objects = new ObservableCollection<ObjectViewModel>(items.Select(i => new ObjectViewModel(i)));
                newTypeViewModel.Containers.Add(newContainer);
                if (ViewModel.CurrentContainer != null && check.CodeName == ViewModel.CurrentContainer.CodeName)
                {
                    ViewModel.CurrentContainer = newContainer;
                    ViewModel.UpdateDisplayedObjects();
                }
            }
            
            if (newTypeViewModel.Containers.Count > 0)
            {
                ViewModel.Categories.Add(newTypeViewModel);
            }
        }

        ViewModel.UpdateFilteredCategories();
    }
}









