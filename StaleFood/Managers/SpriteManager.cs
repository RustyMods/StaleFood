using System.Reflection;
using UnityEngine;

namespace StaleFood.Managers;

public static class SpriteManager
{
    public static readonly Sprite? HoneyIconSprite = RegisterSprite("honeyed_icon.png");
    public static readonly Sprite? DriedIconSprite = RegisterSprite("dried_icon.png");
    public static readonly Sprite? SaltedIconSprite = RegisterSprite("salted_icon.png");
    
    private static Sprite? RegisterSprite(string fileName, string folderName = "icons")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{StaleFoodPlugin.ModName}.{folderName}.{fileName}";
        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        int readStream = stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        
        return texture.LoadImage(buffer) ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
    }
}