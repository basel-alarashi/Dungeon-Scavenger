using UnityEngine;

namespace DungeonScavenger.Enemy
{
    public class EnemyDeathState : EnemyState
    {
        private float deathTimer = 2f;

        public override void OnEnter(Enemy enemy)
        {
            enemy.StopMovement();

            // Disable collider
            var collider = enemy.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            // Disable NavMeshAgent
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
                agent.enabled = false;
        }

        public override void Update(Enemy enemy)
        {
            deathTimer -= Time.deltaTime;

            // Sink into ground
            enemy.transform.position += Vector3.down * 0.5f * Time.deltaTime;

            if (deathTimer <= 0)
            {
                GameObject.Destroy(enemy.gameObject);
            }
        }

        public override void OnExit(Enemy enemy)
        {
            // Called when destroyed
        }
    }
}