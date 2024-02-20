using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;
using StaleFood.CookingStation;
using StaleFood.CustomPrefabs;
using StaleFood.Utility;
using UnityEngine;
using YamlDotNet.Serialization;

namespace StaleFood.Configurations;

public static class DataManager
{
    private static readonly string DegradeDataFilePath = YmlConfigurations.FolderPath + Path.DirectorySeparatorChar + "DegradeItemData.yml";
    
    private static readonly List<DegradeData> TempDegradeData = new();
    private static readonly List<string> UniquePrefabNames = new();

    public static readonly Dictionary<string, ValidatedDegradeData> DegradeDataMap = new();

    private static readonly CustomSyncedValue<string> ServerDegradeData = new(StaleFoodPlugin.ConfigSync, "ServerDegradeData", "");

    public static void ClearTempDegradeDataList() => TempDegradeData.Clear();
    
    public static void AddDegradeData(ItemDrop itemDrop)
    {
        if (UniquePrefabNames.Contains(itemDrop.name)) return;
        UniquePrefabNames.Add(itemDrop.name);
        TempDegradeData.Add(new DegradeData()
        {
            prefab_name = itemDrop.name,
            shared_name = itemDrop.m_itemData.m_shared.m_name,
        });
    }

    public static void InitDegradeDataMap()
    {
        WriteDegradeDataToFile();
        ReadDegradeDataFile();
        AddCustomItemsToDegradeMap();
    }

    private static void AddCustomItemsToDegradeMap()
    {
        foreach (FoodItemData item in GourmetStation.TempFoodItems)
        {
            if (item.m_spoilItemName.IsNullOrWhiteSpace()) continue;
            if (!ValidateCustomSpoilItem(item.m_spoilItemName, out ItemDrop component)) continue;
            DegradeDataMap[item.m_sharedName] = new ValidatedDegradeData()
            {
                m_prefabName = item.m_prefabName,
                m_sharedName = item.m_sharedName,
                m_spoilItemDrop = component,
                m_duration = item.m_duration
            };
        }
    }

    private static void WriteDegradeDataToFile()
    {
        if (!Directory.Exists(YmlConfigurations.FolderPath)) Directory.CreateDirectory(YmlConfigurations.FolderPath);
        ISerializer serializer = new SerializerBuilder().Build();
        if (File.Exists(DegradeDataFilePath))
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            List<DegradeData> DegradeDataList = deserializer.Deserialize<List<DegradeData>>(File.ReadAllText(DegradeDataFilePath));
            if (DegradeDataList.Count < TempDegradeData.Count)
            {
                StaleFoodPlugin.StaleFoodLogger.LogDebug("Degrade Item Data file has missing values, updating");
                foreach (DegradeData item in TempDegradeData)
                {
                    if (DegradeDataList.Find(x => x.prefab_name == item.prefab_name) != null) continue;
                    StaleFoodPlugin.StaleFoodLogger.LogDebug("Adding " + item.prefab_name);
                    DegradeDataList.Add(item);
                }
                string updatedData = serializer.Serialize(DegradeDataList);
                File.WriteAllText(DegradeDataFilePath, updatedData);
            }
            return;
        }
        string data = serializer.Serialize(TempDegradeData);
        File.WriteAllText(DegradeDataFilePath, data);
    }

    public static void ReadDegradeDataFile()
    {
        if (!File.Exists(DegradeDataFilePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string FileData = File.ReadAllText(DegradeDataFilePath);
        List<DegradeData> DegradeDataList = deserializer.Deserialize<List<DegradeData>>(File.ReadAllText(DegradeDataFilePath));
        foreach (DegradeData data in DegradeDataList)
        {
            if (data.spoil_item_name.IsNullOrWhiteSpace()) continue;
            if (!ValidateCustomSpoilItem(data.spoil_item_name, out ItemDrop component)) continue;
            DegradeDataMap[data.shared_name] = new ValidatedDegradeData()
            {
                m_prefabName = data.prefab_name,
                m_sharedName = data.shared_name,
                m_spoilItemDrop = component,
                m_duration = data.food_duration
            };
        }

        if (ZNet.instance.IsServer())
        {
            ServerDegradeData.Value = FileData;
        }
        else
        {
            ServerDegradeData.ValueChanged += OnServerDegradeDataChange;
        }
    }

    private static bool ValidateCustomSpoilItem(string prefabName, out ItemDrop itemDrop)
    {
        itemDrop = LoadAssets.PukeBerriesItemDrop;
        if (!ZNetScene.instance) return false;
        if (prefabName.IsNullOrWhiteSpace()) return false;
        GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
        if (prefab == null) return false;
        if (!prefab.TryGetComponent(out ItemDrop component)) return false;
        itemDrop = component;
        return true;
    }

    private static void OnServerDegradeDataChange()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        List<DegradeData> DegradeDataList = deserializer.Deserialize<List<DegradeData>>(ServerDegradeData.Value);
        foreach (DegradeData data in DegradeDataList)
        {
            if (data.spoil_item_name.IsNullOrWhiteSpace()) continue;
            if (!ValidateCustomSpoilItem(data.spoil_item_name, out ItemDrop component)) continue;
            DegradeDataMap[data.shared_name] = new ValidatedDegradeData()
            {
                m_prefabName = data.prefab_name,
                m_sharedName = data.shared_name,
                m_spoilItemDrop = component,
                m_duration = data.food_duration
            };
        }
    }
    
}