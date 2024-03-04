using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*--------------------------------------------------------*/
/*
/*  要項　Enemy_Base
/*  ・このクラスは敵キャラの基底クラスである。                    
/*  ・序盤の基本的な敵の動きを提供する
/*　・関数の上書きをすることで独自の動き処理を実装する
/*
/*--------------------------------------------------------*/

public class Enemy_Base : MonoBehaviour
{
    //当たり判定・動きの滑らかさ
    [Range(0, .3f)] public float m_MovementSmoothing = .05f;
    public Transform GroundCheck;
    public Transform[] WallCheck = new Transform[2];

    //ベースステート
    BaseState currentState;

    /*------------------------------------*/
    /*     継承以外から参照される関数            　
    /*------------------------------------*/

    //初期化
    public void Init(float defspeed, float chasespeed,float waittime,float AttackRange)//tank,Init(350f, 400f);
    {
        //enemy_base = new Enemy_Base_Class(gameObject, defspeed, chasespeed,waittime,AttackRange);
        currentState = BaseState.WAIT;
    }

    //被ダメージ
    public virtual void Damaged()
    {
        Debug.Log("Damaged");
        //GameObject _eff = Instantiate(eff, transform.position, Quaternion.identity);
        //_eff.transform.SetParent(transform);
        StartCoroutine(MotionTime_Damaged());
        enemy_base.AM.SetTrigger("Damaged");
    }

    //被ダメージのコルーチン
    IEnumerator MotionTime_Damaged()
    {
        yield return new WaitForSeconds(1.0f);
        Guro guro = FindObjectOfType<Guro>();
        //guro.StartGuro(transform.position);
        Destroy(gameObject);
    }

    /*----------------------------------*/
    /*     継承で使われる変数・関数      　　  　     　
    /*----------------------------------*/

    /// <summary>
    /// 変数の引き出し　
    /// </summary>
    /// ・public変数を全て定義するとヒエラルキーに表示してしまうので変数をクラス化
    /// ・管理しやすく
    public Enemy_Base_Class enemy_base;

    //継承で使うFixedUpdateメソッド
    public void FixedUpdateMehod()
    {
        CheckFront();
        if (!enemy_base.Canjump && enemy_base.onFall && CheckGround())
        {
            enemy_base.Canjump = true;
        }
        switch (currentState)
        {
            case BaseState.WAIT:
                enemy_base.count += Time.fixedDeltaTime;
                if (enemy_base.count > 1f)
                {
                    enemy_base.count = 0;
                    SetRandomDir();
                    currentState = BaseState.WALK;//to Walk state
                }
                break;
            case BaseState.WALK:
                WalkMethod_Evaluation();
                if (enemy_base.Canjump)
                {
                    if (!CheckFloor() || CheckColider(WallCheck[0].position))
                    {
                        enemy_base.timmer_walk = enemy_base.count;
                        enemy_base.count = 0;
                        enemy_base.currentdir_x *= -1;
                        enemy_base.Isback = Random.Range(0, 3) == 0 ? true : false;

                        if (enemy_base.currentdir_x > 0.0f && !enemy_base.FacetoRight || enemy_base.currentdir_x < 0.0f && enemy_base.FacetoRight) Flip();
                    }
                    else if (CheckColider(WallCheck[1].position))
                    {
                        enemy_base.Canjump = false;
                        enemy_base.RB.AddForce(Vector2.up * 300f);
                        enemy_base.jumpCount++;
                    }
                }
                WalkMethod();
                WalkMethod_Branching();
                break;
            case BaseState.CHASE:
                ChaseMethod();
                WalkMethod();
                break;
            case BaseState.ESCAPE:
                EscapeMethod();
                break;
            case BaseState.ATTACK:
                AttackMethod();
                break;
        }
    }

    /*-----------------------------------------------*/
    /*     Enemy_Base FixedUpdateで使われる関数        　　   　
    /*-----------------------------------------------*/

    public virtual void WalkMethod_Evaluation()
    {
        /*
enemy_base.count += Time.fixedDeltaTime;
if (enemy_base.Isback)
{
    if (enemy_base.count > enemy_base.timmer_walk)
    {
        enemy_base.Isback = false;
        enemy_base.count = 0;
        enemy_base.currentdir_x = 0;
        currentState = BaseState.WAIT;
    }
}*/
    }

    public virtual void WalkMethod()
    {
        Vector3 movement = new Vector3(enemy_base.currentdir_x, 0f, 0f);
        enemy_base.RB.velocity = Vector3.SmoothDamp(enemy_base.RB.velocity, movement * enemy_base.moveSpeed * Time.fixedDeltaTime, ref enemy_base.velocity, m_MovementSmoothing);
        enemy_base.AM.SetFloat("Float_Walk", Mathf.Abs(enemy_base.currentdir_x));
    }

    public virtual void WalkMethod_Branching()
    {
        if (enemy_base.ho.tag == "Player")//Playerが視界にいるなら　
        {
            enemy_base.moveSpeed = enemy_base.ChaseSpeed;
            currentState = BaseState.CHASE;//to Chase state
        }
    }

