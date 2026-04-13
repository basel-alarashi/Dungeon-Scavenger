using UnityEngine;
using DungeonScavenger.Player;

namespace DungeonScavenger.Traps
{
    /// <summary>
    /// Simple damage zone for testing damage sounds.
    /// </summary>
    public class DamageZone : MonoBehaviour
    {
        [SerializeField] private int damageAmount = 10;
        [SerializeField] private float damageCooldown = 1f;

        private float lastDamageTime;

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (Time.time >= lastDamageTime + damageCooldown)
            {
                PlayerStats stats = other.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.TakeDamage(damageAmount);
                    lastDamageTime = Time.time;

                    Debug.Log($"[DamageZone] Player took {damageAmount} damage");
                }
            }
        }
    }
}