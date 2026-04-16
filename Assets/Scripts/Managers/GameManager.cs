using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace DungeonScavenger.Core
{
    /// <summary>
    /// Manages overall game state, player death, pausing, and scene transitions.
    /// </summary>
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
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Game State Enum

        public enum GameState
        {
            Playing,
            Paused,
            GameOver,
            Victory,
            Loading
        }

        #endregion

        #region Events

        public event Action<GameState, GameState> OnGameStateChanged; // oldState, newState
        public event Action OnPlayerDied;
        public event Action OnPlayerRespawned;
        public event Action OnGamePaused;
        public event Action OnGameResumed;

        #endregion

        #region Inspector Fields

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string gameScene = "MainScene";

        [Header("Player Settings")]
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField] private bool autoRespawn = true;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = true;

        #endregion

        #region Private Data

        private GameState currentState = GameState.Playing;
        private GameState previousState;
        private float timeScaleBeforePause = 1f;

        #endregion

        #region Properties

        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsGameOver => currentState == GameState.GameOver;
        public bool IsVictory => currentState == GameState.Victory;

        #endregion

        #region Initialization

        private void Initialize()
        {
            Debug.Log("[GameManager] Initialized");
        }

        private void Start()
        {
            // Find player reference
            Player.PlayerStats playerStats = FindAnyObjectByType<Player.PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnPlayerDied += HandlePlayerDeath;
            }
        }

        #endregion

        #region State Management

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            previousState = currentState;
            currentState = newState;

            if (logStateChanges)
                Debug.Log($"[GameManager] State changed: {previousState} → {currentState}");

            // Handle state entry
            switch (currentState)
            {
                case GameState.Playing:
                    HandlePlayingState();
                    break;
                case GameState.Paused:
                    HandlePausedState();
                    break;
                case GameState.GameOver:
                    HandleGameOverState();
                    break;
                case GameState.Victory:
                    HandleVictoryState();
                    break;
            }

            OnGameStateChanged?.Invoke(previousState, currentState);
        }

        private void HandlePlayingState()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            OnGameResumed?.Invoke();
        }

        private void HandlePausedState()
        {
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnGamePaused?.Invoke();
        }

        private void HandleGameOverState()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleVictoryState()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        #endregion

        #region Player Death & Respawning

        private void HandlePlayerDeath()
        {
            Debug.Log("[GameManager] Player died!");
            OnPlayerDied?.Invoke();

            SetState(GameState.GameOver);

            if (autoRespawn)
            {
                Invoke(nameof(RespawnPlayer), respawnDelay);
            }
        }

        public void RespawnPlayer()
        {
            Debug.Log("[GameManager] Respawning player...");

            // Reload the scene (simplest respawn method)
            RestartGame();

            OnPlayerRespawned?.Invoke();
        }

        #endregion

        #region Pause Control

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
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

        public void RestartGame()
        {
            SetState(GameState.Loading);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            SetState(GameState.Loading);
            SceneManager.LoadScene(mainMenuScene);
        }

        public void LoadGameScene()
        {
            SetState(GameState.Loading);
            SceneManager.LoadScene(gameScene);
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

        #region Victory

        public void TriggerVictory()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Victory);
            }
        }

        #endregion

        #region Update

        private void Update()
        {
            // Toggle pause with Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsPlaying)
                    PauseGame();
                else if (IsPaused)
                    ResumeGame();
            }
        }

        #endregion
    }
}