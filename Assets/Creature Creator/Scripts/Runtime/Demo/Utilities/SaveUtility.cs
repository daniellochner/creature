using System.IO;
using UnityEngine;

public static class SaveUtility
{
    private static readonly string data_path = Application.persistentDataPath + "/Creatures/";

    public static void Save(string text, string fileName)
    {
        if (!Directory.Exists(data_path))
        {
            Directory.CreateDirectory(data_path);
        }

        File.WriteAllText(data_path + fileName, text);
    }
    public static string Load(string fileName)
    {
        if (File.Exists(data_path + fileName))
        {
            string text = File.ReadAllText(data_path + fileName);
            return text;
        }
        else
        {
            return null;
        }
    }
}