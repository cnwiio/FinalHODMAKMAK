using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

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
            //Debug.WriteLine("Enter idle");
        }
        public virtual void Update(IMonster monster, float deltaTime)
        {
            if (monster is MonsterMelee)
            {
                if (monster.isAttack)
                {
                    monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer));
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
                        monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer));
                        monster.WanderTimer = 2f;
                        monster.WaitingToReturn = true;
                    }
                }
                else
                {
                    monster.animation.SetAnimation("Idle", "right");
                } 
            } 
            else if (monster is MonsterRange)
            {
                var range = monster as MonsterRange;
                
                if (monster.isAwayHome)
                {
                    monster.IgnorePlayer = true;
                    monster.ChangeState(new ReturnState());
                }
                else if (monster.isAttack)
                {
                    monster.ChangeState(new ChasingState());
                    return;
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
                        monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer));
                        monster.WanderTimer = 2f;
                        monster.WaitingToReturn = true;
                    }
                }
                else
                {
                    monster.animation.SetAnimation("Idle", "right");
                }
            } 
            else if (monster is MonsterBoss)
            {
                if (!monster.isInActiveRadius)
                {
                    monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer));
                    return;
                }
                else if (monster.isAttack)
                {
                    monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer));
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
                        monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer));
                        monster.WanderTimer = 2f;
                        monster.WaitingToReturn = true;
                    }
                }
                else
                {
                    monster.animation.SetAnimation("Idle", "right");
                }
            } else if (monster is MonsterSlime)
            {
                if (monster.isAwayHome)
                {
                    monster.IgnorePlayer = true;
                    monster.ChangeState(new ReturnState());
                }
                else if (monster.isInRange)
                {
                    monster.ChangeState(new ChasingState());
                }
                else if (monster.isInWander)
                {
                    if (monster.WaitingToReturn == false)
                    {
                        //monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer));
                        monster.WanderTimer = 2f;
                        monster.WaitingToReturn = true;
                    }
                }
                else
                {
                    monster.animation.SetAnimation("Idle", "right");
                }
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
            //Debug.WriteLine("Enter Chasing");
        }
        public virtual void Update(IMonster monster, float deltaTime)
        {

            if (monster is MonsterMelee)
            {
                var melee = monster as MonsterMelee;

                if (!monster.isInRange || monster.isAwayHome)
                {
                    monster.ChangeState(new IdleState());
                }
                else if (monster.isInAttack)
                {
                    monster.ChangeState(new AttackState());
                }
                else if (monster.isInAttackList)
                {
                    monster.MoveTo(deltaTime, monster.TargetPos);
                }
                else if (monster.isInActiveRadius)
                {
                    if (!monster.isInAttackList)
                        melee.PreventMonster.ActiveAttacker.Add(monster);
                }
                else if (monster.preventMonsterEdge >= 1f)
                {
                    monster.MoveTo(deltaTime, melee.TargetPos);
                }
                else 
                {
                    monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer)); // still in chasing state but in idle animation
                }
            }
            else if (monster is MonsterRange)
            {
                var range = monster as MonsterRange;
                var distance = Vector2.Distance(range.Position, range.TargetPos);

                if (!range.isInRange || range.isAwayHome)
                {
                    monster.ChangeState(new IdleState());
                }else if (range.isAttack)
                {
                    if (distance < range.AttackRange) {
                        range.MoveToDirection(deltaTime, -range.DirectionToPlayer);
                    }
                    else if (distance >= range.AttackRange * 1.2f)
                    {
                        range.MoveTo(deltaTime, range.TargetPos);
                    }
                    else
                    {
                        range.animation.SetAnimation("Idle", range.GetDirection(monster.DirectionToPlayer));
                    }
                }
                else if (range.isInAttack)
                {
                    monster.ChangeState(new AttackState());
                }
                else if (range.isInAttackList)
                {
                    range.MoveTo(deltaTime, range.TargetPos);
                }
                else
                {
                    range.animation.SetAnimation("Idle", range.GetDirection(monster.DirectionToPlayer)); // still in chasing state but in idle animation
                }
            }
            else if (monster is MonsterBoss)
            {
                var boss = monster as MonsterBoss;
                var distance = Vector2.Distance(boss.Position, boss.TargetPos);
                if (!monster.isInRange || monster.isAwayHome)
                {
                    monster.ChangeState(new IdleState());
                }
                else if (monster.isInAttack)
                {
                    monster.ChangeState(new AttackState());
                }
                else if (distance > boss.AttackRange)
                {
                    boss.MoveTo(deltaTime, boss.TargetPos);
                }
                else
                {
                    monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer)); // still in chasing state but in idle animation
                }
            }
            else if (monster is MonsterSlime)
            {
                var slime = monster as MonsterSlime;

                if (!monster.isInRange || monster.isAwayHome)
                {
                    monster.ChangeState(new IdleState());
                }
                else if (monster.isInAttack)
                {
                    monster.ChangeState(new AttackState());
                }
                else if (monster.isInAttackList)
                {
                    monster.MoveTo(deltaTime, slime.TargetPos);
                }
                else if (monster.isInActiveRadius)
                {
                    if (!monster.isInAttackList)
                        slime.PreventMonster.ActiveAttacker.Add(monster);
                }
                else if (monster.preventMonsterEdge >= 1f)
                {
                    monster.MoveTo(deltaTime, monster.TargetPos);
                }
                else
                {
                    if (monster.animation.CurrentSpriteSheet != "Attack")
                        monster.animation.SetAnimation("Idle", monster.GetDirection(monster.DirectionToPlayer)); // still in chasing state but in idle animation
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
            //Debug.WriteLine("Enter attack");
        }
        public virtual void Update(IMonster monster, float deltaTime)
        {
            if (monster is MonsterBoss)
            {
                if (monster.isInAttack || monster.isAttack)
                {
                    monster.Attack();
                }
                else
                {
                    monster.ChangeState(new IdleState());
                }
                return;
            }


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
            //Debug.WriteLine("Enter Return");
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