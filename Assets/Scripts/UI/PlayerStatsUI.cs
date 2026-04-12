using UnityEngine;
using TMPro;
using DungeonScavenger.Player;

namespace DungeonScavenger.UI
{
    public class PlayerStatsUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private PlayerStats playerStats;

        private void Start()
        {
            if (playerStats == null)
                playerStats = FindAnyObjectByType<PlayerStats>();

            if (playerStats != null)
            {
                playerStats.OnHealthChanged += UpdateHealthDisplay;
                playerStats.OnAmmoChanged += UpdateAmmoDisplay;

                // Initial update
                UpdateHealthDisplay(playerStats.CurrentHealth, playerStats.MaxHealth);
                UpdateAmmoDisplay(playerStats.CurrentAmmo, playerStats.MaxAmmo);
            }
        }

        private void UpdateHealthDisplay(int current, int max)
        {
            if (healthText != null)
                healthText.text = $"Health: {current}/{max}";
        }

        private void UpdateAmmoDisplay(int current, int max)
        {
            if (ammoText != null)
                ammoText.text = $"Ammo: {current}/{max}";
        }

        private void OnDestroy()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= UpdateHealthDisplay;
                playerStats.OnAmmoChanged -= UpdateAmmoDisplay;
            }
        }
    }
}