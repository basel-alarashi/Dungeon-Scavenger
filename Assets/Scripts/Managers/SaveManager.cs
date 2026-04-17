using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using DungeonScavenger.Player;
using DungeonScavenger.Inventory;

namespace DungeonScavenger.Core
{
    /// <summary>
    /// Handles saving and loading game data using JSON serialization.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        #region Singleton

        public static SaveManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DeleteAllCorruptedSaveFiles();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Constants

        private const string SAVE_FOLDER = "/Saves/";
        private const string SAVE_EXTENSION = ".json";
        private const int CURRENT_SAVE_VERSION = 1;

        #endregion

        #region Inspector Fields

        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private bool logSaveOperations = true;

        #endregion

        #region Save/Load Methods

        /// <summary>
        /// Saves the current game state to a slot using JSON.
        /// </summary>
        public bool SaveGame(int slotIndex)
        {
            try
            {
                GameSaveData saveData = CreateSaveData();
                string path = GetSavePath(slotIndex);

                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Convert to JSON
                string json = JsonUtility.ToJson(saveData, true);

                // Write to file
                File.WriteAllText(path, json);

                if (logSaveOperations)
                    Debug.Log($"[SaveManager] Game saved to slot {slotIndex}. Health: {saveData.playerHealth}, Version: {saveData.saveVersion}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game to slot {slotIndex}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads game data from a slot using JSON.
        /// </summary>
        public bool LoadGame(int slotIndex)
        {
            string path = GetSavePath(slotIndex);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveManager] No save file found in slot {slotIndex}");
                return false;
            }

            try
            {
                // Read JSON from file
                string json = File.ReadAllText(path);

                // Deserialize
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

                if (saveData == null)
                {
                    Debug.LogError($"[SaveManager] Failed to deserialize save data from slot {slotIndex}");
                    return false;
                }

                // Check version compatibility
                if (saveData.saveVersion != CURRENT_SAVE_VERSION)
                {
                    Debug.LogWarning($"[SaveManager] Save file version mismatch. Expected {CURRENT_SAVE_VERSION}, got {saveData.saveVersion}.");
                }

                ApplySaveData(saveData);

                if (logSaveOperations)
                    Debug.Log($"[SaveManager] Game loaded from slot {slotIndex}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game from slot {slotIndex}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a save file exists in a slot.
        /// </summary>
        public bool SaveFileExists(int slotIndex)
        {
            string path = GetSavePath(slotIndex);
            return File.Exists(path);
        }

        /// <summary>
        /// Deletes a save file.
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            string path = GetSavePath(slotIndex);

            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    if (logSaveOperations)
                        Debug.Log($"[SaveManager] Deleted save slot {slotIndex}");
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to delete save slot {slotIndex}: {e.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets metadata about a save file safely.
        /// </summary>
        public SaveMetadata GetSaveMetadata(int slotIndex)
        {
            string path = GetSavePath(slotIndex);

            if (!File.Exists(path))
                return null;

            try
            {
                string json = File.ReadAllText(path);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

                if (saveData == null)
                    return null;

                return new SaveMetadata
                {
                    slotIndex = slotIndex,
                    saveTime = saveData.saveTime,
                    playerHealth = saveData.playerHealth,
                    playerMaxHealth = saveData.playerMaxHealth,
                    playerAmmo = saveData.playerAmmo,
                    itemCount = saveData.inventoryItemNames != null ? saveData.inventoryItemNames.Length : 0,
                    playTime = saveData.playTime,
                    saveVersion = saveData.saveVersion
                };
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Could not read save file in slot {slotIndex}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes all save files.
        /// </summary>
        public void DeleteAllSaves()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                DeleteSave(i);
            }
            Debug.Log("[SaveManager] All save files deleted");
        }

        /// <summary>
        /// Deletes corrupted save files on startup.
        /// </summary>
        private void DeleteAllCorruptedSaveFiles()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                string path = GetSavePath(i);
                if (File.Exists(path))
                {
                    try
                    {
                        string json = File.ReadAllText(path);
                        JsonUtility.FromJson<GameSaveData>(json);
                    }
                    catch
                    {
                        File.Delete(path);
                        Debug.LogWarning($"[SaveManager] Deleted corrupted save file in slot {i}");
                    }
                }
            }
        }

        #endregion

        #region Data Creation & Application

        private GameSaveData CreateSaveData()
        {
            GameSaveData data = new GameSaveData();

            // Set version
            data.saveVersion = CURRENT_SAVE_VERSION;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Position - store as serializable vectors
                data.playerPosition = new SerializableVector3(player.transform.position);
                data.playerRotation = new SerializableVector3(player.transform.eulerAngles);

                // Stats
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    data.playerHealth = stats.CurrentHealth;
                    data.playerMaxHealth = stats.MaxHealth;
                    data.playerAmmo = stats.CurrentAmmo;
                    data.playerMaxAmmo = stats.MaxAmmo;
                }

                // Inventory
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    var slots = inventory.GetAllSlots();
                    data.inventoryItemNames = new string[slots.Count];
                    data.inventoryQuantities = new int[slots.Count];

                    for (int i = 0; i < slots.Count; i++)
                    {
                        if (slots[i].itemData != null)
                        {
                            data.inventoryItemNames[i] = slots[i].itemData.itemName;
                            data.inventoryQuantities[i] = slots[i].quantity;
                        }
                    }
                }
            }

            // Meta data
            data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.playTime = Time.timeSinceLevelLoad;
            data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            return data;
        }

