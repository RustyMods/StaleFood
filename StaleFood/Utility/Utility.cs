using System.Collections.Generic;
using System.Globalization;
using StaleFood.Configurations;

namespace StaleFood.Utility;

public static class Utility
{
    public static int GetMaxDuration(ItemDrop.ItemData item)
    {
        if (StaleFoodPlugin._UseDegradeItemDataYml.Value is StaleFoodPlugin.Toggle.Off) return StaleFoodPlugin._FoodDuration.Value;
        return !DataManager.DegradeDataMap.TryGetValue(item.m_shared.m_name, out ValidatedDegradeData CustomData) ? StaleFoodPlugin._FoodDuration.Value : CustomData.m_duration;
    }
    public static bool ShouldUpdateFood(ItemDrop.ItemData item)
    {
        if (item.m_shared.m_name.EndsWith("barleywine")) return false;
        if (item.m_shared.m_name.EndsWith("barleywinebase")) return false;
        if (item.m_shared.m_name.Contains("mead")) return false;
        if (item.m_shared.m_name == LoadAssets.PukeBerriesItemDrop.m_itemData.m_shared.m_name) return false;
        if (item.m_shared.m_name == LoadAssets.RottenMeatItemDrop.m_itemData.m_shared.m_name) return false;
        if (item.m_shared.m_name == LoadAssets.CoolingItemDrop.m_itemData.m_shared.m_name) return false;

        return item.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Consumable || SpecialCases.ShouldBeAffected(item.m_shared.m_name);
    }

    private static ItemDrop.ItemData GetCustomSpoilItem(ItemDrop.ItemData item)
    {
        if (!DataManager.DegradeDataMap.TryGetValue(item.m_shared.m_name, out ValidatedDegradeData CustomData)) return LoadAssets.PukeBerriesItemDrop.m_itemData;
        ItemDrop.ItemData itemData = CustomData.m_spoilItemDrop.m_itemData;
        itemData.m_dropPrefab = ObjectDB.instance.GetItemPrefab(CustomData.m_spoilItemDrop.name);
        return itemData;

    }

    public static void UpdateItem(ItemDrop.ItemData item, float remainder, ref int PukeCount, ref int RottenCount, List<ItemDrop.ItemData> ItemsToRemove, List<ItemDrop.ItemData> ItemsToAdd)
    {
        float maxDuration = GetMaxDuration(item);
        if (remainder <= 0)
        {
            ItemsToRemove.Add(item);

            item.m_customData["StaleFood"] = (maxDuration - 1).ToString(CultureInfo.InvariantCulture);

            if (StaleFoodPlugin._UseDegradeItemDataYml.Value is StaleFoodPlugin.Toggle.On)
            {
                ItemDrop.ItemData clone = GetCustomSpoilItem(item).Clone();
                ItemsToAdd.Add(clone);
            }
            else
            {
                if (item.m_shared.m_weight < 1) ++PukeCount;
                else ++RottenCount;
            }
        }
        else
        {
            item.m_customData["StaleFood"] = remainder.ToString(CultureInfo.InvariantCulture);
        }
    }

    public static string ConvertDamageModifiers(HitData.DamageModifier mod)
    {
        return (mod) switch
        {
            HitData.DamageModifier.Normal => "Normal",
            HitData.DamageModifier.Ignore => "Ignore",
            HitData.DamageModifier.Immune => "Immune",
            HitData.DamageModifier.VeryResistant => "Very Resistant",
            HitData.DamageModifier.Resistant => "Resistant",
            HitData.DamageModifier.Weak => "Weak",
            HitData.DamageModifier.VeryWeak => "Very Weak",
            _ => "Unknown Damage Modifier"
        };
    }
    
    public static string ConvertDamageTypes(HitData.DamageType type)
    {
        return (type) switch
        {
            HitData.DamageType.Blunt => "Blunt",
            HitData.DamageType.Slash => "Slash",
            HitData.DamageType.Pierce => "Pierce",
            HitData.DamageType.Chop => "Chop",
            HitData.DamageType.Pickaxe => "Pickaxe",
            HitData.DamageType.Fire => "Fire",
            HitData.DamageType.Frost => "Frost",
            HitData.DamageType.Lightning => "Lightning",
            HitData.DamageType.Poison => "Poison",
            HitData.DamageType.Spirit => "Spirit",
            HitData.DamageType.Physical => "Physical",
            HitData.DamageType.Elemental => "Elemental",
            _ => "Unknown Damage Type"
        };
    }
    
    public static string ConvertEffectModifiers(StatusEffects.StatusEffectManager.Modifier type)
    {
        return (type) switch
        {
            StatusEffects.StatusEffectManager.Modifier.None => "None",
            StatusEffects.StatusEffectManager.Modifier.Attack => "Attack",
            StatusEffects.StatusEffectManager.Modifier.HealthRegen => "Health Regen",
            StatusEffects.StatusEffectManager.Modifier.StaminaRegen => "Stamina Regen",
            StatusEffects.StatusEffectManager.Modifier.RaiseSkills => "Raise Skills",
            StatusEffects.StatusEffectManager.Modifier.Speed => "Speed",
            StatusEffects.StatusEffectManager.Modifier.Noise => "Noise",
            StatusEffects.StatusEffectManager.Modifier.MaxCarryWeight => "Max Carry Weight",
            StatusEffects.StatusEffectManager.Modifier.Stealth => "Stealth",
            StatusEffects.StatusEffectManager.Modifier.RunStaminaDrain => "Run Stamina Drain",
            StatusEffects.StatusEffectManager.Modifier.DamageReduction => "Damage Reduction",
            StatusEffects.StatusEffectManager.Modifier.FallDamage => "Fall Damage",
            StatusEffects.StatusEffectManager.Modifier.EitrRegen => "Eitr Regen",
            _ => "Unknown Modifier"
        };
    }
}