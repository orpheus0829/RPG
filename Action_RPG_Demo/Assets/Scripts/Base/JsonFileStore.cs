using System;
using System.IO;
using UnityEngine;

public static class JsonFileStore
{
    public static string PersistentPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName + ".json");
    }

    public static void Save<T>(string path, T value)
    {
        File.WriteAllText(path, JsonUtility.ToJson(value));
    }

    public static bool TryLoad<T>(string path, out T value) where T : class
    {
        value = null;
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            value = JsonUtility.FromJson<T>(File.ReadAllText(path));
            return value != null;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
