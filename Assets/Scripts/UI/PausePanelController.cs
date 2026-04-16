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
            // Wire buttons
            resumeButton.onClick.AddListener(OnResumeClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
            loadButton.onClick.AddListener(OnLoadClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            quitButton.onClick.AddListener(OnQuitClicked);

            // Subscribe to game state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused += Show;
                GameManager.Instance.OnGameResumed += Hide;
            }

            pausePanel.SetActive(false);
        }

        private void OnResumeClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.ResumeGame();
        }

        private void OnSaveClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            saveLoadPanel?.Show(true);
        }

        private void OnLoadClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            saveLoadPanel?.Show(false);
        }

        private void OnSettingsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            settingsPanel?.Show();
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.LoadMainMenu();
        }

        private void Show()
        {
            pausePanel.SetActive(true);
        }

        private void Hide()
        {
            pausePanel.SetActive(false);
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