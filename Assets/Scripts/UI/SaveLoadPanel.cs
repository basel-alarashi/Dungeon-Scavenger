using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DungeonScavenger.Core;

namespace DungeonScavenger.UI
{
    /// <summary>
    /// Manages the save/load panel UI.
    /// </summary>
    public class SaveLoadPanel : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Panel References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Mode Toggles")]
        [SerializeField] private Toggle saveModeToggle;
        [SerializeField] private Toggle loadModeToggle;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;

        [Header("Settings")]
        [SerializeField] private int maxSlots = 3;
        // [SerializeField] private bool showConfirmation = true;

        #endregion

        #region Private Data

        private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();
        private bool isSaveMode = true;
        private int selectedSlotIndex = -1;
        private System.Action<bool> onClose; // true = action performed, false = cancelled

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Create slots
            CreateSlots();

            // Wire mode toggles
            if (saveModeToggle != null)
                saveModeToggle.onValueChanged.AddListener(OnSaveModeToggled);

            if (loadModeToggle != null)
                loadModeToggle.onValueChanged.AddListener(OnLoadModeToggled);

            // Wire buttons
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            // Start in save mode
            SetMode(true);

            // Hide initially
            panelRoot.SetActive(false);
        }

        #endregion

        #region Slot Creation

        private void CreateSlots()
        {
            // Clear existing
            foreach (Transform child in slotContainer)
                Destroy(child.gameObject);

            slotUIs.Clear();

            // Create new slots
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotContainer);
                slotObj.name = $"SaveSlot_{i}";

                SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
                if (slotUI == null)
                    slotUI = slotObj.AddComponent<SaveSlotUI>();

                slotUI.Initialize(i, OnSlotClicked, OnDeleteClicked);
                slotUIs.Add(slotUI);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the panel in save or load mode.
        /// </summary>
        public void Show(bool saveMode, System.Action<bool> closeCallback = null)
        {
            onClose = closeCallback;
            SetMode(saveMode);
            RefreshAllSlots();
            DeselectAllSlots();

            panelRoot.SetActive(true);

            // Play open sound
            AudioManager.Instance?.PlayInventoryOpen();
        }

        /// <summary>
        /// Hides the panel.
        /// </summary>
        public void Hide(bool actionPerformed = false)
        {
            panelRoot.SetActive(false);
            onClose?.Invoke(actionPerformed);

            // Play close sound
            AudioManager.Instance?.PlayInventoryClose();
        }

        /// <summary>
        /// Refreshes all slot displays.
        /// </summary>
        public void RefreshAllSlots()
        {
            foreach (var slot in slotUIs)
            {
                slot.RefreshSaveData();
            }

            UpdateConfirmButton();
        }

        #endregion

        #region Mode Management

        public void SetMode(bool saveMode)
        {
            isSaveMode = saveMode;

            // Update title
            if (titleText != null)
                titleText.text = isSaveMode ? "SAVE GAME" : "LOAD GAME";

            // Update confirm button text
            if (confirmButtonText != null)
                confirmButtonText.text = isSaveMode ? "SAVE" : "LOAD";

            // Update toggles without triggering events
            if (saveModeToggle != null)
            {
                saveModeToggle.SetIsOnWithoutNotify(isSaveMode);
            }

            if (loadModeToggle != null)
            {
                loadModeToggle.SetIsOnWithoutNotify(!isSaveMode);
            }

            // Update slots
            foreach (var slot in slotUIs)
            {
                slot.SetMode(isSaveMode);
            }

            DeselectAllSlots();
            UpdateConfirmButton();

            Debug.Log($"[SaveLoadPanel] Mode set to: {(isSaveMode ? "SAVE" : "LOAD")}");
        }

        // Fix toggle handlers
        private void OnSaveModeToggled(bool isOn)
        {
            if (isOn)
            {
                SetMode(true);
                Debug.Log("[SaveLoadPanel] Switched to SAVE mode");
            }
        }

        private void OnLoadModeToggled(bool isOn)
        {
            if (isOn)
            {
                SetMode(false);
                Debug.Log("[SaveLoadPanel] Switched to LOAD mode");
            }
        }

        #endregion

        #region Slot Selection

        private void OnSlotClicked(int slotIndex, bool slotSaveMode)
        {
            // Play click sound
            AudioManager.Instance?.PlayButtonClick();

            // Select the clicked slot
            SelectSlot(slotIndex);
        }

        private void SelectSlot(int slotIndex)
        {
            // Deselect previous
            if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
            {
                slotUIs[selectedSlotIndex].SetSelected(false);
            }

            // Select new
            selectedSlotIndex = slotIndex;

            if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
            {
                slotUIs[selectedSlotIndex].SetSelected(true);
            }

            UpdateConfirmButton();
        }

        private void DeselectAllSlots()
        {
            selectedSlotIndex = -1;

            foreach (var slot in slotUIs)
            {
                slot.SetSelected(false);
            }

            UpdateConfirmButton();
        }

        #endregion

        #region Delete Handling

        private void OnDeleteClicked(int slotIndex)
        {
            // Confirm deletion
            if (SaveManager.Instance != null)
            {
                bool deleted = SaveManager.Instance.DeleteSave(slotIndex);

                if (deleted)
                {
                    RefreshAllSlots();

                    if (selectedSlotIndex == slotIndex)
                        DeselectAllSlots();

                    Debug.Log($"[SaveLoadPanel] Deleted save in slot {slotIndex}");
                }
            }
        }

        #endregion

        #region Confirm Action

        private void UpdateConfirmButton()
        {
            if (confirmButton == null) return;

            bool canConfirm = false;

            if (isSaveMode)
            {
                // Save mode: always can confirm if a slot is selected
                canConfirm = selectedSlotIndex >= 0;
            }
            else
            {
                // Load mode: can only confirm if selected slot has data
                canConfirm = selectedSlotIndex >= 0 &&
                             selectedSlotIndex < slotUIs.Count &&
                             slotUIs[selectedSlotIndex].HasSaveData;
            }

            confirmButton.interactable = canConfirm;
        }

        private void OnConfirmClicked()
        {
            if (selectedSlotIndex < 0) return;

            // Play click sound
            AudioManager.Instance?.PlayButtonClick();

            bool success = false;

            if (isSaveMode)
            {
                success = SaveManager.Instance?.SaveGame(selectedSlotIndex) ?? false;
                Debug.Log($"[SaveLoadPanel] Saved game to slot {selectedSlotIndex}");
            }
            else
            {
                success = SaveManager.Instance?.LoadGame(selectedSlotIndex) ?? false;
                Debug.Log($"[SaveLoadPanel] Loaded game from slot {selectedSlotIndex}");
            }

            if (success)
            {
                RefreshAllSlots();
                DeselectAllSlots();

                // Hide panel if successful
                Hide(true);
            }
        }

        #endregion

        #region Button Handlers

        private void OnCloseClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Hide(false);
        }

        #endregion
    }
}