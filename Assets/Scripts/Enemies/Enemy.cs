using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using DungeonScavenger.Inventory;
using DungeonScavenger.Player;
using DungeonScavenger.Core;

namespace DungeonScavenger.Enemy
{
    /// <summary>
    /// Main enemy controller using a finite state machine.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class Enemy : MonoBehaviour
    {
        #region Components

        [Header("References")]
        [SerializeField] public EnemyData enemyData;
        [SerializeField] public Transform playerTarget;
        [SerializeField] private Transform eyePosition;
        [SerializeField] private Renderer enemyRenderer;

        private NavMeshAgent agent;
        private Animator animator;
        private float currentHealth;

        #endregion

        #region State Machine

        private EnemyState currentState;
        public readonly EnemyIdleState IdleState = new EnemyIdleState();
        public readonly EnemyPatrolState PatrolState = new EnemyPatrolState();
        public readonly EnemyChaseState ChaseState = new EnemyChaseState();
        public readonly EnemyAttackState AttackState = new EnemyAttackState();
        public readonly EnemyDeathState DeathState = new EnemyDeathState();

        #endregion

        #region State Properties

        public Vector3 StartPosition { get; private set; }
        public float LastAttackTime { get; set; }
        public float TimeSinceLastAttack => Time.time - LastAttackTime;
        public bool CanAttack => TimeSinceLastAttack >= enemyData.attackCooldown;
        public bool IsPlayerInAttackRange { get; private set; }
        public bool CanSeePlayer { get; private set; }
        public float DistanceToPlayer { get; private set; }
        public float LastSeenPlayerTime { get; set; }
        public bool IsDead => currentHealth <= 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            if (enemyRenderer == null)
                enemyRenderer = GetComponentInChildren<Renderer>();

            StartPosition = transform.position;
        }

        private void Start()
        {
            if (enemyData == null)
            {
                Debug.LogError($"[Enemy] No EnemyData assigned to {gameObject.name}!");
                return;
            }

            currentHealth = enemyData.maxHealth;

            // Find player if not assigned
            if (playerTarget == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTarget = player.transform;
            }

            // Configure NavMeshAgent
            agent.speed = enemyData.moveSpeed;
            agent.stoppingDistance = enemyData.attackRange * 0.8f;

            // Start in idle state
            TransitionToState(IdleState);

            // Apply visual color
            if (enemyRenderer != null)
                enemyRenderer.material.color = enemyData.enemyColor;
        }

        private void Update()
        {
            if (enemyData == null || playerTarget == null || IsDead) return;

            // Update detection
            UpdateDetection();

            // Update current state
            currentState?.Update(this);

            // Update animator parameters
            UpdateAnimator();
        }

        #endregion

        #region State Management

        public void TransitionToState(EnemyState newState)
        {
            if (currentState != null)
            {
                Debug.Log($"[Enemy] {enemyData.enemyName} exiting {currentState.GetType().Name}");
                currentState.OnExit(this);
            }

            currentState = newState;

            if (currentState != null)
            {
                Debug.Log($"[Enemy] {enemyData.enemyName} entering {currentState.GetType().Name}");
                currentState.OnEnter(this);
            }
        }

        #endregion

        #region Detection

        private void UpdateDetection()
        {
            if (playerTarget == null) return;

            Vector3 directionToPlayer = playerTarget.position - transform.position;
            DistanceToPlayer = directionToPlayer.magnitude;

            // Check if player is within detection range
            bool inRange = DistanceToPlayer <= enemyData.detectionRange;

            // Check if player is within field of view
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            bool inFOV = angle <= enemyData.detectionAngle * 0.5f;

            // Check line of sight (raycast)
            bool hasLineOfSight = false;
            if (inRange)
            {
                RaycastHit hit;
                Vector3 rayOrigin = eyePosition != null ? eyePosition.position : transform.position + Vector3.up * 1.5f;

                if (Physics.Raycast(rayOrigin, directionToPlayer.normalized, out hit, enemyData.detectionRange))
                {
                    hasLineOfSight = hit.transform.CompareTag("Player");
                }
            }

            CanSeePlayer = inRange && inFOV && hasLineOfSight;

            if (CanSeePlayer)
            {
                LastSeenPlayerTime = Time.time;
            }

            // Check attack range
            IsPlayerInAttackRange = DistanceToPlayer <= enemyData.attackRange && hasLineOfSight;
        }

        #endregion

        #region Movement

