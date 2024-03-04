using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*--------------------------------------*/
/*プレイヤーコントローラー
 *
 *このスクリプトは、プレイヤーの移動を制御します
 * 
 *・移動
 *　プレイヤーの移動を行います
 *　プレイヤーが画面端に到達した場合、Flip関数を呼び出して反転させます
 *
 *・L/Rトリガー
 *　Lボタンを押すと、プレイヤーが左に移動します
 *　Rボタンを押すと、プレイヤーが右に移動します
 * 
 *・X + LRButton
 *　XボタンとLボタンまたはRボタンを同時に押すと、ジャンプします
 * 
 *・Aボタン
 *　攻撃を行います
 * 
 *・Bボタン
 *　防御します
 *
 *・その他
 *　HPバーUIを制御します
 *　プレイヤーがダメージを受けると、HPが減少し、HPバーが更新されます
 *　プレイヤーが敵に触れると、ダメージが与えられます
 *　敵の攻撃範囲内にいる場合、ダメージを受けます
/*--------------------------------------*/

public class PlayerChara : MonoBehaviour
{
    private PlayerOperation pope;
    private PlayerMoveMent pmove;
    private PlayerArrow pArrow;

    private bool isArrowMoving = false;

    public bool IsArrowMoving
    {
        get { return isArrowMoving; }
        set { isArrowMoving = value; }
    }

    public Transform GroundCheck;
    public Transform Arrow;
    public Transform FeatherBallet;
    public Transform[] HitChecks;

    void Start()
    {
        pmove = new PlayerMoveMent(this);
        pope = new PlayerOperation(this);
        pArrow = new PlayerArrow(this);
    }

    private bool canMove = true;

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }

    private void FixedUpdate()
    {    
        if(CanMove){
            pmove.MOVE(pope.MoveValue_X);
            pmove.GroundCheck(GroundCheck.position);
        }
        else if(pArrow.DisplayArrow)
        {
            pmove.Reset();
        }
        pmove.FallControl();
        pArrow.MOVE(pope.MoveValue, transform.position);
    }

    //ADS
    public void AimDownSight(bool IsStart)
    {
        pArrow.ADS(IsStart);
        //pArrow.ADS(IsStart);
    }

    /*
    //撃つ
    public void Shoot(bool IsStart)
    {
        //pArrow.Shoot(IsStart);
        pArrow.tema();
    }*/

    //アイテム使用1
    public void UseItem_L()
    {
    }
    //アイテム使用2
    public void UseItem_R()
    {
    }

    //ジャンプ
    public void Jump()
    {
        pmove.JUMP();
    }

    //決定ボタン
    public void DecideButton()
    {
        Debug.Log("����{�^��");
    }

    //小攻撃
    Coroutine myCoroutine;
    public void LightAttack()
    {
        if(myCoroutine == null){
            myCoroutine = StartCoroutine(WaitMotion(pmove.LightAttack()));           
        }
    }

    public IEnumerator LarpToEnemy(Vector3 Enemypos)
    {
        if (myCoroutine != null)
        {
            myCoroutine = null;
        }
        yield return myCoroutine = StartCoroutine(WaitMotion(pmove.DashToEnemy(Enemypos)));
    }

    public IEnumerator LarpToBlock(Vector3 first,Vector3 second,BreakWall targetWall,Vector3 dir)
    {
        if (myCoroutine != null)
        {
            myCoroutine = null;
        }
        yield return myCoroutine = StartCoroutine(WaitMotion(pmove.DashToBlock(first,second,targetWall,dir)));
    }

    IEnumerator WaitMotion(IEnumerator ie)
    {
        yield return StartCoroutine(ie);
        myCoroutine = null;
    }

    public void Damaged(int damage)
    {
        if (Nodamage) return;

        GameManager.instance.hpm.Damaged(damage);

        if(gameObject.activeSelf)
        StartCoroutine(NoDamage());
    }

    public bool Nodamage = false;

    IEnumerator NoDamage()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        Nodamage = true;
        int blinkCount = 4;
        float blinkInterval = 0.5f;
        for (int i = 0; i < blinkCount; i++)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(blinkInterval / 2);
            GetComponent<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(blinkInterval / 2);
        }
        GetComponent<SpriteRenderer>().color = Color.white;
        Nodamage = false;
    }
}

public class PlayerOperation{

