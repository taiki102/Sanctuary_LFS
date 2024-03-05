using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    Generating _gen;

    [SerializeField]
    public GameObject player;

    [SerializeField]
    public GameObject enemy;

    [SerializeField]
    public HpManager hpm;

    [SerializeField]
    public TextMeshProUGUI displaytext;

    [SerializeField]
    public TextMeshProUGUI displaytext2;

    public List<EnemyBase> EnemyList = new List<EnemyBase>();

    [SerializeField]
    public AudioManager audioPlayer;

    public static GameManager instance;

    private View view;

    public Vector3 CurrentRoomPos;

    [SerializeField]
    private GameObject[] EnemyPref;

    [SerializeField]
    private GameObject startPanel;

    [SerializeField]
    private GameObject endPanel;

    [SerializeField]
    private GameObject wingsPrefs;

    public void dispendPanel()
    {
        endPanel.SetActive(true);
    }

    public void Update()
    {
        if (Input.GetButtonDown("Submit"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void StartVeiwChange(Vector3 Dir, bool test)
    {
        view.ChengeView(Dir);

        if (test){
            StartCoroutine(VeiwChange());
        }
    }

    private IEnumerator VeiwChange()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null){
            yield break;
        }
        yield return new WaitForSeconds(1.0f);
        gamepad.SetMotorSpeeds(1.0f, 1.0f);
        yield return new WaitForSeconds(0.5f);
        gamepad.SetMotorSpeeds(0.0f, 0.0f);
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        view = FindObjectOfType<View>();
        StartCoroutine(Generatine());
    }

    IEnumerator Generatine()
    {
        if (_gen == null)
        {
            Debug.Log("NoSetGameManager");
            yield break;
        }

        _gen.InitMethod(30, 30, 10);

        while (!_gen.IsGenerated)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1.0f);
        CurrentRoomPos = Vector3.zero;
        EnemySpawnInit(_gen.RoomData(CurrentRoomPos));

        player.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        startPanel.GetComponent<Animator>().SetTrigger("On");
        yield return new WaitForSeconds(1.5f);
        enemy.SetActive(true);
    }

    void EnemySpawnInit(WallData wd)
    {
        List<Vector3> spawn = wd.SpawnPos();

        int count = 0;
        foreach (Vector3 pos in spawn)
        {
            //Debug.Log(spawn);
            GameObject enemyPrefab = EnemyPref[Random.Range(0,3)];
            GameObject obj = Instantiate(enemyPrefab, pos, Quaternion.identity);
            obj.transform.parent = enemy.transform;
            EnemyBase enemyBase = obj.GetComponent<EnemyBase>();
            if (enemyBase != null)
                EnemyList.Add(enemyBase);
            count++;
        }
        MaxCount = count;
        SetText(MaxCount, MaxCount);
    }

    int MaxCount;

    public bool RoomClear = false;

    public void RemoveList(EnemyBase eb)
    {
        Instantiate(wingsPrefs,eb.EnemyTf().position,Quaternion.identity);

        ECount++;
        EnemyList.Remove(eb);
        SetText(EnemyList.Count, MaxCount);
        //SetSubText();

        if (EnemyList.Count == 0)
        {
            // Clear
            displaytext.text = "ルームクリア";
            RoomClear = true;
            //Debug.Log("Clear");
        }       
    }

    int ECount = 0;
    int WCount = 0;

    public void SetText(int x,int y)
    {
        displaytext.text = $"残りの敵の数　{x}/{y}"; 
    }

    public void SetSubText()
    {
        displaytext2.text = $"倒した敵の数  {ECount}";
    }

    public void SetUpNewRoom(WallPattern wp)
    {
        RoomClear = false;

        Vector3 dir = Vector3.zero;

        switch (wp)
        {
            case WallPattern.DOWN:
                dir = Vector3.down;
                break;
            case WallPattern.UP:
                dir = Vector3.up;
                break;
            case WallPattern.LEFT:
                dir = Vector3.left;
                break;
            case WallPattern.RIGHT:
                dir = Vector3.right;
                break;
        }

        CurrentRoomPos += new Vector3(27f * dir.x, 14f * dir.y, 0);
        Debug.Log(CurrentRoomPos);
        EnemySpawnInit(_gen.RoomData(CurrentRoomPos));
    }

    public void ChangeRoom(Vector3 dir)
    {
        CurrentRoomPos += new Vector3(27f * dir.x, 14f * dir.y, 0);
        displaytext.text = "";
    }

    public void GetWing()
    {
        WCount++;
        displaytext2.text = $"{WCount}";
    }

    /*
    public void InitEnemyList()
    {
        EnemyList.Clear();

        foreach (Transform child in enemy.transform)
        {
            EnemyBase enemyBase = child.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                EnemyList.Add(enemyBase);
            }
        }       
    }*/
}
