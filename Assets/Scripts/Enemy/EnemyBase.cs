using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*----------------------------------------------------------------*/
/*
/*  敵キャラクターの基底クラス
/*  作者: 匿名希望
/*  更新日:
/*
/*----------------------------------------------------------------*/

public class EnemyBase : MonoBehaviour, IDamageable
{
    /*--------------------------------------*/
    /*  メンバ変数定義
    /*--------------------------------------*/

    List<StateMethod> StateMachine;//状態マシン
    public EnemyStatus status;//敵キャラクターのステータス
    Transform group;//敵キャラクターグループ

    /// <summary>
    /// 変数の引き出し用クラス
    /// </summary>
    /// ・インスペクターに表示しないため、管理しやすくするため
    public Enemy_Base_Class _eb;

    /// <summary>
    /// 初期化関数
    /// </summary>
    /// <param name="hp">体力</param>
    /// <param name="speed">移動速度</param>
    /// <param name="motivation">行動意欲</param>
    /// <param name="list">状態マシン</param>
    public void InstanceMethod(int hp, float speed, float motivation, List<StateMethod> list)
    {
        status = new EnemyStatus(hp, speed, motivation);
        group = transform.parent;
        _eb = new Enemy_Base_Class(gameObject);
        StateMachine = list;
        currentStateNum = 0;
    }

    int currentStateNum = 0;
    int beforeStateNum = -1;

    public bool StateStopper = false;

    //更新メソッド
    public void FixedUpdateMehod()
    {
        if (StateMachine == null || StateStopper)
        {
            _eb.RB.velocity = Vector2.zero;
            return;
        }
            
        if (beforeStateNum != currentStateNum)
        {
            StateMachine[currentStateNum].OnEnter();
            beforeStateNum = currentStateNum;
        }

        StateMachine[currentStateNum].Main();

        int nextStateNum = StateMachine[currentStateNum].CheckTransitions();

        if (nextStateNum != currentStateNum)
        {
            StateMachine[currentStateNum].OnExit();
            currentStateNum = nextStateNum;
        }
    }

    /*-------------------------------------*/
    /*  以下、敵キャラクターの移動関数
    /*-------------------------------------*/

    //移動
    [Range(0, .3f)] public float m_MovementSmoothing = .05f;
    public Transform GroundCheck;
    public Transform[] WallCheck = new Transform[2];

    //オブジェクト検索
    public bool SearchObj<T>(ref Transform target)
    {
        foreach (Transform child in group)
        {
            if (child.gameObject.GetComponent<T>() != null)
            {
                target = child;
                return true;
            }
        }
        return false;
    }

    //移動
    public virtual void Move(Vector3 dir)//Vector3.right,left等の方向を持つベクトルを引数とする
    {
        if (_eb.onFall) return;
        Vector3 vel = Vector3.SmoothDamp(_eb.RB.velocity, dir * _eb.moveSpeed * Time.fixedDeltaTime, ref _eb.velocity, m_MovementSmoothing);
        _eb.RB.velocity = vel;
        //Debug.Log("Walk" + vel);
        _eb.AM.SetFloat("Float_Walk", Mathf.Abs(vel.x));
    }

    //ランダムな方向を設定
    public void SetRandomDir()
    {
        float dir_x = Random.Range(0, 2) == 0 ? -1 : 1;
        if (dir_x > 0.0f && !_eb.FacetoRight || dir_x < 0.0f && _eb.FacetoRight) Flip();
    }

    //プレイヤーの方向を設定
    public void SetDir()
    {
        if (_eb.player == null) return;
        float myPos_x = transform.position.x;
        float playerPos_x = _eb.player.position.x;
        if (playerPos_x > myPos_x && !_eb.FacetoRight || myPos_x > playerPos_x && _eb.FacetoRight) Flip();
    }

