using UnityEngine;
using System.Collections.Generic;
using DungeonScavenger.Inventory;

namespace DungeonScavenger.Enemy
{
    /// <summary>
    /// Defines enemy stats, behavior parameters, and loot drops.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyName = "Enemy";
        public Sprite icon;
        public Color enemyColor = Color.red;

        [Header("Stats")]
        public float maxHealth = 50f;
        public float moveSpeed = 3.5f;
        public float chaseSpeed = 5f;
        public float rotationSpeed = 10f;
        public float attackDamage = 10f;
        public float attackCooldown = 1.5f;
        public float attackRange = 2f;

        [Header("Detection")]
        public float detectionRange = 10f;
        public float detectionAngle = 60f;  // Field of view
        public float loseTargetTime = 3f;   // Time to stop chasing after losing sight

        [Header("Patrol")]
        public float patrolWaitTime = 2f;
        public float patrolRadius = 5f;

        [Header("Visual/Audio")]
        public AudioClip spawnSound;
        public AudioClip idleSound;
        public AudioClip alertSound;
        public AudioClip attackSound;
        public AudioClip hurtSound;
        public AudioClip deathSound;
        public GameObject deathEffectPrefab;

        [Header("Loot")]
        public List<LootDrop> lootTable;
        public int experienceReward = 10;

        /// <summary>
        /// Returns a random loot item based on drop chances.
        /// </summary>
        public ItemData GetRandomLoot()
        {
            if (lootTable == null || lootTable.Count == 0)
                return null;

            float totalChance = 0f;
            foreach (var drop in lootTable)
                totalChance += drop.dropChance;

            float random = Random.Range(0f, totalChance);
            float currentChance = 0f;

            foreach (var drop in lootTable)
            {
                currentChance += drop.dropChance;
                if (random <= currentChance)
                    return drop.itemData;
            }

            return lootTable[0].itemData;
        }
    }

    [System.Serializable]
    public class LootDrop
    {
        public ItemData itemData;
        [Range(0f, 100f)]
        public float dropChance = 50f;
        public int minQuantity = 1;
        public int maxQuantity = 1;

        public int GetRandomQuantity()
        {
            return Random.Range(minQuantity, maxQuantity + 1);
        }
    }
}