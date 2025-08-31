using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace game
{
    public interface IMonsterState
    {
        void Enter(IMonster monster);
        void Update(IMonster monster, float deltaTime);
        void Exit(IMonster monster);
    }

    public class IdleState : IMonsterState
    {
        public virtual void Enter(IMonster monster)
        {
        }
        public virtual void Update(IMonster monster, float deltaTime)
        {
            if (monster.isAttack)
            {
                monster.animation.SetAnimation("Idle", /*melee.GetDirection(monster.DirectionToPlayer)*/"down");
                return;
            }
            else if (monster.isAwayHome)
            {
                monster.IgnorePlayer = true;
                monster.ChangeState(new ReturnState());
            }
            else if (monster.isInAttack)
            {
                monster.ChangeState(new AttackState());
            }
            else if (monster.isInRange)
            {
                monster.ChangeState(new ChasingState());
            }
            else if (monster.isInWander)
            {
                if (monster.WaitingToReturn == false)
                {
                    monster.animation.SetAnimation("Idle", "down");
                    monster.WanderTimer = 2f;
                    monster.WaitingToReturn = true;
                }
            }
            else
            {
                monster.animation.SetAnimation("Idle", "down");
            }
        }
        public virtual void Exit(IMonster monster)
        {
        }
    }
    public class  ChasingState : IMonsterState
    {
        public virtual void Enter(IMonster monster)
        {

        }
        public virtual void Update(IMonster monster, float deltaTime)
        {

            if (monster is MonsterMelee)
            {
                var melee = monster as MonsterMelee;

                if (!melee.isInRange || melee.isAwayHome)
                {
                    monster.ChangeState(new IdleState());
                }
                else if (melee.isInAttack)
                {
                    monster.ChangeState(new AttackState());
                }
                else if (melee.isInAttackList)
                {
                    melee.MoveTo(deltaTime, melee.TargetPos);
                }
                else if (melee.isInActiveRadius)
                {
                    if (!melee.isInAttackList)
                        melee.PreventMonster.ActiveAttacker.Add(melee);
                }
                else if (melee.preventMonsterEdge >= 1f)
                {
                    melee.MoveTo(deltaTime, melee.TargetPos);
                }
                else
                {
                    melee.animation.SetAnimation("Idle", /*melee.GetDirection(monster.DirectionToPlayer)*/"down"); // still in chasing state but in idle animation
                }
            }
        }
        public virtual void Exit(IMonster monster)
        {
        }
    }
    
    public class AttackState : IMonsterState
    {
        public virtual void Enter(IMonster monster)
        {
        }
        public virtual void Update(IMonster monster, float deltaTime)
        {

            if (monster.isInAttack)
            {
                monster.Attack();
            }
            else if (monster.isAttack)
            {
                return;
            }
            else 
            {
                monster.ChangeState(new IdleState());
            }

        }
        public virtual void Exit(IMonster monster)
        {
        }
    }
    public class ReturnState : IMonsterState
    {
        public virtual void Enter(IMonster monster)
        {
        }
        public virtual void Update(IMonster monster, float deltaTime)
        {

            if (monster.IgnorePlayer)
            {
                monster.Reset();
                monster.Return(deltaTime);
            }
            else if (monster.isInRange)
            {
                monster.ChangeState(new ChasingState());
            }
            else if (monster.IsReturning)
            {
                monster.Return(deltaTime);
            }
        }
        public virtual void Exit(IMonster monster)
        {
        }
    }
}

/*    public class StateChecking
    {
        public float Width, Height, preventMonsterEdge;
        public bool inAttackList, isAwayHome, isInRange, isInAttack, isAttack, isInWander, isInActiveRadius;
        public Vector2 direction;
        public void Update(IMonster monster) 
        {
            // move it to monsterMelee
            if (monster is MonsterMelee) 
            {
                var melee = monster as MonsterMelee;
                var Position = melee.Position;
                var TargetPos = melee.TargetPos;
                var bounds = melee.HurtBox.Bounds.BoundingRectangle;
                Width = (int)bounds.Width;
                Height = (int)bounds.Height;
                float preventMonsterEdge = Vector2.Distance(Position, melee.PreventMonster.Position) - melee.PreventMonster.Radius;
                inAttackList = melee.PreventMonster.ActiveAttacker.Contains(melee);
                isAwayHome = Vector2.Distance(Position, melee.SpawnPosition) > melee.AwaySpawnRadius;
                isInRange = Vector2.Distance(Position, TargetPos) <= melee.SreachRadius;
                isInAttack = !(Math.Abs(Position.X - TargetPos.X) > Width * 1.2f || Math.Abs(Position.Y - TargetPos.Y) > Height * 1.2f);
                isInWander = Vector2.Distance(Position, TargetPos) > melee.SreachRadius && Vector2.Distance(Position, melee.SpawnPosition) > Width;
                isInActiveRadius = Vector2.Distance(Position, TargetPos) <= melee.ActiveRadius;
                isAttack = melee.isAttack;
                direction = TargetPos - Position;
                if (direction != Vector2.Zero)
                    direction.Normalize();
            }
        }
    }*/