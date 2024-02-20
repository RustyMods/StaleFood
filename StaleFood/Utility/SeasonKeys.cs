using System;
using System.Collections.Generic;

namespace StaleFood.Utility;

public static class SeasonKeys
{
    [Flags]
    public enum Seasons
    {
        None = 0, 
        Spring = 1, 
        Summer = 1 << 1, 
        Fall = 1 << 2, 
        Winter = 1 << 3
    }
    public static Seasons season = Seasons.Winter;
    public static string currentSeason = "season_summer";
    public static void UpdateSeasonalKeys()
    {
        if (!Player.m_localPlayer) return;
        if (!ZoneSystem.instance) return;
            
        List<string>? currentKeys = ZoneSystem.instance.GetGlobalKeys();
        string key = currentKeys.Find(x => x.StartsWith("season"));
        if (key == null) return;
        if (currentSeason != key)
        {
            switch (key)
            {
                case "season_winter": season = Seasons.Winter; break;
                case "season_summer": season = Seasons.Summer; break;
                case "season_spring": season = Seasons.Spring; break;
                case "season_fall": season = Seasons.Fall; break;
            }
            currentSeason = key;
        }
    }
}