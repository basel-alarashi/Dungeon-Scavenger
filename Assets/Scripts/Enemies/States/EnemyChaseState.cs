using UnityEngine;

namespace DungeonScavenger.Enemy
{
    public class EnemyChaseState : EnemyState
    {
        private float loseTargetTimer;

        public override void OnEnter(Enemy enemy)
        {
            // Play alert sound
            if (enemy.enemyData.alertSound != null)
                DungeonScavenger.Core.AudioManager.Instance?.PlaySFXAtPoint(
                    enemy.enemyData.alertSound,
                    enemy.transform.position
                );

            // Increase speed for chase
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
                agent.speed = enemy.enemyData.chaseSpeed;

            loseTargetTimer = 0f;
        }

        public override void Update(Enemy enemy)
        {
            // Check if in attack range
            if (enemy.IsPlayerInAttackRange && enemy.CanAttack)
            {
                enemy.TransitionToState(enemy.AttackState);
                return;
            }

            // Update destination to player position
            if (enemy.CanSeePlayer)
            {
                loseTargetTimer = 0f;
                enemy.SetDestination(enemy.playerTarget.position);
            }
            else
            {
                loseTargetTimer += Time.deltaTime;

                // Lost player, return to patrol
                if (loseTargetTimer >= enemy.enemyData.loseTargetTime)
                {
                    enemy.TransitionToState(enemy.PatrolState);
                }
            }
        }

        public override void OnExit(Enemy enemy)
        {
            // Reset speed
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
                agent.speed = enemy.enemyData.moveSpeed;
        }
    }
}