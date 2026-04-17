using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonScavenger.Core;
using DungeonScavenger.Inventory;

namespace DungeonScavenger.UI
{
    public class GameOverPanelController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject gameOverPanel;

        [Header("Stats Text")]
        [SerializeField] private TextMeshProUGUI enemiesKilledText;
        [SerializeField] private TextMeshProUGUI itemsCollectedText;
        [SerializeField] private TextMeshProUGUI survivalTimeText;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Sub-Panels")]
        [SerializeField] private SaveLoadPanel saveLoadPanel;

        // Stats tracking
        private int enemiesKilled = 0;
        private int itemsCollected = 0;
        private float survivalTimer = 0f;
        private bool isGameOver = false;

        private void Start()
        {
            // Wire buttons
            restartButton.onClick.AddListener(OnRestartClicked);
            loadButton.onClick.AddListener(OnLoadClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // Subscribe to events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied += Show;
            }

            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory != null)
            {
                inventory.OnInventoryChanged += () => itemsCollected++;
            }

            gameOverPanel.SetActive(false);
        }

        private void Update()
        {
            if (!isGameOver && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                survivalTimer += Time.deltaTime;
            }
        }

        public void OnEnemyKilled()
        {
            enemiesKilled++;
        }

        private void Show()
        {
            isGameOver = true;
            UpdateStatsDisplay();
            gameOverPanel.SetActive(true);

            // Play game over sound
            AudioManager.Instance?.PlayDamageSound(); // Or dedicated game over sound
        }

        private void UpdateStatsDisplay()
        {
            enemiesKilledText.text = $"Enemies Defeated: {enemiesKilled}";
            itemsCollectedText.text = $"Items Collected: {itemsCollected}";
            survivalTimeText.text = $"Survival Time: {FormatTime(survivalTimer)}";
        }

        private string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{minutes:D2}:{secs:D2}";
        }

        private void OnRestartClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.RestartGame();
        }

        private void OnLoadClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            saveLoadPanel?.Show(false);
        }

        private void OnMainMenuClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.LoadMainMenu();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied -= Show;
            }
        }
    }
}