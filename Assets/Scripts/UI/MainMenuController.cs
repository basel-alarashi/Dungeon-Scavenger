using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DungeonScavenger.Core;

namespace DungeonScavenger.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Panels")]
        [SerializeField] private SaveLoadPanel saveLoadPanel;
        [SerializeField] private SettingsUI settingsPanel;

        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "MainScene";

        private void Start()
        {
            // Wire buttons
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(OnLoadGameClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Ensure panels start hidden
            // if (saveLoadPanel != null)
            //     saveLoadPanel.Hide();

            // if (settingsPanel != null)
            //     settingsPanel.Hide();

            Debug.Log("[MainMenuController] Initialized. All panels hidden.");
        }

        private void OnNewGameClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[MainMenuController] Starting new game...");
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnLoadGameClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[MainMenuController] Opening load panel...");

            if (saveLoadPanel != null)
            {
                // Show in load mode
                saveLoadPanel.Show(false, (success) =>
                {
                    if (success)
                    {
                        Debug.Log("[MainMenuController] Game loaded successfully!");
                        // The SaveLoadPanel will handle scene transition
                        // or you can load the game scene here
                    }
                    else
                    {
                        Debug.Log("[MainMenuController] Load cancelled.");
                    }
                });
            }
            else
            {
                Debug.LogError("[MainMenuController] SaveLoadPanel is not assigned!");
            }
        }

        private void OnSettingsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[MainMenuController] Opening settings panel...");

            if (settingsPanel != null)
            {
                settingsPanel.Show();
            }
            else
            {
                Debug.LogError("[MainMenuController] SettingsPanel is not assigned!");
            }
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[MainMenuController] Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (newGameButton != null)
                newGameButton.onClick.RemoveListener(OnNewGameClicked);

            if (loadGameButton != null)
                loadGameButton.onClick.RemoveListener(OnLoadGameClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}