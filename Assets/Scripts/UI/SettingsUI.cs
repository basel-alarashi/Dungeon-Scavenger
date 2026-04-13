using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonScavenger.Core;

namespace DungeonScavenger.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private TextMeshProUGUI masterValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;
        [SerializeField] private TextMeshProUGUI musicValueText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button openButton;

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        private void Start()
        {
            // Load current volumes
            LoadVolumeSettings();

            // Wire sliders
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            // Wire buttons
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (openButton != null)
                openButton.onClick.AddListener(Show);

            // Start hidden
            Hide();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                Toggle();
            }
        }

        private void LoadVolumeSettings()
        {
            if (AudioManager.Instance == null) return;

            masterSlider.value = AudioManager.Instance.MasterVolume;
            sfxSlider.value = AudioManager.Instance.SFXVolume;
            musicSlider.value = AudioManager.Instance.MusicVolume;

            UpdateValueTexts();
        }

        private void OnMasterVolumeChanged(float value)
        {
            AudioManager.Instance?.SetMasterVolume(value);
            UpdateValueTexts();
        }

        private void OnSFXVolumeChanged(float value)
        {
            AudioManager.Instance?.SetSFXVolume(value);
            UpdateValueTexts();
        }

        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance?.SetMusicVolumeMixer(value);
            UpdateValueTexts();
        }

        private void UpdateValueTexts()
        {
            if (masterValueText != null)
                masterValueText.text = $"{(masterSlider.value * 100):F0}%";

            if (sfxValueText != null)
                sfxValueText.text = $"{(sfxSlider.value * 100):F0}%";

            if (musicValueText != null)
                musicValueText.text = $"{(musicSlider.value * 100):F0}%";
        }

        public void Show()
        {
            Debug.Log("[SettingsUI] Show called");

            if (settingsPanel == null)
            {
                Debug.LogError("[SettingsUI] Settings Panel is not assigned!");
                return;
            }

            settingsPanel.SetActive(true);

            // CRITICAL FIX: Bring to front
            settingsPanel.transform.SetAsLastSibling();

            // Also ensure it's the topmost in canvas
            Canvas canvas = settingsPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                settingsPanel.transform.SetParent(canvas.transform);
                settingsPanel.transform.SetAsLastSibling();
            }

            // Make sure it blocks raycasts
            CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = settingsPanel.AddComponent<CanvasGroup>();
            }
            cg.blocksRaycasts = true;
            cg.interactable = true;

            // Unlock and show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            AudioManager.Instance?.PlayInventoryOpen();
        }

        public void Hide()
        {
            settingsPanel.SetActive(false);
            AudioManager.Instance?.SaveVolumeSettings();
            AudioManager.Instance?.PlayInventoryClose();
        }

        public void Toggle()
        {
            if (settingsPanel.activeSelf)
                Hide();
            else
                Show();
        }
    }
}