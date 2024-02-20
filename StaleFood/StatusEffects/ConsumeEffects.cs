using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using UnityEngine;

namespace StaleFood.StatusEffects;

public static class ConsumeEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeItem))]
    private static class ConsumeItemPatch
    {
        private static void Postfix(Player __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!__instance || !__result) return;
            int maxDuration = Utility.Utility.GetMaxDuration(item);
            item.m_customData["StaleFood"] = maxDuration.ToString(CultureInfo.InvariantCulture);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
    private static class EatFoodPatch
    {
        private static void Postfix(Player __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!__result || StaleFoodPlugin._UseConsumeEffects.Value is StaleFoodPlugin.Toggle.Off) return;

            if (item.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable) return;
            if (Localization.instance.Localize(item.m_shared.m_name).ToLower().EndsWith("mead")) return;
            
            int maxDuration = Utility.Utility.GetMaxDuration(item);
            int currentDuration = maxDuration;
            if (item.m_customData.TryGetValue("StaleFood", out string StaleData))
            {
                currentDuration = int.Parse(StaleData);
            }
            float threshold = maxDuration * (StaleFoodPlugin._EffectThreshold.Value / 100f);
            float currentPercentage = (float)currentDuration / maxDuration;

            if (currentPercentage < threshold)
            {
                string LocalizedName = Localization.instance.Localize(item.m_shared.m_name);
                StatusEffectManager.FoodEffectData data = new StatusEffectManager.FoodEffectData()
                {
                    effectName = "SE_StaleFood",
                    displayName = "Spoiled " + LocalizedName,
                    duration = (int)item.m_shared.m_foodBurnTime,
                    sprite = item.GetIcon(),
                    startMsg = "You ate some spoiled " + LocalizedName,
                    effectTooltip = "Lower health and stamina regeneration",
                    Modifiers = new Dictionary<StatusEffectManager.Modifier, float>()
                    {
                        { StatusEffectManager.Modifier.Attack, 1f },
                        { StatusEffectManager.Modifier.HealthRegen , Mathf.Clamp(currentPercentage / StaleFoodPlugin._SpoiledMultiplier.Value, 0.5f, 0.99f)},
                        { StatusEffectManager.Modifier.StaminaRegen , Mathf.Clamp(currentPercentage / StaleFoodPlugin._SpoiledMultiplier.Value, 0.5f, 0.99f) },
                        { StatusEffectManager.Modifier.RaiseSkills , 1f },
                        { StatusEffectManager.Modifier.Speed , 1f },
                        { StatusEffectManager.Modifier.Noise , 1f },
                        { StatusEffectManager.Modifier.MaxCarryWeight , 0f },
                        { StatusEffectManager.Modifier.Stealth , 1f },
                        { StatusEffectManager.Modifier.RunStaminaDrain , 1f },
                        { StatusEffectManager.Modifier.DamageReduction , 0f },
                        { StatusEffectManager.Modifier.FallDamage , 1f },
                        { StatusEffectManager.Modifier.EitrRegen , 1f }
                    }
                };
                StatusEffect? effect = data.Init();
                if (!effect) return;
                if (__instance.GetSEMan().HaveStatusEffect("SE_StaleFood".GetStableHashCode()))
                {
                    __instance.GetSEMan().RemoveStatusEffect("SE_StaleFood".GetStableHashCode());
                }
                __instance.GetSEMan().AddStatusEffect(effect);
            }
        }
    }
}