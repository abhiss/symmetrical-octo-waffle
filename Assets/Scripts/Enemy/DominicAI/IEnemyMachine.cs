namespace EnemyMachine
{
    public enum EnemyState {
        Idle,
        Chasing,
        Attacking
    }

    public interface IEnemyMachine
    {
        EnemyState IdleStateHandler();
        EnemyState ChasingStateHandler();
        EnemyState AttackingStateHandler();
    }
}
