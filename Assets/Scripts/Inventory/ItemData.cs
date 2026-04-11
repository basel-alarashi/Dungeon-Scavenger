using UnityEngine;

namespace DungeonScavenger.Inventory
{
    /// <summary>
    /// Defines what happens when an item is used.
    /// </summary>
    public enum ItemUseType
    {
        None,           // Cannot be used (junk items)
        Consumable,     // One-time use (potions, food)
        Ammo,           // Adds to ammo pool
        Equipment,      // Can be equipped (weapons, armor)
        Quest           // Quest items (cannot be discarded)
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        public string itemName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Usage Settings")]
        public ItemUseType useType = ItemUseType.None;
        public bool isConsumable = true;  // If true, item is destroyed on use

        [Header("Effect Values")]
        public int healthRestoreAmount = 0;
        public int ammoRestoreAmount = 0;

        [Header("Inventory Behavior")]
        public bool isStackable = true;
        public int maxStackSize = 99;

        [Header("World Representation")]
        public GameObject worldPrefab;
        public AudioClip pickupSound;
        public AudioClip useSound;

        [Header("Visual Feedback")]
        public Color itemColor = Color.white;

        public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
        public Rarity rarity = Rarity.Common;

        /// <summary>
        /// Uses this item on the player.
        /// </summary>
        /// <returns>True if the item was successfully used.</returns>
        public bool Use(Player.PlayerStats targetStats)
        {
            if (targetStats == null)
            {
                Debug.LogError($"[ItemData] Cannot use {itemName}: No PlayerStats target!");
                return false;
            }

            bool wasUsed = false;

            switch (useType)
            {
                case ItemUseType.Consumable:
                    wasUsed = UseAsConsumable(targetStats);
                    break;

                case ItemUseType.Ammo:
                    wasUsed = UseAsAmmo(targetStats);
                    break;

                case ItemUseType.None:
                    Debug.LogWarning($"[ItemData] {itemName} cannot be used (UseType = None)");
                    break;

                default:
                    Debug.LogWarning($"[ItemData] {itemName} use type {useType} not implemented yet");
                    break;
            }

            if (wasUsed)
            {
                Debug.Log($"[ItemData] Used {itemName}");

                // Play use sound if available
                if (useSound != null)
                {
                    // AudioManager.Instance?.PlaySFX(useSound);
                }
            }

            return wasUsed;
        }

        private bool UseAsConsumable(Player.PlayerStats targetStats)
        {
            if (healthRestoreAmount > 0)
            {
                if (targetStats.IsHealthFull())
                {
                    Debug.Log($"[ItemData] Cannot use {itemName}: Health already full!");
                    return false;
                }

                targetStats.Heal(healthRestoreAmount);
                return true;
            }

            Debug.LogWarning($"[ItemData] {itemName} has no restore values set!");
            return false;
        }

        private bool UseAsAmmo(Player.PlayerStats targetStats)
        {
            if (ammoRestoreAmount > 0)
            {
                if (targetStats.IsAmmoFull())
                {
                    Debug.Log($"[ItemData] Cannot use {itemName}: Ammo already full!");
                    return false;
                }

                return targetStats.AddAmmo(ammoRestoreAmount);
            }

            Debug.LogWarning($"[ItemData] {itemName} has no ammo restore value set!");
            return false;
        }
    }
}