    //反転
    public void Flip()
    {
        _eb.FacetoRight = !_eb.FacetoRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    //攻撃アニメーション
    public void Bool_Attack(bool value)
    {
        _eb.AM.SetBool("Attack", value);
        Debug.Log("Attack" + value);
    }
    public virtual void Trigger_Attack(bool value)
    {
        if (value) _eb.AM.SetTrigger("AttackStart"); else _eb.AM.SetTrigger("AttackEnd");
    }

    //前方ビュー
    public Vector2 FrontView()
    {
        return _eb.FacetoRight ? Vector2.right : Vector2.left;
    }

    //周囲ビュー
    public Vector2 AroundView()
    {
        return (_eb.player.position - transform.position).normalized;
    }

    /// <summary>
    /// Vector2 FrontView() と Vector2 AroundView()のを組み合わせて使う
    /// </summary>
    /// Vector2 dir = _eb.FacetoRight ? Vector2.right: Vector2.left;
    /// Vector2 dir = (transform.position - _eb.player.position).normalized;
    public bool View(Vector2 dir)
    {
        float distance = 20.0f;
        RaycastHit2D hit2D;
        Vector3 offsetPos = transform.position;
        offsetPos.y += 0.5f;
        offsetPos.x += _eb.FacetoRight ? 1.0f : -1.0f;
        Ray2D ray2D = new Ray2D(offsetPos, dir);
        //Debug.DrawRay(ray2D.origin, ray2D.direction * distance, Color.red);
        if (Physics2D.Raycast(ray2D.origin, ray2D.direction, distance))
        {
            hit2D = Physics2D.Raycast(ray2D.origin, ray2D.direction, distance);
            //Debug.Log("Hit object: " + hit2D.collider.gameObject.tag);
            if (hit2D.collider.gameObject.tag == "Player")
            {
                return true;
            }
        }
        return false;
    }

    //障害物回避
    public void AvoidObstacles()
    {
        float rad = _eb.GroundedRadius;

        Count += Time.fixedDeltaTime;
        if (_eb.AvoidCount > 4)
        {
            Debug.Log(Count);
            if (Count < 1.0f)
            {
                _eb.SafeBool = true;
            }
            Count = 0;
            _eb.AvoidCount = 0;
        }

        if (!CheckFloor() || CheckColider(WallCheck[0].position, rad))
        {
            _eb.AvoidCount++;
            Flip();
        }
        else if (CheckColider(WallCheck[1].position, rad)) Jump();
    }
    float Count;

    //階段チェック
    public void CheckStair()
    {
        float rad = _eb.GroundedRadius;
        if (!CheckColider(WallCheck[0].position, rad) && CheckColider(WallCheck[1].position, rad))
            Jump();
    }

    //床チェック
    public bool CheckFloor()
    {
        if (_eb.SafeBool) return true;

        if (CheckColider(GroundCheck.position, _eb.GroundedRadius))
        {
            return true;
        }
        return false;
    }

    //壁チェック
    public bool CheckWall()
    {
        if (CheckColider(WallCheck[0].position, _eb.GroundedRadius))
            return true;
        return false;
    }

    //ジャンプ
    public virtual void Jump()
    {
        Debug.Log("Jump");
        _eb.RB.AddForce(Vector2.up * 170f);
    }

    //当たり判定チェック
    private bool CheckColider(Vector3 pos, float Rad)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, Rad);
        for (int k = 0; k < colliders.Length; k++)
        {
            if (colliders[k].gameObject != gameObject)
                return true;
        }
        return false;
    }

    /*-------------------------------*/
    /*  以下、各種関数
    /*-------------------------------*/

    //武器
    public virtual Transform Weapon()
    {
        return null;
    }

    //サブ武器
    public virtual Transform subWeapon()
    {
        return null;
    }

    //歩行アニメーションリセット
    public void WalkAnimReset()
    {
        _eb.AM.SetFloat("Float_Walk", 0f);
    }

    //待機時間計算
    public float CalcWaitTime()
    {
        float basecount = _eb.base_wait_count;
        return basecount - (status.FightMotivation * basecount * 0.75f);
    }

    //プレイヤーへの距離計算
    public bool CalcDistanceToPlayer()
    {
        if (_eb.player == null) return false;

        if (Mathf.Abs(Vector3.Distance(_eb.player.position, transform.position)) < _eb.threshold_enemy)
        {
            return true;
        }
        return false;
    }

    //プレイヤーとの距離
    public float DistanceToPlayer()
    {
        return Mathf.Abs(_eb.player.position.x - transform.position.x);
    }

    //モーション時間取得
    public float GetMotionTime()
    {
        return _eb.AM.GetCurrentAnimatorStateInfo(0).length;
    }

    //プレイヤー探索
    public bool FindPlayer()
    {
        _eb.player = GameManager.instance.player.transform;
        if (_eb.player != null)
        {
            return true;
        }
        return false;
    }

    //速度設定
    public void SetSpeed()
    {
        _eb.moveSpeed = status.MoveSpeed;
    }
    public void SetSpeed(float x)//オーバーロード
    {
        _eb.moveSpeed = status.MoveSpeed * x;
    }

    //生存チェック
    public bool AliveCheck()
    {
        if (status.HP <= 0)
        {
            return true;
        }
        return false;
    }

    //ダメージ処理
    public virtual void Damaged(int damage)
    {
        StateStopper = true;
        status.HP -= damage;
        _eb.AM.SetBool("Damaged", true);
        if (status.HP > 0){
            StartCoroutine(DamagedCor());
        }
        EffectManager.instance.Play_AttackEffect(transform.position);
        StateStopper = false;
    }

    IEnumerator DamagedCor()
    {
        yield return new WaitForSeconds(1.0f);
        _eb.AM.SetBool("Damaged", false);
        _eb.RB.isKinematic = false;
        _eb.AM.speed = 1.0f;
        //StateStopper = false;
    }

    public void DefiniteDamaged()
    {
        _eb.RB.isKinematic = true;
        _eb.AM.speed = 0f;
        _eb.moveSpeed = 0;
        StateStopper = true;
    }

    //死亡処理
    public virtual void Dead()
    {
        Debug.Log("dead1");
        GameManager.instance.RemoveList(this);
        StartCoroutine(DeadCor());
        //GameManager.instance.GetComponent<AudioSource>().PlaySFX_Player(6);
    }

    IEnumerator DeadCor()
    {
        Debug.Log("dead2");
        EffectManager.instance.Play_guro(transform.position);
        GameManager.instance.audioPlayer.PlaySFX_Player(6);
        yield return null;
        Destroy(gameObject);
    }

    public Vector2 EnemyPos()
    {
        //Debug.Log(transform.position);
        return transform.position;
    }

    public Transform EnemyTf()
    {
        return transform;
    }

    public IEnumerator HitChecker(int damage,float count,float rad,Transform rangePos)
    {
        yield return new WaitForSeconds(0.3f);
        float Count = 0;
        float MaxCount = count;
        int Damage = damage;
        float Rad = rad;

        while (Count < MaxCount)
        {
            Debug.Log("Attack");

            Count += Time.deltaTime;
            
            Collider2D[] colliders = Physics2D.OverlapCircleAll(rangePos.position, Rad);
            for (int k = 0; k < colliders.Length; k++)
            {
                string Tag = colliders[k].gameObject.tag;
                if (Tag == "Player")
                {
                    colliders[k].gameObject.GetComponent<PlayerChara>().Damaged(Damage);
                    yield break;
                }
            }
            yield return null;
        }
    }
}

