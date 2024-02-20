using System.Collections.Generic;

namespace StaleFood.CookingStation;

public class FoodItemData
{
    public string m_cloneName = null!;
    public string m_prefabName = null!;
    public string m_sharedName = null!;
    public string m_description = null!;
    public float m_health;
    public float m_stamina;
    public float m_eitr;
    public float m_regen = 1;
    public float m_burnTime;
    public List<Ingredient> m_recipe = new();
    public int m_minStationLevel = 1;
    public int m_amount = 1;
    public int m_duration = 100;
    public string m_spoilItemName = "Pukeberries";
    public string m_iconType = "";
}

public class Ingredient
{
    public string m_prefabName = null!;
    public int m_amount = 1;
    public bool m_recover = true;
    public int m_extraAmountOnlyOneIngredient = 1;
    public int m_amountPerLevel = 1;
}