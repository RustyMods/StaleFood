using System;
using HarmonyLib;
using StaleFood.Configurations;
using StaleFood.CookingStation;
using StaleFood.Managers;
using UnityEngine;

namespace StaleFood.Utility;

public static class UI
{
    [HarmonyPatch(typeof(HotkeyBar), nameof(HotkeyBar.UpdateIcons))]
    private static class HotkeyBarUpdateIconsPatch
    {
        private static void Postfix(HotkeyBar __instance, Player player)
        {
            if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return;
            if (!__instance || !player || player.IsDead()) return;

            foreach (ItemDrop.ItemData itemData in __instance.m_items)
            {
                if (!Utility.ShouldUpdateFood(itemData)) continue;
                HotkeyBar.ElementData element = __instance.m_elements[itemData.m_gridPos.x];

                int maxDuration = Utility.GetMaxDuration(itemData);
                int currentDuration = maxDuration;
                if (itemData.m_customData.TryGetValue("StaleFood", out string StaleData))
                {
                    currentDuration = int.Parse(StaleData);
                }
                float percentage = (float)currentDuration / maxDuration;

                bool flag = itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable;
                if (itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable)
                {
                    if (SpecialCases.ShouldBeAffected(itemData.m_shared.m_name)) flag = true;
                }

                if (!flag) continue;
                
                element.m_icon.color = new Color(percentage, 1f, percentage, 1f);
            }
        }
    }
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    private static class FoodIconColorPatch
    {
        private static void Postfix(InventoryGrid __instance)
        {
            if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return;
            if (!__instance) return;
            Vector3 foodIconPos = __instance.m_elementPrefab.transform.Find("foodicon").localPosition;
            
            foreach (ItemDrop.ItemData itemData in __instance.m_inventory.GetAllItems())
            {
                if (!Utility.ShouldUpdateFood(itemData)) continue;
                InventoryGrid.Element element = __instance.GetElement(itemData.m_gridPos.x, itemData.m_gridPos.y, __instance.m_inventory.GetWidth());
                int maxDuration = Utility.GetMaxDuration(itemData);
                int currentDuration = maxDuration;
                if (itemData.m_customData.TryGetValue("StaleFood", out string StaleData))
                {
                    currentDuration = int.Parse(StaleData);
                }
                float percentage = (float)currentDuration / maxDuration;
                
                bool flag = itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable;
                if (itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable)
                {
                    if (SpecialCases.ShouldBeAffected(itemData.m_shared.m_name)) flag = true;
                }

                if (!flag) continue;
                
                element.m_icon.color = new Color(percentage, 1f, percentage, 1f);

                if (GourmetStation.CustomItems.Contains(itemData.m_shared.m_name))
                {
                    FoodItemData data = GourmetStation.TempFoodItems.Find(x => x.m_sharedName == itemData.m_shared.m_name);
                    if (data == null) continue;
                    switch (data.m_iconType)
                    {
                        case "Honey":
                            element.m_food.sprite = SpriteManager.HoneyIconSprite;
                            element.m_go.transform.Find("foodicon").localPosition = foodIconPos + new Vector3(-6f, 12f, 0f);
                            break;
                        case "Dried":
                            element.m_food.sprite = SpriteManager.DriedIconSprite;
                            element.m_go.transform.Find("foodicon").localPosition = foodIconPos + new Vector3(-6f, 8f, 0f);
                            break;
                        case "Cured":
                            element.m_food.sprite = SpriteManager.SaltedIconSprite;
                            element.m_go.transform.Find("foodicon").localPosition = foodIconPos + new Vector3(-6f, 8f, 0f);
                            break;
                    }
                    element.m_food.color = Color.white;
                }
            }
        }
    }
    

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip),typeof(ItemDrop.ItemData),typeof(int),typeof(bool),typeof(float))]
    private static class ItemDataGetTooltipPatch
    {
        private static void Postfix(ItemDrop.ItemData item, ref string __result)
        {
            if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return;
            if (!Utility.ShouldUpdateFood(item))
            {
                if (item.m_shared.m_name != LoadAssets.CoolingItemDrop.m_itemData.m_shared.m_name) return;
                ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n\nCan be used as a cooling item");
                __result = ItemDrop.ItemData.m_stringBuilder.ToString();
                return;
            }
            
            bool flag = item.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable;
            if (item.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable)
            {
                flag = SpecialCases.ShouldBeAffected(item.m_shared.m_name);
            }

            if (!flag) return;
            
            int maxDuration = Utility.GetMaxDuration(item);
            int currentDuration = maxDuration;
            if (item.m_customData.TryGetValue("StaleFood", out string StaleData))
            {
                currentDuration = int.Parse(StaleData);
            }

            bool hasCoolingEffect = false;
            string coolingEffect = "";
            if (item.m_customData.TryGetValue("StaleFoodInventory", out string inventoryName))
            {
                switch (inventoryName)
                {
                    case "$piece_freezer":
                        currentDuration *= StaleFoodPlugin._FreezerMultiplier.Value;
                        hasCoolingEffect = true;
                        coolingEffect = "Inside active freezer";
                        break;
                    case "$piece_refrigerator":
                        currentDuration *= StaleFoodPlugin._RefrigeratorMultiplier.Value;
                        hasCoolingEffect = true;
                        coolingEffect = "Inside active refrigerator";
                        break;
                    case "Winter":
                        currentDuration *= 2;
                        hasCoolingEffect = true;
                        coolingEffect = "Winter slows decay";
                        break;
                }
            }

            float TotalMinutes = currentDuration;
            double Days = TimeSpan.FromMinutes(TotalMinutes).Days;
            double Hours = TimeSpan.FromMinutes(TotalMinutes).Hours;
            double Minutes = TimeSpan.FromMinutes(TotalMinutes).Minutes;

            string time = Days > 0 
                ? $"Fresh for <color=orange>{TimeSpan.FromMinutes(TotalMinutes).TotalDays:0.0}</color> Days" 
                : Hours > 0 
                    ? $"Fresh for <color=orange>{TimeSpan.FromMinutes(TotalMinutes).TotalHours:0.0}</color> Hours" 
                    : Minutes > 1 
                        ? $"Fresh for <color=orange>{TimeSpan.FromMinutes(TotalMinutes).TotalMinutes:0}</color> Minutes" 
                        : $"Fresh for <color=orange>{TimeSpan.FromMinutes(TotalMinutes).TotalSeconds:0}</color> Seconds";
            
            float currentPercentage = (float)currentDuration / maxDuration;

            ItemDrop.ItemData.m_stringBuilder.AppendFormat(
                "\n<color=red>Decay</color>: {0} <color=yellow>({1}%)</color>", time, (int)(currentPercentage * 100));

            if (hasCoolingEffect)
            {
                ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n" + coolingEffect);
            }

            if (item.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable)
            {
                if (StaleFoodPlugin._UseConsumeEffects.Value is StaleFoodPlugin.Toggle.On)
                {
                    ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n\nIf decay is below <color=orange>{0}</color>%, you will suffer", StaleFoodPlugin._EffectThreshold.Value);
                    if (currentPercentage * 100f < StaleFoodPlugin._EffectThreshold.Value)
                    {
                        float spoiledModifier = Mathf.Clamp(currentPercentage / StaleFoodPlugin._SpoiledMultiplier.Value, 0.5f, 0.99f);
                        float modifierPercentage = Mathf.Round(1 - spoiledModifier) * 100;
                        ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n\n<color=orange>Spoiled {0}</color>", Localization.instance.Localize(item.m_shared.m_name));
                        ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n$item_food_duration: <color=orange>{0}</color>", ItemDrop.ItemData.GetDurationString(item.m_shared.m_foodBurnTime));
                        ItemDrop.ItemData.m_stringBuilder.AppendFormat("\nHealth regen: <color=orange>-{0}%</color>", modifierPercentage);
                        ItemDrop.ItemData.m_stringBuilder.AppendFormat("\nStamina regen: <color=orange>-{0}%</color>", modifierPercentage);
                    }
                }
            }
            
            if (StaleFoodPlugin._UseDegradeItemDataYml.Value is StaleFoodPlugin.Toggle.On)
            {
                if (DataManager.DegradeDataMap.TryGetValue(item.m_shared.m_name, out ValidatedDegradeData data))
                {
                    ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n\nFood will spoil into <color=yellow>{0}</color>", Localization.instance.Localize(data.m_spoilItemDrop.m_itemData.m_shared.m_name));
                }
            }
            else
            {
                ItemDrop.ItemData.m_stringBuilder.AppendFormat("\n\nFood will spoil into <color=yellow>{0}</color>",
                    Localization.instance.Localize(item.m_shared.m_weight > 1 
                        ? LoadAssets.RottenMeatItemDrop.m_itemData.m_shared.m_name 
                        : LoadAssets.PukeBerriesItemDrop.m_itemData.m_shared.m_name));
            }

            __result = ItemDrop.ItemData.m_stringBuilder.ToString();
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.GetHoverText))]
    private static class ItemDropHoverPatch
    {
        private static void Postfix(ItemDrop __instance, ref string __result)
        {
            if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return;
            if (!__instance) return;
            if (!Utility.ShouldUpdateFood(__instance.m_itemData)) return;
            int maxDuration = Utility.GetMaxDuration(__instance.m_itemData);
            int currentDuration = maxDuration;
            if (__instance.m_itemData.m_customData.TryGetValue("StaleFood", out string StaleData))
            {
                currentDuration = int.Parse(StaleData);
            }

            if (StaleFoodPlugin.SeasonalityLoaded)
            {
                if (SeasonKeys.season is SeasonKeys.Seasons.Winter)
                {
                    currentDuration *= 2;
                }
            }

            float percentage = (float)currentDuration / maxDuration;
            
            bool flag = __instance.m_itemData.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable;
            if (__instance.m_itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable)
            {
                if (SpecialCases.ShouldBeAffected(__instance.m_itemData.m_shared.m_name)) flag = true;
            }

            if (!flag) return;
            
            __result += $"\n<color=red>Decay</color>: <color=orange>{(int)(percentage * 100)}%</color> <color=yellow>({currentDuration}/{maxDuration})</color>";
        }
    }
}