//敵キャラクターのステータス
public class EnemyStatus
{
    public int HP;//hp
    public float MoveSpeed;//speed
    [Range(0, 1f)] public float FightMotivation;//行動意欲

    public EnemyStatus(int hp, float moveSpeed, float fightMotivation)
    {
        HP = hp;
        MoveSpeed = moveSpeed;
        FightMotivation = fightMotivation;
    }
}

//状態マシン
public class StateMethod
{
    public EnemyBase EB;
    public StateMethod(EnemyBase e) { EB = e; }

    public virtual void OnEnter() { }

    public virtual void Main() { }

    public virtual void OnExit() { }

    public virtual int CheckTransitions() { return 0; }
}

//敵キャラクターの基本クラス
public interface IDamageable
{
    public void Damaged(int damage);
}

//敵キャラクターの基本クラス
public class Enemy_Base_Class
{
    public Rigidbody2D RB;
    public Animator AM;
    public Vector3 velocity = Vector3.zero;
    public Transform player;

    public float moveSpeed;

    public float threshold_enemy = 1.5f;
    public float GroundedRadius = 0.1f;
    public float AttackedRadius = 2.0f;

    public bool FacetoRight = true;
    public bool Canjump = false;
    public bool onFall = false;

    public bool SafeBool;
    public int AvoidCount;
    public float base_wait_count = 3.5f;

    public Enemy_Base_Class(GameObject chara)
    {
        RB = chara.GetComponent<Rigidbody2D>();
        AM = chara.GetComponent<Animator>();
    }
}
