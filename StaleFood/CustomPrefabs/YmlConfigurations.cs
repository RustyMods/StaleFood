using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using ServerSync;
using StaleFood.Utility;
using UnityEngine;

namespace StaleFood.CustomPrefabs;

public static class YmlConfigurations
{
    public static readonly string FolderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "StaleFood";
    private static readonly string CustomPrefabFilePath = FolderPath + Path.DirectorySeparatorChar + "CustomPrefabs.yml";

    private static readonly CustomSyncedValue<List<string>> ServerCustomPrefabs =
        new(StaleFoodPlugin.ConfigSync, "ServerCustomPrefabs", new());

    public static void InitCustomPrefabs()
    {
        SpecialCases.NamesOrSharedNames.Clear();
        SpecialCases.NamesOrSharedNames.AddRange(SpecialCases.DefaultNames);
        SpecialCases.NamesOrSharedNames.Add(LoadAssets.CoolingItemDrop.m_itemData.m_shared.m_name);
        SpecialCases.NamesOrSharedNames.Add(LoadAssets.CoolingItem.name);
        
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
        if (!File.Exists(CustomPrefabFilePath))
        {
            List<string> DefaultList = new()
            {
                "#Add prefab names you want to be affected by degradation below",
                "#Any entries that start with # will be ignored"
            };
            File.WriteAllLines(CustomPrefabFilePath, DefaultList);
        }
        
        List<string> data = File.ReadAllLines(CustomPrefabFilePath).ToList();
        foreach (string input in data)
        {
            if (input.StartsWith("#")) continue;
            if (!ValidateCustomPrefabs(input, out GameObject prefab)) continue;
            if (!prefab.TryGetComponent(out ItemDrop component)) continue;
            SpecialCases.NamesOrSharedNames.Add(prefab.name.ToLower());
            SpecialCases.NamesOrSharedNames.Add(component.m_itemData.m_shared.m_name);
        }

        if (ZNet.instance.IsServer())
        {
            ServerCustomPrefabs.Value = data;
        }
        else
        {
            ServerCustomPrefabs.ValueChanged += OnServerCustomPrefabChange;
        }
    }

    private static bool ValidateCustomPrefabs(string input, out GameObject prefab)
    {
        prefab = ObjectDB.instance.GetItemPrefab(input);
        return prefab;
    }

    private static void OnServerCustomPrefabChange()
    {
        if (ServerCustomPrefabs.Value.Count == 0) return;
        foreach (string input in ServerCustomPrefabs.Value)
        {
            if (input.StartsWith("#")) continue;
            if (!ValidateCustomPrefabs(input, out GameObject prefab)) continue;
            if (!prefab.TryGetComponent(out ItemDrop component)) continue;
            if (!SpecialCases.NamesOrSharedNames.Contains(prefab.name.ToLower())) SpecialCases.NamesOrSharedNames.Add(prefab.name.ToLower());
            if (!SpecialCases.NamesOrSharedNames.Contains(component.m_itemData.m_shared.m_name)) SpecialCases.NamesOrSharedNames.Add(component.m_itemData.m_shared.m_name);
        }
    }
    
}