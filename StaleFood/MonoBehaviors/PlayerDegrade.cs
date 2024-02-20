using System.Collections.Generic;
using Managers;
using StaleFood.Utility;
using UnityEngine;

namespace StaleFood.MonoBehaviors;
public class InventoryDegrade : MonoBehaviour
{
    private ZNetView _znv = null!;
    private Player player = null!;
    
    private void Awake()
    {
        _znv = GetComponent<ZNetView>();
        if (!_znv.IsValid()) return;

        player = GetComponent<Player>();
    }

    private void Start()
    {
        CancelInvoke(nameof(UpdatePlayerFood));
        if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return;
        InvokeRepeating(nameof(UpdatePlayerFood), 60, 60);
    }

    private void UpdatePlayerFood()
    {
        if (!_znv.IsValid() || !player) return;

        List<ItemDrop.ItemData> ItemsToRemove = new();
        List<ItemDrop.ItemData> ItemsToAdd = new();
        int PukeCount = 0;
        int RottenCount = 0;
        foreach (ItemDrop.ItemData item in player.GetInventory().m_inventory)
        {
            if (!Utility.Utility.ShouldUpdateFood(item)) continue;

            int maxDuration = Utility.Utility.GetMaxDuration(item);
            int currentDuration = maxDuration;
            if (item.m_customData.TryGetValue("StaleFood", out string StaleData))
            {
                if (int.Parse(StaleData) < currentDuration) currentDuration = int.Parse(StaleData);
            }
            float remainder = currentDuration - 1;

            Utility.Utility.UpdateItem(item, remainder, ref PukeCount, ref RottenCount, ItemsToRemove, ItemsToAdd);

            if (StaleFoodPlugin.SeasonalityLoaded)
            {
                item.m_customData["StaleFoodInventory"] = SeasonKeys.season is SeasonKeys.Seasons.Winter ? "Winter" : "false";
            }
        }

        foreach (ItemDrop.ItemData item in ItemsToRemove) player.GetInventory().RemoveItem(item, 1);
        
        foreach (ItemDrop.ItemData item in ItemsToAdd) player.GetInventory().AddItem(item);
        
        if (player.GetInventory().HaveEmptySlot() && PukeCount > 0) player.GetInventory().AddItem(LoadAssets.PukeBerries, PukeCount);
        
        if (player.GetInventory().HaveEmptySlot() && RottenCount > 0) player.GetInventory().AddItem(LoadAssets.RottenMeat, RottenCount);

        if (StaleFoodPlugin.SeasonalityLoaded)
        {
            CancelInvoke(nameof(UpdatePlayerFood));
            if (SeasonKeys.season is SeasonKeys.Seasons.Winter)
            {
                InvokeRepeating(nameof(UpdatePlayerFood), 120, 120);
            }
            else
            {
                InvokeRepeating(nameof(UpdatePlayerFood), 60, 60);
            }
        }
    }
}