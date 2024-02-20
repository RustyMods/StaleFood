using System.Collections.Generic;
using System.IO;
using System.Linq;
using StaleFood.CustomPrefabs;
using YamlDotNet.Serialization;

namespace StaleFood.CookingStation;

public static class FoodManager
{
    private static readonly string FoodItemFolder = YmlConfigurations.FolderPath + Path.DirectorySeparatorChar + "CustomFood";

    private static readonly List<FoodItemData> CustomFoodFileData = new();
    
    public static void InitFoodManager()
    {
        CustomFoodFileData.Clear();
        
        if (!Directory.Exists(YmlConfigurations.FolderPath)) Directory.CreateDirectory(YmlConfigurations.FolderPath);
        if (!Directory.Exists(FoodItemFolder)) Directory.CreateDirectory(FoodItemFolder);

        ISerializer serializer = new SerializerBuilder().Build();
        foreach (FoodItemData FoodData in GetDefaultFoodData())
        {
            string filePath = FoodItemFolder + Path.DirectorySeparatorChar + FoodData.m_prefabName + ".yml";

            if (File.Exists(filePath)) continue;
            
            string data = serializer.Serialize(FoodData);
            File.WriteAllText(filePath, data);
        }

        List<string> FoodPaths = Directory.GetFiles(FoodItemFolder, "*.yml").ToList();

        IDeserializer deserializer = new DeserializerBuilder().Build();
        
        foreach (string path in FoodPaths)
        {
            string data = File.ReadAllText(path);
            FoodItemData FoodData = deserializer.Deserialize<FoodItemData>(data);
            CustomFoodFileData.Add(FoodData);
        }
    }

    private static List<FoodItemData> GetDefaultFoodData()
    {
        return new List<FoodItemData>()
        {
            new ()
            {
                m_cloneName = "Raspberry",
                m_prefabName = "HoneyRaspberry",
                m_sharedName = "Honey Raspberry",
                m_description = "Sweetened raspberry with added preservative properties",
                m_iconType = "Honey",
                m_health = 15,
                m_stamina = 25,
                m_burnTime = 800,
                m_duration = 200,
                m_recipe = new List<Ingredient>()
                {
                    new ()
                    {
                        m_prefabName = "Raspberry",
                    },
                    new ()
                    {
                        m_prefabName = "Honey"
                    }
                },
            },
            new ()
            {
                m_cloneName = "Blueberries",
                m_prefabName = "HoneyBlueberry",
                m_sharedName = "Honey Blueberries",
                m_description = "Sweetened blueberries with added preservative properties",
                m_iconType = "Honey",
                m_health = 12,
                m_stamina = 30,
                m_burnTime = 700,
                m_duration = 200,
                m_recipe = new List<Ingredient>()
                {
                    new ()
                    {
                        m_prefabName = "Blueberries",
                    },
                    new ()
                    {
                        m_prefabName = "Honey"
                    }
                },
            },
            new ()
            {
                m_cloneName = "Carrot",
                m_prefabName = "HoneyCarrot",
                m_sharedName = "Honey Carrot",
                m_description = "Sweetened carrot with added preservative properties",
                m_iconType = "Honey",
                m_health = 12,
                m_stamina = 40,
                m_burnTime = 1000,
                m_duration = 200,
                m_recipe = new List<Ingredient>()
                {
                    new ()
                    {
                        m_prefabName = "Carrot",
                    },
                    new ()
                    {
                        m_prefabName = "Honey"
                    }
                },
            },
            new ()
            {
                m_cloneName = "CookedMeat",
                m_prefabName = "CuredBoarMeat",
                m_sharedName = "Cured Pork",
                m_description = "Cured boar meat last longer and tastier",
                m_iconType = "Cured",
                m_health = 35,
                m_stamina = 12,
                m_burnTime = 1200,
                m_regen = 2,
                m_duration = 200,
                m_recipe = new List<Ingredient>()
                {
                    new ()
                    {
                        m_prefabName = "RawMeat",
                    },
                    new ()
                    {
                        m_prefabName = "Salt_RS"
                    }
                },
            },
            new ()
            {
                m_cloneName = "NeckTail",
                m_prefabName = "CuredNeckTail",
                m_sharedName = "Cured Neck Tail",
                m_description = "Cured neck tail last longer and probably tastier",
                m_iconType = "Cured",
                m_health = 25,
                m_stamina = 12,
                m_burnTime = 1200,
                m_regen = 2,
                m_duration = 200,
                m_recipe = new List<Ingredient>()
                {
                    new ()
                    {
                        m_prefabName = "NeckTail",
                    },
                    new ()
                    {
                        m_prefabName = "Salt_RS"
                    }
                },
            },
            new ()
            {
                m_cloneName = "CookedDeerMeat",
                m_prefabName = "CuredDeerMeat",
                m_sharedName = "Cured Deer Ribs",
                m_description = "Cured deer ribs last longer and are tastier",
                m_iconType = "Cured",
                m_health = 35,
                m_stamina = 15,
                m_burnTime = 1200,
                m_regen = 2,
                m_duration = 200,
                m_recipe = new List<Ingredient>()
                {
                    new ()
                    {
                        m_prefabName = "DeerMeat",
                    },
                    new ()
                    {
                        m_prefabName = "Salt_RS"
                    }
                },
            },
            new ()
            {
                m_cloneName = "BarleyFlour",
                m_prefabName = "Sugar",
                m_sharedName = "Sugar",
                m_description = "Everything is sweeter with sugar",
                m_iconType = "Honey",
                m_health = 5,
                m_stamina = 25,
                m_burnTime = 800,
                m_regen = 1,
                m_duration = 300,
                m_recipe = new List<Ingredient>()
                {
                    new ()
                    {
                        m_prefabName = "Honey",
                        m_amount = 2,
                    },
                },
            },
            new ()
            {
                m_cloneName = "Raspberry",
                m_prefabName = "DriedRaspberry",
                m_sharedName = "Dried Raspberry",
                m_description = "Dried Raspberry is perfect for long trips",
                m_iconType = "Dried",
                m_health = 15,
                m_stamina = 25,
                m_burnTime = 800,
                m_duration = 250,
                m_recipe = new List<Ingredient>()
                {
                    new ()
                    {
                        m_prefabName = "Raspberry",
                    },
                    new ()
                    {
                        m_prefabName = "Sugar"
                    }
                },
            }
        };
    }

    public static List<FoodItemData> GetCustomFoodData() => CustomFoodFileData;
}