    PlayerChara playerScript;
    PlayerCtr inputActions;
    public Vector2 MoveValue = new Vector2();
    public Vector2 RotateValue = new Vector2();
    public float MoveValue_X { get { return MoveValue.x; } }
    public float MoveValue_Y { get { return MoveValue.y; } }
    public PlayerOperation(PlayerChara _ps)
    {
        playerScript = _ps;
        inputActions = new PlayerCtr();
        inputActions.GamePad.Enable();
        inputActions.GamePad.Move.performed += ctx => MoveValue = ctx.ReadValue<Vector2>();
        inputActions.GamePad.Move.canceled += ctx => MoveValue = Vector2.zero;
        inputActions.GamePad.Rotate.performed += ctx => RotateValue = ctx.ReadValue<Vector2>();
        inputActions.GamePad.Rotate.canceled += ctx => RotateValue = Vector2.zero;
        inputActions.GamePad.Jump.performed += ctx => playerScript.Jump();
        inputActions.GamePad.Attack_normal.performed += ctx => playerScript.LightAttack();
        inputActions.GamePad.LB.performed += ctx => playerScript.UseItem_L();
        inputActions.GamePad.RB.performed += ctx => playerScript.UseItem_R();
        //inputActions.GamePad.LT.performed += ctx => playerScript.AimDownSight(true);
        inputActions.GamePad.RT.performed += ctx => playerScript.AimDownSight(true);
        //inputActions.GamePad.LT.canceled += ctx => playerScript.AimDownSight(false);
        inputActions.GamePad.RT.canceled += ctx => playerScript.AimDownSight(false);
    }
}

