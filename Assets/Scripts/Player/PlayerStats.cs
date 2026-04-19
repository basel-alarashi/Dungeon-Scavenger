using UnityEngine;
using System;
using DungeonScavenger.Core;

namespace DungeonScavenger.Player
{
    /// <summary>
    /// Manages player health, ammo, and other stats that items can modify.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        #region Singleton

        public static PlayerStats Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        #endregion

        #region Events

        public event Action<int, int> OnHealthChanged;   // current, max
        public event Action<int, int> OnAmmoChanged;     // current, max
        public event Action OnPlayerDied;

        private bool hasDied = false;  // Prevent multiple death triggers

        #endregion

        #region Health Settings

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        [SerializeField] private bool regenerateHealth = false;
        // [SerializeField] private float healthRegenRate = 1f;
        // [SerializeField] private int healthRegenAmount = 1;

        #endregion

        #region Ammo Settings

        [Header("Ammo")]
        [SerializeField] private int maxAmmo = 30;
        [SerializeField] private int currentAmmo = 15;
        [SerializeField] private bool infiniteAmmo = false;

        #endregion

        #region Debug

        [Header("Debug")]
        [SerializeField] private bool logStatChanges = true;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            currentHealth = maxHealth;
            currentAmmo = maxAmmo / 2; // Start with half ammo

            if (logStatChanges)
                Debug.Log($"[PlayerStats] Initialized. Health: {currentHealth}/{maxHealth}, Ammo: {currentAmmo}/{maxAmmo}");
        }

        private void Update()
        {
            if (regenerateHealth && currentHealth < maxHealth)
            {
                // Simple regeneration over time
                // For better control, use a coroutine in production
            }
        }

        #endregion

        #region Health Methods

        /// <summary>
        /// Heals the player by a specified amount.
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0) return;

            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            int healedAmount = currentHealth - oldHealth;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (logStatChanges)
                Debug.Log($"[PlayerStats] Healed {healedAmount} HP. Health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Damages the player by a specified amount.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (amount <= 0 || hasDied) return;

            currentHealth = Mathf.Max(currentHealth - amount, 0);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // PLAY DAMAGE SOUND
            AudioManager.Instance?.PlayDamageSound();

            if (logStatChanges)
                Debug.Log($"[PlayerStats] Took {amount} damage. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                hasDied = true;
                Die();
            }
        }

        /// <summary>
        /// Returns true if health is at maximum.
        /// </summary>
        public bool IsHealthFull()
        {
            return currentHealth >= maxHealth;
        }

        /// <summary>
        /// Gets the current health percentage (0-1).
        /// </summary>
        public float GetHealthPercentage()
        {
            return (float)currentHealth / maxHealth;
        }

        #endregion

        #region Ammo Methods

        public bool HasAmmo(int amount = 1)
        {
            return infiniteAmmo || currentAmmo >= amount;
        }

        /// <summary>
        /// Consumes ammo (returns true if ammo was available).
        /// </summary>
        public bool ConsumeAmmo(int amount = 1)
        {
            if (infiniteAmmo) return true;

            if (currentAmmo >= amount)
            {
                currentAmmo -= amount;
                OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);

                Debug.Log($"[PlayerStats] Consumed {amount} ammo. Remaining: {currentAmmo}/{maxAmmo}");
                return true;
            }

            Debug.Log($"[PlayerStats] Not enough ammo! Have: {currentAmmo}, Need: {amount}");
            return false;
        }

        /// <summary>
        /// Adds ammo to the player's reserves.
        /// </summary>
        public void AddAmmo(int amount)
        {
            currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            Debug.Log($"[PlayerStats] Added {amount} ammo. Total: {currentAmmo}/{maxAmmo}");
        }

        /// <summary>
        /// Returns true if ammo is at maximum.
        /// </summary>
        public bool IsAmmoFull()
        {
            return infiniteAmmo || currentAmmo >= maxAmmo;
        }

        #endregion

        #region Death & Respawn

        private void Die()
        {
            if (hasDied && currentHealth > 0) hasDied = false; // Reset if revived

            if (logStatChanges)
                Debug.Log("[PlayerStats] Player died!");

            OnPlayerDied?.Invoke();

            // Optional: Reload scene or show game over screen
            // UnityEngine.SceneManagement.SceneManager.LoadScene(
            //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            // );
        }

        #endregion

        #region Public Properties

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int CurrentAmmo => currentAmmo;
        public int MaxAmmo => maxAmmo;

        #endregion

        #region Editor Helpers

        [ContextMenu("Debug/Take 10 Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(10);
        }

        [ContextMenu("Debug/Heal 10")]
        private void DebugHeal()
        {
            Heal(10);
        }

        [ContextMenu("Debug/Add 5 Ammo")]
        private void DebugAddAmmo()
        {
            AddAmmo(5);
        }

        /// <summary>
        /// Resets player stats to default values (used when loading saves).
        /// </summary>
        public void ResetStats()
        {
            currentHealth = maxHealth;
            currentAmmo = maxAmmo / 2;
            hasDied = false;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);

            if (logStatChanges)
                Debug.Log($"[PlayerStats] Stats reset. Health: {currentHealth}/{maxHealth}, Ammo: {currentAmmo}/{maxAmmo}");
        }

        #endregion
    }
}