using UnityEngine;
using UnityEngine.AI;

namespace DungeonScavenger.Enemy
{
    public class EnemyPatrolState : EnemyState
    {
        private Vector3 patrolDestination;
        private float waitTimer;
        private bool isWaiting;

        public override void OnEnter(Enemy enemy)
        {
            SetNewPatrolDestination(enemy);
            isWaiting = false;
        }

        public override void Update(Enemy enemy)
        {
            // Check if can see player
            if (enemy.CanSeePlayer)
            {
                enemy.TransitionToState(enemy.ChaseState);
                return;
            }

            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    SetNewPatrolDestination(enemy);
                    isWaiting = false;
                }
                return;
            }

            // Check if reached destination
            if (enemy.HasReachedDestination())
            {
                isWaiting = true;
                waitTimer = Random.Range(1f, 3f);
                enemy.StopMovement();
            }
        }

        private void SetNewPatrolDestination(Enemy enemy)
        {
            // Get random point within patrol radius of start position
            Vector3 randomDirection = Random.insideUnitSphere * enemy.enemyData.patrolRadius;
            randomDirection += enemy.StartPosition;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, enemy.enemyData.patrolRadius, NavMesh.AllAreas))
            {
                patrolDestination = hit.position;
                enemy.SetDestination(patrolDestination);
            }
            else
            {
                // Fallback to start position
                enemy.SetDestination(enemy.StartPosition);
            }
        }

        public override void OnExit(Enemy enemy)
        {
            // Cleanup
        }
    }
}