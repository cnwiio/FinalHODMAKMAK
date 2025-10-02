using Microsoft.Xna.Framework;

namespace game
{
    public enum Element
    {
        light,
        dark
    }
    public interface IMonster
    {
        Vector2 Position { get; set; }
        Vector2 DesiredPosition { get; set; }
        Vector2 TargetPos { get; set; }
        Vector2 SpawnPosition { get; set; }
        Vector2 DirectionToPlayer { get; set; }
        float WanderTimer { get; set; }
        bool WaitingToReturn { get; set; }
        bool IsReturning { get; set; }
        bool IgnorePlayer { get; set; }
        int MAXHP { get; set; }
        int HP { get; set; }
        int Damage { get; set; }
        float preventMonsterEdge { get; set; }
        bool isHit { get; set; }
        bool isInAttackList { get; set; }
        bool isAwayHome { get; set; }
        bool isInRange { get; set; }
        bool isInAttack { get; set; }
        bool isInWander { get; set; }
        bool isInActiveRadius { get; set; }
        bool isAttack { get; set; }
        float Speed { get; set; }
        float SreachRadius { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        IEntity HurtBox { get; set; }
        float AwaySpawnRadius { get; set; }
        AnimController animation { get; set; }
        IMonsterState CurrentState { get; set; }
        ElementType ElementType { get; set; }
        void MoveTo(float deltaTime, Vector2 position);
        void ChangeState(IMonsterState newState);
        void Attack();
        void Return(float deltaTime);
        void StateChecking(float deltaTime);
        void Reset();
        string GetDirection(Vector2 direction);
        void UnLoad();
        void ApplyDamage(int value);
    }
}


