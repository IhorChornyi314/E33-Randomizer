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
            checkType = fileName.Contains("DT_LootTable_UpgradeItems") ?  "Enemy drops" : "Dialogue rewards";
            checkType = fileName.Contains("DT_LootTable_UpgradeItems_Exploration") ? "Map pickups" : checkType;
        }
        newSource.LoadFromAsset(asset);
        ItemsSources.Add(newSource);

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
        ApplyViewModel();
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

    public static void ReadChecksTxt(string fileName)
    {
        foreach (var line in File.ReadLines(fileName, Encoding.UTF8))
        {
            var itemSourceName = line.Split('#')[0];
            var sectionKey = line.Split('#')[1].Split('|')[0];
            var particles = line.Contains(":") ? line.Split('|')[1].Split(',').Select(ItemSourceParticle.FromString).ToList() : [];
            var source = ItemsSources.Find(i => i.FileName == itemSourceName);
            source.SourceSections[sectionKey] = particles;
        }
        UpdateViewModel();
    }
    
    public static void WriteChecksTxt(string fileName)
    {
        ApplyViewModel();
        var result = "";
        foreach (var itemsSource in ItemsSources)
        {
            foreach (var section in itemsSource.SourceSections)
            {
                result += $"{itemsSource.FileName}#{section.Key}|" + string.Join(',', section.Value) + "\n";
            }
        }
        File.WriteAllText(fileName, result, Encoding.UTF8);
    }

    public static void Init()
    {
        ReadTableAsset($"{RandomizerLogic.DataDirectory}/Originals/DT_jRPG_Items_Composite.uasset");
        BuildItemSources($"{RandomizerLogic.DataDirectory}/ItemData");
        ViewModel.ContainerName = "Check";
        ViewModel.ObjectName = "Item";
    }
    
    public static void AddItemToCheck(string itemCodeName, string checkViewModelCodeName)
    {
        ApplyViewModel();
        var itemData = GetItemData(itemCodeName);
        var itemSourceFileName = checkViewModelCodeName.Split("#")[0];
        var checkKey = checkViewModelCodeName.Split("#")[1];
        var itemSource = ItemsSources.FirstOrDefault(i => i.FileName == itemSourceFileName);
        var check = itemSource.Checks.FirstOrDefault(c => c.Key == checkKey);
        itemSource.AddItem(check.Key, itemData);
        UpdateViewModel();
    }
    
    public static void RemoveItemFromCheck(int itemIndex, string checkViewModelCodeName)
    {
        ApplyViewModel();
        var itemSourceFileName = checkViewModelCodeName.Split("#")[0];
        var checkKey = checkViewModelCodeName.Split("#")[1];
        var itemSource = ItemsSources.FirstOrDefault(i => i.FileName == itemSourceFileName);
        var check = itemSource.Checks.FirstOrDefault(c => c.Key == checkKey);
        itemSource.RemoveItem(check.Key, itemIndex);
        UpdateViewModel();
    }

    public static void ApplyViewModel()
    {
        foreach (var categoryViewModel in ViewModel.Categories)
        {
            foreach (var checkViewModel in categoryViewModel.Containers)
            {
                var checkViewModelCodeName = checkViewModel.CodeName;
                var itemSourceFileName = checkViewModelCodeName.Split("#")[0];
                var checkKey = checkViewModelCodeName.Split("#")[1];
                var itemSource = ItemsSources.FirstOrDefault(i => i.FileName == itemSourceFileName);
                var check = itemSource.Checks.FirstOrDefault(c => c.Key == checkKey);
                var checkItemViewModels = checkViewModel.Objects;
                for (int i = 0; i < checkItemViewModels.Count; i++)
                {
                    var itemViewModel = checkItemViewModels[i];
                    itemSource.SourceSections[check.Key][i].Item = GetItemData(itemViewModel.CodeName);
                    itemSource.SourceSections[check.Key][i].Quantity = itemViewModel.ItemQuantity;
                    itemSource.SourceSections[check.Key][i].MerchantInventoryLocked = itemViewModel.MerchantInventoryLocked;
                }
            }
        }
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
                var newContainer = new ContainerViewModel($"{check.ItemSource.FileName}#{check.Key}", check.CustomName);
                var itemSource = check.ItemSource;
                var items = check.IsPartialCheck ? itemSource.GetCheckItems(check.CodeName) : itemSource.Items;
                newContainer.Objects = new ObservableCollection<ObjectViewModel>(items.Select(i => new ObjectViewModel(i)));

                for (int i = 0; i < newContainer.Objects.Count; i++)
                {
                    newContainer.Objects[i].Index = i;
                    if (itemSource.HasItemQuantities)
                    {
                        newContainer.Objects[i].ItemQuantity = itemSource.GetItemQuantity(check.Key, i);
                    }
                    if (checkCategory.Key == "Merchant inventories")
                    {
                        newContainer.Objects[i].IsMerchantInventory = true;
                        newContainer.Objects[i].MerchantInventoryLocked =
                            itemSource.SourceSections[check.Key][i].MerchantInventoryLocked;
                    }
                }
                
                newTypeViewModel.Containers.Add(newContainer);
                if (ViewModel.CurrentContainer != null && $"{check.ItemSource.FileName}#{check.Key}" == ViewModel.CurrentContainer.CodeName)
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









