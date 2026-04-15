using UnityEngine;

namespace DungeonScavenger.Enemy
{
    public class EnemyIdleState : EnemyState
    {
        private float idleTimer;
        private float idleDuration = 2f;

        public override void OnEnter(Enemy enemy)
        {
            enemy.StopMovement();
            idleTimer = 0f;
            idleDuration = Random.Range(1f, 3f);
        }

        public override void Update(Enemy enemy)
        {
            // Check if can see player
            if (enemy.CanSeePlayer)
            {
                enemy.TransitionToState(enemy.ChaseState);
                return;
            }

            idleTimer += Time.deltaTime;

            if (idleTimer >= idleDuration)
            {
                enemy.TransitionToState(enemy.PatrolState);
            }
        }

        public override void OnExit(Enemy enemy)
        {
            // Cleanup
        }
    }
}