        public void SetDestination(Vector3 destination)
        {
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.SetDestination(destination);
            }
        }

        public void StopMovement()
        {
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.ResetPath();
            }
        }

        public bool HasReachedDestination()
        {
            return agent != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        }

        #endregion

        #region Combat

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);

            // Play hurt sound
            if (enemyData.hurtSound != null)
                AudioManager.Instance?.PlaySFXAtPoint(enemyData.hurtSound, transform.position);

            // Flash red
            StartCoroutine(FlashDamage());

            if (IsDead)
            {
                Die();
            }
            else
            {
                // Immediately chase player when hit
                LastSeenPlayerTime = Time.time;
                if (!(currentState is EnemyChaseState) && !(currentState is EnemyAttackState))
                {
                    TransitionToState(ChaseState);
                }
            }

            Debug.Log($"[Enemy] {enemyData.enemyName} took {damage} damage. Health: {currentHealth}/{enemyData.maxHealth}");
        }

        private IEnumerator FlashDamage()
        {
            if (enemyRenderer != null)
            {
                Color originalColor = enemyRenderer.material.color;
                enemyRenderer.material.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                enemyRenderer.material.color = originalColor;
            }
        }

        public void Attack()
        {
            if (playerTarget == null || !CanAttack) return;

            LastAttackTime = Time.time;

            // Play attack sound
            if (enemyData.attackSound != null)
                AudioManager.Instance?.PlaySFXAtPoint(enemyData.attackSound, transform.position);

            // Deal damage to player
            PlayerStats playerStats = playerTarget.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage((int)enemyData.attackDamage);
                Debug.Log($"[Enemy] {enemyData.enemyName} attacked player for {enemyData.attackDamage} damage!");
            }
        }

        private void Die()
        {
            // Play death sound
            if (enemyData.deathSound != null)
                AudioManager.Instance?.PlaySFXAtPoint(enemyData.deathSound, transform.position);

            // Spawn death effect
            if (enemyData.deathEffectPrefab != null)
            {
                Instantiate(enemyData.deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Drop loot
            DropLoot();

            // Transition to death state
            TransitionToState(DeathState);
        }

        private void DropLoot()
        {
            Debug.Log($"[Enemy] DropLoot called. Loot table count: {enemyData.lootTable?.Count ?? 0}");

            if (enemyData.lootTable == null || enemyData.lootTable.Count == 0)
            {
                Debug.LogWarning($"[Enemy] No loot table configured for {enemyData.enemyName}!");
                return;
            }

            ItemData lootItem = enemyData.GetRandomLoot();
            if (lootItem == null)
            {
                Debug.Log($"[Enemy] No loot dropped (random chance missed)");
                return;
            }

            Debug.Log($"[Enemy] Selected loot item: {lootItem.itemName}");

            // Find the drop from loot table to get quantity
            LootDrop drop = enemyData.lootTable.Find(d => d.itemData == lootItem);
            int quantity = drop != null ? drop.GetRandomQuantity() : 1;

            Debug.Log($"[Enemy] Dropping {quantity}x {lootItem.itemName}");

            // Spawn the item in world
            if (lootItem.worldPrefab != null)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
                GameObject lootObject = Instantiate(lootItem.worldPrefab, spawnPosition, Quaternion.identity);

                Debug.Log($"[Enemy] Spawned loot prefab: {lootObject.name} at {spawnPosition}");

                // Configure the pickup
                var pickup = lootObject.GetComponent<PickupItem>();
                if (pickup == null)
                {
                    pickup = lootObject.AddComponent<PickupItem>();
                    Debug.Log("[Enemy] Added PickupItem component to loot");
                }

                // Set item data
                pickup.itemData = lootItem;

                // Ensure collider is trigger
                Collider col = lootObject.GetComponent<Collider>();
                if (col != null)
                {
                    col.isTrigger = true;
                }
            }
            else
            {
                Debug.LogError($"[Enemy] worldPrefab is null for {lootItem.itemName}! Cannot spawn loot!");
            }
        }

        #endregion

        #region Animation

        private void UpdateAnimator()
        {
            if (animator == null) return;

            animator.SetFloat("Speed", agent.velocity.magnitude);
            animator.SetBool("IsAttacking", currentState is EnemyAttackState);
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (enemyData == null) return;

            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

            // Field of view
            Vector3 leftBoundary = Quaternion.Euler(0, -enemyData.detectionAngle * 0.5f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, enemyData.detectionAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, leftBoundary * enemyData.detectionRange);
            Gizmos.DrawRay(transform.position, rightBoundary * enemyData.detectionRange);
        }

        #endregion
    }
}