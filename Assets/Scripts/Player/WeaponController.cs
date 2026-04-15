using UnityEngine;
using DungeonScavenger.Core;
using DungeonScavenger.UI;

namespace DungeonScavenger.Player
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [SerializeField] private float attackRange = 50f;
        [SerializeField] private float attackDamage = 25f;
        [SerializeField] private float fireRate = 0.5f;
        [SerializeField] private int ammoPerShot = 1;
        [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;

        [Header("Visual/Audio")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip noAmmoSound;
        [SerializeField] private AudioClip hitSound;

        [Header("Effects")]
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private LineRenderer bulletTrailPrefab;

        private PlayerStats playerStats;
        private float lastFireTime;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
                playerStats = FindAnyObjectByType<PlayerStats>();
        }

        private void Update()
        {
            // CRITICAL: Don't shoot if any UI panel is open
            if (IsAnyUIOpen())
            {
                return;
            }

            if (Input.GetKeyDown(fireKey) || Input.GetButtonDown("Fire1"))
            {
                TryShoot();
            }
        }

        /// <summary>
        /// Checks if any UI panel that should block shooting is open.
        /// </summary>
        private bool IsAnyUIOpen()
        {
            // Check if inventory is open
            if (InventoryUI.Instance != null && InventoryUI.Instance.IsVisible)
            {
                return true;
            }

            // Check if settings panel is open (optional)
            // You can add more UI checks here

            // Check if cursor is visible (general indicator of UI being open)
            if (Cursor.visible)
            {
                return true;
            }

            return false;
        }

        private void TryShoot()
        {
            if (!CanShoot()) return;

            Shoot();
        }

        private bool CanShoot()
        {
            // Check fire rate
            if (Time.time < lastFireTime + fireRate)
                return false;

            // Check ammo
            if (!playerStats.HasAmmo(ammoPerShot))
            {
                Debug.Log("[Weapon] No ammo!");
                if (noAmmoSound != null)
                    AudioManager.Instance?.PlaySFX(noAmmoSound);
                return false;
            }

            return true;
        }

        private void Shoot()
        {
            lastFireTime = Time.time;

            // Consume ammo
            playerStats.ConsumeAmmo(ammoPerShot);

            // Play shoot sound
            if (shootSound != null)
                AudioManager.Instance?.PlaySFX(shootSound);

            // Show muzzle flash
            if (muzzleFlashPrefab != null && muzzlePoint != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Destroy(flash, 0.1f);
            }

            // Perform raycast
            Vector3 shootOrigin = muzzlePoint != null ? muzzlePoint.position :
                                 transform.position + transform.forward * 0.5f + Vector3.up * 1f;
            Vector3 shootDirection = transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(shootOrigin, shootDirection, out hit, attackRange))
            {
                Debug.Log($"[Weapon] Hit: {hit.transform.name}");

                // Show bullet trail
                if (bulletTrailPrefab != null)
                {
                    LineRenderer trail = Instantiate(bulletTrailPrefab);
                    trail.SetPosition(0, shootOrigin);
                    trail.SetPosition(1, hit.point);
                    Destroy(trail.gameObject, 0.1f);
                }

                // Show impact effect
                if (impactEffectPrefab != null)
                {
                    GameObject impact = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 1f);
                }

                // Deal damage to enemy
                Enemy.Enemy enemy = hit.transform.GetComponent<Enemy.Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);

                    if (hitSound != null)
                        AudioManager.Instance?.PlaySFX(hitSound);

                    Debug.Log($"[Weapon] Hit enemy for {attackDamage} damage!");
                }
            }

            // Debug visualization
            Debug.DrawRay(shootOrigin, shootDirection * attackRange, Color.red, 1f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 origin = muzzlePoint != null ? muzzlePoint.position :
                            transform.position + transform.forward * 0.5f + Vector3.up * 1f;
            Gizmos.DrawRay(origin, transform.forward * attackRange);
        }
    }
}