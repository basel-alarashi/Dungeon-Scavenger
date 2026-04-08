using System.Collections.Generic;
using UnityEngine;
using System;

namespace DungeonScavenger.Inventory
{
    /// <summary>
    /// Manages the player's inventory system.
    /// Follows the Singleton pattern for easy access from other scripts.
    /// Uses events to notify UI of changes without tight coupling.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        #region Singleton Pattern
        
        public static PlayerInventory Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton setup - ensures only one inventory exists
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scene loads
            }
            else
            {
                Destroy(gameObject);
                Debug.LogWarning("[PlayerInventory] Duplicate instance destroyed.");
                return;
            }
            
            // Initialize the items list
            items = new List<InventorySlot>();
            
            Debug.Log($"[PlayerInventory] Initialized with {maxSlots} slots.");
        }
        
        #endregion
        
        #region Public Events
        
        /// <summary>
        /// Fired whenever items are added, removed, or stacked.
        /// UI components should subscribe to this to refresh their display.
        /// </summary>
        public event Action OnInventoryChanged;
        
        #endregion
        
        #region Inspector Fields
        
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 16;
        [SerializeField] private bool allowStacking = true;
        [SerializeField] private int maxStackSize = 99;
        
        [Header("Debug")]
        [SerializeField] private bool logInventoryChanges = true;
        
        #endregion
        
        #region Private Data
        
        // The actual inventory storage
        public List<InventorySlot> items;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Returns the current number of items in the inventory.
        /// </summary>
        public int ItemCount => items.Count;
        
        /// <summary>
        /// Returns the maximum number of slots available.
        /// </summary>
        public int MaxSlots => maxSlots;
        
        /// <summary>
        /// Checks if the inventory is completely full.
        /// </summary>
        public bool IsFull => items.Count >= maxSlots;
        
        #endregion
        
        #region Core Inventory Methods
        
        /// <summary>
        /// Attempts to add an item to the inventory.
        /// </summary>
        /// <param name="itemToAdd">The ItemData to add.</param>
        /// <param name="quantity">How many to add (default 1).</param>
        /// <returns>True if the item was added successfully.</returns>
        public bool AddItem(ItemData itemToAdd, int quantity = 1)
        {
            if (itemToAdd == null)
            {
                Debug.LogError("[PlayerInventory] Cannot add null item!");
                return false;
            }
            
            if (quantity <= 0)
            {
                Debug.LogWarning($"[PlayerInventory] Invalid quantity: {quantity}");
                return false;
            }
            
            // Try to stack with existing items first (if stacking is allowed)
            if (allowStacking && itemToAdd.isStackable)
            {
                int remainingQuantity = TryStackExistingItems(itemToAdd, quantity);
                if (remainingQuantity <= 0)
                {
                    // All items were stacked successfully
                    OnInventoryChanged?.Invoke();
                    
                    if (logInventoryChanges)
                        Debug.Log($"[PlayerInventory] Added {quantity}x {itemToAdd.itemName} (stacked)");
                    
                    return true;
                }
                
                // Some items were stacked, need to create new slots for the rest
                quantity = remainingQuantity;
            }
            
            // Create new slots for remaining items (or all items if not stackable)
            return AddNewSlots(itemToAdd, quantity);
        }
        
        /// <summary>
        /// Removes an item from the inventory.
        /// </summary>
        /// <param name="itemToRemove">The ItemData to remove.</param>
        /// <param name="quantity">How many to remove (default 1).</param>
        /// <returns>True if the item was removed successfully.</returns>
        public bool RemoveItem(ItemData itemToRemove, int quantity = 1)
        {
            if (itemToRemove == null)
            {
                Debug.LogError("[PlayerInventory] Cannot remove null item!");
                return false;
            }
            
            int remainingToRemove = quantity;
            
            // Search for matching slots (start from the end for cleaner removal)
            for (int i = items.Count - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                if (items[i].itemData == itemToRemove)
                {
                    int amountInSlot = items[i].quantity;
                    
                    if (amountInSlot <= remainingToRemove)
                    {
                        // Remove the entire slot
                        remainingToRemove -= amountInSlot;
                        items.RemoveAt(i);
                    }
                    else
                    {
                        // Reduce the quantity in this slot
                        items[i].quantity -= remainingToRemove;
                        remainingToRemove = 0;
                    }
                }
            }
            
            if (remainingToRemove < quantity)
            {
                // At least some items were removed
                OnInventoryChanged?.Invoke();
                
                if (logInventoryChanges)
                    Debug.Log($"[PlayerInventory] Removed {quantity - remainingToRemove}x {itemToRemove.itemName}");
                
                return true;
            }
            
            // No items were removed
            if (logInventoryChanges)
                Debug.LogWarning($"[PlayerInventory] Could not find {quantity}x {itemToRemove.itemName} to remove.");
            
            return false;
        }
        
        /// <summary>
        /// Checks if the inventory contains at least the specified quantity of an item.
        /// </summary>
        public bool HasItem(ItemData itemToCheck, int quantity = 1)
        {
            int totalFound = 0;
            
            foreach (InventorySlot slot in items)
            {
                if (slot.itemData == itemToCheck)
                {
                    totalFound += slot.quantity;
                    if (totalFound >= quantity)
                        return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets the total quantity of a specific item in the inventory.
        /// </summary>
        public int GetItemCount(ItemData itemToCount)
        {
            int total = 0;
            
            foreach (InventorySlot slot in items)
            {
                if (slot.itemData == itemToCount)
                    total += slot.quantity;
            }
            
            return total;
        }
        
        /// <summary>
        /// Returns a list of all inventory slots (for UI display).
        /// Note: Returns a COPY to prevent external modification.
        /// </summary>
        public List<InventorySlot> GetAllSlots()
        {
            return new List<InventorySlot>(items);
        }
        
        /// <summary>
        /// Clears the entire inventory.
        /// </summary>
        public void ClearInventory()
        {
            items.Clear();
            OnInventoryChanged?.Invoke();
            
            if (logInventoryChanges)
                Debug.Log("[PlayerInventory] Inventory cleared.");
        }
        
        #endregion
        
        #region Private Helper Methods
        
        /// <summary>
        /// Attempts to stack items with existing slots.
        /// </summary>
        /// <returns>The quantity that couldn't be stacked.</returns>
        private int TryStackExistingItems(ItemData itemToAdd, int quantity)
        {
            int remaining = quantity;
            
            foreach (InventorySlot slot in items)
            {
                if (slot.itemData == itemToAdd && slot.quantity < maxStackSize)
                {
                    int spaceInSlot = maxStackSize - slot.quantity;
                    int amountToAdd = Mathf.Min(spaceInSlot, remaining);
                    
                    slot.quantity += amountToAdd;
                    remaining -= amountToAdd;
                    
                    if (remaining <= 0)
                        break;
                }
            }
            
            return remaining;
        }
        
        /// <summary>
        /// Creates new inventory slots for items.
        /// </summary>
        private bool AddNewSlots(ItemData itemToAdd, int quantity)
        {
            int itemsAdded = 0;
            
            while (quantity > 0 && !IsFull)
            {
                int amountForThisSlot = Mathf.Min(quantity, 
                    allowStacking && itemToAdd.isStackable ? maxStackSize : 1);
                
                InventorySlot newSlot = new InventorySlot(itemToAdd, amountForThisSlot);
                items.Add(newSlot);
                
                quantity -= amountForThisSlot;
                itemsAdded += amountForThisSlot;
            }
            
            if (itemsAdded > 0)
            {
                OnInventoryChanged?.Invoke();
                
                if (logInventoryChanges)
                    Debug.Log($"[PlayerInventory] Added {itemsAdded}x {itemToAdd.itemName} (new slots)");
                
                return true;
            }
            
            // Inventory was full
            if (logInventoryChanges)
                Debug.LogWarning($"[PlayerInventory] Cannot add {itemToAdd.itemName}. Inventory full!");
            
            return false;
        }
        
        #endregion
        
        #region Unity Editor Helpers
        
        #if UNITY_EDITOR
        [ContextMenu("Debug/Print Inventory Contents")]
        private void PrintInventoryContents()
        {
            Debug.Log($"=== INVENTORY CONTENTS ({items.Count}/{maxSlots} slots) ===");
            
            if (items.Count == 0)
            {
                Debug.Log("Inventory is empty.");
                return;
            }
            
            for (int i = 0; i < items.Count; i++)
            {
                Debug.Log($"Slot {i}: {items[i].quantity}x {items[i].itemData.itemName}");
            }
        }
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// Represents a single slot in the inventory.
    /// Can hold one type of item with a specific quantity.
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData itemData;
        public int quantity;
        
        public InventorySlot(ItemData data, int qty = 1)
        {
            itemData = data;
            quantity = qty;
        }
        
        public bool IsEmpty => itemData == null || quantity <= 0;
        public bool CanStack => itemData != null && itemData.isStackable && quantity < 99;
    }
}