using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guro : MonoBehaviour
{
    private SpriteRenderer _sr;
    private int rows = 4; //縦の分割数
    private int columns = 4; //横の分割数
    private Texture2D textureToSplit;

    private GameObject guroPref;

    public void PlayGuro(Vector3 worldPos)
    {
        GameObject obj = Instantiate(guroPref, transform);
        obj.transform.position = worldPos;

        Transform[] childTransforms = obj.GetComponentsInChildren<Transform>();

        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] == obj.transform) // 親オブジェクト自身をスキップする
                continue;

            Rigidbody2D rb = childTransforms[i].GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false;
                float power_x = Random.Range(10, 40);
                float power_y = Random.Range(10, 40);
                Vector2 randomDirection = Random.onUnitSphere; // ランダムな方向を取得
                Vector2 force = new Vector2(randomDirection.x * power_x, randomDirection.y * power_y);
                rb.velocity = force; // 速度を設定してオブジェクトを飛び散らせる
            }
        }

        StartCoroutine(End(obj));
    }

    IEnumerator End(GameObject obj)
    {
        yield return new WaitForSeconds(5.5f);
        Destroy(obj);
    }

    public void SplitImage()
    {
        _sr = GetComponent<SpriteRenderer>();

        GameObject tempParent = new GameObject("Parent");
        tempParent.transform.position = transform.position;
        tempParent.transform.SetParent(transform);

        if (_sr.sprite == null)
        {
            Debug.LogError("Sprite is not assigned!");
            return;
        }

        textureToSplit = _sr.sprite.texture; // Spriteからテクスチャを取得
        int width = textureToSplit.width / columns; // 一つの分割画像の幅
        int height = textureToSplit.height / rows; // 一つの分割画像の高さ

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject quad = new GameObject("Quad"); // Quadを生成
                quad.transform.SetParent(tempParent.transform);
                Vector3 temp = quad.transform.localScale;
                quad.transform.localScale = new Vector3(0.5f / columns, 0.5f / rows, temp.z);
                quad.transform.localPosition = new Vector3(x * temp.x / columns - (temp.x / columns), y * temp.y / rows - (temp.y / rows), 0);
                quad.transform.localScale = new Vector3(0.25f, 0.25f, temp.z);
                //quad.transform.position = pos;
                SpriteRenderer spriteRenderer = quad.AddComponent<SpriteRenderer>();
                Sprite slicedSprite = Sprite.Create(textureToSplit, new Rect(x * width, y * height, width, height), new Vector2(0.5f, 0.5f));
                spriteRenderer.sprite = slicedSprite;
                spriteRenderer.sortingOrder = 2;
            }
        }

        GameObject[] obj;
        Rigidbody2D[] rbs;
        BoxCollider2D[] bc2;
        int a = tempParent.transform.childCount;
        rbs = new Rigidbody2D[a];
        obj = new GameObject[a];
        bc2 = new BoxCollider2D[a];

        for (int i = 0; i < rbs.Length; i++)
        {
            obj[i] = tempParent.transform.GetChild(i).gameObject;
            rbs[i] = tempParent.transform.GetChild(i).gameObject.AddComponent<Rigidbody2D>();
            rbs[i].isKinematic = true;
            bc2[i] = tempParent.transform.GetChild(i).gameObject.AddComponent<BoxCollider2D>();
            bc2[i].size = new Vector2(0.1f, 0.1f);
            //bc2[i].isTrigger = true;
        }

        guroPref = tempParent;
    }
}
