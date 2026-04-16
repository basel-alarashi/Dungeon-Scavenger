using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonScavenger.Core;

namespace DungeonScavenger.UI
{
    /// <summary>
    /// Represents a single save slot in the save/load menu.
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        #region UI References

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI slotNumberText;
        [SerializeField] private TextMeshProUGUI saveInfoText;
        [SerializeField] private GameObject emptyTextObject;
        [SerializeField] private Button actionButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.16f, 0.16f, 0.16f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color hasSaveColor = new Color(0.2f, 0.25f, 0.2f);

        #endregion

        #region Private Data

        private int slotIndex;
        private bool isSaveMode = true; // true = save mode, false = load mode
        private bool hasSaveData;
        private SaveMetadata metadata;
        private System.Action<int, bool> onSlotClicked; // slotIndex, isSaveMode
        private System.Action<int> onDeleteClicked;

        #endregion

        #region Properties

        public int SlotIndex => slotIndex;
        public bool HasSaveData => hasSaveData;
        public bool IsSelected { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the save slot.
        /// </summary>
        public void Initialize(int index, System.Action<int, bool> clickCallback, System.Action<int> deleteCallback)
        {
            slotIndex = index;
            onSlotClicked = clickCallback;
            onDeleteClicked = deleteCallback;

            // Set slot number
            if (slotNumberText != null)
                slotNumberText.text = $"SLOT {index + 1}";

            // Wire buttons
            if (actionButton != null)
                actionButton.onClick.AddListener(OnSlotClicked);

            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnDeleteClicked);

            // Check for existing save data
            RefreshSaveData();
        }

        #endregion

        #region Refresh & Update

        /// <summary>
        /// Refreshes the slot display based on current save data.
        /// </summary>
        public void RefreshSaveData()
        {
            if (SaveManager.Instance == null) return;

            metadata = SaveManager.Instance.GetSaveMetadata(slotIndex);
            hasSaveData = metadata != null;

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the visual display based on save data and mode.
        /// </summary>
        private void UpdateDisplay()
        {
            if (hasSaveData && metadata != null)
            {
                // Show save info
                if (saveInfoText != null)
                {
                    saveInfoText.text = metadata.GetDisplayText();
                    saveInfoText.gameObject.SetActive(true);
                }

                if (emptyTextObject != null)
                    emptyTextObject.SetActive(false);

                // Enable delete button
                if (deleteButton != null)
                    deleteButton.gameObject.SetActive(true);

                // Update background color
                if (backgroundImage != null)
                    backgroundImage.color = hasSaveColor;
            }
            else
            {
                // Show empty state
                if (saveInfoText != null)
                    saveInfoText.gameObject.SetActive(false);

                if (emptyTextObject != null)
                    emptyTextObject.SetActive(true);

                // Disable delete button
                if (deleteButton != null)
                    deleteButton.gameObject.SetActive(false);

                // Reset background color
                if (backgroundImage != null)
                    backgroundImage.color = normalColor;
            }

            // Update button interactability based on mode
            UpdateButtonState();
        }

        /// <summary>
        /// Updates button states based on current mode.
        /// </summary>
        private void UpdateButtonState()
        {
            if (actionButton == null) return;

            if (isSaveMode)
            {
                // Save mode: always clickable (will overwrite if has data)
                actionButton.interactable = true;
            }
            else
            {
                // Load mode: only clickable if has save data
                actionButton.interactable = hasSaveData;
            }
        }

        #endregion

        #region Mode Switching

        /// <summary>
        /// Sets whether the slot is in save or load mode.
        /// </summary>
        public void SetMode(bool saveMode)
        {
            isSaveMode = saveMode;
            UpdateButtonState();
        }

        #endregion

        #region Selection

        /// <summary>
        /// Sets the selection state of this slot.
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (highlightImage != null)
                highlightImage.gameObject.SetActive(selected);

            if (backgroundImage != null && !hasSaveData)
                backgroundImage.color = selected ? selectedColor : normalColor;
        }

        #endregion

        #region Button Handlers

        private void OnSlotClicked()
        {
            // Play click sound
            AudioManager.Instance?.PlayButtonClick();

            // Notify parent
            onSlotClicked?.Invoke(slotIndex, isSaveMode);

            Debug.Log($"[SaveSlotUI] Slot {slotIndex} clicked. Mode: {(isSaveMode ? "Save" : "Load")}");
        }

        private void OnDeleteClicked()
        {
            // Play click sound
            AudioManager.Instance?.PlayButtonClick();

            // Confirm deletion (simple version - could add confirmation dialog)
            if (hasSaveData)
            {
                onDeleteClicked?.Invoke(slotIndex);
                Debug.Log($"[SaveSlotUI] Delete requested for slot {slotIndex}");
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (actionButton != null)
                actionButton.onClick.RemoveListener(OnSlotClicked);

            if (deleteButton != null)
                deleteButton.onClick.RemoveListener(OnDeleteClicked);
        }

        #endregion
    }
}