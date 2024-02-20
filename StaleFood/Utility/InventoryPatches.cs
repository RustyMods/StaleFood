using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using UnityEngine;

namespace StaleFood.Utility;

public static class InventoryPatches
{
    public static readonly List<string> FridgeNames = new();
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem),typeof(ItemDrop.ItemData),typeof(int),typeof(int),typeof(int))]
    private static class InventoryAddItemPatch
    {
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData item, int x, int y, ref bool __result)
        {
            if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return true;
            item.m_customData["StaleFoodInventory"] = "false";
            // Make sure that the smallest durability between the two items is the smallest
            // To avoid "healing" items by stacking them
            ItemDrop.ItemData itemAt = __instance.GetItemAt(x, y);
            if (itemAt != null)
            {
                if (itemAt.m_shared.m_name == item.m_shared.m_name)
                {
                    int itemAtMaxDuration = Utility.GetMaxDuration(itemAt);
                    int itemMaxDuration = Utility.GetMaxDuration(item);

                    int itemAtCurrentDuration = itemAtMaxDuration;
                    if (itemAt.m_customData.TryGetValue("StaleFood", out string itemAtStaleData))
                    {
                        itemAtCurrentDuration = int.Parse(itemAtStaleData);
                    }

                    int itemCurrentDuration = itemMaxDuration;
                    if (item.m_customData.TryGetValue("StaleFood", out string itemStaleData))
                    {
                        itemCurrentDuration = int.Parse(itemStaleData);
                    }

                    float duration = Mathf.Min(itemAtCurrentDuration, itemCurrentDuration);
                    item.m_customData["StaleFood"] = duration.ToString(CultureInfo.InvariantCulture);
                    itemAt.m_customData["StaleFood"] = duration.ToString(CultureInfo.InvariantCulture);
                }
            }
            if (!FridgeNames.Contains(__instance.GetName())) return true;
            // Control which items are allowed in fridges
            bool flag = CanAddItem(item);
            __result = flag;
            return flag;
        }
    }
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem),typeof(ItemDrop.ItemData))]
    private static class InventoryAddItemFastPatch
    {
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return true;
            item.m_customData["StaleFoodInventory"] = "false";
            // Control which items are allowed in fridges
            if (!FridgeNames.Contains(__instance.m_name)) return true;
            bool flag = CanAddItem(item);
            __result = flag;
            return flag;
        }
    }

    private static bool CanAddItem(ItemDrop.ItemData item) => item.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable || SpecialCases.ShouldBeAffected(item.m_shared.m_name);
}