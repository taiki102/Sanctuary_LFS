using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*-----------------------------------------------------------*/
/*               �@�@PlayerMoveMent�@class
 *              �@�@�@�v���C���[����Ɠ���
/*-----------------------------------------------------------*/
public class PlayerMoveMenta
{
    public Vector2 Movevalue;
    public Vector2 Rotatevalue;
    public DashState currentDashState;
    private Vector3 SecondTargetPoint;
    private Vector3 FirstTargetPoint;
    public ArrowMoveMent arrowMovement;

    private float speed = 10f; //���x
    private float JumpForce = 30f; //�W�����v��

    private Transform GroundCheck;
    private ParticleSystem PS_feather;
    private Transform player;
    private Rigidbody2D _RB;
    private CapsuleCollider2D _CC;
    private Animator _AT;
    private PlayerCtr inputActions;
    private Vector3 velocity = Vector3.zero;
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    private float gravity;

    //��������
    private bool FacetoRight = true;
    private void Flip()
    {
        FacetoRight = !FacetoRight;
        Vector3 theScale = player.localScale;
        theScale.x *= -1;
        player.localScale = theScale;
    }

    //��������
    public void FixedUpdateMethod()
    {
        float horizon_InputValue;
        horizon_InputValue = Movevalue.x * speed;
        _AT.SetFloat("walk", Mathf.Abs(horizon_InputValue));
        if (horizon_InputValue > 0.0f && !FacetoRight || horizon_InputValue < 0.0f && FacetoRight) Flip();   
        Vector3 movement = new Vector3(horizon_InputValue, _RB.velocity.y, 0f);
        _RB.velocity = Vector3.SmoothDamp(_RB.velocity, movement * 50f * Time.fixedDeltaTime, ref velocity, m_MovementSmoothing);

        switch (arrowMovement.currentState){
            case ArrowState.Ready:
                arrowMovement.UpdateTargetPos(Rotatevalue, player.position, FacetoRight);
                break;
            case ArrowState.Shoot:
                arrowMovement.FixedUpdateMethod();
                break;
            case ArrowState.End:
                arrowMovement.UpdateTargetPos(Rotatevalue, player.position, FacetoRight);
                arrowMovement.ResetReady();
                break;
        }
    }

    #region Dash
    private float DashSpeed = 5.0f; //���x
    private float arrivalThreshold = 1.1f;  // 臒l
    public void Dash(DashTarget target)
    {
        //�_�b�V���J�n
        target.obj.GetComponent<Enemy_Base>().Damaged();
        SecondTargetPoint = target.point;
        float dist = Vector3.Distance(player.position, SecondTargetPoint);
        if (dist > 0.0f && !FacetoRight || dist < 0.0f && FacetoRight) Flip();
        DashAnimation(DashState.Dash);

        //EffectManager.instance.Play_shadow(player, target.obj.transform);
        //PS_feather.Play();
    }

    public void Dash_Block(DashTarget target,Vector3 dir)
    {
        //�_�b�V���J�n
        SecondTargetPoint = target.point;
        BreakWall bw = target.obj.GetComponent<BreakWall>();
        if (bw.ISCANBREAK())
        {
            GameManager.instance.StartVeiwChange(dir,true);
            //GameManager.instance.GetComponent<AudioSource>().PlaySFX_Player(0);
            bw.OnBreak();
            DashAnimation(DashState.Dash);
        }
        else if (bw.ISBREAKED())
        {
            GameManager.instance.StartVeiwChange(dir,false);
            DashAnimation(DashState.Dash);
        }
    }

    public void UpdateDashMovementMethod()
    {
        //�_�b�V������
        if (SecondTargetPoint == Vector3.zero) return;

        if(FirstTargetPoint != SecondTargetPoint)
        {

        }
        else
        {
            player.position = Vector3.Lerp(player.position, FirstTargetPoint, DashSpeed * Time.deltaTime);
            if (Vector3.Distance(player.position, FirstTargetPoint) < arrivalThreshold)
            {
                DashAnimation(DashState.Attack);
            }
        }        
    }

    public void UpdateAttackMovementMethod()
    {

    }

