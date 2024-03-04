using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class afterImage : MonoBehaviour
{
    [SerializeField]
    private GameObject pref;

    private GameObject[] shadowPref;
    Vector3 StragePos = new Vector3(-100,-100,0);

    Transform tf_p;
    Vector3 tf_tar;

    void Start()
    {
        transform.position = StragePos;

        shadowPref = new GameObject[2];
        for(int i = 0; i < shadowPref.Length; i++)
        {
            shadowPref[i] = Instantiate(pref, transform);
            //shadowPref[i].GetComponent<SpriteRenderer>().sprite = ShadowSprites[i];
        }
    }

    bool rev = false;

    public void DrawShadow(Transform tf, Vector3 target_tf)
    {
        //Debug.Log("draw");
        tf_p = tf;
        tf_tar = target_tf;

        if(tf_p.position.x > tf_tar.x)
        {
            rev = true;
        }

        StartCoroutine(Process());
    }

    IEnumerator Process()
    {
        bool Processing = true;
        float temp = Mathf.Abs(Vector3.Distance(tf_p.position, tf_tar));

        while (Processing)
        {
            float X = Mathf.Abs(Vector3.Distance(tf_p.position, tf_tar));
            yield return null;

            if (temp - X > 0.8f)
            {
                //Debug.Log("draw_now");
                temp = X;
                StartCoroutine(Draw(tf_p.position));
            }

            if (Mathf.Abs(X) < 1.5f)
            {
                Processing = false;
                yield break;
            }
        }    
    }

    IEnumerator Draw(Vector3 pos)
    {
        GameObject obj = Instantiate(pref, transform);

        if (rev) obj.transform.localScale = new Vector3(-1f,1,1);

        obj.transform.position = pos;
        SpriteRenderer sp = obj.GetComponent<SpriteRenderer>();

        float startAlpha = sp.color.a;
        float elapsedTime = 0f;
        float duration = 1.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            Color color = sp.color;
            color.a = newAlpha;
            sp.color = color;
            yield return null;
        }

        Destroy(obj);
    }

    //25192B color
}
