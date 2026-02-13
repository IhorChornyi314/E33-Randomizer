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


public class ItemsController: Controller<ItemData>
{
    private static List<string> _ignoredFiles = [];
    
    public List<ItemSource> ItemsSources = new();

    public Dictionary<string, List<CheckData>> CheckTypes = new();

    public List<string> ItemsWithQuantities = [
        "ChromaPack_Regular",
        "ChromaPack_Large",
        "ChromaPack_ExtraLarge",
        "UpgradeMaterial_Level1",
        "UpgradeMaterial_Level2",
        "UpgradeMaterial_Level3",
        "UpgradeMaterial_Level4",
        "UpgradeMaterial_Level5",
        "Consumable_Respec",
        "Consumable_LuminaPoint",
    ];
    
    private UAsset _compositeTableAsset;
    private UDataTable itemsCompositeTable;
    private Dictionary<string, UAsset> _itemsDataTables = new();
    private string _cleanSnapshot;
    
    public bool IsItem(string itemCodeName)
    {
        return ObjectsByName.ContainsKey(itemCodeName);
    }

    public bool IsGearItem(ItemData item)
    {
        return item.CustomName.EndsWith("Weapon)") || item.CustomName.EndsWith("Pictos)");
    }

    public ItemData GetRandomWeapon(string characterName)
    {
        var allCharacterWeapons = ObjectsData.Where(i => i.CustomName.Contains($"{characterName} Weapon")).ToList();
        var filteredWeapons = allCharacterWeapons.Where(w => !RandomizerLogic.CustomItemPlacement.Excluded.Contains(w.CodeName)).ToList();
        if (!RandomizerLogic.Settings.IncludeCutContentItems) filteredWeapons = filteredWeapons.Where(w => !w.IsCutContent).ToList();
        
        if (filteredWeapons.Any()) allCharacterWeapons = filteredWeapons;
        
        return Utils.Pick(allCharacterWeapons);
    }
    
