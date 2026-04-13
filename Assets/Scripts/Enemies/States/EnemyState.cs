using UnityEngine;

namespace DungeonScavenger.Enemy
{
    /// <summary>
    /// Base class for all enemy states.
    /// </summary>
    public abstract class EnemyState
    {
        public abstract void OnEnter(Enemy enemy);
        public abstract void Update(Enemy enemy);
        public abstract void OnExit(Enemy enemy);
    }
}