    public void DashAnimation(DashState ds)
    {
        switch (ds)
        {
            case DashState.Dash:
                //_RB.isKinematic = true;
                _CC.enabled = false;
                _AT.SetBool("Dash", true);
                currentDashState = DashState.Dash;
                break;
            case DashState.Attack:
                _AT.SetBool("Dash", false);
                currentDashState = DashState.Attack;
                break;
            case DashState.End:
                _RB.isKinematic = false;
                _CC.enabled = true;
                currentDashState = DashState.none;
                arrowMovement.currentState = ArrowState.End;
                break;
        }
    }

    public bool CheckX(Vector3 targetPos)
    {
        if (targetPos.x > player.position.x)
        {
            return true;
        }
        return false;
    }

    public bool CheckY(Vector3 targetPos)
    {
        if (targetPos.y > player.position.y)
        {
            return true;
        }
        return false;
    }
    #endregion

    //�R���X�g���N�^
    public PlayerMoveMenta(GameObject obj,ControllerType ctr)
    {
        player = obj.transform;
        _AT = obj.GetComponent<Animator>();
        _RB = obj.GetComponent<Rigidbody2D>();
        _CC = obj.GetComponent<CapsuleCollider2D>();
        gravity = _RB.gravityScale;
        GroundCheck = player.GetChild(1);
        PS_feather = player.GetChild(2).gameObject.GetComponent<ParticleSystem>();
        //arrowMovement = new ArrowMoveMent(player.GetChild(0).gameObject,this);
        inputActions = new PlayerCtr();
        currentDashState = DashState.none;
        switch (ctr){
            case ControllerType.KeyBoad:
                break;
            case ControllerType.GamePad:
                inputActions.GamePad.Enable();
                inputActions.GamePad.Move.performed += ctx => Movevalue = ctx.ReadValue<Vector2>();
                inputActions.GamePad.Move.canceled += ctx => Movevalue = Vector2.zero;
                inputActions.GamePad.Rotate.performed += ctx => Rotatevalue = ctx.ReadValue<Vector2>();
                inputActions.GamePad.Rotate.canceled += ctx => Rotatevalue = Vector2.zero;
                inputActions.GamePad.Jump.performed += ctx => South_button();
                inputActions.GamePad.Attack_normal.performed += ctx => East_button();
                inputActions.GamePad.LB.performed += ctx => LRButton(false);
                inputActions.GamePad.RB.performed += ctx => LRButton(true);
                inputActions.GamePad.LT.performed += ctx => LRTrigger(false);
                inputActions.GamePad.RT.performed += ctx => LRTrigger(true);
                inputActions.GamePad.LT.canceled += ctx => LRTriggerCanceld(false);
                inputActions.GamePad.RT.canceled += ctx => LRTriggerCanceld(true);
                break;
            case ControllerType.Switch:
                break;
        }
    }

    #region Ctrl
    void LRButton(bool IsPushed_R)
    {
        if (IsPushed_R)
        {
            //ReadyShooting();
            //_AT.SetBool("Shoot", true);

        }
        else
        {

        }
    }

    void LRTrigger(bool IsPushed_R)
    {
        if (IsPushed_R)
        {
            if(arrowMovement.currentState == ArrowState.Ready) arrowMovement.Shoot();
        }
        else
        {
            if (arrowMovement.currentState == ArrowState.NotReady) arrowMovement.UpdateTargetPos(Rotatevalue, player.position, FacetoRight);
            arrowMovement.flg = true;
            arrowMovement.ReadyShooting();                  
        }
    }

    void LRTriggerCanceld(bool IsPushed_R)
    {
        if (IsPushed_R)
        {

        }
        else
        {
            arrowMovement.flg = false;
            arrowMovement.ResetReady();
        }      
    }

    void East_button()
    {
        //_AT.SetBool("IsAttacking", true);
        //StartCoroutine(WaitMotion());
    }

