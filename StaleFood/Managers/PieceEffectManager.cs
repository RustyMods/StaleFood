﻿using System.Collections.Generic;
using UnityEngine;

namespace StaleFood.Managers;

public static class PieceEffectManager
{
    public static readonly List<GameObject> PrefabsToSet = new();
    
    public static void AddPieceEffects(ZNetScene instance)
    {
        if (!instance) return;
        foreach (GameObject prefab in PrefabsToSet)
        {
            GameObject Object = instance.GetPrefab(prefab.name);
            if (!Object) continue;
            SetPieceScript(instance, Object, "vfx_Place_stone_wall_2x1", "sfx_build_hammer_stone");
            SetWearNTearScript(instance, Object,
                "vfx_RockHit", "sfx_rock_destroyed",
                "vfx_RockHit", "sfx_rock_hit",
                "vfx_Place_throne02", 0f);
        }
    }
    
    private static void SetPieceScript(ZNetScene scene, GameObject prefab, string placementEffectName1, string placementEffectName2)
        {
        GameObject placeEffect1 = scene.GetPrefab(placementEffectName1);
        GameObject placeEffect2 = scene.GetPrefab(placementEffectName2);
        if (!placeEffect1 || !placeEffect2) return;
        EffectList placementEffects = new EffectList()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = placeEffect1,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = false,
                    m_inheritParentRotation = false,
                    m_inheritParentScale = false,
                    m_randomRotation = false,
                    m_scale = false,
                    m_childTransform = ""
                },
                new EffectList.EffectData()
                {
                    m_prefab = placeEffect2,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = false,
                    m_inheritParentRotation = false,
                    m_inheritParentScale = false,
                    m_randomRotation = false,
                    m_scale = false,
                    m_childTransform = ""
                }
            }
        };
        Piece pieceScript = prefab.GetComponent<Piece>();
        pieceScript.m_placeEffect = placementEffects;
        // Configure piece placement restrictions
        pieceScript.m_groundPiece = false;
        pieceScript.m_allowAltGroundPlacement = false;
        pieceScript.m_cultivatedGroundOnly = false;
        pieceScript.m_waterPiece = false;
        pieceScript.m_clipGround = true;
        pieceScript.m_clipEverything = false;
        pieceScript.m_noInWater = false;
        pieceScript.m_notOnWood = false;
        pieceScript.m_notOnTiltingSurface = false;
        pieceScript.m_inCeilingOnly = false;
        pieceScript.m_notOnFloor = false;
        pieceScript.m_noClipping = false;
        pieceScript.m_onlyInTeleportArea = false;
        pieceScript.m_allowedInDungeons = false;
        pieceScript.m_spaceRequirement = 0f;
        pieceScript.m_repairPiece = false;
        pieceScript.m_canBeRemoved = true;
        pieceScript.m_allowRotatedOverlap = false;
        pieceScript.m_vegetationGroundOnly = false;
        }
    
    private static void SetWearNTearScript(ZNetScene scene, GameObject prefab, string destroyedEffectName1, string destroyEffectName2, string hitEffectName1, string hitEffectName2, string switchEffectName, float health)
    {
        // Format destroy effect data
        GameObject destroyEffect1 = scene.GetPrefab(destroyedEffectName1);
        GameObject destroyEffect2 = scene.GetPrefab(destroyEffectName2);
        if (!destroyEffect1 || !destroyEffect2) return;
        EffectList destroyEffects = new EffectList()
        {
            m_effectPrefabs = new []
            {
                new EffectList.EffectData()
                {
                    m_prefab = destroyEffect1,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = false,
                    m_inheritParentRotation = false,
                    m_inheritParentScale = false,
                    m_randomRotation = false,
                    m_scale = false,
                    m_childTransform = ""
                },
                new EffectList.EffectData()
                {
                    m_prefab = destroyEffect2,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = false,
                    m_inheritParentRotation = false,
                    m_inheritParentScale = false,
                    m_randomRotation = false,
                    m_scale = false,
                    m_childTransform = ""
                }
            }
        };
        // Format hit effect data
        GameObject hitEffect1 = scene.GetPrefab(hitEffectName1);
        GameObject hitEffect2 = scene.GetPrefab(hitEffectName2);
        if (!hitEffect1 || !hitEffect2) return;
        EffectList hitEffects = new EffectList()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = hitEffect1,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = false,
                    m_inheritParentRotation = false,
                    m_inheritParentScale = false,
                    m_randomRotation = false,
                    m_scale = false,
                    m_childTransform = ""
                },
                new EffectList.EffectData()
                {
                    m_prefab = hitEffect2,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = false,
                    m_inheritParentRotation = false,
                    m_inheritParentScale = false,
                    m_randomRotation = false,
                    m_scale = false,
                    m_childTransform = ""
                }
            }
        };
        // Format switch effect data
        GameObject switchEffect = scene.GetPrefab(switchEffectName);
        EffectList switchEffects = new EffectList()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = switchEffect,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = false,
                    m_inheritParentRotation = false,
                    m_inheritParentScale = false,
                    m_randomRotation = false,
                    m_scale = false,
                    m_childTransform = ""
                }
            }
        };
        // Set data
        WearNTear WearNTearScript = prefab.GetComponent<WearNTear>();
        WearNTearScript.m_destroyedEffect = destroyEffects;
        WearNTearScript.m_hitEffect = hitEffects;
        WearNTearScript.m_switchEffect = switchEffects;
        if (health != 0f) WearNTearScript.m_health = health;
    }
}