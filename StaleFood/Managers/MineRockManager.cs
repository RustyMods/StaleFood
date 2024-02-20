using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using StaleFood.CustomPrefabs;
using UnityEngine;
using YamlDotNet.Serialization;

namespace StaleFood.Managers;

public static class MineRockManager
{
    private static readonly List<GameObject> PrefabsToSet = new();

    private static GameObject MineRockSalt = null!;

    public static void AddDestructibleEffects(ZNetScene instance)
    {
        GameObject VFX_RockDestroyedObsidian = instance.GetPrefab("vfx_RockDestroyed_Obsidian");
        GameObject SFX_RockDestroyed = instance.GetPrefab("sfx_rock_destroyed");
        GameObject VFX_RockHitObsidian = instance.GetPrefab("vfx_RockHit_Obsidian");
        GameObject SFX_RockHit = instance.GetPrefab("sfx_rock_hit");

        if (!VFX_RockDestroyedObsidian || !SFX_RockDestroyed || !VFX_RockHitObsidian || !SFX_RockHit) return;

        foreach (GameObject prefab in PrefabsToSet)
        {
            if (!prefab.TryGetComponent(out Destructible component)) continue;
            component.m_destroyedEffect = new EffectList()
            {
                m_effectPrefabs = new[]
                {
                    new EffectList.EffectData()
                    {
                        m_prefab = VFX_RockDestroyedObsidian,
                        m_enabled = true,
                        m_variant = -1,
                        m_attach = false,
                        m_follow = false,
                        m_inheritParentRotation = false,
                        m_inheritParentScale = false,
                        m_multiplyParentVisualScale = false,
                        m_scale = false,
                        m_randomRotation = false
                    },
                    new EffectList.EffectData()
                    {
                        m_prefab = SFX_RockDestroyed,
                        m_enabled = true,
                        m_variant = -1,
                        m_attach = false,
                        m_follow = false,
                        m_inheritParentRotation = false,
                        m_inheritParentScale = false,
                        m_multiplyParentVisualScale = false,
                        m_scale = false,
                        m_randomRotation = false
                    }
                }
            };
            component.m_hitEffect = new EffectList()
            {
                m_effectPrefabs = new[]
                {
                    new EffectList.EffectData()
                    {
                        m_prefab = VFX_RockHitObsidian,
                        m_enabled = true,
                        m_variant = -1,
                        m_attach = false,
                        m_follow = false,
                        m_inheritParentRotation = false,
                        m_inheritParentScale = false,
                        m_multiplyParentVisualScale = false,
                        m_scale = false,
                        m_randomRotation = false
                    },
                    new EffectList.EffectData()
                    {
                        m_prefab = SFX_RockHit,
                        m_enabled = true,
                        m_variant = -1,
                        m_attach = false,
                        m_follow = false,
                        m_inheritParentRotation = false,
                        m_inheritParentScale = false,
                        m_multiplyParentVisualScale = false,
                        m_scale = false,
                        m_randomRotation = false
                    }
                }
            };
        }
    }

    public static void RegisterMineRocks()
    {
        MineRockSalt = Object.Instantiate(StaleFoodPlugin._AssetBundle.LoadAsset<GameObject>("MineRock_Salt_RS"), StaleFoodPlugin.Root.transform, false);
        MineRockSalt.name = MineRockSalt.name.Replace("(Clone)", "");
        PrefabsToSet.Add(MineRockSalt);
    }

    public static void AddMineRocksToZNetScene(ZNetScene instance)
    {
        AddDropOnDestroyedData(MineRockSalt, LoadItems.SaltPrefab.name, instance);
        
        foreach (GameObject prefab in PrefabsToSet)
        {
            if (instance.m_namedPrefabs.ContainsKey(prefab.name.GetStableHashCode())) continue;
            if (instance.GetComponent<ZNetView>())
            {
                instance.m_prefabs.Add(prefab);
            }
            else
            {
                instance.m_nonNetViewPrefabs.Add(prefab);
            }
            instance.m_namedPrefabs.Add(prefab.name.GetStableHashCode(), prefab);

        }
    }

    private static void AddDropOnDestroyedData(GameObject MineRock, string drop, ZNetScene instance)
    {
        GameObject prefab = instance.GetPrefab(drop);
        if (!prefab) return;
        if (!MineRock.TryGetComponent(out DropOnDestroyed component)) return;
        component.m_dropWhenDestroyed.m_drops = new List<DropTable.DropData>()
        {
            new ()
            {
                m_item = prefab,
                m_stackMin = 1,
                m_stackMax = 2,
                m_weight = 1f,
                m_dontScale = false
            }
        };
    }

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
    private static class AddCustomMineRocksPatch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            // WriteAllZoneVegetationToFile(__instance);
            __instance.m_vegetation.AddRange(PrepareZoneVegetation());
        }
    }

    private static void WriteAllZoneVegetationToFile(ZoneSystem instance)
    {
        ISerializer serializer = new SerializerBuilder().Build();
        if (!Directory.Exists(YmlConfigurations.FolderPath)) Directory.CreateDirectory(YmlConfigurations.FolderPath);
        if (!Directory.Exists(YmlConfigurations.FolderPath + Path.DirectorySeparatorChar + "ZoneVegetation"))
        {
            Directory.CreateDirectory(YmlConfigurations.FolderPath + Path.DirectorySeparatorChar + "ZoneVegetation");
        }
            
        foreach (ZoneSystem.ZoneVegetation veg in instance.m_vegetation)
        {
            string data = serializer.Serialize(veg);
            string fileName = veg.m_prefab.name + ".yml";
            string filePath = YmlConfigurations.FolderPath + Path.DirectorySeparatorChar + "ZoneVegetation" +
                              Path.DirectorySeparatorChar + fileName;
            File.WriteAllText(filePath, data);
        }
    }

    private static List<ZoneSystem.ZoneVegetation> PrepareZoneVegetation()
    {
        return new List<ZoneSystem.ZoneVegetation>()
        {
            new ()
            {
                m_name = "flint",
                m_prefab = MineRockSalt,
                m_enable = true,
                m_min = 20f,
                m_max = 20f,
                m_forcePlacement = false,
                m_scaleMin = 1f,
                m_scaleMax = 1f,
                m_randTilt = 0f,
                m_chanceToUseGroundTilt = 1f,
                m_biome = Heightmap.Biome.Meadows,
                m_biomeArea = Heightmap.BiomeArea.Everything,
                m_blockCheck = true,
                m_snapToStaticSolid = false,
                m_minAltitude = -0.6f,
                m_maxAltitude = 1.1f,
                m_minVegetation = 0f,
                m_maxVegetation = 0f,
                m_minOceanDepth = 0f,
                m_maxOceanDepth = 0f,
                m_minTilt = 0f,
                m_maxTilt = 90f,
                m_terrainDeltaRadius = 0f,
                m_maxTerrainDelta = 2f,
                m_minTerrainDelta = 1f,
                m_snapToWater = false,
                m_groundOffset = -0.5f,
                m_groupSizeMin = 1,
                m_groupSizeMax = 1,
                m_groupRadius = 2f,
                m_inForest = false,
                m_forestTresholdMin = 0f,
                m_forestTresholdMax = 1f,
                m_foldout = false
            }
        };
    }
}