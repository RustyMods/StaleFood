using System.Globalization;
using StaleFood.Configurations;
using StaleFood.Utility;
using UnityEngine;

namespace StaleFood.MonoBehaviors;
public class Degradation : MonoBehaviour
{
    private ZNetView _znv = null!;
    private ItemDrop item = null!;
    private void Awake()
    {
        _znv = GetComponent<ZNetView>();
        if (!_znv.IsValid()) return;
        
        item = GetComponent<ItemDrop>();
    }

    private void Start()
    {
        if (!Utility.Utility.ShouldUpdateFood(item.m_itemData)) return;
        CancelInvoke(nameof(UpdateFood));
        if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return;
        InvokeRepeating(nameof(UpdateFood), 60, 60);
    }

    private void UpdateFood()
    {
        int maxDuration = Utility.Utility.GetMaxDuration(item.m_itemData);
        float currentDuration = maxDuration;
        if (item.m_itemData.m_customData.TryGetValue("StaleFood", out string StaleData))
        {
            if (int.Parse(StaleData) < currentDuration) currentDuration = int.Parse(StaleData);
        }
        
        float remainder = currentDuration - 1;

        if (remainder <= 0)
        {
            if (item.m_itemData.m_stack > 1)
            {
                _znv.GetZDO().Set(ZDOVars.s_stack, item.m_itemData.m_stack - 1);
                if (StaleFoodPlugin._UseDegradeItemDataYml.Value is StaleFoodPlugin.Toggle.On)
                {
                    Instantiate(
                        DataManager.DegradeDataMap.TryGetValue(item.m_itemData.m_shared.m_name, out ValidatedDegradeData CustomData)
                            ? ObjectDB.instance.GetItemPrefab(CustomData.m_spoilItemDrop.name)
                            : LoadAssets.PukeBerries, transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(LoadAssets.PukeBerries, transform.position, Quaternion.identity);
                }
            }
            else
            {
                if (StaleFoodPlugin._UseDegradeItemDataYml.Value is StaleFoodPlugin.Toggle.On)
                {
                    Instantiate(DataManager.DegradeDataMap.TryGetValue(item.m_itemData.m_shared.m_name, out ValidatedDegradeData CustomData)
                        ? ObjectDB.instance.GetItemPrefab(CustomData.m_spoilItemDrop.name)
                        : LoadAssets.PukeBerries, transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(item.m_itemData.m_shared.m_weight < 1 ? LoadAssets.PukeBerries : LoadAssets.RottenMeat, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }

            item.m_itemData.m_customData["StaleFood"] = maxDuration.ToString();
            item.Save();
        }
        else
        {
            item.m_itemData.m_customData["StaleFood"] = remainder.ToString(CultureInfo.InvariantCulture);
            item.Save();
        }

        if (StaleFoodPlugin.SeasonalityLoaded)
        {
            CancelInvoke(nameof(UpdateFood));
            if (SeasonKeys.season is SeasonKeys.Seasons.Winter)
            {
                item.m_itemData.m_customData["StaleFoodInventory"] = "Winter";
                InvokeRepeating(nameof(UpdateFood), 120, 120);
            }
            else
            {
                item.m_itemData.m_customData["StaleFoodInventory"] = "false";
                InvokeRepeating(nameof(UpdateFood), 60, 60);
            }
        }
    }
}
