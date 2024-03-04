using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakWall : MonoBehaviour
{
    private Tilemap tilemap;
    private GameObject tilePrefab;
    private bool IsBreaked = false;
    private bool CanBreak = false;

    [SerializeField]
    public WallPattern CurrentWallPtn;

    public void OnBreak()
    {
        if (CanBreak)
        {
            GameManager.instance.SetUpNewRoom(CurrentWallPtn);
            CanBreak = false;
            StartCoroutine(Break());
        }     
    }

    public bool ISBREAKED()
    {
        if (GameManager.instance.RoomClear)
        {
            return IsBreaked;
        }
        return false;
    }

    public bool ISCANBREAK()
    {
        if (GameManager.instance.RoomClear)
        {
            return CanBreak;
        }
        return false;
    }

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemap.color = originalColor;
    }

    //èâä˙âª
    public void SetCanBreak(bool temp)
    {       
        CanBreak = temp;

        tilePrefab = Resources.Load<GameObject>("Prefabs/TilePref");

        if (tilePrefab == null) { Debug.Log("Resources/tilePref Error"); return; }

        tilemap = GetComponent<Tilemap>();
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(position);

            if (tile != null)
            {
                Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
                Sprite sprite = ((Tile)tile).sprite;
                GameObject tileGameObject = Instantiate(tilePrefab, worldPosition, Quaternion.identity);
                tileGameObject.transform.SetParent(gameObject.transform);
                SpriteRenderer sp = tileGameObject.GetComponent<SpriteRenderer>();
                sp.sprite = sprite;
                sp.sortingOrder = -1;
                tileGameObject.transform.position = worldPosition;
                //tileGameObject.AddComponent<BreakSprite>();
            }
        }
    }

    private float blinkInterval = 0.7f;
    private float timer = 0f;
    private bool isBlinking = false;

    private Color blinkColor = new Color(1.0f, 1.0f, 0.75f, 1.0f);
    private Color originalColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    private void FixedUpdate()
    {
        if (GameManager.instance.RoomClear)
        {
            if (CanBreak)
            {
                timer += Time.fixedDeltaTime;
                if (timer >= blinkInterval)
                {
                    timer = 0f;
                    isBlinking = !isBlinking;
                    tilemap.color = isBlinking ? blinkColor : originalColor;
                }
            }
            else if (IsBreaked)
            {
                tilemap.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
        }
        else
        {          
            if (flg)
            {
                tilemap.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                timer += Time.fixedDeltaTime;
                if (timer >= 2.0f)
                {
                    timer = 0f;
                    flg = false;
                }
            }
            else
            {
                tilemap.color = originalColor;
            }            
        }
    }

    bool flg = false;

    IEnumerator Break()
    {      
        foreach(Transform a in transform)
        {
            if (a != transform)
            {
                a.gameObject.GetComponent<BreakSprite>().ExproudRb();
            }
        }
        GameManager.instance.audioPlayer.PlaySFX_Player(1);//ê›íuéûÇÃâπ
        IsBreaked = true;
        yield return null;
        flg = true;
        tilemap.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    }
}

public enum WallPattern{
    none,
    LEFT,
    RIGHT,
    UP,
    DOWN,
}
