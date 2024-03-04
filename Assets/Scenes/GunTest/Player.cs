using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    private float speed = 4f;
    private float JumpForce = 100f;
    private Rigidbody2D _RB;
    private Animator _AT;

    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    private Vector3 velocity = Vector3.zero;

    #region PlayerControls
    PlayerCtr inputActions;
    private void OnEnable()
    {
        inputActions.Xbox.Enable();
    }

    private void OnDisable()
    {
        inputActions.Xbox.Disable();
    }
    #endregion

    Vector2 Movevalue;
    Vector2 Rotatevalue;

    [Header("Events")]
    [Space]

    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    private void Awake()
    {
        /*
        if (OnFallEvent == null)
            OnFallEvent = new UnityEvent();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();*/

        _RB = GetComponent<Rigidbody2D>();
        _AT = GetComponent<Animator>();
        //targetObj = GameObject.FindGameObjectWithTag("Target").transform;
        inputActions = new PlayerCtr(); 
        inputActions.Xbox.Move.performed += ctx => Movevalue = ctx.ReadValue<Vector2>();
        inputActions.Xbox.Move.canceled += ctx => Movevalue = Vector2.zero;
        inputActions.Xbox.Rotate.performed += ctx => Rotatevalue = ctx.ReadValue<Vector2>();
        inputActions.Xbox.Rotate.canceled += ctx => Rotatevalue = Vector2.zero;
        inputActions.Xbox.Jump.performed += ctx => Jump();
        inputActions.Xbox.Attack_normal.performed += ctx => Attack();
        inputActions.Xbox.LB.performed += ctx => Skill(false);
        inputActions.Xbox.RB.performed += ctx => Skill(true);

        inputActions.PS4.Move.performed += ctx => Movevalue = ctx.ReadValue<Vector2>();
        inputActions.PS4.Move.canceled += ctx => Movevalue = Vector2.zero;
        inputActions.PS4.Rotate.performed += ctx => Rotatevalue = ctx.ReadValue<Vector2>();
        inputActions.PS4.Rotate.canceled += ctx => Rotatevalue = Vector2.zero;
        inputActions.PS4.Jump.performed += ctx => Jump();
        inputActions.PS4.Attack_normal.performed += ctx => Attack();

        _RB.mass = 1f;
        _RB.gravityScale = 1f;//5 1  
    }

    private bool FacetoRight = true;

    private void Update()
    {
        /*
        if (Input.GetKey(KeyCode.D))
        {
            Movevalue.x = 1.0f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Movevalue.x = -1.0f;
        }
        else
        {
            Movevalue.x = 0.0f;
        }

        if (Input.GetKey(KeyCode.Z))
        {
            Attack();
        }

        if (Input.GetKey(KeyCode.X))
        {
            Jump();
        }*/

        if (Movevalue.y < 0.2f && Movevalue.y > -0.2f)
        {
            float horizon_InputValue = Movevalue.x * speed;
            _AT.SetFloat("Speed", Mathf.Abs(horizon_InputValue));
            if (horizon_InputValue > 0 && !FacetoRight) Flip();
            else if (horizon_InputValue < 0 && FacetoRight) Flip();
        }
        else
        {
            _AT.SetFloat("Speed", Mathf.Abs(0));
        }

        if (_RB.velocity.y == 0f) OnLandEvent.Invoke();
    }

    private void FixedUpdate()
    {
        //UpdateTargetPos();
        float horizon_InputValue;
        if (Movevalue.y < 0.2f && Movevalue.y > -0.2f)
        {
            horizon_InputValue = Movevalue.x * speed;           
        }
        else
        {
            horizon_InputValue = 0;
        }
        Vector3 movement = new Vector3(horizon_InputValue, _RB.velocity.y, 0f);
        _RB.velocity = Vector3.SmoothDamp(_RB.velocity, movement * 50f * Time.fixedDeltaTime, ref velocity, m_MovementSmoothing);
    }

    void Flip() {
        FacetoRight = !FacetoRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private Transform targetObj;

    public void ReadyShooting()
    {
        targetObj = gameObject.transform.GetChild(0);
        targetObj.gameObject.SetActive(true);
        targetObj.gameObject.GetComponent<GUN>().shoot();
        Debug.Log("a");
    }

    void UpdateTargetPos()
    {
        if(Movevalue != Vector2.zero && targetObj != null)
        {
            Vector3 test = new Vector3(Movevalue.x, Movevalue.y, 0).normalized  * 2 + transform.position;
            //if (test.magnitude < 1.0f) 
            targetObj.position = test;

            targetObj.rotation = Quaternion.Euler(0f,0f, CalculateAngle(transform.position, targetObj.position));
            targetObj.rotation = Quaternion.Euler(0f, 0f, CalculateAngle(Vector2.zero, Movevalue));
        }
    }
    
    float CalculateAngle(Vector2 pointA, Vector2 pointB)
    {
        // ベクトルBからベクトルAへの角度を求める
        float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * Mathf.Rad2Deg;
        return angle-90f;
        //return angle;
    }

    void Jump()
    {
        /*
        Vector2 Addforce_value = Movevalue.normalized;
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
        Vector2 Addforce_value = Movevalue.normalized;
        Addforce_value.y = 0;
        _RB.AddForce(Addforce_value * JumpForce);

        _AT.SetTrigger("s");
        //_AT.SetBool("IsJumping", true);
        //_AT.SetBool("JumpUp", true);
    }

    public void OnFall()
    {
        _AT.SetBool("IsJumping", true);
    }

    public void OnLanding()
    {
        _AT.SetBool("IsJumping", false);
    }

    void Attack()
    {
        _AT.SetBool("IsAttacking", true);
        StartCoroutine(WaitMotion());
    }

    public void DoDashDamage()
    {
        // Your implementation here
    }

    IEnumerator WaitMotion()
    {
        yield return new WaitForSeconds(1.0f);
        _AT.SetBool("IsAttacking", false);
    }

    void Skill(bool IsR)
    {
        if (IsR)
        {
            //ReadyShooting();
            //_AT.SetBool("Shoot", true);

        }
    }
}
