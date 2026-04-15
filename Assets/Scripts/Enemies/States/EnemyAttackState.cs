using UnityEngine;

namespace DungeonScavenger.Enemy
{
    public class EnemyAttackState : EnemyState
    {
        public override void OnEnter(Enemy enemy)
        {
            enemy.StopMovement();
        }

        public override void Update(Enemy enemy)
        {
            // Check if player is still in attack range
            if (!enemy.IsPlayerInAttackRange)
            {
                enemy.TransitionToState(enemy.ChaseState);
                return;
            }

            // Face the player
            Vector3 direction = (enemy.playerTarget.position - enemy.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                enemy.transform.rotation = Quaternion.Slerp(
                    enemy.transform.rotation,
                    targetRotation,
                    enemy.enemyData.rotationSpeed * Time.deltaTime
                );
            }

            // Attack if possible
            if (enemy.CanAttack)
            {
                enemy.Attack();
            }
        }

        public override void OnExit(Enemy enemy)
        {
            // Cleanup
        }
    }
}