    public virtual void ChaseMethod()
    {
        CheckFront();
        if (enemy_base.ho.tag != "Player")
        {
            enemy_base.moveSpeed = enemy_base.DefaultSpeed;
            currentState = BaseState.WAIT;
        }
        else if (Mathf.Abs(enemy_base.ho.obj.transform.position.x - transform.position.x) < enemy_base.threshold_enemy)
        {
            currentState = BaseState.ATTACK;
        }      
    }

    public virtual void EscapeMethod()
    {
    }

    public virtual void AttackMethod()
    {
        currentState = BaseState.NONE;
        enemy_base.AM.SetTrigger("Attack");
        StartCoroutine(MotionTime_Attack());            
    }

    public virtual IEnumerator MotionTime_Attack()
    {
        yield return new WaitForSeconds(0.5f);
        if (CheckAttack())
        {
            Debug.Log("Attacked");
        }     
        float normalizedTime = enemy_base.AM.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(normalizedTime - 0.4f);
        currentState = BaseState.CHASE;
    }

    /*--------------------------------------------*/
    /*     　　　　 Enemy_Base メイン
    /*--------------------------------------------*/

    //前方チェック
    public virtual void CheckFront()//virtual void
    {
        float distance = 20.0f;
        Vector2 dir = Vector2.right;
        Vector3 rayoffset = new Vector3(1.0f, 0.5f, 0.0f);
        if (!enemy_base.FacetoRight)
        {
            rayoffset.x *= -1.0f;
            dir *= -1;
        }
        RaycastHit2D hit2D;
        Ray2D ray2D = new Ray2D(transform.position + rayoffset, dir);

        Debug.DrawRay(ray2D.origin, ray2D.direction * distance, Color.red);

        if (Physics2D.Raycast(ray2D.origin, ray2D.direction, distance))
        {
            hit2D = Physics2D.Raycast(ray2D.origin, ray2D.direction, distance);
            //Debug.Log("Hit object: " + hit2D.collider.gameObject.tag);
            if (hit2D.collider.gameObject != gameObject)
            {
                enemy_base.ho = new HitObj(hit2D.collider.gameObject.tag, hit2D.collider.gameObject, hit2D.point);
            }
        }
    }

    //ランダムな方向決定
    private void SetRandomDir()
    {
        float dir_x = Random.Range(0, 2) == 0 ? -1 : 1;
        if (dir_x > 0.0f && !enemy_base.FacetoRight || dir_x < 0.0f && enemy_base.FacetoRight) Flip();
        enemy_base.currentdir_x = dir_x;
    }

    //コライダーチェック
    private bool CheckColider(Vector3 pos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, enemy_base.GroundedRadius);
        for (int k = 0; k < colliders.Length; k++)
        {
            if (colliders[k].gameObject != gameObject)
                return true;
        }
        return false;
    }

    //攻撃判定チェック
    private bool CheckAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(WallCheck[1].position, enemy_base.AttackedRadius);
        for (int k = 0; k < colliders.Length; k++)
        {
            if (colliders[k].gameObject.tag == "Player")
                return true;
        }
        return false;
    }

    //床チェック
    private bool CheckFloor()
    {
        if (CheckColider(GroundCheck.position))
        {
            return true;
        }

        if (enemy_base.jumpCount > 0)
        {
            enemy_base.jumpCount--;
            enemy_base.Canjump = false;    
            return true;
        }
        return false;
    }

    //地面チェック
    private bool CheckGround()
    {
        Vector3 groundPos = GroundCheck.position;
        groundPos.x = transform.position.x;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundPos, enemy_base.GroundedRadius);
        for (int k = 0; k < colliders.Length; k++)
        {
            if (colliders[k].gameObject.tag == "Obstacle" || colliders[k].gameObject.tag == "Blocks")
                return true;
        }
        return false;
    }

    //フリップ
    private void Flip()
    {
        enemy_base.FacetoRight = !enemy_base.FacetoRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    //モーションタイム
    public　IEnumerator MotionTime(float x)
    {
        yield return new WaitForSeconds(x);
    }
    public class Enemy_Base_Class
    {
        public Rigidbody2D RB;
        public Animator AM;

        public float moveSpeed;

        public float threshold_enemy = 1.5f;
        public float GroundedRadius = 0.1f;
        public float AttackedRadius = 2.0f;

        public bool FacetoRight = true;
        public bool Isback = false;
        public bool Canjump = false;

        public float count;
        public float timmer_wait_base = 3.5f;
        public float timmer_walk = 3.0f;

        public float base_wait_count = 3.5f;

        public int jumpCount;

        public HitObj ho;
        public Vector3 velocity = Vector3.zero;
        public float currentdir_x;

        public float DefaultSpeed;
        public float ChaseSpeed;

        public float motionTime;
        public bool onFall = false;

        public Transform player;

        public Enemy_Base_Class(GameObject chara)
        {
            RB = chara.GetComponent<Rigidbody2D>();
            AM = chara.GetComponent<Animator>();
        }
    }
}

public enum BaseState
{
    NONE,
    WAIT,
    WALK,
    CHASE,
    ATTACK,
    ESCAPE
}


//RayHit時のオブジェクト処理
public struct HitObj
{
    public string tag;
    public GameObject obj;
    public Vector3 point;
    public HitObj(string _tag, GameObject _obj, Vector3 _point)
    {
        tag = _tag;
        obj = _obj;
        point = _point;
    }

    public void Clear()
    {
        tag = string.Empty;
        obj = null;
        point = Vector3.zero;
    }
}

