using UnityEngine;

namespace DungeonScavenger.Inventory
{
    /// <summary>
    /// ScriptableObject that defines the properties of an inventory item.
    /// This separates data from logic, making it easy to create new items
    /// without writing code.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        public string itemName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Inventory Behavior")]
        public bool isStackable = true;
        public int maxStackSize = 99;

        [Header("World Representation")]
        public GameObject worldPrefab;      // Prefab to spawn when dropping item
        public AudioClip pickupSound;
        public AudioClip dropSound;

        [Header("Visual Feedback")]
        public Color itemColor = Color.white;

        // Optional: Item rarity system (good for portfolio)
        public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
        public Rarity rarity = Rarity.Common;
    }
}