public class PlayerMoveMent{
    PlayerChara playerScript;
    Transform playerTf;
    float MoveSpeed = 500f;
    float JumpForce = 80f;
    Vector3 tempvelocity = Vector3.zero;
    Animator AM;
    Rigidbody2D RB;
    [Range(0, .3f)] private float MoveSmoothing = .05f;
    [Range(0, .3f)] private float FlipSmoothing = .05f;
    public PlayerMoveMent(PlayerChara _p)
    {
        playerScript = _p;
        playerTf = _p.transform;
        AM = _p.gameObject.GetComponent<Animator>();
        RB = _p.gameObject.GetComponent<Rigidbody2D>();
        Gravity = RB.gravityScale;
    }
    //X軸の移動
    float PlayerMoveValue;
    float limitFallSpeed = 25f;
    public void MOVE(float L_stick_value)
    {
        float horizon_InputValue;
        horizon_InputValue = L_stick_value * MoveSpeed;
        AM.SetFloat("walk", Mathf.Abs(horizon_InputValue));
        Vector3 movement = new Vector3(horizon_InputValue * Time.fixedDeltaTime, RB.velocity.y, 0f);
        RB.velocity = Vector3.SmoothDamp(RB.velocity, movement, ref tempvelocity, MoveSmoothing);

        if (horizon_InputValue > 0.0f){
            PlayerMoveValue = Mathf.Clamp(PlayerMoveValue + Time.fixedDeltaTime, 0, 3.0f);
            if (!FacetoRight && Mathf.Abs(PlayerMoveValue) >= FlipSmoothing){
                FLIP();
                PlayerMoveValue = 0;
            }
        }
        // horizon_InputValueが反転したとき
        else if (horizon_InputValue < 0.0f){
            PlayerMoveValue = Mathf.Clamp(PlayerMoveValue - Time.fixedDeltaTime, -3.0f, 0);
            if (FacetoRight && Mathf.Abs(PlayerMoveValue) >= FlipSmoothing){
                FLIP();
                PlayerMoveValue = 0;
            }
        }           
    }
    public void Reset() {
        RB.velocity = new Vector2(0,RB.velocity.y);
        AM.SetFloat("walk", 0);
    }
    public void FallControl()
    {
        if (RB.velocity.y < -limitFallSpeed)
            RB.velocity = new Vector2(RB.velocity.x, -limitFallSpeed);
    }
    //振り向き
    public bool FacetoRight = true;
    private void FLIP()
    {
        FacetoRight = !FacetoRight;
        Vector3 theScale = playerTf.localScale;
        theScale.x *= -1;
        playerTf.localScale = theScale;
    }
    private float jumpCount;//ジャンプできる残り回数
    public void JUMP()
    {
        if (RB == null) return;

        if (IsGrounded && jumpCount <= 1)
        {
            jumpCount = 2;
        }
        //Debug.Log(jumpCount);
        if (jumpCount > 0)
        {       
            int number = 7;       
            if (jumpCount >= 2){
                number = 7;
                RB.velocity = new Vector2(RB.velocity.x, 0);
                RB.AddForce(new Vector2(0f, JumpForce));
            }
            else{
                number = 8;
                RB.velocity = new Vector2(RB.velocity.x, 0);
                RB.AddForce(new Vector2(0f, JumpForce * 1.6f));           
            }
            GameManager.instance.audioPlayer.PlaySFX_Player(number);//ジャンプの音
            jumpCount--;
        }
    }
    private bool IsGrounded = false;
    public void GroundCheck(Vector3 groundpos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundpos, 0.1f);
        for (int k = 0; k < colliders.Length; k++)
        {
            string Tag = colliders[k].gameObject.tag;           
            if (Tag == "Blocks" || Tag == "Obstacle")
            {                
                if (!IsGrounded)
                {
                    GameManager.instance.audioPlayer.PlaySFX_Player(4);//設置時の音
                }            
                IsGrounded = true;
                return;
            }
        }
        IsGrounded = false;
    }
    public IEnumerator LightAttack()
    {
        AM.SetTrigger("Attack");
        float dir = FacetoRight ? 1 : -1;
        RB.velocity = new Vector2(MoveSpeed * dir / 100, 0);
        float tempfloat = limitFallSpeed;
        limitFallSpeed = 0;
        yield return new WaitForSeconds(0.1f);
        RB.velocity = new Vector2(0, 0);
        yield return HitChecker(AttackRange.FlyAttack);
        yield return new WaitForSeconds(0.1f);
        limitFallSpeed = tempfloat;
    }

    float DashSpeed = 40f;//40f5f
    float arrivalThreshold = 1.0f;
    float Gravity;
    public IEnumerator DashToEnemy(Vector3 target)
    {
        float myPos_x = playerTf.position.x;
        float targetPos_x = target.x;
        if (targetPos_x > myPos_x)//right side
        {
            target.x -= 0.8f;
            if (!FacetoRight) FLIP();
        }
        else//left side
        {
            target.x += 0.8f;
            if (FacetoRight) FLIP();
        }        
        RB.isKinematic = true;
        playerScript.Nodamage = true;
        RB.gravityScale = 0;
        RB.velocity = Vector2.zero;
        AM.SetBool("Dash", true);
        EffectManager.instance.Play_shadow(playerTf, target);
        yield return MoveCor_Lerp(target);
        AM.SetBool("Dash",false);
        yield return new WaitForSeconds(0.24f);
        yield return HitChecker(AttackRange.FlyAttack);
        yield return new WaitForSeconds(0.1f);
        ///Vector2 dir = new Vector2(1,1).normalized;
        playerScript.Nodamage = false;
        RB.isKinematic = false;
        RB.gravityScale = Gravity;
        ///RB.AddForce(dir * 100f);     
    }

    IEnumerator MoveCor_Lerp(Vector3 target)
    {
        ///Vector3 startPosition = playerTf.position;
        while (Mathf.Abs(Vector3.Distance(playerTf.position, target)) >= arrivalThreshold)
        {
            playerTf.position = Vector3.MoveTowards(playerTf.position, target, DashSpeed * Time.deltaTime);//等速Ver
            //playerTf.position = Vector3.Lerp(playerTf.position, target, DashSpeed * Time.deltaTime);
            yield return null;
        }
        ///playerTf.position = target;
    }

    enum AttackRange{
        LightAttack,
        FlyAttack
    }

    IEnumerator HitChecker(AttackRange ar)
    {
        GameManager.instance.audioPlayer.PlaySFX_Player(3);
        float Count = 0;
        float MaxCount = 0;
        int Damage = 0;
        Transform target = null;
        switch (ar)
        {
            case AttackRange.LightAttack:
                target = playerScript.HitChecks[0];
                Damage = 1;
                MaxCount = 0.2f;
                break;
            case AttackRange.FlyAttack:
                target = playerScript.HitChecks[1];
                Damage = 3;
                MaxCount = 0.2f;
                break;
        }
        while (Count < MaxCount)
        {
            Count += Time.deltaTime;
            float Rad = 1.5f;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(target.position, Rad);
            for (int k = 0; k < colliders.Length; k++)
            {
                string Tag = colliders[k].gameObject.tag;
                if (Tag == "Enemy")
                {
                    colliders[k].gameObject.GetComponent<EnemyBase>().Damaged(Damage);
                    GameManager.instance.audioPlayer.PlaySFX_Player(2);
                }
            }
            yield return null;
        }
    }

    public IEnumerator DashToBlock(Vector3 first, Vector3 second, BreakWall targetWall, Vector3 dir)
    {
        float myPos_x = playerTf.position.x;
        float targetPos_x = first.x;
        if (targetPos_x > myPos_x)//right side
        {
            if (!FacetoRight) FLIP();
        }
        else//left side
{
            if (FacetoRight) FLIP();
        }

        RB.isKinematic = true;
        RB.gravityScale = 0;
        RB.velocity = Vector2.zero;
        bool flg = false;

        if (Mathf.Abs(Vector2.Distance(new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y),new Vector2(second.x, second.y))) >= 17.0f)
        {
            
        }
        else if(targetWall.ISCANBREAK())
        {
            flg = true;
            //GameManager.instance.GetComponent<AudioSource>().PlaySFX_Player(0);
            targetWall.OnBreak();
            AM.SetBool("Dash", true);
            EffectManager.instance.Play_shadow(playerTf, first);
            yield return MoveCor_Lerp(first);
            AM.SetBool("Dash", false);
            GameManager.instance.audioPlayer.PlaySFX_Player(0);
            yield return new WaitForSeconds(1.0f);
            while (Mathf.Abs(Vector3.Distance(playerTf.position, second)) >= 0.5f)
            {
                playerTf.position = Vector3.MoveTowards(playerTf.position, second, 25f * Time.deltaTime);
                yield return null;
            }
            GameManager.instance.StartVeiwChange(dir, flg);
        }
        else if (targetWall.ISBREAKED())
        {
            flg = false;
            AM.SetBool("DashOnly", true);
            GameManager.instance.ChangeRoom(dir);
            GameManager.instance.StartVeiwChange(dir, flg);
            EffectManager.instance.Play_shadow(playerTf, first);
            while (Mathf.Abs(Vector3.Distance(playerTf.position, second)) >= 0.5f)
            {
                playerTf.position = Vector3.MoveTowards(playerTf.position, second, 25f * Time.deltaTime);
                yield return null;
            }
            AM.SetBool("DashOnly", false);
        }
        //AM.SetBool("Dash", true);
           
        RB.isKinematic = false;
        RB.gravityScale = Gravity;
        playerScript.CanMove = true;
    }
}

