namespace StaleFood.Configurations;

public class DegradeData
{
    public string prefab_name = null!;
    public string shared_name = null!;
    public string spoil_item_name = "Pukeberries";
    public int food_duration = 100;
}

public class ValidatedDegradeData
{
    public string m_prefabName = null!;
    public string m_sharedName = null!;
    public ItemDrop m_spoilItemDrop = null!;
    public int m_duration;
}