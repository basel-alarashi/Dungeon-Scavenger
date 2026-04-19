using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace DungeonScavenger.Core
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        #endregion

        #region Game State

        public enum GameState
        {
            Playing,
            Paused,
            GameOver,
            Loading
        }

        public event Action<GameState, GameState> OnGameStateChanged;
        public event Action OnPlayerDied;
        public event Action OnGamePaused;
        public event Action OnGameResumed;

        private GameState currentState = GameState.Playing;
        private GameState previousState;
        private bool isPlayerDead = false;  // PREVENTS MULTIPLE DEATH EVENTS
        private bool isRespawning = false;   // PREVENTS RESPAWN LOOPS

        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsGameOver => currentState == GameState.GameOver;

        #endregion

        #region Inspector Fields

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string gameScene = "MainScene";

        [Header("Death Settings")]
        [SerializeField] private float gameOverDelay = 2f;
        [SerializeField] private bool autoRespawn = false; // Changed to false - let player choose

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = true;

        #endregion

        #region Initialization

        private void Start()
        {
            // Reset death flags on scene load
            isPlayerDead = false;
            isRespawning = false;

            // Find player and subscribe
            Player.PlayerStats playerStats = FindAnyObjectByType<Player.PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnPlayerDied += HandlePlayerDeath;
                Debug.Log("[GameManager] Subscribed to player death event");
            }
            else
            {
                Debug.LogWarning("[GameManager] PlayerStats not found in scene!");
            }

            SetState(GameState.Playing);
        }

        #endregion

        #region State Management

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            previousState = currentState;
            currentState = newState;

            if (logStateChanges)
                Debug.Log($"[GameManager] State: {previousState} → {currentState}");

            switch (currentState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    OnGameResumed?.Invoke();
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    OnGamePaused?.Invoke();
                    break;

                case GameState.GameOver:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }

            OnGameStateChanged?.Invoke(previousState, currentState);
        }

        #endregion

        #region Player Death

        private void HandlePlayerDeath()
        {
            // CRITICAL: Prevent multiple death events
            if (isPlayerDead || isRespawning)
            {
                Debug.LogWarning("[GameManager] Player death already processed, ignoring duplicate event");
                return;
            }

            isPlayerDead = true;
            Debug.Log("[GameManager] Player died! Showing Game Over screen.");

            SetState(GameState.GameOver);
            OnPlayerDied?.Invoke();

            // DO NOT auto-respawn - let player click Restart
        }

        public void RestartGame()
        {
            if (isRespawning) return;

            isRespawning = true;
            Debug.Log("[GameManager] Restarting game...");

            SetState(GameState.Loading);

            // Reload the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void RespawnPlayer()
        {
            RestartGame();
        }

        #endregion

        #region Pause Control

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
                SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
                SetState(GameState.Playing);
        }

        public void TogglePause()
        {
            if (IsPaused)
                ResumeGame();
            else if (IsPlaying)
                PauseGame();
        }

        #endregion

        #region Scene Management

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }

        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Update

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsPlaying)
                    PauseGame();
                else if (IsPaused)
                    ResumeGame();
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            Player.PlayerStats playerStats = FindAnyObjectByType<Player.PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnPlayerDied -= HandlePlayerDeath;
            }
        }

        #endregion
    }
}