public class PlayerArrow
{
    //スクリプト参照
    PlayerChara playerScript;
    Transform Arrow;
    Transform Ballet;
    Animator AT;
    public PlayerArrow(PlayerChara a)
    {
        playerScript = a;
        Arrow = a.Arrow;
        Arrow.parent = null;
        Ballet = a.FeatherBallet;
        AT = Arrow.gameObject.GetComponent<Animator>();
    }
    Vector2 currentValue = Vector2.right;
    float currentAngle = 0;
    public void MOVE(Vector2 RotateValue,Vector3 PlayerPos)
    {
        if (!playerScript.IsArrowMoving)
        {                      
            if (RotateValue != Vector2.zero)
            {
                currentValue = RotateValue.normalized * 2;
                currentAngle = Mathf.Atan2(currentValue.y - 0, currentValue.x - 0) * Mathf.Rad2Deg;
                currentValue = ValueControl(PlayerPos);
                Arrow.transform.position = new Vector3(PlayerPos.x + currentValue.x, PlayerPos.y + currentValue.y, PlayerPos.z);
                Arrow.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            }
            else{
                Arrow.transform.position = new Vector3(PlayerPos.x + currentValue.x, PlayerPos.y + currentValue.y, PlayerPos.z);
            }         
        }
    }

    List<EnemyBase> enemies = new List<EnemyBase>();

