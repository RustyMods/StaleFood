using HarmonyLib;
using StaleFood.Configurations;
using StaleFood.Managers;
using StaleFood.MonoBehaviors;
using UnityEngine;

namespace StaleFood.Utility;

public static class LoadAssets
{
    public static GameObject PukeBerries = null!;
    public static ItemDrop PukeBerriesItemDrop = null!;
    public static GameObject RottenMeat = null!;
    public static ItemDrop RottenMeatItemDrop = null!;
    public static GameObject CoolingItem = null!;
    public static ItemDrop CoolingItemDrop = null!;
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetSceneAwakePatch
    {
        private static void Postfix(ZNetScene __instance)
        {
            if (!__instance) return;
            PieceEffectManager.AddPieceEffects(__instance);
            MineRockManager.AddDestructibleEffects(__instance);
            DataManager.ClearTempDegradeDataList();
            CacheAssets(__instance);
            CustomPrefabs.YmlConfigurations.InitCustomPrefabs();
            InitDegrade(__instance);
            DataManager.InitDegradeDataMap();
            FileWatcherSystem.InitFileSystem();
            MineRockManager.AddMineRocksToZNetScene(__instance);
        }
    }
    
    private static void CacheAssets(ZNetScene instance)
    {
        PukeBerries = instance.GetPrefab("Pukeberries");
        PukeBerriesItemDrop = PukeBerries.GetComponent<ItemDrop>();

        RottenMeat = instance.GetPrefab("RottenMeat");
        RottenMeatItemDrop = RottenMeat.GetComponent<ItemDrop>();

        CoolingItem = ValidateCoolingItem(instance, out ItemDrop CoolingComponent);
        CoolingItemDrop = CoolingComponent;
    }

    private static void InitDegrade(ZNetScene instance)
    {
        foreach (GameObject prefab in instance.m_prefabs)
        {
            if (prefab.name is "RottenMeat" or "Pukeberries" or "BarleyWine" or "BarleyWineBase") continue;
            if (prefab.name.StartsWith("Mead")) continue;
            
            if (prefab.TryGetComponent(out ItemDrop component))
            {
                bool flag = component.m_itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable;
                if (component.m_itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable)
                {
                    if (SpecialCases.ShouldBeAffected(component.m_itemData.m_shared.m_name)) flag = true;
                }

                if (flag)
                {
                    prefab.AddComponent<Degradation>();
                    DataManager.AddDegradeData(component);
                }

                continue;
            }
            if (prefab.GetComponent<Container>()) prefab.AddComponent<ContainerDegrade>();
            if (prefab.GetComponent<Player>()) prefab.AddComponent<InventoryDegrade>();
        }
    }

    private static GameObject ValidateCoolingItem(ZNetScene instance, out ItemDrop data)
    {
        GameObject prefab = instance.GetPrefab(StaleFoodPlugin._CoolingItem.Value);
        if (prefab)
        {
            if (prefab.TryGetComponent(out ItemDrop component))
            {
                data = component;
                return prefab;
            }
        }

        GameObject FreezeGland = instance.GetPrefab("FreezeGland");
        data = FreezeGland.GetComponent<ItemDrop>();
        return FreezeGland;
    }
}
