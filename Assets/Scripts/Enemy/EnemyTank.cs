using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*-----------------------------------------*/
/*�@���Ղ̋ߋ����G�@�^���N�L����
 *�@�E�̗� �����@3
 *�@�E���x �x���@300 
 *�@�E���C�@0.1 ~ 0.5f�@
 *�@�X�e�[�g
 *�@�EWait 
 *�@�EMove 
 *�@�EMeleeAttack 
 *�@�ERangedAttackMethod�@
 *�@�ERunAway
 *�@�EDead
/*-----------------------------------------*/

public class EnemyTank : EnemyBase
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
            EB.WalkAnimReset();
            if (EB._eb.player != null)
                EB.SetDir();
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
            if (EB._eb.player != null)
            {
                if (Count == 0) return 3;//�b���~�܂����畀���� 
                if (EB.AliveCheck()) return 5;//�����m�F
                if (EB.CalcDistanceToPlayer()) return 2;//�G���߂��Ȃ�U��
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
            EB.CheckStair();

            if (EB._eb.player != null)
                EB.SetDir();

            dir = EB._eb.FacetoRight ? Vector3.right : Vector3.left;
            EB.Move(dir);
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 5;
            if (EB.CalcDistanceToPlayer())return 2;//�G���߂��Ȃ�U��
            if (EB.DistanceToPlayer() < 2.0f || EB._eb.player == null) return 0;//�����I�������~�܂�  
            return 1;
        }
        public override void OnExit()
        {
            EB.WalkAnimReset();
        }
    }

    public class MeleeAttackMethod : StateMethod// 2
    {
        public MeleeAttackMethod(EnemyBase e) : base(e) { }
        float Count;
        float maxCount;
        bool MotionEnd;
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
            if (Count > 0.5f)
            {
                if (maxCount == 0){
                    float x = EB.GetMotionTime();
                    maxCount = x - (x * 0.2f * EB.status.FightMotivation);
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
            if (EB.AliveCheck()) return 5;
            if (MotionEnd) return 1;
            return 2;
        }
    }

    public class RangedAttackMethod : StateMethod// 3
    {
        public RangedAttackMethod(EnemyBase e) : base(e) { }
        bool AttackEnd;
        //float Count;

        Transform Axe;

        public Vector3 startPoint;    // �J�n�_
        public Vector3 targetPoint;   // �^�[�Q�b�g�_
        public float arcHeight = 2.0f;  // �ʂ̍���
        public float speed = 10.0f;      // �ړ����x

        private Vector3 initialPosition; // �����ʒu
        private bool forwardMovement = true; // �O�i�ړ����ǂ���

        float angleInDegrees;

        public override void OnEnter()
        {
            if (EB._eb.player != null)
                EB.SetDir();

            Axe = EB.Weapon();
            AttackEnd = false;
            startPoint = Axe.position;
            initialPosition = Axe.position;
            targetPoint = EB._eb.player.transform.position;
        }

        public override void Main()
        {
            if (forwardMovement)
            {
                Axe.position = Vector3.MoveTowards(Axe.position, targetPoint, speed * Time.deltaTime);
                angleInDegrees += speed * 100 * Time.fixedDeltaTime;
                Axe.rotation = Quaternion.Euler(0, 0, angleInDegrees);

                if (Axe.position == targetPoint)
                {
                    forwardMovement = false;
                }
            }
            else
            {
                Axe.position = Vector3.MoveTowards(Axe.position, initialPosition, speed * Time.deltaTime);
                angleInDegrees += speed * 100 * Time.fixedDeltaTime;
                Axe.rotation = Quaternion.Euler(0, 0, angleInDegrees);

                if (Axe.position == initialPosition)
                {
                    forwardMovement = true;
                    Axe.rotation = Quaternion.Euler(0, 0, 0);
                    AttackEnd = true;
                }
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(Axe.position, 0.1f);
            for (int k = 0; k < colliders.Length; k++)
            {
                string Tag = colliders[k].gameObject.tag;
                if (Tag == "Player")
                {
                    colliders[k].gameObject.GetComponent<PlayerChara>().Damaged(1);
                }
            }
        }

        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 5;
            if (AttackEnd) return 1;
            return 3;
        }
    }

    public class RunAwayMethod : StateMethod// 4
    {
        public RunAwayMethod(EnemyBase e) : base(e) { }
        public override void OnEnter()
        {
            if (EB._eb.player != null)
                EB.SetDir();
            EB.Flip();
            EB.SetSpeed(1.3f);
        }
        Vector3 dir;
        public override void Main()
        {
            EB.CheckStair();
            dir = EB._eb.FacetoRight ? Vector3.right : Vector3.left;
            EB.Move(dir);
        }
        public override int CheckTransitions()
        {
            if (EB.AliveCheck()) return 5;
            if (EB.DistanceToPlayer() > 7.0f || EB._eb.player == null || EB.CheckWall()) return 0;
            return 4;
        }
        public override void OnExit()
        {
            EB.WalkAnimReset();
            if (EB._eb.player != null)
                EB.SetDir();
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
            if (EB.AliveCheck()) return 5;
            return 5;
        }
    }

    /*--------------------------------*/
    /*      �@�@�ȏ�X�e�[�g      �@ �@�@�@
    /*--------------------------------*/

    void Start()
    {
        Instantiate();
    }

    //�������̏���
    public void Instantiate()
    {
        List<StateMethod> states = new List<StateMethod>();
        states.Add(new WaitMethod(this));
        states.Add(new MoveMethod(this));
        states.Add(new MeleeAttackMethod(this));
        states.Add(new RangedAttackMethod(this));
        states.Add(new RunAwayMethod(this));
        states.Add(new DeadMethod(this));
        InstanceMethod(1, 300f, 0.5f, states);
    }

    void FixedUpdate()
    {
        FixedUpdateMehod();
    }

    [SerializeField]
    Animator animAx;

    [SerializeField]
    Transform wepon;

    public override Transform Weapon()
    {
        return wepon;
    }

    public override void Move(Vector3 dir)
    {
        if (_eb.onFall) return;
        Vector3 vel = Vector3.SmoothDamp(_eb.RB.velocity, dir * _eb.moveSpeed * Time.fixedDeltaTime, ref _eb.velocity, m_MovementSmoothing);
        _eb.RB.velocity = vel;
        _eb.AM.SetFloat("Float_Walk", Mathf.Abs(vel.x));
        animAx.SetFloat("Float_Walk", Mathf.Abs(vel.x));
    }   

    public override void Trigger_Attack(bool value)
    {
        _eb.AM.SetTrigger("Attack");
        animAx.SetTrigger("Attack");
    }

    public override void Damaged(int damage)
    {
        base.Damaged(damage);
        _eb.AM.SetTrigger("Damaged");
        animAx.SetTrigger("Damaged");
    }

    public override void Dead()
    {
        base.Dead();
        _eb.AM.SetTrigger("Dead");
        animAx.SetTrigger("Dead");
    }
}