    Vector2 ValueControl(Vector3 PPos)
    {           
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;
            Vector2 dirToEnemy = (enemies[i].EnemyPos() - new Vector2(PPos.x, PPos.y)).normalized;
            float enemyAngle = Mathf.Atan2(dirToEnemy.y, dirToEnemy.x) * Mathf.Rad2Deg;
            float angleDifference = Mathf.Abs(currentAngle - enemyAngle);
            //Debug.Log(enemyAngle +" a "+ angleDifference);
            if (angleDifference <= 25f)
            {
                //Debug.Log("Lock on");
                currentAngle = enemyAngle;
                return dirToEnemy * 2;
            }
        }
        return currentValue;
    }

    public bool DisplayArrow = false;
    bool DisplayForce = false;
    public void ADS(bool IsStart)
    {
        enemies = GameManager.instance.EnemyList;
        if (IsStart)
        {
            if (playerScript.IsArrowMoving)
            {
                Debug.Log("aiming aim");
                DisplayForce = true;
                playerScript.CanMove = false;
                return;
            }

            DisplayArrow = IsStart;
            AT.SetBool("Aiming", DisplayArrow);
            playerScript.CanMove = false;

        }
        else
        {
            if (DisplayForce)
            {
                return;
            }

            Shoot();
            GameManager.instance.audioPlayer.PlaySFX_Player(5);
        }
    }

    public void Shoot()
    {
        if (!playerScript.IsArrowMoving)
        {
            if (DisplayArrow)
            {
                playerScript.IsArrowMoving = true;
                ///GameObject.Instantiate(Ballet,Arrow.position,Quaternion.Euler(0, 0, currentAngle));
                playerScript.StartCoroutine(moveforward());              
            }          
        }
    }
    IEnumerator moveforward()
    {
        float acceleration = 40f;
        float maxSpeed = 45.0f;
        float currentSpeed = 20f;
        /*
        float count = 0;
        while (count < 0.1f)
        {
            Arrow.Translate(Vector3.left * currentSpeed * Time.deltaTime);
            count += Time.deltaTime;
            yield return null;
        }*/
        while (currentSpeed < maxSpeed)
        {
            Arrow.Translate(Vector3.right * currentSpeed * Time.deltaTime);
            currentSpeed += acceleration * Time.deltaTime;
            if (CheckColider())
            {
                if (HitObj.obj.tag == "Enemy")
                {
                    //Debug.Log(HitObj.obj.name + HitObj.obj.transform.position);
                    HitObj.obj.GetComponent<EnemyBase>().DefiniteDamaged();
                    yield return playerScript.LarpToEnemy(HitObj.obj.transform.position);
                }
                else if (HitObj.obj.tag == "Blocks")
                {
                    //Debug.Log(HitObj.obj.name + HitObj.obj.transform.position);
                    BreakWall bw = HitObj.obj.GetComponent<BreakWall>();
                    WallPattern wallPattern = bw.CurrentWallPtn;
                    Vector3 dir = Vector3.zero;
                    Vector3 first = HitObj.point;
                    Vector3 second = HitObj.point;

                    if (wallPattern == WallPattern.UP || wallPattern == WallPattern.DOWN)
                    {
                        if (CheckY(HitObj.point))
                        { second.y += offset_wall.y + 2.0f; first.y -= offset_wall.y; dir = Vector3.up; }
                        else
                        { second.y -= offset_wall.y + 2.0f; first.y += offset_wall.y; dir = -Vector3.up; }
                    }
                    else
                    {
                        if (CheckX(HitObj.point))
                        { second.x += offset_wall.x + 2.0f; first.x -= offset_wall.x; dir = Vector3.right; }
                        else
                        { second.x -= offset_wall.x + 2.0f; first.x += offset_wall.x; dir = -Vector3.right; }
                    }

                    yield return playerScript.LarpToBlock(first,second,bw,dir);
                }
                break;
            }
            yield return null;
        }

        if (DisplayForce)
        {
            DisplayForce = false;
            playerScript.IsArrowMoving = false;
        }
        else
        {
            DisplayArrow = false;
            AT.SetBool("Aiming", DisplayArrow);
            playerScript.IsArrowMoving = false;
            playerScript.CanMove = true;
        }
    }

    private Vector2 offset_wall = new Vector2(2.0f, 2.0f);
    public bool CheckX(Vector3 targetPos)
    {
        if (targetPos.x > playerScript.transform.position.x)
        {
            return true;
        }
        return false;
    }

    public bool CheckY(Vector3 targetPos)
    {
        if (targetPos.y > playerScript.transform.position.y)
        {
            return true;
        }
        return false;
    }

    TargetData HitObj;   
    bool CheckColider()
    {
        float Rad = 0.3f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Arrow.position, Rad);
        for (int k = 0; k < colliders.Length; k++)
        {
            string Tag = colliders[k].gameObject.tag;
            if (Tag == "Enemy" || Tag == "Blocks")
            {
                Vector3 closestPoint = colliders[k].ClosestPoint(Camera.main.transform.position);
                HitObj = new TargetData(colliders[k].gameObject, closestPoint,Tag);
                return true;
            }
        }
        return false;
    }
}
public struct TargetData
{
    public GameObject obj;
    public Vector3 point;
    public string tag;
    public TargetData(GameObject _obj, Vector3 _point,string _tag)
    {
        obj = _obj;
        point = _point;
        tag = _tag;
    }
    public void Clear()
    {
        obj = null;
        point = Vector3.zero;
    }
}