using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Generating : MonoBehaviour
{
    private Vector3 offset;
    private TileMapAssets _tma;

    private int rows = 30; //�c�̕�����
    private int columns = 30; //���̕�����

    private int MoveArea = 10;//�g��

    private Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    public void InitMethod(int _rows,int _columns,int _MoveArea)
    {
        rows = _rows;
        columns = _columns;
        MoveArea = _MoveArea;

        offset = new Vector3(27f, 14f, 0);
        _tma = FindObjectOfType<TileMapAssets>();
        StartCoroutine(LoadTileMapAssets());//���ꂪ���C��
    }

    //�v���t�@�u�ݒu
    private GameObject SetTileMap(Vector3 InitPos,GameObject[] pool)
    {
        int random = Random.Range(0, pool.Length);
        GameObject Parent = Instantiate(pool[random], InitPos, Quaternion.identity);//obj pos
        Parent.transform.SetParent(transform);
        RoomList.Add(Parent);//add List
        return Parent;
    }

    private GameObject ChildTileMap(Vector3 InitPos,Vector3 dir)
    {
        WallPattern dir_ptn = GetDirectionPattern(dir);
        GameObject[] pool = tilemapDictionary[dir_ptn];
        return SetTileMap(InitPos, pool);
    }

    //�G���A�̏�����
    private List<Vector2> InitPlane()
    {
        List<Vector2> _p = new List<Vector2>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                _p.Add(new Vector2(j,i));
            }
        }
        return _p;
    }

    //�J�n�ʒu
    private Vector2 StartPos()
    {
        //new Vector2(Random.Range(0, columns - 1), Random.Range(0, rows - 1))
        return new Vector2((columns - 1) / 2, (rows - 1) / 2);
    }

    //�ړ��\�G���A�ƌ��ݒn����@�ړ��\�����̐ݒ�
    private List<Vector2> GetAdjacentPositions(List<Vector2> area, Vector2 pos)
    {
        List<Vector2> adjacentDir = new List<Vector2>();
        foreach (Vector2 _p in area)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2 adjacentPos = pos + directions[i];
                if (adjacentPos == _p)
                {
                    adjacentDir.Add(directions[i]);
                    break;  
                }
            }
        }
        return adjacentDir;
    }

    //�s�σ^�C���}�b�v�v�[��
    private GameObject[] tilemaps;
    Dictionary<WallPattern, GameObject[]> tilemapDictionary = new Dictionary<WallPattern, GameObject[]>();

    //�����܂ށ@�ݒu
    IEnumerator LoadTileMapAssets()
    {
        while (_tma.tilemapObj() == null)
        {
            yield return null;
        }

        //�g���ꏊ
        tilemaps = _tma.tilemapObj();

        //�f�B�N�V���i��������
        yield return InitTilemapDic(WallPattern.LEFT);
        yield return InitTilemapDic(WallPattern.RIGHT);
        yield return InitTilemapDic(WallPattern.UP);
        yield return InitTilemapDic(WallPattern.DOWN);

        //�K�v�ȕϐ�������,�����_������
        int count = MoveArea;
        Vector3 ObjPos = Vector3.zero;//obj pos  
        GameObject Parent = SetTileMap(ObjPos, tilemaps);
        GameObject Child;

        //�ݒu�\�G���A������
        List<Vector2> Area = InitPlane();
        List<Vector2> NewDir = new List<Vector2>();
        TOV tov = new TOV();
        TOV subtov = new TOV();
        Vector2 CurrentPos = StartPos();
        List<WallPattern> Parenthwp;

        //�P�}�X�ڍ폜
        Area.Remove(CurrentPos);

        //�ړ��\�G���A���Ȃ��Ȃ�܂Ń��[�v
        while (count > 0)
        {
            //�e�̈ړ��\�����̎擾
            while (Parent.GetComponent<WallData>().WallPtnList == null)
            {
                yield return null;
            }
            Parenthwp = Parent.GetComponent<WallData>().WallPtnList;

            NewDir.Clear();
            foreach (Vector2 _p in Area)
            {
                for (int i = 0; i < directions.Length; i++)
                {
                    Vector2 adjacentPos = CurrentPos + directions[i];
                    if (adjacentPos == _p)
                    {
                        WallPattern ptn = GetDirectionPattern(directions[i]);
                        if (Parenthwp.Contains(ptn))
                        {
                            NewDir.Add(directions[i]);
                        }                     
                        break;
                    }
                }
            }

            if (NewDir.Count >= 2 && 1 == Random.Range(0, 2))
            {
                for(int i = 0; i < 2; i++)
                {
                    Vector2 Temp_ = CurrentPos + NewDir[i];
                    Area.Remove(Temp_);               
                    Vector3 Temp = ObjPos + new Vector3(NewDir[i].x * offset.x, NewDir[i].y * offset.y, 0);     
                    Child = ChildTileMap(Temp, -NewDir[i]);    
                    StartCoroutine(TunnelConstruction(Parent, Child, NewDir[i]));
                    tov = new TOV(Child, Temp_, Temp);
                }        

                count--;
            }
            else
            {
                if (NewDir.Count != 0)
                {
                    subtov = new TOV(Parent, CurrentPos, ObjPos);
                    int random = Random.Range(0, NewDir.Count);
                    CurrentPos += NewDir[random];
                    Area.Remove(CurrentPos);
                    ObjPos += new Vector3(NewDir[random].x * offset.x, NewDir[random].y * offset.y, 0);
                    Child = ChildTileMap(ObjPos, -NewDir[random]);          
                    StartCoroutine(TunnelConstruction(Parent, Child, NewDir[random]));
                    Parent = Child;         
                }
                else
                {
                    if (tov.obj == null)
                    {
                        tov = subtov;
                    }

                    if(CurrentPos == tov.currentpos)
                    {
                        //Debug.Log("END" + tov.currentpos + tov.obj.name);
                        break;
                    }
                    else
                    {                
                        CurrentPos = tov.currentpos;
                        ObjPos = tov.objpos;
                        Parent = tov.obj;
                        count++;
                    }
                }           
            }

            count--;
        }
        IsGenerated = true;
    }

    [HideInInspector]
    public bool IsGenerated = false;

    //�g���l���H��
    IEnumerator TunnelConstruction(GameObject _p, GameObject _c,Vector2 dir)
    {
        GameObject me;

        for (int i = 0; i < 2; i++)
        {
            me = (i == 0) ? _p : _c;
            dir *= (i == 0) ? 1 : -1;

            WallPattern dir_ptn = GetDirectionPattern(dir);

            while (me.GetComponent<WallData>().HaveWallPtn == null)
            {
                yield return null;
            }

            List<CBWD> hwp = me.GetComponent<WallData>().HaveWallPtn;

            foreach (CBWD cbwd in hwp)
            {
                if (cbwd.pattern == dir_ptn)
                {
                    cbwd.obj.GetComponent<BreakWall>().SetCanBreak(true);

                    if (i != 0)
                    {
                        //Debug.Log("Destroy");
                        Destroy(cbwd.obj);
                    }
                }
            }
        }
    }

    //�E�H�[���p�^�[���̃n�b�V���}�b�v�@�炵��
    WallPattern GetDirectionPattern(Vector2 dir)
    {
        Dictionary<Vector2, WallPattern> directionMap = new Dictionary<Vector2, WallPattern>
    {
        { directions[0], WallPattern.UP },
        { directions[1], WallPattern.DOWN },
        { directions[2], WallPattern.LEFT },
        { directions[3], WallPattern.RIGHT }
    };

        if (directionMap.TryGetValue(dir, out WallPattern result)){
            //Debug.Log(result);
            return result;
        }
        else{
            Debug.Log("Error WallPattern None" + dir);
            return WallPattern.none;
        }
    }

    IEnumerator InitTilemapDic(WallPattern wallPattern)
    {
        List<GameObject> objs = new List<GameObject>();
        List<CBWD> hwp;

        foreach (GameObject _obj in tilemaps)
        {
            while (_obj.GetComponent<WallData>() == null || _obj.GetComponent<WallData>().HaveWallPtn == null)
            {            
                yield return null;
            }

            hwp = _obj.GetComponent<WallData>().HaveWallPtn;

            foreach (CBWD cbwd in hwp)
            {
                if (cbwd.pattern == wallPattern)
                {
                    objs.Add(_obj);
                }
            }
        }
        tilemapDictionary.Add(wallPattern, objs.ToArray());
    }

    //3x3�Ŕz�u���邾��
    IEnumerator LoadTileMapAssets(bool test)
    {
        GameObject tileGameObject;
        Vector3 InitPos = Vector3.zero;
        InitPos -= offset;

        while (tilemaps == null)
        {
            yield return null;
        }
       
        for(int i= 0; i < rows; i++)
        {
            for(int j = 0; j < columns; j++)
            {
                tileGameObject = Instantiate(tilemaps[0], new Vector3(InitPos.x + offset.x * j,InitPos.y + offset.y * i,InitPos.z), Quaternion.identity);
                tileGameObject.transform.SetParent(transform);
            }
        }
    }

    //�z�u�������[���������W�Ŏ擾
    private List<GameObject> RoomList = new List<GameObject>();

    public WallData RoomData(Vector3 pos)
    {
        foreach(GameObject obj in RoomList)
        {
            if(obj.transform.position == pos)
            {
                Debug.Log(pos + obj.name);
                return obj.GetComponent<WallData>();
            }
        }
        return null;
    }
}

//TempObjandVector
public struct TOV {
    public GameObject obj;
    public Vector2 currentpos;
    public Vector2 objpos;
    public TOV(GameObject _obj, Vector2 _cpos, Vector2 _opos)
    {
        obj = _obj;
        currentpos = _cpos;
        objpos = _opos;
    }
}