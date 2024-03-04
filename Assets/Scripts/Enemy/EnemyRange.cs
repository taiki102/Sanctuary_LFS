using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*-----------------------------------------*/
/*　序盤の近距離敵　レンジキャラ
 *　・体力 低め　1
 *　・速度 遅い　350 
 *　・やる気　0.3 ~ 0.7f　
 *　ステート
 *　・Wait 
 *　・Move 
 *　・Attack
 *　・Dead
/*-----------------------------------------*/

public class EnemyRange : EnemyBase
{
    public class WaitMethod : StateMethod// 0
    {
        public WaitMethod(EnemyBase e) : base(e) { }
        float Count;
        float maxCount;
        public override void OnEnter()
        {
            Count = 0.1f;
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
            if (EB.AliveCheck()) return 3;

            if (EB._eb.player != null)
            {
                if (Count == 0)
                {
                    return 1;
                }              
            }
            return 0;
        }
    }

    public class MoveMethod : StateMethod// 1
    {
        public MoveMethod(EnemyBase e) : base(e) { }
        public override void OnEnter()
        {
            EB.SetSpeed();
        }
        Vector3 dir;
        public override void Main()
        {
            if (EB._eb.player != null)
            {
                EB.SetDir();
            }
                
            EB.CheckStair();

            dir = EB._eb.FacetoRight ? Vector3.right : Vector3.left;
            EB.Move(dir);
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 3;

            if (EB.View(EB.FrontView())) return 2;

            if (EB.DistanceToPlayer() < 4.0f) return 0;
  
            return 1;
        }
    }

    public class AttackMethod : StateMethod// 2
    {
        public AttackMethod(EnemyBase e) : base(e) { }
        public override void OnEnter()
        {
            Count = 0;
            maxCount = 0;
            MotionEnd = false;
            EB.Trigger_Attack(true);
        }
        float Count;
        float maxCount;
        bool MotionEnd;
        public override void Main()
        {
            Count += Time.fixedDeltaTime;
            if (Count > 1.3f)
            {            
                if (maxCount == 0)
                {
                    float x = EB.GetMotionTime();
                    maxCount = x - (x * 0.5f * EB.status.FightMotivation);
                    float dir_X = EB._eb.FacetoRight ? 0.0f : -180f;
                    Instantiate(EB.Weapon(), EB.subWeapon().position, Quaternion.Euler(0, dir_X, 0));
                }
                if (Count > maxCount)
                {
                    Count = 0;
                    MotionEnd = true;
                }
            }
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 3;

            if (MotionEnd)
            {
                return 1;
            }
            return 2;
        }
    }

    public class DeadMethod : StateMethod// 3
    {
        public DeadMethod(EnemyBase e) : base(e) { }
        public override void OnEnter()
        {
            EB.Dead();
        }
        public override int CheckTransitions()
        {
            return 3;
        }
    }

    /*--------------------------------*/
    /*      　　以上ステート      　 　　　
    /*--------------------------------*/

    [SerializeField]
    Transform Ballet;

    [SerializeField]
    Transform BalletPos;

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
        states.Add(new AttackMethod(this));
        states.Add(new DeadMethod(this));
        InstanceMethod(1, 350f, 0.5f, states);
    }

    void FixedUpdate()
    {
        FixedUpdateMehod();
    }

    public override Transform Weapon()
    {
        return Ballet;
    }

    public override Transform subWeapon()
    {
        return BalletPos;
    }
}