    void South_button()
    {
        //GameManager.instance.GetComponent<AudioSource>().PlaySFX_Player(7);
        Vector2 Addforce_value = Vector3.up;
        _RB.AddForce(Addforce_value * JumpForce * 3);
        /*
        if (Addforce_value.y > 0.6f && Addforce_value.x < 0.8f && Addforce_value.x > -0.8f && !(Addforce_value.x == 0))
        {
            _RB.AddForce(Addforce_value * JumpForce * 3);
        }
        else
        {
            Addforce_value = new Vector2(0f,1f);
            _RB.AddForce(Addforce_value * JumpForce);
        }*/
        //Vector2 Addforce_value = Movevalue.normalized;
        //Addforce_value.y = 0;
        //_RB.AddForce(Addforce_value * JumpForce);
        //_AT.SetTrigger("s");
        //_AT.SetBool("IsJumping", true);
        //_AT.SetBool("JumpUp", true);
    }

    //�ڑ��؂�
    public void DisableMethod()
    {
        inputActions.GamePad.Disable();
        //inputActions..Disable();
    }
    #endregion
}

public enum ControllerType{
    GamePad,
    Switch,
    KeyBoad
}

public enum DashState
{
    none,
    Dash,
    Attack,
    End
}

public enum ArrowState
{
    NotReady,
    Ready,
    Shoot,
    End
}

/*---------------------------------------------------------*/
/*               �@�@ArrowMoveMent�@class
 *              �@�@�@ �@�A���[�̓���
/*---------------------------------------------------------*/
public class ArrowMoveMent
{
    public ArrowState currentState;

    private float initialSpeed = 25f; // �����x
    private float acceleration = 10.0f; // �����x
    private float maxSpeed = 60.0f; // �ő呬�x
    private float currentSpeed; //���x
    private string hitObjectTag = string.Empty;

    private GameObject ArrowObj;
    private Animator at;
    private Vector3 temp_movevalue;
    private DashTarget dt;
    private PlayerMoveMenta pm;
    private Vector2 offset_wall = new Vector2(4.0f, 4.0f);
    private float offset_enemy;
    ///private Transform originalParent;
    ///ArrowObj.transform.parent = originalParent;

    //FixedUpdate�ł�肽�����Ƃ܂Ƃ�
    public void FixedUpdateMethod()
    {
        currentSpeed += acceleration * Time.fixedDeltaTime;
        ArrowObj.transform.Translate(Vector3.right * currentSpeed * Time.fixedDeltaTime);
        //Debug.Log(hitobj.transform.position);

        if (Vector3.Distance(ArrowObj.transform.position, dt.point) < 1.2f)
        {
            //Debug.Log(hitObjectTag);
            switch (hitObjectTag)
            {
                case "Enemy":
                    if (pm.CheckX(dt.obj.transform.position)){
                        dt.point = dt.obj.transform.position;
                        dt.point.x += offset_enemy;
                    }
                    else{
                        dt.point = dt.obj.transform.position;
                        dt.point.x -= offset_enemy;
                    }
                    pm.Dash(dt);                 
                    currentSpeed = 0;
                    break;
                case "Blocks":
                    WallPattern wallPattern = dt.obj.GetComponent<BreakWall>().CurrentWallPtn;
                    Vector3 dir = Vector3.zero;
                    switch (wallPattern)
                    {
                        case WallPattern.UP:
                        case WallPattern.DOWN:          
                            if (pm.CheckY(dt.point)) 
                            { dt.point.y += offset_wall.y; dir = Vector3.up; } 
                            else 
                            { dt.point.y -= offset_wall.y; dir = -Vector3.up; }

                            break;
                        case WallPattern.LEFT:                           
                        case WallPattern.RIGHT:
                            if (pm.CheckX(dt.point))
                            { dt.point.x += offset_wall.x; dir = Vector3.right; }
                            else
                            { dt.point.x -= offset_wall.x; dir = -Vector3.right; }

                            break;
                        default:
                            Debug.Log("Error BreakWall NoSet Wall Ptn");
                            break;
                    }
                    pm.Dash_Block(dt,dir);
                    currentSpeed = 0;
                    break;
            }

            currentSpeed = 0;;
            currentState = ArrowState.End;
        }

        if(currentSpeed > maxSpeed)
        {
            currentSpeed = 0;
            currentState = ArrowState.End;
        }
    }

    //�R���X�g���N�^
    public ArrowMoveMent(GameObject obj,PlayerMoveMenta _pm)
    {
        ArrowObj = obj;
        ArrowObj.SetActive(false);
        ArrowObj.SetActive(true);
        at = ArrowObj.GetComponent<Animator>();
        currentState = ArrowState.NotReady;
        //originalParent = ArrowObj.transform.parent;
        ArrowObj.transform.parent = null;
        pm = _pm;
        offset_enemy = 1.0f;
        offset_wall = new Vector2(4.0f,4.0f);
    }

