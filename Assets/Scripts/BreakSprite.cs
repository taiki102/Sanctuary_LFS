using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakSprite : MonoBehaviour
{
    private SpriteRenderer _sr;
    private int rows = 4; //縦の分割数
    private int columns = 4; //横の分割数
    private Texture2D textureToSplit;

    void Start()
    {
        SplitImage();
    }

    public void ExproudRb()
    {
        //GameManager.instance.GetComponent<AudioSource>().PlaySFX_Player(1);

        foreach (SpriteRenderer sp in DisplayWalls)
        {
            sp.sortingOrder = 3;
        }

        GameObject[] obj;
        Rigidbody2D[] rbs;
        int a = gameObject.transform.childCount;
        rbs = new Rigidbody2D[a];
        obj = new GameObject[a];
        for (int i = 0; i < rbs.Length; i++)
        {
            obj[i] = gameObject.transform.GetChild(i).gameObject;
            rbs[i] = gameObject.transform.GetChild(i).gameObject.AddComponent<Rigidbody2D>();
            //gameObject.transform.GetChild(i).gameObject.AddComponent<BoxCollider2D>().size = new Vector2(0.1f,0.1f);
        }

        for (int i = 0; i < rbs.Length; i++)
        {
            float power_x = Random.Range(100, 400);
            float power_y = Random.Range(100, 400);
            Vector3 dir = (transform.position - obj[i].transform.position).normalized;
            if(dir == Vector3.zero)
            {
                dir = new Vector3(power_x, power_y,0).normalized;
                //Debug.Log(dir);
            }

            dir.x *= power_x / 2;// 3
            dir.y *= power_y / 2;
            rbs[i].AddForce(dir);
            StartCoroutine(End(obj[i]));
        }
        _sr.enabled = false;        
    }

    IEnumerator End(GameObject obj)
    {
        yield return new WaitForSeconds(2.5f);
        Destroy(obj);
    }

    List<SpriteRenderer> DisplayWalls = new List<SpriteRenderer>();

    void SplitImage()
    {
        _sr = GetComponent<SpriteRenderer>();

        if (_sr.sprite == null)
        {
            Debug.LogError("Sprite is not assigned!");
            return;
        }

        textureToSplit = _sr.sprite.texture; // Spriteからテクスチャを取得
        float width = textureToSplit.width / columns; // 一つの分割画像の幅
        float height = textureToSplit.height / rows; // 一つの分割画像の高さ

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject quad = new GameObject("Quad"); // Quadを生成
                quad.transform.SetParent(this.gameObject.transform);

                Vector3 temp = quad.transform.localScale;
                quad.transform.localScale = new Vector3(0.63f / columns, 0.63f / rows, temp.z);
                quad.transform.localPosition = new Vector3(x * temp.x / columns - (temp.x / columns), y * temp.y / rows - (temp.y / rows), 0);

                SpriteRenderer spriteRenderer = quad.AddComponent<SpriteRenderer>(); 
                Sprite slicedSprite = Sprite.Create(textureToSplit, new Rect(x * width, y * height, width, height), new Vector2(1.0f, 1.0f));
                spriteRenderer.sprite = slicedSprite;
                spriteRenderer.sortingOrder = 0;               
                spriteRenderer.material = _sr.material;
                DisplayWalls.Add(spriteRenderer);
            }
        }
    }
}
