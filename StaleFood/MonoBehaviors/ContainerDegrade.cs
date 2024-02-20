using System.Collections.Generic;
using System.Linq;
using StaleFood.Utility;
using UnityEngine;

namespace StaleFood.MonoBehaviors;

public class ContainerDegrade : MonoBehaviour
{
    private ZNetView _znv = null!;
    private Container data = null!;

    public List<ParticleSystem> particles = new();

    private float time;

    private readonly int EffectHash = "FridgeEffects".GetStableHashCode();
    private void Awake()
    {
        _znv = GetComponent<ZNetView>();
        if (!_znv.IsValid()) return;

        data = GetComponent<Container>();
        
        List<ParticleSystem>? openEffects = transform.Find("open")?.GetComponentsInChildren<ParticleSystem>().ToList();
        List<ParticleSystem>? closedEffects = transform.Find("closed")?.GetComponentsInChildren<ParticleSystem>().ToList();
            
        if (openEffects != null) particles.AddRange(openEffects);
        if (closedEffects != null) particles.AddRange(closedEffects);
            
        _znv.GetZDO().Set(EffectHash, false);
    }

    private void Start()
    {
        CancelInvoke(nameof(RemoveCoolingItem));
        if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return;
        InvokeRepeating(nameof(RemoveCoolingItem), 60, 60);
    }

    private void RemoveCoolingItem()
    {
        if (data.GetInventory().HaveItem(LoadAssets.CoolingItemDrop.m_itemData.m_shared.m_name))
        {
            data.GetInventory().RemoveItem(LoadAssets.CoolingItemDrop.m_itemData.m_shared.m_name, 1);
        }
    }
    private void FixedUpdate()
    {
        float datetime = Time.fixedDeltaTime;
        CheckForCoolingItem(datetime);

        if (!gameObject || !data) return;
        bool flag = _znv.GetZDO().GetBool(EffectHash);
        if (particles.Count <= 0) return;
        foreach (ParticleSystem particle in particles)
        {
            ParticleSystem.EmissionModule particleEmission = particle.emission;
            particleEmission.enabled = flag;
        }
    }

    private void CheckForCoolingItem(float dt)
    {
        if (!gameObject || !data) return;
        if (StaleFoodPlugin._FoodDecays.Value is StaleFoodPlugin.Toggle.Off) return;
        time += dt;
        bool flag = data.GetInventory().HaveItem(LoadAssets.CoolingItemDrop.m_itemData.m_shared.m_name);
        _znv.GetZDO().Set(EffectHash, flag);

        switch (gameObject.name.Replace("(Clone)", ""))
        {
            case "Refrigerator" when flag:
                if (time < 60 * StaleFoodPlugin._RefrigeratorMultiplier.Value) break;
                UpdateFood(true);
                time = 0;
                break;
            case "Freezer" when flag:
                if (time < 60 * StaleFoodPlugin._FreezerMultiplier.Value) break;
                UpdateFood(true);
                time = 0;
                break;
            default:
                if (time < 60) break;
                UpdateFood();
                time = 0;
                break;
        }
    }

    private void UpdateFood(bool hasCoolingItem = false)
    {
        if (!_znv.IsValid()) return;
        if (data == null) return;
        List<ItemDrop.ItemData> ItemsToRemove = new();
        List<ItemDrop.ItemData> ItemsToAdd = new();
        int PukeCount = 0;
        int RottenCount = 0;
        foreach (ItemDrop.ItemData item in data.GetInventory().m_inventory)
        {
            if (!Utility.Utility.ShouldUpdateFood(item)) continue;
            item.m_customData["StaleFoodInventory"] = hasCoolingItem ? data.GetInventory().GetName() : "false";
            int maxDuration = Utility.Utility.GetMaxDuration(item);
            int currentDuration = maxDuration;
            if (item.m_customData.TryGetValue("StaleFood", out string StaleData))
            {
                if (int.Parse(StaleData) < currentDuration) currentDuration = int.Parse(StaleData);
            }
            else
            {
                item.m_customData["StaleFood"] = maxDuration.ToString();
            }

            float remainder = currentDuration - 1;

            Utility.Utility.UpdateItem(item, remainder, ref PukeCount, ref RottenCount, ItemsToRemove, ItemsToAdd);
        }

        foreach (ItemDrop.ItemData item in ItemsToRemove) data.GetInventory().RemoveItem(item, 1);
        foreach (ItemDrop.ItemData item in ItemsToAdd)
        {
            data.GetInventory().AddItem(item);
        }

        if (data.GetInventory().HaveEmptySlot() && PukeCount > 0)
            data.GetInventory().AddItem(LoadAssets.PukeBerries, PukeCount);

        if (data.GetInventory().HaveEmptySlot() && RottenCount > 0)
            data.GetInventory().AddItem(LoadAssets.RottenMeat, RottenCount);
    }
}