    public bool flg = false;
    //�G�C�����Ă���
    public void ReadyShooting()
    {
        if(currentState == ArrowState.NotReady)
        {
            currentState = ArrowState.Ready;
        }    
        at.SetBool("Aiming", true);
    }

    //�G�C����߂�
    public void ResetReady()
    {
        if(currentState == ArrowState.Ready || currentState == ArrowState.End){
            if (flg){
                currentState = ArrowState.Ready;
            }
            else{
                currentState = ArrowState.NotReady;
                at.SetBool("Aiming", false);
            }       
        }    
    }

    //������
    public void Shoot()
    {
        //GameManager.instance.GetComponent<AudioSource>().PlaySFX_Player(5);

        dt.Clear();

        float distance = 20.0f;
        RaycastHit2D hit2D;
        Ray2D ray2D = new Ray2D(ArrowObj.transform.position, temp_movevalue);

        //Debug.DrawRay(ray2D.origin, ray2D.direction * distance, Color.red);

        if (Physics2D.Raycast(ray2D.origin, ray2D.direction, distance))
        {        
            hit2D = Physics2D.Raycast(ray2D.origin, ray2D.direction, distance);
            Debug.Log("Hit object: " + hit2D.collider.gameObject.tag);
            hitObjectTag = hit2D.collider.gameObject.tag;
            dt = new DashTarget(hit2D.collider.gameObject, new Vector3(hit2D.point.x, hit2D.point.y, 0));
        }

        currentState = ArrowState.Shoot;
        currentSpeed += initialSpeed;
    }

    //���]�@���邵�Ȃ�bool
    private bool inversion = false;

    //���E���]�i�V �A���[�̈ړ�
    public void UpdateTargetPos(Vector2 RightStick_Movevalue, Vector3 pPos, bool FacingRight)
    {
        if (RightStick_Movevalue.magnitude >= 1.0f)
            temp_movevalue = new Vector3(RightStick_Movevalue.x, RightStick_Movevalue.y, 0).normalized;
        else if (temp_movevalue == Vector3.zero) temp_movevalue = FacingRight ? Vector3.right : Vector3.left;

        ArrowObj.transform.position = temp_movevalue * 2 + pPos;
        ArrowObj.transform.rotation = Quaternion.Euler(0f, 0f, CalculateAngle(Vector2.zero, temp_movevalue));
    }

    //���E���]�A���@�A���[�̈ړ�
    public void UpdateTargetPos(Vector2 RightStick_Movevalue,bool FacingRight)
    {
        if (RightStick_Movevalue.magnitude >= 1.0f)
            temp_movevalue = new Vector3(RightStick_Movevalue.x, RightStick_Movevalue.y, 0).normalized * 2;
        else return;

        Vector3 target = temp_movevalue;

        if (!FacingRight && !inversion)
        {
            Vector3 temp = ArrowObj.transform.localScale;
            temp *= -1.0f;
            ArrowObj.transform.localScale = temp;
            inversion = true;
        }
        else if(FacingRight && inversion){
            Vector3 temp = ArrowObj.transform.localScale;
            temp *= -1.0f;
            ArrowObj.transform.localScale = temp;
        }

        if (!FacingRight){
            target.x *= -1.0f;
        }else if (FacingRight){
            inversion = false;
        }     

        ArrowObj.transform.localPosition = target;
        ArrowObj.transform.rotation = Quaternion.Euler(0f, 0f, CalculateAngle(Vector2.zero, RightStick_Movevalue));
    }

    // �_B����_A�ւ̊p�x�����߂�
    private float CalculateAngle(Vector2 pointA, Vector2 pointB)
    {    
        float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * Mathf.Rad2Deg;
        return angle;
    }
}

//Target Obj and Vector
public struct DashTarget{
    public GameObject obj;
    public Vector3 point;
    public DashTarget(GameObject _obj,Vector3 _point)
    {
        obj = _obj;
        point = _point;
    }

    public void Clear()
    {
        obj = null;
        point = Vector3.zero;
    }
}