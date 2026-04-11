using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DungeonScavenger.Inventory;
using TMPro;

namespace DungeonScavenger.UI
{
    /// <summary>
    /// Manages the inventory UI panel.
    /// Subscribes to PlayerInventory events and updates the display.
    /// Follows the Singleton pattern for easy access.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        #region Singleton

        public static InventoryUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        #endregion

        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI slotCountText;
        [SerializeField] private Image backgroundDim;

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private bool lockCursorWhenOpen = true;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip itemAddedSound;

        [Header("Animation")]
        [SerializeField] private float animationSpeed = 8f;
        [SerializeField] private CanvasGroup canvasGroup;

        #endregion

        #region Private Data

        private PlayerInventory playerInventory;
        private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
        private int selectedSlotIndex = -1;
        private bool isVisible;

        #endregion

        #region Properties

        public bool IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                inventoryPanel.SetActive(isVisible);
                UpdateCursorState();

                if (backgroundDim != null)
                    backgroundDim.gameObject.SetActive(isVisible);

                if (isVisible)
                    RefreshDisplay();
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Find player inventory
            playerInventory = FindAnyObjectByType<PlayerInventory>();

            if (playerInventory == null)
            {
                Debug.LogError("[InventoryUI] No PlayerInventory found in scene!");
                return;
            }

            // Subscribe to inventory changes
            playerInventory.OnInventoryChanged += RefreshDisplay;

            // Setup close button
            if (closeButton != null)
                closeButton.onClick.AddListener(() => IsVisible = false);

            // Initialize slots
            CreateSlots();

            // Set initial visibility
            IsVisible = showOnStart;
        }

        private void Update()
        {
            // Toggle inventory with key press
            if (Input.GetKeyDown(toggleKey))
            {
                IsVisible = !IsVisible;
            }

            // Animate panel alpha
            if (canvasGroup != null)
            {
                float targetAlpha = isVisible ? 1f : 0f;
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * animationSpeed);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (playerInventory != null)
                playerInventory.OnInventoryChanged -= RefreshDisplay;
        }

        #endregion

        #region Slot Management

        /// <summary>
        /// Creates the UI slot objects based on max inventory size.
        /// </summary>
        private void CreateSlots()
        {
            if (slotPrefab == null || slotContainer == null)
            {
                Debug.LogError("[InventoryUI] Missing slot prefab or container!");
                return;
            }

            // Clear existing slots
            foreach (Transform child in slotContainer)
                Destroy(child.gameObject);

            slotUIs.Clear();

            // Create slots up to max inventory size
            for (int i = 0; i < playerInventory.MaxSlots; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotContainer);
                slotObj.name = $"Slot_{i:00}";

                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI == null)
                    slotUI = slotObj.AddComponent<InventorySlotUI>();

                // Add click handler
                Button button = slotObj.GetComponent<Button>();
                if (button == null)
                    button = slotObj.AddComponent<Button>();

                int index = i; // Capture for lambda
                button.onClick.AddListener(() => OnSlotClicked(index));

                slotUIs.Add(slotUI);
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Refreshes all slot displays with current inventory data.
        /// </summary>
        public void RefreshDisplay()
        {
            if (playerInventory == null) return;

            List<InventorySlot> inventorySlots = playerInventory.GetAllSlots();

            // Update each UI slot
            for (int i = 0; i < slotUIs.Count; i++)
            {
                if (i < inventorySlots.Count)
                {
                    slotUIs[i].SetSlotData(inventorySlots[i], i);
                }
                else
                {
                    slotUIs[i].ClearSlot();
                }
            }

            // Update slot count text
            if (slotCountText != null)
            {
                slotCountText.text = $"{inventorySlots.Count}/{playerInventory.MaxSlots}";
            }

            // Restore selection
            if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
            {
                slotUIs[selectedSlotIndex].SetSelected(true);
            }
        }

        #endregion

        #region Slot Interaction

        /// <summary>
        /// Handles slot click events.
        /// </summary>
        private void OnSlotClicked(int index)
        {
            SelectSlot(index);
        }

        /// <summary>
        /// Selects a specific inventory slot.
        /// </summary>
        public void SelectSlot(int index)
        {
            // Deselect previous
            if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
            {
                slotUIs[selectedSlotIndex].SetSelected(false);
            }

            // Select new
            selectedSlotIndex = index;

            if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
            {
                slotUIs[selectedSlotIndex].SetSelected(true);

                // Log item info
                InventorySlot slot = slotUIs[selectedSlotIndex].SlotData;
                if (slot != null && !slot.IsEmpty)
                {
                    Debug.Log($"[InventoryUI] Selected: {slot.quantity}x {slot.itemData.itemName}");
                }
            }
        }

        /// <summary>
        /// Deselects the current slot.
        /// </summary>
        public void DeselectSlot()
        {
            if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
            {
                slotUIs[selectedSlotIndex].SetSelected(false);
            }
            selectedSlotIndex = -1;
        }

        #endregion

        #region Cursor Management

        /// <summary>
        /// Updates cursor visibility and lock state based on inventory visibility.
        /// </summary>
        private void UpdateCursorState()
        {
            if (lockCursorWhenOpen)
            {
                if (isVisible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the inventory UI.
        /// </summary>
        public void Show()
        {
            IsVisible = true;
        }

        /// <summary>
        /// Hides the inventory UI.
        /// </summary>
        public void Hide()
        {
            IsVisible = false;
        }

        /// <summary>
        /// Toggles inventory visibility.
        /// </summary>
        public void Toggle()
        {
            IsVisible = !IsVisible;
        }

        #endregion
    }
}