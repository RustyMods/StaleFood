using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using StaleFood.CookingStation;
using StaleFood.Managers;
using StaleFood.Utility;
using UnityEngine;

namespace StaleFood
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("RustyMods.Seasonality", BepInDependency.DependencyFlags.SoftDependency)]
    public class StaleFoodPlugin : BaseUnityPlugin
    {
        internal const string ModName = "StaleFood";
        internal const string ModVersion = "0.0.8";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource StaleFoodLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public static StaleFoodPlugin _plugin = null!;
        public static GameObject Root = null!;
        public static AssetBundle _AssetBundle = null!;
        public static bool SeasonalityLoaded;
        public enum Toggle { On = 1, Off = 0 }
        public void Awake()
        {
            // Uncomment the line below to use the LocalizationManager for localizing your mod.
            //Localizer.Load(); // Use this to initialize the LocalizationManager (for more information on LocalizationManager, see the LocalizationManager documentation https://github.com/blaxxun-boop/LocalizationManager#example-project).
            
            _plugin = this;
            Root = new GameObject("root");
            Root.SetActive(false);
            DontDestroyOnLoad(Root);
            _AssetBundle = GetAssetBundle("stalefoodbundle");
            
            InitConfigs();
            LoadPieces.InitPieces();
            LoadItems.InitItems();
            MineRockManager.RegisterMineRocks();
            FoodManager.InitFoodManager();
            GourmetStation.PrepareCustomItems();

            SeasonalityLoaded = Chainloader.PluginInfos.ContainsKey("RustyMods.Seasonality");

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        public void Start()
        {
            if (SeasonalityLoaded) StartCoroutine(UpdateSeasons());
        }
        private IEnumerator UpdateSeasons()
        {
            while (true)
            {
                SeasonKeys.UpdateSeasonalKeys();
                yield return new WaitForSeconds(10f);
            }
        }
        private void OnDestroy() => Config.Save();

        #region ConfigOptions
        #region General
        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        public static ConfigEntry<Toggle> _UseDegradeItemDataYml = null!;
        private void InitGeneralConfigs()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            _UseDegradeItemDataYml = config("1 - General", "Use Degrade YML", Toggle.Off,
                "If on, plugin uses YML file to determine decay increment and spoil item");
        }
        #endregion
        #region Gourmet Station
        public static ConfigEntry<Toggle> _CookingStationRequireRoof = null!;
        public static ConfigEntry<Toggle> _UseGourmetStation = null!;

        private void InitGourmetStationConfigs()
        {
            _CookingStationRequireRoof = config("4 - Gourmet Station", "Cooking Station Require Roof", Toggle.On,
                "If on, cooking station requires roof");
            _UseGourmetStation = config("4 - Gourmet Station", "Enabled", Toggle.On,
                "If on, plugin recipes will be added to gourmet station, if off, then it will add to cauldron");
            _UseGourmetStation.SettingChanged += GourmetStation.OnSettingChanged;
        }
        #endregion
        #region Degradation
        public static ConfigEntry<Toggle> _FoodDecays = null!;
        public static ConfigEntry<int> _FoodDuration = null!;
        public static ConfigEntry<string> _CoolingItem = null!;
        public static ConfigEntry<int> _RefrigeratorMultiplier = null!;
        public static ConfigEntry<int> _FreezerMultiplier = null!;

        private void InitDegradationConfigs()
        {
            _FoodDecays = config("2 - Degradation", "Enabled", Toggle.On, "If on, food will decay over time");
            _CoolingItem = config("2 - Degradation", "Cooling Item", "FreezeGland", "Set the item used as fuel for fridges");
            _FoodDuration = config("2 - Degradation", "Duration (Minutes)", 100, new ConfigDescription("Set the duration of food decay", new AcceptableValueRange<int>(1, 1000)));
            _RefrigeratorMultiplier = config("2 - Degradation", "Refrigerator", 2, new ConfigDescription("Set the multiplier refrigerators extend food duration", new AcceptableValueRange<int>(1, 10)));
            _FreezerMultiplier = config("2 - Degradation", "Freezer", 3, new ConfigDescription("Set the multiplier freezers extend food duration", new AcceptableValueRange<int>(1, 10)));
        }
        #endregion
        #region StatusEffects
        public static ConfigEntry<Toggle> _UseConsumeEffects = null!;
        public static ConfigEntry<float> _SpoiledMultiplier = null!;
        public static ConfigEntry<int> _EffectThreshold = null!;

        private void InitStatusEffectConfigs()
        {
            _UseConsumeEffects = config("3 - Status Effects", "Enabled", Toggle.On, "If on, consuming spoiled food gives spoiled status effect");
            _SpoiledMultiplier = config("3 - Status Effects", "Spoiled Effect", 0.5f, 
                new ConfigDescription("Set the multiplier for the intensity of the spoiled effect when consuming a food item below the threshold, 1 = exact decay value match, less than 1 = reduction of intensity", 
                    new AcceptableValueRange<float>(0.1f, 0.99f)));
            _EffectThreshold = config("3 - Status Effects", "Spoiled Threshold", 50, new ConfigDescription("Set the threshold that activates spoiled status effect", new AcceptableValueRange<int>(0, 100)));
        }
        #endregion
        
        private void InitConfigs()
        {
            InitGeneralConfigs();
            InitGourmetStationConfigs();
            InitDegradationConfigs();
            InitStatusEffectConfigs();
        }
        #region Utils
        
        private static AssetBundle GetAssetBundle(string fileName)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
            return AssetBundle.LoadFromStream(stream);
        }
        
        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                StaleFoodLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                StaleFoodLogger.LogError($"There was an issue loading your {ConfigFileName}");
                StaleFoodLogger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
        private ConfigEntry<T> config<T>(string group, string title, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, title, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string title, T value, string description, bool synchronizedSetting = true)
        {
            return config(group, title, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        }
        #endregion

        #endregion
    }
}