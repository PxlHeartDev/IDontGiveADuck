using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoader : MonoBehaviour
{
    // Singleton pattern - static instance accessible from anywhere
    public static SaveLoader Instance { get; private set; }

    public SaveData saveData;

    string filePath = "";

    void Awake()
    {
        filePath = Application.persistentDataPath + "/saveFile.json";

        // Singleton pattern implementation
        // If no instance exists, make this the singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep alive when scenes change
        }
        else
        {
            // If another instance exists, destroy this duplicate
            Destroy(gameObject);
        }
    }

    public void LoadData()
    {
        if (File.Exists(filePath))
        {
            string fileData = File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<SaveData>(fileData);
        }
        else
        {
            Debug.Log("Save data file does not exist, creating new one");
            CreateEmptyData();
        }
    }

    public void CreateEmptyData()
    {
        saveData = new();
        List<int> scores = new();

        for (int i = 0; i < LevelLoader.Instance.GetLevelCount(); i++)
            scores.Add(0);

        saveData.scores = scores;

        // If file already exists, clear it out
        if (File.Exists(filePath))
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(saveData));
        }
        else
        {
            File.CreateText(filePath).Dispose();
            SaveDataToFile();
        }
    }

    public void SaveDataToFile()
    {
        if (File.Exists(filePath))
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(saveData));
        }
        else
        {
            CreateEmptyData();
            SaveDataToFile();
        }
    }
}
