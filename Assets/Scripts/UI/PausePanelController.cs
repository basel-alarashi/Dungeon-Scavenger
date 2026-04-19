using UnityEngine;
using UnityEngine.UI;
using DungeonScavenger.Core;

namespace DungeonScavenger.UI
{
    public class PausePanelController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject pausePanel;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Sub-Panels")]
        [SerializeField] private SaveLoadPanel saveLoadPanel;
        [SerializeField] private SettingsUI settingsPanel;

        private void Start()
        {
            // CRITICAL: Clear existing listeners first
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.RemoveAllListeners();
                saveButton.onClick.AddListener(OnSaveClicked);
            }

            if (loadButton != null)
            {
                loadButton.onClick.RemoveAllListeners();
                loadButton.onClick.AddListener(OnLoadClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            // Subscribe to game state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused += Show;
                GameManager.Instance.OnGameResumed += Hide;
            }

            pausePanel.SetActive(false);
            Debug.Log("[PausePanel] Initialized. Buttons wired.");
        }

        private void Show()
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
            Debug.Log("[PausePanel] Shown");
        }

        private void Hide()
        {
            pausePanel.SetActive(false);
            Debug.Log("[PausePanel] Hidden");
        }

        private void OnResumeClicked()
        {
            Debug.Log("[PausePanel] Resume clicked");
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.ResumeGame();
        }

        private void OnSaveClicked()
        {
            Debug.Log("[PausePanel] Save clicked");
            AudioManager.Instance?.PlayButtonClick();
            saveLoadPanel?.Show(true);
            Hide();
        }

        private void OnLoadClicked()
        {
            Debug.Log("[PausePanel] Load clicked");
            AudioManager.Instance?.PlayButtonClick();
            saveLoadPanel?.Show(false);
            Hide();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[PausePanel] Settings clicked");
            AudioManager.Instance?.PlayButtonClick();
            settingsPanel?.Show();
            Hide();
        }

        private void OnQuitClicked()
        {
            Debug.Log("[PausePanel] Quit clicked");
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.LoadMainMenu();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused -= Show;
                GameManager.Instance.OnGameResumed -= Hide;
            }
        }
    }
}