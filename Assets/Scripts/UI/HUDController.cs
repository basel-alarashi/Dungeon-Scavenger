using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonScavenger.Player;

namespace DungeonScavenger.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFill;
        [SerializeField] private TextMeshProUGUI healthText;
        // [SerializeField] private Gradient healthGradient;

        [Header("Ammo")]
        [SerializeField] private Slider ammoSlider;
        [SerializeField] private Image ammoFill;
        [SerializeField] private TextMeshProUGUI ammoText;
        // [SerializeField] private Gradient ammoGradient;

        private PlayerStats playerStats;

        private void Start()
        {
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
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            // if (healthFill != null && healthGradient != null)
            // {
            //     float percent = (float)current / max;
            //     healthFill.color = healthGradient.Evaluate(percent);
            // }

            if (healthText != null)
            {
                healthText.text = $"Health: {current}/{max}";
            }
        }

        private void UpdateAmmoDisplay(int current, int max)
        {
            if (ammoSlider != null)
            {
                ammoSlider.maxValue = max;
                ammoSlider.value = current;
            }

            // if (ammoFill != null && ammoGradient != null)
            // {
            //     float percent = (float)current / max;
            //     ammoFill.color = ammoGradient.Evaluate(percent);
            // }

            if (ammoText != null)
            {
                ammoText.text = $"Ammo: {current}/{max}";
            }
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