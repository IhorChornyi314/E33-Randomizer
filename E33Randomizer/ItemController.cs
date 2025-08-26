using System.IO;
using E33Randomizer.ItemSources;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;


public static class ItemController
{
    public static List<ItemData> ItemsData = new();
    public static List<ItemData> AccountedItemsData = new();
    public static Dictionary<string, ItemData> ItemsByName = new();
    public static List<ItemSource> ItemsSources = new();
    public static List<string> ItemCodeNames = new();
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
        if (fileName.Contains("DT_Merchant"))
        {
            var merchantInventory = new MerchantInventoryItemSource();
            merchantInventory.LoadFromAsset(asset);
            ItemsSources.Add(merchantInventory);
            AccountedItemsData.AddRange(merchantInventory.Items);
        }
        else if (fileName.Contains("GA_"))
        {
            var gameAction = new GameActionItemSource();
            gameAction.LoadFromAsset(asset);
            ItemsSources.Add(gameAction);
            AccountedItemsData.AddRange(gameAction.Items);
        }
        else if (fileName.Contains("DT_ChestsContent"))
        {
            var chestsContent = new ChestsContentItemSource();
            chestsContent.LoadFromAsset(asset);
            ItemsSources.Add(chestsContent);
            AccountedItemsData.AddRange(chestsContent.Items);
        }
        else if (fileName.Contains("DT_jRPG_Enemies"))
        {
            var enemyLootDrops = new EnemyLootDropsItemSource();
            enemyLootDrops.LoadFromAsset(asset);
            ItemsSources.Add(enemyLootDrops);
            AccountedItemsData.AddRange(enemyLootDrops.Items);
        }
        else if (fileName.Contains("DT_BattleTowerStages"))
        {
            var battleTowerRewards = new BattleTowerItemSource();
            battleTowerRewards.LoadFromAsset(asset);
            ItemsSources.Add(battleTowerRewards);
            AccountedItemsData.AddRange(battleTowerRewards.Items);
        }
        else
        {
            var genericItemSource = new GenericItemSource();
            genericItemSource.LoadFromAsset(asset);
            ItemsSources.Add(genericItemSource);
            AccountedItemsData.AddRange(genericItemSource.Items);
        }
    }
    
    public static void BuildItemSources(string filesDirectory)
    {
        if(!Directory.Exists(filesDirectory))
        {
            throw new DirectoryNotFoundException($"Items data directory {filesDirectory} not found");
        }
        ItemsSources.Clear();
        var fileEntries = new List<string> (Directory.GetFiles(filesDirectory));
        fileEntries.AddRange(Directory.GetFiles(filesDirectory + "/DialoguesData"));
        fileEntries.AddRange(Directory.GetFiles(filesDirectory + "/GameActionsData"));
        fileEntries.AddRange(Directory.GetFiles(filesDirectory + "/MerchantsData"));
        fileEntries = fileEntries.Where(x => Path.GetExtension(x) == ".uasset").ToList();
        foreach(string fileName in fileEntries)
            ProcessFile(fileName);

        // foreach (var itemData in ItemsData)
        // {
        //     if (itemData.Category != "Lovely Foot" && itemData.Category != "Journal" && itemData.Category != "Pictos" && !AccountedItemsData.Contains(itemData))
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
        ItemsSources.ForEach(i => i.Randomize());
    }

    public static void Init()
    {
        ReadTableAsset("Data/Originals/DT_jRPG_Items_Composite.uasset");
        BuildItemSources("Data/ItemData");
    }
}









