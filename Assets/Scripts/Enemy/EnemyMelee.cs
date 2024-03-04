using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*-----------------------------------------*/
/*　序盤の近距離敵　メレーキャラ
 *　・体力 普通　2
 *　・速度 普通　400 
 *　・やる気　0.5 ~ 1.0f　
 *　ステート
 *　・Wait 
 *　・Move 
 *　・Search 
 *　・Attack1 
 *　・Attack2　
 *　・Dead
/*-----------------------------------------*/

public class EnemyMelee : EnemyBase
{
    public class WaitMethod : StateMethod// 0
    {
        public WaitMethod(EnemyBase e) : base(e){}
        float Count;
        float maxCount;
        public override void OnEnter()
        {
            Count = 0;
            maxCount = EB.CalcWaitTime();
            EB.SetRandomDir();
        }
        public override void Main()
        {                 
            Count += Time.fixedDeltaTime;
            if (Count > maxCount)
            {          
                if (!EB.FindPlayer())
                {
                    EB.Flip();
                }            
                Count = 0;
            }
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 5;

            if (EB._eb.player != null)
            {
                return 1;
            }
            return 0;
        }
    }

    public class MoveMethod : StateMethod// 1
    {
        public MoveMethod(EnemyBase e) : base(e) { }
        public override void OnEnter()
        {
            EB.SetSpeed(1.1f);
        }
        Vector3 dir;
        public override void Main()
        {
            EB.CheckStair();

            if (EB._eb.player != null)
                EB.SetDir();

            dir = EB._eb.FacetoRight ? Vector3.right : Vector3.left;
            EB.Move(dir);
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 5;
            if (!EB.View(EB.AroundView())) return 2;
            if (EB.CalcDistanceToPlayer())
            {
                return Random.Range(0, 5) == 0 ? 4 : 3;
            }
            return 1;
        }
    }

    public class SearchMethod : StateMethod// 2
    {
        public SearchMethod(EnemyBase e) : base(e) { }
        public override void OnEnter()
        {
            EB.SetSpeed(0.8f);
            EB._eb.AvoidCount = 0;
            Count = 0;
            maxCount = EB.CalcWaitTime();
        }

        float Count;
        float maxCount;
        Vector3 dir;

        public override void Main()
        {
            Count += Time.fixedDeltaTime;
            if (Count > maxCount)
            {
                EB.Flip();
                Count = 0;               
            }

            EB.AvoidObstacles();
            dir = EB._eb.FacetoRight ? Vector3.right : Vector3.left;
            EB.Move(dir);
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 5;
            if (EB.View(EB.AroundView())) return 1;
            if (EB.CalcDistanceToPlayer())
            {
                return Random.Range(0, 3) == 0 ? 4 : 3;
            }
            return 2;
        }
    }

    public class FirstAttackMethod : StateMethod// 3
    {
        public FirstAttackMethod(EnemyBase e) : base(e) { }

        bool MotionEnd;
        float Count;
        float maxCount;

        public override void OnEnter()
        {
            if (EB._eb.player != null)
                EB.SetDir();

            MotionEnd = false;
            Count = 0;
            EB.Trigger_Attack(true);
            EB.StartCoroutine(EB.HitChecker(1, 0.5f, 0.5f, EB.transform));
        }
        public override void Main()
        {
            Count += Time.fixedDeltaTime;
            if (Count > 0.1f)
            {
                if (maxCount == 0){
                    float x = EB.GetMotionTime();
                    maxCount = x - (x * 0.5f * EB.status.FightMotivation);
                }
                if (Count > maxCount)
                {
                    EB.Trigger_Attack(false);
                    Count = 0;
                    MotionEnd = true;
                }
            }
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 5;
   
            if (MotionEnd)
            {
                if (EB.CalcDistanceToPlayer()) return 4;
                return 1;
            }
            return 3;
        }
    }

    public class SecondAttackMethod : StateMethod// 4
    {
        public SecondAttackMethod(EnemyBase e) : base(e) { }

        bool MotionEnd;
        bool AttackEnd;
        public override void OnEnter()
        {
            if (EB._eb.player != null)
                EB.SetDir();

            AttackEnd = false;
            MotionEnd = false;
            Count = 0;
            EB.Trigger_Attack(true);
            EB.StartCoroutine(EB.HitChecker(1,0.5f,0.5f,EB.transform));
        }
        float Count;
        float maxCount;
        Vector3 dir;
        public override void Main()
        {
            if (MotionEnd)
            {
                Count += Time.fixedDeltaTime;
                if (Count > 0.4f)
                {
                    if (Count > 0.8f)
                    {
                        Count = 0;
                        AttackEnd = true;
                    }                  
                }
                else
                {
                    dir = !EB._eb.FacetoRight ? Vector3.right : Vector3.left;
                    EB.Move(dir);
                }
            }
            else
            {
                Count += Time.fixedDeltaTime;
                if (Count > 0.1f)
                {
                    if (maxCount == 0)
                    {
                        maxCount = EB.GetMotionTime() * 0.6f;
                    }

                    if (Count > maxCount)
                    {
                        Count = 0;
                        MotionEnd = true;
                        EB.SetSpeed(1.9f);
                    }
                }
            }         
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 5;

            if (AttackEnd)
            {
                return 1;
            }
            return 4;
        }
        public override void OnExit()
        {
            EB.Trigger_Attack(false);
        }
    }

    public class DeadMethod : StateMethod// 5
    {
        public DeadMethod(EnemyBase e) : base(e) { }
        public override void OnEnter()
        {
            EB.Dead();
        }
        public override int CheckTransitions()
        {
            return 5;
        }
    }

    /*--------------------------------*/
    /*      　　以上ステート      　 　　　
    /*--------------------------------*/

    void Start()
    {
        Instantiate();
    }

    //生成時の処理
    public void Instantiate()
    {
        List<StateMethod> states = new List<StateMethod>();
        states.Add(new WaitMethod(this));
        states.Add(new MoveMethod(this));
        states.Add(new SearchMethod(this));
        states.Add(new FirstAttackMethod(this));
        states.Add(new SecondAttackMethod(this));
        states.Add(new DeadMethod(this));
        InstanceMethod(1, 400f, 0.5f, states);
    }

    void FixedUpdate()
    {
        FixedUpdateMehod();
    }
}