        private void ApplySaveData(GameSaveData data)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[SaveManager] Cannot apply save data: Player not found!");
                return;
            }

            // Position
            Vector3 position = data.playerPosition.ToVector3();
            Vector3 rotation = data.playerRotation.ToVector3();

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.transform.position = position;
                player.transform.eulerAngles = rotation;
                cc.enabled = true;
            }
            else
            {
                player.transform.position = position;
                player.transform.eulerAngles = rotation;
            }

            // Stats - Force set using public methods
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.ResetStats();

                int healthDiff = data.playerHealth - stats.CurrentHealth;
                if (healthDiff > 0)
                    stats.Heal(healthDiff);

                int ammoDiff = data.playerAmmo - stats.CurrentAmmo;
                if (ammoDiff > 0)
                    stats.AddAmmo(ammoDiff);
            }

            // Inventory
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null && data.inventoryItemNames != null)
            {
                inventory.ClearInventory();

                for (int i = 0; i < data.inventoryItemNames.Length; i++)
                {
                    if (!string.IsNullOrEmpty(data.inventoryItemNames[i]))
                    {
                        ItemData item = FindItemDataByName(data.inventoryItemNames[i]);
                        if (item != null)
                        {
                            inventory.AddItem(item, data.inventoryQuantities[i]);
                        }
                    }
                }
            }

            Debug.Log($"[SaveManager] Save data applied. Health: {data.playerHealth}, Position: {position}");
        }

        private ItemData FindItemDataByName(string itemName)
        {
            // Try loading from Resources first
            ItemData[] allItems = Resources.LoadAll<ItemData>("ScriptableObjects/Items");

            foreach (var item in allItems)
            {
                if (item.itemName == itemName)
                    return item;
            }

            // Fallback: Search all ItemData in project (Editor only)
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null && item.itemName == itemName)
                    return item;
            }
#endif

            Debug.LogWarning($"[SaveManager] Could not find ItemData: {itemName}");
            return null;
        }

        #endregion

        #region Helpers

        private string GetSavePath(int slotIndex)
        {
            return Application.persistentDataPath + SAVE_FOLDER + "save_" + slotIndex + SAVE_EXTENSION;
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Serializable version of Vector3 for JSON serialization.
    /// </summary>
    [Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class GameSaveData
    {
        // Version control
        public int saveVersion = 1;

        // Player data (using serializable vectors)
        public SerializableVector3 playerPosition;
        public SerializableVector3 playerRotation;
        public int playerHealth;
        public int playerMaxHealth;
        public int playerAmmo;
        public int playerMaxAmmo;

        // Inventory
        public string[] inventoryItemNames;
        public int[] inventoryQuantities;

        // Meta
        public string saveTime;
        public float playTime;
        public string sceneName;
    }

    [Serializable]
    public class SaveMetadata
    {
        public int slotIndex;
        public string saveTime;
        public int playerHealth;
        public int playerMaxHealth;
        public int playerAmmo;
        public int itemCount;
        public float playTime;
        public int saveVersion;

        public string GetDisplayText()
        {
            return $"Health: {playerHealth}/{playerMaxHealth}\n" +
                   $"Ammo: {playerAmmo}\n" +
                   $"Items: {itemCount}\n" +
                   $"Time: {FormatPlayTime(playTime)}\n" +
                   $"Saved: {saveTime}";
        }

        private string FormatPlayTime(float seconds)
        {
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{minutes:D2}:{secs:D2}";
        }
    }

    #endregion
}