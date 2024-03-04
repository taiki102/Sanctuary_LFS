using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{
    [SerializeField]
    GameObject OutView;

    [SerializeField]
    GameObject[] GrayBack;

    private Vector3 offset;

    public void ChengeView(Vector3 dir)
    {
        offset = new Vector3(27f, 14f, 0);
        StartCoroutine(ViewChange(dir));
    }

    IEnumerator ViewChange(Vector3 dir)
    {
        Vector3 pos = new Vector3(offset.x * dir.x, offset.y * dir.y, 0);
        GrayBack[1].transform.position += pos;

        Transform temp = transform.parent;
        Transform _c = Camera.main.transform;

        OutView.transform.SetParent(_c);

        yield return Lerp(dir);

        OutView.transform.SetParent(temp);
        GrayBack[0].transform.position += pos;
    }

    IEnumerator Lerp(Vector3 dir)
    {
        Transform me = Camera.main.transform;
        Vector3 targetPos = me.position + new Vector3(offset.x * dir.x, offset.y * dir.y, 0);

        float elapsedTime = 0f;
        float lerpDuration = 0.5f; // ˆÚ“®‚É‚©‚©‚éŽžŠÔ

        Vector3 startingPos = me.position;

        while (elapsedTime < lerpDuration)
        {
            me.position = Vector3.Lerp(startingPos, targetPos, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        me.position = targetPos;
    }
}
