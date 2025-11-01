using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static string SavePath =>
        Path.Combine(Application.persistentDataPath, "save.json");
    public static void Write(GameProgressData data)
    {
        data.timestampIso = DateTime.UtcNow.ToString("o");
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
#if UNITY_EDITOR
        Debug.Log($"[SaveSystem] Loaded: {SavePath}\n{json}");
#endif
    }

    public static GameProgressData Read()
    {
        if(!Exists()) return null;
        var json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<GameProgressData>(json);
        return data;
    }

    public static bool Exists() => File.Exists(SavePath);

    public static void Delete()
    {
        if (!Exists()) return;

        File.Delete(SavePath);
    }
}
