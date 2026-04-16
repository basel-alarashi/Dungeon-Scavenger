using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DungeonScavenger.Player;
using DungeonScavenger.Inventory;

namespace DungeonScavenger.Core
{
    /// <summary>
    /// Handles saving and loading game data with multiple save slots.
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
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Constants

        private const string SAVE_FOLDER = "/Saves/";
        private const string SAVE_EXTENSION = ".sav";
        private const string SETTINGS_KEY = "GameSettings";

        #endregion

        #region Inspector Fields

        [Header("Save Settings")]
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private bool autoSaveOnPickup = true;
        [SerializeField] private bool autoSaveOnKill = true;
        [SerializeField] private float autoSaveInterval = 60f; // seconds

        [Header("Debug")]
        [SerializeField] private bool logSaveOperations = true;

        #endregion

        #region Private Data

        private float autoSaveTimer;

        #endregion

        #region Save/Load Game Data

        /// <summary>
        /// Saves the current game state to a slot.
        /// </summary>
        public bool SaveGame(int slotIndex)
        {
            try
            {
                GameSaveData saveData = CreateSaveData();
                string path = GetSavePath(slotIndex);

                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, saveData);
                }

                if (logSaveOperations)
                    Debug.Log($"[SaveManager] Game saved to slot {slotIndex}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads game data from a slot.
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
                GameSaveData saveData;

                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    saveData = (GameSaveData)formatter.Deserialize(stream);
                }

                ApplySaveData(saveData);

                if (logSaveOperations)
                    Debug.Log($"[SaveManager] Game loaded from slot {slotIndex}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a save file exists in a slot.
        /// </summary>
        public bool SaveFileExists(int slotIndex)
        {
            return File.Exists(GetSavePath(slotIndex));
        }

        /// <summary>
        /// Deletes a save file.
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            string path = GetSavePath(slotIndex);

            if (File.Exists(path))
            {
                File.Delete(path);

                if (logSaveOperations)
                    Debug.Log($"[SaveManager] Deleted save slot {slotIndex}");

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets metadata about a save file.
        /// </summary>
        public SaveMetadata GetSaveMetadata(int slotIndex)
        {
            string path = GetSavePath(slotIndex);

            if (!File.Exists(path))
                return null;

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    GameSaveData saveData = (GameSaveData)formatter.Deserialize(stream);

                    return new SaveMetadata
                    {
                        slotIndex = slotIndex,
                        saveTime = saveData.saveTime,
                        playerHealth = saveData.playerHealth,
                        playerAmmo = saveData.playerAmmo,
                        itemCount = saveData.inventoryItems.Length,
                        playTime = saveData.playTime
                    };
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Data Creation & Application

        private GameSaveData CreateSaveData()
        {
            GameSaveData data = new GameSaveData();

            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                data.playerPosition = player.transform.position;
                data.playerRotation = player.transform.rotation;

                // Player stats
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
                    data.inventoryItems = SerializeInventory(inventory);
                }
            }

            // Meta data
            data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.playTime = Time.time;
            data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            return data;
        }

        private void ApplySaveData(GameSaveData data)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // Position
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.transform.position = data.playerPosition;
                player.transform.rotation = data.playerRotation;
                cc.enabled = true;
            }

            // Stats
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // Use reflection or add public methods to set health/ammo
                stats.Heal(data.playerHealth - stats.CurrentHealth);
                stats.AddAmmo(data.playerAmmo - stats.CurrentAmmo);
            }

            // Inventory
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                DeserializeInventory(inventory, data.inventoryItems);
            }
        }

        private InventoryItemData[] SerializeInventory(PlayerInventory inventory)
        {
            var slots = inventory.GetAllSlots();
            InventoryItemData[] items = new InventoryItemData[slots.Count];

            for (int i = 0; i < slots.Count; i++)
            {
                items[i] = new InventoryItemData
                {
                    itemName = slots[i].itemData.itemName,
                    quantity = slots[i].quantity
                };
            }

            return items;
        }

        private void DeserializeInventory(PlayerInventory inventory, InventoryItemData[] items)
        {
            inventory.ClearInventory();

            foreach (var itemData in items)
            {
                // Find ItemData by name (you'll need a way to look up ItemData assets)
                ItemData item = Resources.Load<ItemData>($"Items/{itemData.itemName}");
                if (item != null)
                {
                    inventory.AddItem(item, itemData.quantity);
                }
            }
        }

        #endregion

        #region Settings Save/Load

        public void SaveSettings(GameSettingsData settings)
        {
            string json = JsonUtility.ToJson(settings);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();

            if (logSaveOperations)
                Debug.Log("[SaveManager] Settings saved");
        }

        public GameSettingsData LoadSettings()
        {
            if (PlayerPrefs.HasKey(SETTINGS_KEY))
            {
                string json = PlayerPrefs.GetString(SETTINGS_KEY);
                return JsonUtility.FromJson<GameSettingsData>(json);
            }

            return new GameSettingsData(); // Default settings
        }

        #endregion

        #region Auto-Save

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying) return;

            autoSaveTimer += Time.deltaTime;

            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                SaveGame(0); // Auto-save to slot 0

                if (logSaveOperations)
                    Debug.Log("[SaveManager] Auto-saved game");
            }
        }

        public void TriggerAutoSave(string reason)
        {
            if (!autoSaveOnPickup && !autoSaveOnKill) return;

            SaveGame(0);

            if (logSaveOperations)
                Debug.Log($"[SaveManager] Auto-saved: {reason}");
        }

        #endregion

        #region Helpers

        private string GetSavePath(int slotIndex)
        {
            string folder = Application.persistentDataPath + SAVE_FOLDER;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder + "save_" + slotIndex + SAVE_EXTENSION;
        }

        #endregion
    }

    #region Data Structures

    [Serializable]
    public class GameSaveData
    {
        // Player data
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public int playerHealth;
        public int playerMaxHealth;
        public int playerAmmo;
        public int playerMaxAmmo;

        // Inventory
        public InventoryItemData[] inventoryItems;

        // Meta
        public string saveTime;
        public float playTime;
        public string sceneName;
    }

    [Serializable]
    public class InventoryItemData
    {
        public string itemName;
        public int quantity;
    }

    [Serializable]
    public class GameSettingsData
    {
        public float masterVolume = 0.8f;
        public float sfxVolume = 0.8f;
        public float musicVolume = 0.6f;
        public int qualityLevel = 2;
        public bool fullscreen = true;
    }

    public class SaveMetadata
    {
        public int slotIndex;
        public string saveTime;
        public int playerHealth;
        public int playerAmmo;
        public int itemCount;
        public float playTime;

        public string GetDisplayText()
        {
            return $"Slot {slotIndex + 1}\n" +
                   $"Time: {saveTime}\n" +
                   $"Health: {playerHealth}\n" +
                   $"Items: {itemCount}\n" +
                   $"Playtime: {FormatPlayTime(playTime)}";
        }

        private string FormatPlayTime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
            return $"{hours:D2}:{minutes:D2}";
        }
    }

    #endregion
}