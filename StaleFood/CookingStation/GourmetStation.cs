using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using StaleFood.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StaleFood.CookingStation;

public static class GourmetStation
{
    public static readonly List<string> CustomItems = new();
    private static readonly List<FoodItemData> FailedRecipes = new();
    public static List<FoodItemData> TempFoodItems = new();

    private static CraftingStation CauldronStation = null!;
    
    [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.CheckUsable))]
    private static class CheckUsablePatch
    {
        private static void Prefix(CraftingStation __instance)
        {
            if (__instance.name.Replace("(Clone)","") != "CookingStation_RS") return;
            __instance.m_craftRequireRoof = StaleFoodPlugin._CookingStationRequireRoof.Value is StaleFoodPlugin.Toggle.On;
        }
    }
    
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDBAwakePatch
    {
        private static void Postfix(ObjectDB __instance)
        {
            if (!__instance) return;
            if (!ZNetScene.instance) return;
            CauldronStation = ZNetScene.instance.GetPrefab("piece_cauldron").GetComponent<CraftingStation>();
            
            RegisterItemsToObjectDB(__instance);
            RegisterItemsToZNetScene(ZNetScene.instance, __instance);
            UpdateRegisteredRecipes(__instance);
            UpdateFailedRecipes(__instance);
            __instance.UpdateItemHashes();
        }
    }

    public static void PrepareCustomItems()
    {
        TempFoodItems.Clear();
        TempFoodItems = FoodManager.GetCustomFoodData();
    }

    private static void RegisterItemsToZNetScene(ZNetScene instance, ObjectDB DB)
    {
        foreach (FoodItemData item in TempFoodItems)
        {
            int hash = item.m_prefabName.GetStableHashCode();
            if (instance.m_namedPrefabs.ContainsKey(hash))
            {
                StaleFoodPlugin.StaleFoodLogger.LogDebug("ZNetScene already contains item");
            }
            else
            {
                GameObject newItem = DB.GetItemPrefab(item.m_prefabName);
                if (!newItem)
                {
                    StaleFoodPlugin.StaleFoodLogger.LogDebug("Failed to find new item in object DB");
                    continue;
                }
                if (newItem.GetComponent<ZNetView>() != null)
                {
                    instance.m_prefabs.Add(newItem);
                }
                else
                {
                    instance.m_nonNetViewPrefabs.Add(newItem);
                }
                instance.m_namedPrefabs.Add(hash, newItem);
            }
        }
    }

    private static void RegisterItemsToObjectDB(ObjectDB instance)
    {
        if (!StaleFoodPlugin.Root) return;
        CustomItems.Clear();
        
        foreach (FoodItemData item in TempFoodItems)
        {
            GameObject clone = instance.GetItemPrefab(item.m_cloneName);
            if (!clone)
            {
                StaleFoodPlugin.StaleFoodLogger.LogDebug("Failed to find " + item.m_cloneName + " in ObjectDB");
                continue;
            }

            GameObject newItem = Object.Instantiate(clone, StaleFoodPlugin.Root.transform, false);
            if (newItem == null)
            {
                StaleFoodPlugin.StaleFoodLogger.LogDebug("new item is null: " + item.m_prefabName);
                continue;
            }
            newItem.SetActive(true);
            if (!newItem.TryGetComponent(out ItemDrop component))
            {
                StaleFoodPlugin.StaleFoodLogger.LogDebug(item.m_cloneName + " does not have ItemDrop component");
                continue;
            }
            int hash = item.m_prefabName.GetStableHashCode();

            if (component.m_itemData.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Consumable)
            {
                component.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
                StaleFoodPlugin.StaleFoodLogger.LogDebug("Changing " + item.m_prefabName + " to consumable");
            }
            
            newItem.name = item.m_prefabName;
            component.name = item.m_prefabName;
            component.m_nameHash = hash;
            component.m_itemData.m_shared.m_name = item.m_sharedName;
            component.m_itemData.m_shared.m_description = item.m_description;
            component.m_itemData.m_dropPrefab = newItem;
            component.m_itemData.m_shared.m_food = item.m_health;
            component.m_itemData.m_shared.m_foodStamina = item.m_stamina;
            component.m_itemData.m_shared.m_foodEitr = item.m_eitr;
            component.m_itemData.m_shared.m_foodBurnTime = item.m_burnTime;
            component.m_itemData.m_customData["StaleFood"] = item.m_duration.ToString(CultureInfo.InvariantCulture);
            component.Save();

            instance.m_items.Add(newItem);
            instance.m_itemByHash.Add(hash, newItem);
            
            List<Piece.Requirement> ingredients = new();
            foreach (Ingredient ingredient in item.m_recipe)
            {
                GameObject prefab = instance.GetItemPrefab(ingredient.m_prefabName);
                if (!prefab)
                {
                    StaleFoodPlugin.StaleFoodLogger.LogDebug("Failed to find " + ingredient.m_prefabName + " in ObjectDB");
                    FailedRecipes.Add(item);
                    continue;
                }
            
                if (!prefab.TryGetComponent(out ItemDrop ingredientComponent))
                {
                    StaleFoodPlugin.StaleFoodLogger.LogDebug(ingredient.m_prefabName + " does not have ItemDrop component");
                    continue;
                }
                Piece.Requirement requirement = new Piece.Requirement()
                {
                    m_resItem = ingredientComponent,
                    m_amount = ingredient.m_amount,
                    m_recover = ingredient.m_recover,
                    m_extraAmountOnlyOneIngredient = ingredient.m_extraAmountOnlyOneIngredient,
                    m_amountPerLevel = ingredient.m_amountPerLevel
                };
                ingredients.Add(requirement);
            }
            
            Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.name = "RS_" + item.m_prefabName;
            recipe.m_amount = item.m_amount;
            recipe.m_enabled = true;
            recipe.m_item = component;
            recipe.m_resources = ingredients.ToArray();
            recipe.m_craftingStation = StaleFoodPlugin._UseGourmetStation.Value is StaleFoodPlugin.Toggle.On ? LoadPieces.CookingCraftingStation : CauldronStation;
            recipe.m_minStationLevel = item.m_minStationLevel;
            recipe.m_requireOnlyOneIngredient = false;
            recipe.m_qualityResultAmountMultiplier = 1f;
            
            instance.m_recipes.Add(recipe);
            CustomItems.Add(component.m_itemData.m_shared.m_name);
        }
    }

    private static void UpdateRegisteredRecipes(ObjectDB instance)
    {
        foreach (Recipe recipe in instance.m_recipes)
        {
            if (!recipe.name.StartsWith("RS_")) continue;
            GameObject prefab = instance.GetItemPrefab(recipe.name.Replace("RS_",""));
            recipe.m_item.m_itemData.m_dropPrefab = prefab;
            recipe.m_item.Save();
        }
    }

    private static void UpdateFailedRecipes(ObjectDB instance)
    {
        foreach (FoodItemData item in FailedRecipes)
        {
            List<Piece.Requirement> ingredients = new();
            foreach (Ingredient ingredient in item.m_recipe)
            {
                GameObject prefab = instance.GetItemPrefab(ingredient.m_prefabName);
                if (!prefab)
                {
                    StaleFoodPlugin.StaleFoodLogger.LogDebug("Failed to find " + ingredient.m_prefabName + " in ObjectDB again");
                    continue;
                }
            
                if (!prefab.TryGetComponent(out ItemDrop ingredientComponent))
                {
                    StaleFoodPlugin.StaleFoodLogger.LogDebug(ingredient.m_prefabName + " does not have ItemDrop component again");
                    continue;
                }
                Piece.Requirement requirement = new Piece.Requirement()
                {
                    m_resItem = ingredientComponent,
                    m_amount = ingredient.m_amount,
                    m_recover = ingredient.m_recover,
                    m_extraAmountOnlyOneIngredient = ingredient.m_extraAmountOnlyOneIngredient,
                    m_amountPerLevel = ingredient.m_amountPerLevel
                };
                ingredients.Add(requirement);
            }

            Recipe recipe = instance.m_recipes.Find(x => x.name == ("RS_" + item.m_prefabName));
            if (!recipe)
            {
                StaleFoodPlugin.StaleFoodLogger.LogDebug("Failed to update failed recipe");
                continue;
            }

            recipe.m_resources = ingredients.ToArray();
        }
    }

    public static void OnSettingChanged(object sender, EventArgs e)
    {
        foreach (FoodItemData item in TempFoodItems)
        {
            Recipe recipe = ObjectDB.instance.m_recipes.Find(x => x.name == "RS_" + item.m_prefabName);
            if (!recipe) continue;
            recipe.m_craftingStation = StaleFoodPlugin._UseGourmetStation.Value is StaleFoodPlugin.Toggle.On ? LoadPieces.CookingCraftingStation : CauldronStation;
        }
    }
}