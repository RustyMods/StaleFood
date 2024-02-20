using System.IO;
using BepInEx;
using StaleFood.CustomPrefabs;
using UnityEngine;

namespace StaleFood.Configurations;

public static class FileWatcherSystem
{
    public static void InitFileSystem()
    {
        FileSystemWatcher DegradeFileWatch = new FileSystemWatcher(YmlConfigurations.FolderPath)
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };

        DegradeFileWatch.Changed += OnFileChange;

    }

    private static void OnFileChange(object sender, FileSystemEventArgs e)
    {
        string fileName = Path.GetFileName(e.Name);
        switch (fileName)
        {
            case "CustomPrefabs.yml":
                StaleFoodPlugin.StaleFoodLogger.LogDebug("Custom Prefabs file changed");
                YmlConfigurations.InitCustomPrefabs();
                break;
            case "DegradeItemData.yml":
                StaleFoodPlugin.StaleFoodLogger.LogDebug("Degrade Item Data file changed");
                DataManager.ReadDegradeDataFile();
                break;
        }
    }
}