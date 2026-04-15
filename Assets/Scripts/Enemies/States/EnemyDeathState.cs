using UnityEngine;

namespace DungeonScavenger.Enemy
{
    public class EnemyDeathState : EnemyState
    {
        private float deathTimer = 2f;
        private bool hasStartedSinking = false;

        public override void OnEnter(Enemy enemy)
        {
            Debug.Log($"[EnemyDeathState] Entered death state for {enemy.enemyData.enemyName}");

            enemy.StopMovement();

            // Disable collider to prevent further hits
            var collider = enemy.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Debug.Log("[EnemyDeathState] Disabled collider");
            }

            // Disable NavMeshAgent
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
                Debug.Log("[EnemyDeathState] Disabled NavMeshAgent");
            }

            hasStartedSinking = true;
        }

        public override void Update(Enemy enemy)
        {
            if (!hasStartedSinking) return;

            deathTimer -= Time.deltaTime;

            // Sink into ground for dramatic effect
            enemy.transform.position += Vector3.down * 1f * Time.deltaTime;

            // Fade out (optional - requires renderer)
            var renderer = enemy.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = deathTimer / 2f;
                renderer.material.color = color;
            }

            if (deathTimer <= 0)
            {
                Debug.Log($"[EnemyDeathState] Destroying enemy GameObject");
                GameObject.Destroy(enemy.gameObject);
            }
        }

        public override void OnExit(Enemy enemy)
        {
            // Called when destroyed
            Debug.Log("[EnemyDeathState] Exiting death state (enemy destroyed)");
        }
    }
}