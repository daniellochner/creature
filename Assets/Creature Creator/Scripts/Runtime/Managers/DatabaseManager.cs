using DanielLochner.Assets.CreatureCreator;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    #region Singleton
    public static DatabaseManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    #region Fields
    [SerializeField] private DatabaseDictionary databases;
    #endregion

    #region Methods
    public static Database GetDatabase(string database)
    {
        return Instance.databases[database];
    }
    public static T GetDatabaseEntry<T>(string database, string key) where T : UnityEngine.Object
    {
        Database db = Instance.databases[database];

        if (db.Objects.ContainsKey(key))
        {
            return db.Objects[key] as T;
        }
        else
        {
            return null;
        }
    }
    #endregion

    #region Inner Classes
    [Serializable] public class DatabaseDictionary : SerializableDictionaryBase<string, Database> { }
    #endregion
}
