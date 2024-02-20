using System.Collections.Generic;

namespace StaleFood.Utility;

public static class SpecialCases
{
    public static readonly List<string> NamesOrSharedNames = new();

    public static readonly List<string> DefaultNames = new()
    {
        "meat",
        "uncooked",
        "necktail",
        "entrails",
        "turnip",
        "barley",
        "flax",
        "breaddough",
        "raw",
    };
    public static bool ShouldBeAffected(string input)
    {
        foreach (string name in NamesOrSharedNames)
        {
            if (input.ToLower().Contains(name.ToLower())) return true;
        }
        return false;
    }
}