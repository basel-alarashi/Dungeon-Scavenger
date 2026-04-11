using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonScavenger.Inventory;

namespace DungeonScavenger.UI
{
    /// <summary>
    /// Represents a single slot in the inventory UI.
    /// Displays item icon, quantity, and handles selection states.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        #region UI References
        
        [Header("UI Elements")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image highlight;
        [SerializeField] private GameObject emptyIcon;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.16f, 0.16f, 0.16f); // #2A2A2A
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.3f, 0.3f);  // #4D4D4D
        [SerializeField] private Color emptyColor = new Color(0.22f, 0.22f, 0.22f); // #383838
        
        #endregion
        
        #region Private Data
        
        private Image backgroundImage;
        private InventorySlot slotData;
        private bool isSelected;
        private int slotIndex;
        
        #endregion
        
        #region Public Properties
        
        public InventorySlot SlotData => slotData;
        public bool IsEmpty => slotData == null || slotData.IsEmpty;
        public int SlotIndex => slotIndex;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
                backgroundImage = gameObject.AddComponent<Image>();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initializes or updates the slot with inventory data.
        /// </summary>
        public void SetSlotData(InventorySlot data, int index)
        {
            slotData = data;
            slotIndex = index;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Clears the slot (called when item is removed).
        /// </summary>
        public void ClearSlot()
        {
            slotData = null;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Updates the visual representation based on current slot data.
        /// </summary>
        public void UpdateDisplay()
        {
            if (IsEmpty)
            {
                // Empty slot display
                if (itemIcon != null)
                {
                    itemIcon.sprite = null;
                    itemIcon.color = Color.clear;
                }
                
                if (quantityText != null)
                    quantityText.text = "";
                
                if (emptyIcon != null)
                    emptyIcon.SetActive(true);
                
                if (backgroundImage != null)
                    backgroundImage.color = emptyColor;
            }
            else
            {
                // Filled slot display
                if (itemIcon != null)
                {
                    itemIcon.sprite = slotData.itemData.icon;
                    itemIcon.color = slotData.itemData.itemColor;
                    
                    // If no icon, show a colored square with the item color
                    if (slotData.itemData.icon == null)
                    {
                        itemIcon.color = slotData.itemData.itemColor;
                    }
                }
                
                if (quantityText != null)
                {
                    // Only show quantity if stackable and quantity > 1
                    if (slotData.itemData.isStackable && slotData.quantity > 1)
                        quantityText.text = slotData.quantity.ToString();
                    else
                        quantityText.text = "";
                }
                
                if (emptyIcon != null)
                    emptyIcon.SetActive(false);
                
                if (backgroundImage != null)
                    backgroundImage.color = isSelected ? selectedColor : normalColor;
            }
        }
        
        /// <summary>
        /// Sets the selection state of this slot.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (highlight != null)
                highlight.gameObject.SetActive(selected);
            
            if (backgroundImage != null && !IsEmpty)
                backgroundImage.color = selected ? selectedColor : normalColor;
        }
        
        /// <summary>
        /// Called when the slot is clicked.
        /// </summary>
        public void OnSlotClicked()
        {
            if (!IsEmpty)
            {
                Debug.Log($"[InventorySlotUI] Clicked slot {slotIndex}: {slotData.quantity}x {slotData.itemData.itemName}");
                
                // TODO: Add item usage/equipment logic here
                // For now, just select the slot
                InventoryUI.Instance?.SelectSlot(slotIndex);
            }
        }
        
        #endregion
        
        #region Editor Helpers
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (itemIcon == null)
                itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            
            if (quantityText == null)
                quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            
            if (highlight == null)
                highlight = transform.Find("Highlight")?.GetComponent<Image>();
            
            if (emptyIcon == null)
                emptyIcon = transform.Find("EmptyIcon")?.gameObject;
        }
        #endif
        
        #endregion
    }
}