    public void ProcessFile(string fileName)
    {
        if (_ignoredFiles.Contains(fileName.Split('\\')[^1])) return;
        if (fileName.Contains("GameActionsData") && !fileName.Contains("_GA_")) return;
        
        if (fileName.Contains("S_ItemOperationData") || fileName.Contains("S_TriggerCinematicVariables") || fileName.Contains("E_GestralFightClub_Fighters") || fileName.Contains("BP_GameAction"))
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
        else if (fileName.Contains("DT_LootTable_SkinGustave_Visage"))
        {
            newSource = new LootTableItemSource();
            checkType = "Map pickups";
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
    
    public void BuildItemSources(string filesDirectory)
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

    public void ReadCompositeTableAsset(string assetPath)
    {
        _compositeTableAsset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        itemsCompositeTable = (_compositeTableAsset.Exports[0] as DataTableExport).Table;

        foreach (StructPropertyData itemData in itemsCompositeTable.Data)
        {
            (itemData.Value[18] as BoolPropertyData).Value = false;
            (itemData.Value[19] as BoolPropertyData).Value = false;
        }
        
        // ItemsData = itemsCompositeTable.Data.Select(e => new ItemData(e)).ToList();
        // ObjectsData = ObjectsData.Where(e => !e.IsBroken).ToList();
        // ObjectsData = ObjectsData.OrderBy(e => e.CustomName).ToList();
        // ItemsByName = ObjectsData.Select(e => new KeyValuePair<string, ItemData>(e.CodeName, e)).ToDictionary();
        // ItemCodeNames = ObjectsData.Select(e => e.CodeName).ToList();
    }

    public void ReadOtherTableAsset(string assetPath)
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
    
    public void ReadTableAssets(string tablesDirectory)
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

    public void WriteTableAssets()
    {
        foreach (var tableAsset in _itemsDataTables.Values)
        {
            Utils.WriteAsset(tableAsset);
        }
        Utils.WriteAsset(_compositeTableAsset);
    }

    public override void WriteAssets()
    {
        ApplyViewModel();
        RandomizeStartingEquipment();
        foreach (var itemsSource in ItemsSources)
        {
            var itemsSourceAsset = itemsSource.SaveToAsset();
            Utils.WriteAsset(itemsSourceAsset);
        }

        if (RandomizerLogic.Settings.MakeEveryItemVisible)
        {
            WriteTableAssets();
        }
    }

    public void AddSkillItemsToChecks(List<CheckData> randomizableChecks)
    {
        randomizableChecks = Utils.ShuffleList(randomizableChecks).Where(c => !c.IsFixedSize).ToList();
        if (randomizableChecks.Count == 0) return;
        
        for (int i = 0; i < Controllers.SkillsController.SkillItems.Count; i++)
        {
            var firstIndex = i % randomizableChecks.Count;
            randomizableChecks[firstIndex].ItemSource.AddItem(randomizableChecks[firstIndex].Key, Controllers.SkillsController.SkillItems[i]);
            // One more for good measure
            var secondIndex = RandomizerLogic.rand.Next(randomizableChecks.Count);
            randomizableChecks[secondIndex].ItemSource.AddItem(randomizableChecks[secondIndex].Key, Controllers.SkillsController.SkillItems[i]);
        }
    }

    public void AddSkillItemsToTables()
    {
        _itemsDataTables["DT_Items_GradientAttackUnlocks"].AddNameReference(FString.FromString("/Game/StringTables/ST_MainCharacters_Skills.ST_MainCharacters_Skills"));
        
        foreach (var skillItem in Controllers.SkillsController.SkillItems)
        {
            var skillData = Controllers.SkillsController.GetObject(skillItem.CodeName);
            AddItemToTable(_compositeTableAsset, "Quest_MaellePainterSkillsUnlock", skillItem, skillData.IconPath, skillData.StringPath);
            AddItemToTable(_itemsDataTables["DT_Items_GradientAttackUnlocks"], "Quest_MaellePainterSkillsUnlock", skillItem, skillData.IconPath, skillData.StringPath);
        }
    }

    public void AddItemToTable(UAsset tableAsset, string dummyItemName, ItemData itemData, string iconPath = "",
        string stringPath = "")
    {
        var table = (tableAsset.Exports[0] as DataTableExport).Table.Data;
        var dummyItem = table.Find(s => s.Name.ToString() == dummyItemName).Clone() as StructPropertyData;
            
        tableAsset.AddNameReference(FString.FromString(itemData.CodeName));
        var newItem = dummyItem.Clone() as StructPropertyData;
            
        newItem.Name = FName.FromString(tableAsset, itemData.CodeName);
        (newItem.Value[0] as NamePropertyData).Value = FName.FromString(tableAsset, itemData.CodeName);

        if (stringPath.Length > 0)
        {
            var itemNameString = stringPath.Split(':').Last();
            var itemNameSTPath = stringPath.Split(':')[0];
            Utils.AddImportToUAsset(tableAsset, "StringTable", itemNameSTPath);
            tableAsset.AddNameReference(FString.FromString(itemNameString));
            tableAsset.AddNameReference(FString.FromString(itemNameSTPath));
            newItem.Value[1] = table[0].Value[1].Clone() as TextPropertyData;
            (newItem.Value[1] as TextPropertyData).Value = FString.FromString(itemNameString);
            (newItem.Value[1] as TextPropertyData).TableId = FName.FromString(tableAsset, itemNameSTPath);
            (newItem.Value[1] as TextPropertyData).Flags = 0;
            (newItem.Value[1] as TextPropertyData).HistoryType = TextHistoryType.StringTableEntry;
        }
        
        if (iconPath.Length > 0)
        {
            tableAsset.AddNameReference(FString.FromString(iconPath));
            tableAsset.AddNameReference(FString.FromString(iconPath.Split('/').Last()));
            (newItem.Value[5] as SoftObjectPropertyData).FromString([iconPath, iconPath.Split('/').Last(), ""], tableAsset);
        }
            
        table.Add(newItem);
    }

    public void AddItem(ItemData newItem)
    {
        if (ObjectsData.Contains(newItem)) return;
        
        ObjectsData.Add(newItem);
        ObjectsByName[newItem.CodeName] = newItem;
    }

    public override void Randomize()
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
        if (RandomizerLogic.Settings.MakeSkillsIntoItems)
        {
            AddSkillItemsToChecks(randomizableSources.SelectMany(iS => iS.Checks).ToList());
        }
        if (!RandomizerLogic.Settings.IncludeCutContentItems && !cutContentAlreadyExcluded)
        {
            RandomizerLogic.CustomItemPlacement.RemoveExcluded("Cut Content Items");
        }
        UpdateViewModel();
    }

    public override void InitFromTxt(string text)
    {
        text = text.ReplaceLineEndings("\n");
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

    public override string ConvertToTxt()
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

    public override void Initialize()
    {
        using (StreamReader r = new StreamReader($"{RandomizerLogic.DataDirectory}/broken_files.txt"))
        {
            string text = r.ReadToEnd();
            text = text.ReplaceLineEndings("\n");
            _ignoredFiles = text.Split('\n').ToList();
        }
        
        ReadObjectsData($"{RandomizerLogic.DataDirectory}/item_data.json");
        ReadTableAssets($"{RandomizerLogic.DataDirectory}/Originals/ItemTables");
        BuildItemSources($"{RandomizerLogic.DataDirectory}/ItemData");
        ViewModel.ContainerName = "Check";
        ViewModel.ObjectName = "Item";
        _cleanSnapshot = ConvertToTxt();
    }

    public override void Reset()
    {
        InitFromTxt(_cleanSnapshot);
    }

    public void RandomizeStartingEquipment()
    {
        List<string> characterNames = ["Gustave", "Lune", "Maelle", "Sciel", "Verso", "Monoco"];
        //TODO: Try changing Noahram in Sandfall/Content/Gameplay/Save/BP_SaveManager
        if (RandomizerLogic.Settings.RandomizeStartingWeapons)
        {
            foreach (var characterName in characterNames)
            {
                var randomWeapon = GetRandomWeapon(characterName);
                CharacterStartingStateManager.SetStartingWeapon(characterName, randomWeapon);
            }
        }
        if (RandomizerLogic.Settings.RandomizeStartingCosmetics)
        {
            foreach (var characterName in characterNames)
            {
                var characterOutfits = ObjectsData.Where(i => i.CustomName.Contains($"{characterName} Outfit")).ToList();
                var randomOutfit = Utils.Pick(characterOutfits);
                var characterHaircuts = ObjectsData.Where(i => i.CustomName.Contains($"{characterName} Haircut")).ToList();
                var randomHaircut = Utils.Pick(characterHaircuts);
                CharacterStartingStateManager.SetStartingCosmetics(characterName, randomOutfit, randomHaircut);
            }
        }
    }
    
    public override void AddObjectToContainer(string itemCodeName, string checkViewModelCodeName)
    {
        var itemData = GetObject(itemCodeName);
        var itemSourceFileName = checkViewModelCodeName.Split("#")[0];
        var checkKey = checkViewModelCodeName.Split("#")[1];
        var itemSource = ItemsSources.FirstOrDefault(i => i.FileName == itemSourceFileName);
        var check = itemSource.Checks.FirstOrDefault(c => c.Key == checkKey);
        itemSource.AddItem(check.Key, itemData);
        AddObjectToContainerVM(itemData, checkViewModelCodeName);
    }
    
    public override void RemoveObjectFromContainer(int itemIndex, string checkViewModelCodeName)
    {
        if (itemIndex < 0) return;
        
        var itemSourceFileName = checkViewModelCodeName.Split("#")[0];
        var checkKey = checkViewModelCodeName.Split("#")[1];
        var itemSource = ItemsSources.FirstOrDefault(i => i.FileName == itemSourceFileName);
        var check = itemSource.Checks.FirstOrDefault(c => c.Key == checkKey);
        itemSource.RemoveItem(check.Key, itemIndex);
        RemoveObjectFromContainerVM(itemIndex, checkViewModelCodeName);
    }

    public override void ApplyViewModel()
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
                    itemSource.SourceSections[check.Key][i].Item = GetObject(itemViewModel.CodeName);
                    itemSource.SourceSections[check.Key][i].Quantity = Math.Abs(itemViewModel.IntProperty);
                    itemSource.SourceSections[check.Key][i].MerchantInventoryLocked = itemViewModel.BoolProperty;
                }
            }
        }
    }
    
    public override void UpdateViewModel()
    {
        ViewModel.FilteredCategories.Clear();
        ViewModel.Categories.Clear();
        
        if (ViewModel.AllObjects.Count != ObjectsData.Count)
        {
            ViewModel.AllObjects = new ObservableCollection<ObjectViewModel>(ObjectsData.Select(i => new ObjectViewModel(i)).OrderBy(ovm => ovm.Name));
            foreach (var objectViewModel in ViewModel.AllObjects)
            {
                objectViewModel.IntProperty = ItemsWithQuantities.Contains(objectViewModel.CodeName) ? 1 : -1;
            }
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
                newContainer.CanAddObjects = checkCategory.Key != "Dialogue rewards";

                for (int i = 0; i < newContainer.Objects.Count; i++)
                {
                    newContainer.Objects[i].Index = i;
                    if (itemSource.HasItemQuantities)
                    {
                        var itemParticle = itemSource.SourceSections[check.Key][i];
                        newContainer.Objects[i].IntProperty = itemParticle.Item.HasQuantities ? itemParticle.Quantity : -1;
                    }
                    if (checkCategory.Key == "Merchant inventories")
                    {
                        newContainer.Objects[i].HasBoolPropertyControl = true;
                        newContainer.Objects[i].BoolProperty =
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









