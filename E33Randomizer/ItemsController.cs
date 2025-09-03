using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using E33Randomizer.ItemSources;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;


public static class ItemsController
{
    public static List<ItemData> ItemsData = new();
    public static Dictionary<string, ItemData> ItemsByName = new();
    public static List<ItemSource> ItemsSources = new();
    public static List<string> ItemCodeNames = new();
    public static EditIndividualObjectsWindowViewModel ViewModel = new();

    public static Dictionary<string, List<CheckData>> CheckTypes = new();
    
    private static UAsset _compositeTableAsset;
    private static UDataTable itemsCompositeTable;
    private static Dictionary<string, UAsset> _itemsDataTables = new();
    private static string _cleanSnapshot;

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

    public static bool IsGearItem(ItemData item)
    {
        return item.CustomName.Contains("Weapon") || item.CustomName.Contains("Pictos");
    }

    public static ItemData GetRandomWeapon(string characterName)
    {
        var allCharacterWeapons = ItemsData.Where(i => i.CustomName.Contains($"{characterName} Weapon")).ToList();
        var filteredWeapons = allCharacterWeapons.Where(w => !RandomizerLogic.CustomItemPlacement.Excluded.Contains(w.CodeName)).ToList();
        if (filteredWeapons.Any()) allCharacterWeapons = filteredWeapons;
        
        return Utils.Pick(allCharacterWeapons);
    }
    
