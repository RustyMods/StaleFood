using Managers;
using UnityEngine;

namespace StaleFood.Managers;

public static class LoadItems
{
    public static GameObject SaltPrefab = null!;

    public static void InitItems()
    {
        Item Salt = new("stalefoodbundle", "Salt_RS");
        Salt.Name.English("Salt");
        Salt.Description.English("Can make food more savory and delicious");
        Salt.Configurable = Configurability.Disabled;
        Salt.DropsFrom.Add("Neck", 0.1f, 1, 2);
        MaterialReplacer.RegisterGameObjectForMatSwap(Salt.Prefab.transform.Find("$part_replace").gameObject);

        SaltPrefab = Salt.Prefab;   

        Item BoarStew = new("stalefoodbundle", "BoarStew_RS");
        BoarStew.Name.English("Boar Stew");
        BoarStew.Description.English("Finally an appetizing stew");
        BoarStew.Configurable = Configurability.Recipe;
        BoarStew.RequiredItems.Add("CookedMeat", 1);
        BoarStew.RequiredItems.Add("Salt_RS", 1);
        BoarStew.RequiredItems.Add("Dandelion", 5);
        BoarStew.RequiredItems.Add("Mushroom", 1);
        BoarStew.Crafting.Add(LoadPieces.CookingCraftingStation.name, 1);
        BoarStew.MaximumRequiredStationLevel = 1;
        BoarStew.CraftAmount = 1;
        if (BoarStew.Prefab.TryGetComponent(out ItemDrop component))
        {
            component.m_itemData.m_shared.m_food = 33;
            component.m_itemData.m_shared.m_foodStamina = 22;
            component.m_itemData.m_shared.m_foodRegen = 2;
        }
    }
}