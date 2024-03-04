using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_0123 : MonoBehaviour
{
    public static Player_0123 instance;
    private PlayerMoveMenta pmove;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //プレイヤー動作　インスタンス作成
        pmove = new PlayerMoveMenta(this.gameObject, ControllerType.GamePad);
    }

    private void FixedUpdate()
    {
        //プレイヤー動作 in FixedUpdate
        pmove.FixedUpdateMethod();
    }

    private void OnDisable()
    {
        pmove.DisableMethod();
    }

    private void Update()
    {
        //ステートに応じたプレイヤー動作 in Update
        if (pmove.currentDashState == DashState.Dash)
        {
            pmove.UpdateDashMovementMethod();

        } else if (pmove.currentDashState == DashState.Attack)
        {
            pmove.UpdateAttackMovementMethod();
        }         
    }
}

