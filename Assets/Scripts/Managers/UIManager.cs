using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonScavenger.Core;

namespace DungeonScavenger.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject saveLoadPanel;

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI ammoText;

        private void Start()
        {
            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                GameManager.Instance.OnPlayerDied += ShowGameOver;
            }

            // Start with HUD only
            ShowHUDOnly();
        }

        private void OnGameStateChanged(GameManager.GameState oldState, GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.Playing:
                    ShowHUDOnly();
                    break;
                case GameManager.GameState.Paused:
                    ShowPauseMenu();
                    break;
                case GameManager.GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }

        private void ShowHUDOnly()
        {
            hudPanel.SetActive(true);
            pausePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            saveLoadPanel.SetActive(false);
        }

        private void ShowPauseMenu()
        {
            hudPanel.SetActive(false);
            pausePanel.SetActive(true);
            gameOverPanel.SetActive(false);
            saveLoadPanel.SetActive(false);
        }

        private void ShowGameOver()
        {
            hudPanel.SetActive(false);
            pausePanel.SetActive(false);
            gameOverPanel.SetActive(true);
            saveLoadPanel.SetActive(false);
        }

        // Button handlers
        public void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        public void OnRestartClicked()
        {
            GameManager.Instance?.RestartGame();
        }

        public void OnMainMenuClicked()
        {
            GameManager.Instance?.LoadMainMenu();
        }

        public void OnQuitClicked()
        {
            GameManager.Instance?.QuitGame();
        }

        public void OnSaveClicked()
        {
            SaveManager.Instance?.SaveGame(0);
        }

        public void OnLoadClicked()
        {
            SaveManager.Instance?.LoadGame(0);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
                GameManager.Instance.OnPlayerDied -= ShowGameOver;
            }
        }
    }
}