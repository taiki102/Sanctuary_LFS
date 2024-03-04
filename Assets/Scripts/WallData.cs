using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallData : MonoBehaviour
{
    [SerializeField]
    public BlockType CurrentBlockType;

    public List<CBWD> HaveWallPtn;

    public List<WallPattern> WallPtnList;

    private readonly string TAG = "ParentBlock";

    private readonly string TAG2 = "EnemySpawn";

    private List<Vector3> SpawnPosition = new List<Vector3>();

    private void Start()
    {
        HaveWallPtn = new List<CBWD>();
        WallPtnList = new List<WallPattern>();
        GetTagObj gto = new GetTagObj();
        GameObject[] ChildrenTagObj = gto.GetChildrenTagObj(TAG,transform);
       
        foreach (GameObject obj in ChildrenTagObj)
        {
            if(obj != null)
            {
                CBWD cbwd = new CBWD(obj, obj.GetComponent<BreakWall>().CurrentWallPtn);
                HaveWallPtn.Add(cbwd);
                WallPtnList.Add(cbwd.pattern);
            }
        }

        GameObject[] spawnPos = gto.GetChildrenTagObj(TAG2, transform);

        //Debug.Log("a1");
        foreach (GameObject obj in spawnPos)
        {
            if (obj != null)
            {
                //Debug.Log("a2");
                SpawnPosition.Add(obj.transform.position);
            }
        }
    }

    public List<Vector3> SpawnPos()
    {
        return SpawnPosition;
    }
}

public class GetTagObj{

    public GetTagObj(){ }

    public GameObject[] GetChildrenTagObj(string tag,Transform my)
    {
        Transform[] children = my.GetComponentsInChildren<Transform>(true);
        var objectsWithTag = new List<GameObject>();
        foreach (var child in children)
        {
            if (child.CompareTag(tag))
            {
                Transform[] TagObjchildren = child.GetComponentsInChildren<Transform>(true);
                foreach (var _child in TagObjchildren)
                {
                    if (_child != child)
                    {
                        objectsWithTag.Add(_child.gameObject);
                    }
                }
                break;
            }
        }
        return objectsWithTag.ToArray();
    }
}

//CanBreakWallData
public struct CBWD
{
    public GameObject obj;
    public WallPattern pattern;
    public CBWD(GameObject _obj,WallPattern _pt)
    {
        obj = _obj;
        pattern = _pt;
    }
}

public enum BlockType
{
    NONE,
    TYPE1,
    TYPE2
}