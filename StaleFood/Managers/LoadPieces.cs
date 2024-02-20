using StaleFood.Utility;

namespace StaleFood.Managers;

public static class LoadPieces
{
    public static CraftingStation CookingCraftingStation = null!;
    
    public static void InitPieces()
    {
        // Globally turn off configuration options for your pieces, omit if you don't want to do this.
        // BuildPiece.ConfigurationEnabled = false;

        BuildPiece Refrigerator = new("stalefoodbundle", "Refrigerator");
        Refrigerator.Name.English("Refrigerator");
        Refrigerator.Description.English("Keep your food fresh");
        Refrigerator.Category.Set(BuildPieceCategory.Misc);
        Refrigerator.Crafting.Set(CraftingTable.Forge);
        Refrigerator.RequiredItems.Add("Bronze", 5, true);
        Refrigerator.RequiredItems.Add("Wood", 10, true);
        MaterialReplacer.RegisterGameObjectForShaderSwap(Refrigerator.Prefab.transform.Find("open/model_open").gameObject, MaterialReplacer.ShaderType.PieceShader);
        MaterialReplacer.RegisterGameObjectForShaderSwap(Refrigerator.Prefab.transform.Find("closed/model_closed").gameObject, MaterialReplacer.ShaderType.PieceShader);
        PieceEffectManager.PrefabsToSet.Add(Refrigerator.Prefab);
        InventoryPatches.FridgeNames.Add("$piece_refrigerator");
        
        BuildPiece Freezer = new("stalefoodbundle", "Freezer");
        Freezer.Name.English("Freezer");
        Freezer.Description.English("Keep your food frozen");
        Freezer.Category.Set(BuildPieceCategory.Misc);
        Freezer.Crafting.Set(CraftingTable.Forge);
        Freezer.RequiredItems.Add("Iron", 5, true);
        Freezer.RequiredItems.Add("FineWood", 10, true);
        MaterialReplacer.RegisterGameObjectForMatSwap(Freezer.Prefab.transform.Find("open/model_open").gameObject);
        MaterialReplacer.RegisterGameObjectForMatSwap(Freezer.Prefab.transform.Find("closed/model_closed").gameObject);
        PieceEffectManager.PrefabsToSet.Add(Freezer.Prefab);
        InventoryPatches.FridgeNames.Add("$piece_freezer");

        BuildPiece CookingStation = new("stalefoodbundle", "CookingStation_RS");
        CookingStation.Name.English("Gourmet Station");
        CookingStation.Description.English("Elevate your cooking to the next level");
        CookingStation.Category.Set(BuildPieceCategory.Crafting);
        CookingStation.Crafting.Set(CraftingTable.Workbench);
        CookingStation.RequiredItems.Add("BronzeNails", 20, true);
        CookingStation.RequiredItems.Add("Wood", 5, true);
        CookingStation.RequiredItems.Add("QueensJam", 2, true);
        CookingStation.RequiredItems.Add("Carrot", 3, true);
        MaterialReplacer.RegisterGameObjectForMatSwap(CookingStation.Prefab.transform.Find("model/$part_replace").gameObject);
        PieceEffectManager.PrefabsToSet.Add(CookingStation.Prefab);
        CookingCraftingStation = CookingStation.Prefab.GetComponent<CraftingStation>();
    }
}