    public static void ProcessFile(string fileName)
    {
        if (fileName.Contains("BP_GameAction") || fileName.Contains("BP_PDT_GameAction") || fileName.Contains("S_ItemOperationData") || fileName.Contains("E_GestralFightClub_Fighters"))
        {
            return;
        }
        var asset = new UAsset(fileName, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        using StreamReader DialogueRewardPathsReader = new StreamReader($"{RandomizerLogic.DataDirectory}/dialogue_reward_paths.json");
        string DialogueRewardPathsJson = DialogueRewardPathsReader.ReadToEnd();
        DialogueItemSource.DialogueRewardPaths = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<int>>>>(DialogueRewardPathsJson);
        
        using StreamReader DialogueRewardQuantitiesPathsReader = new StreamReader($"{RandomizerLogic.DataDirectory}/dialogue_quantity_paths.json");
        string DialogueRewardQuantitiesPathsJson = DialogueRewardQuantitiesPathsReader.ReadToEnd();
        DialogueItemSource.DialogueRewardQuantitiesPaths = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<int>>>>(DialogueRewardQuantitiesPathsJson);
        
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
        else if (fileName.Contains("DT_LootTable_UpgradeItems"))
        {
            newSource = new LootTableItemSource();
            checkType = fileName.Contains("DT_LootTable_UpgradeItems_Exploration") ? "Map pickups" : "Enemy drops";
        }
        else if (DialogueItemSource.DialogueRewardPaths.ContainsKey(asset.FolderName.ToString().Split('/').Last()))
        {
            newSource = new DialogueItemSource();
            checkType = "Dialogue rewards";
        }
        else
        {
            newSource = new GenericItemSource();
            checkType = "Dialogue rewards";
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
    }

    public static void ReadCompositeTableAsset(string assetPath)
    {
        _compositeTableAsset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        itemsCompositeTable = (_compositeTableAsset.Exports[0] as DataTableExport).Table;

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

    public static void ReadOtherTableAsset(string assetPath)
    {
        var tableAsset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);

        _itemsDataTables[tableAsset.FolderName.ToString().Split('/').Last()] = tableAsset;
        
        foreach (StructPropertyData itemData in (tableAsset.Exports[0] as DataTableExport).Table.Data)
        {
            var itemName = itemData.Name.ToString();
            if (itemName.Contains("Consumable_") || itemName == "PartyHealConsumable") continue;
            (itemData.Value[18] as BoolPropertyData).Value = false;
            (itemData.Value[19] as BoolPropertyData).Value = false;
        }
    }
    
    public static void ReadTableAssets(string tablesDirectory)
    {
        if(!Directory.Exists(tablesDirectory))
        {
            throw new DirectoryNotFoundException("ItemTables directory not found");
        }
        var fileEntries = new List<string> (Directory.GetFiles(tablesDirectory));
        fileEntries = fileEntries.Where(x => Path.GetExtension(x) == ".uasset").ToList();
        foreach (var fileEntry in fileEntries)
        {
            if (fileEntry.Contains("DT_jRPG_Items_Composite"))
            {
                ReadCompositeTableAsset(fileEntry);
                continue;
            }
            ReadOtherTableAsset(fileEntry);
        }
    }

    public static void WriteTableAssets()
    {
        foreach (var tableAsset in _itemsDataTables.Values)
        {
            Utils.WriteAsset(tableAsset);
        }
        Utils.WriteAsset(_compositeTableAsset);
    }

    public static void WriteItemAssets()
    {
        ApplyViewModel();
        RandomizeStartingEquipment();
        foreach (var itemsSource in ItemsSources)
        {
            var itemsSourceAsset = itemsSource.SaveToAsset();
            Utils.WriteAsset(itemsSourceAsset);
        }
    }

    public static void GenerateNewItemChecks()
    {
        SpecialRules.Reset();
        Reset();
        var cutContentAlreadyExcluded = RandomizerLogic.CustomItemPlacement.Excluded.Contains("Cut Content Items");
        if (!RandomizerLogic.Settings.IncludeCutContentItems)
        {
            RandomizerLogic.CustomItemPlacement.AddExcluded("Cut Content Items");
        }
        RandomizerLogic.CustomItemPlacement.Update();
        var randomizableSources = ItemsSources.Where(SpecialRules.Randomizable).ToList();
        randomizableSources.ForEach(i => i.Randomize());
        ItemsSources.ForEach(i => i.Checks.ForEach(SpecialRules.ApplySpecialRulesToCheck));
        if (!RandomizerLogic.Settings.IncludeCutContentItems && !cutContentAlreadyExcluded)
        {
            RandomizerLogic.CustomItemPlacement.RemoveExcluded("Cut Content Items");
        }
        UpdateViewModel();
    }

    public static void InitFromTxt(string text)
    {
        foreach (var line in text.Split('\n'))
        {
            if (line == "") continue;
            var itemSourceName = line.Split('#')[0];
            var sectionKey = line.Split('#')[1].Split('|')[0];
            var particles = line.Contains(":") ? line.Split('|')[1].Split(',').Select(ItemSourceParticle.FromString).ToList() : [];
            var source = ItemsSources.Find(i => i.FileName == itemSourceName);
            source.SourceSections[sectionKey] = particles;
        }
        UpdateViewModel();
    }
    
    public static void ReadChecksTxt(string fileName)
    {
        InitFromTxt(File.ReadAllText(fileName));
    }

    public static string ConvertToTxt()
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
        return result;
    }
    
    public static void WriteChecksTxt(string fileName)
    {
        var result = ConvertToTxt();
        File.WriteAllText(fileName, result, Encoding.UTF8);
    }

    public static void Init()
    {
        ReadTableAssets($"{RandomizerLogic.DataDirectory}/Originals/ItemTables");
        BuildItemSources($"{RandomizerLogic.DataDirectory}/ItemData");
        ViewModel.ContainerName = "Check";
        ViewModel.ObjectName = "Item";
        _cleanSnapshot = ConvertToTxt();
    }

    public static void Reset()
    {
        InitFromTxt(_cleanSnapshot);
    }

    public static void RandomizeStartingEquipment()
    {
        if (RandomizerLogic.Settings.RandomizeStartingWeapons)
        {
            List<string> characterNames = ["Lune", "Maelle", "Sciel", "Verso", "Monoco"];
            var tableAsset = new UAsset($"{RandomizerLogic.DataDirectory}/Originals/StartingInfoTables/DT_jRPG_CharacterSaveStates.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
            var tableData = (tableAsset.Exports[0] as DataTableExport).Table.Data;

            foreach (var propertyData in tableData)
            {
                var characterName = propertyData.Name.ToString();
                if (!characterNames.Contains(characterName)) continue;
                var mapValues = (propertyData.Value[10] as MapPropertyData).Value.Values.ToList();
                var nameProperty = mapValues[0] as NamePropertyData;
                var randomWeapon = GetRandomWeapon(characterName);
                tableAsset.AddNameReference(FString.FromString(randomWeapon.CodeName));
                nameProperty.Value = FName.FromString(tableAsset, randomWeapon.CodeName);
            }
            Utils.WriteAsset(tableAsset);
        }
        if (RandomizerLogic.Settings.RandomizeStartingCosmetics)
        {
            var tableAsset = new UAsset($"{RandomizerLogic.DataDirectory}/Originals/StartingInfoTables/DT_jRPG_CharacterDefinitions.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
            var tableNames = tableAsset.GetNameMapIndexList();
            for (int i = 0; i < tableNames.Count; i++)
            {
                var nameString = tableNames[i].ToString();
                if (!nameString.Contains("Face") && !nameString.Contains("Skin")) continue;
                var cosmeticCategory = nameString.Split('_')[0];
                var characterCosmetic = ItemsData.Where(i => i.CodeName.Contains(cosmeticCategory)).ToList();
                var randomCosmetic = Utils.Pick(characterCosmetic);
                tableAsset.SetNameReference(i, FString.FromString(randomCosmetic.CodeName));
            }
            Utils.WriteAsset(tableAsset);
        }
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
                    itemSource.SourceSections[check.Key][i].Quantity = Math.Abs(itemViewModel.ItemQuantity);
                    itemSource.SourceSections[check.Key][i].MerchantInventoryLocked = itemViewModel.MerchantInventoryLocked;
                }
            }
        }
    }
    
    public static void UpdateViewModel()
    {
        ViewModel.FilteredCategories.Clear();
        ViewModel.Categories.Clear();
        var allObjects = ItemsData.Select(i => i as ObjectData).ToList();
        
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
                var items = itemSource.GetCheckItems(check.Key);
                newContainer.Objects = new ObservableCollection<ObjectViewModel>(items.Select(i => new ObjectViewModel(i)));

                for (int i = 0; i < newContainer.Objects.Count; i++)
                {
                    newContainer.Objects[i].Index = i;
                    if (itemSource.HasItemQuantities)
                    {
                        var itemParticle = itemSource.SourceSections[check.Key][i];
                        newContainer.Objects[i].ItemQuantity = itemParticle.Item.Type == "Upgrade Material" ? itemParticle.Quantity : -1;
                    }
                    if (checkCategory.Key == "Merchant inventories")
                    {
                        newContainer.Objects[i].IsMerchantInventory = true;
                        newContainer.Objects[i].MerchantInventoryLocked =
                            itemSource.SourceSections[check.Key][i].MerchantInventoryLocked;
                    }

                    if (checkCategory.Key == "Dialogue rewards")
                    {
                        newContainer.Objects[i].CanDelete = false;
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









