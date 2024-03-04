using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballet : MonoBehaviour
{
    private float currentSpeed = 20f; //‘¬“x
    private float count;

    void FixedUpdate()
    {
        transform.Translate(Vector3.right * currentSpeed * Time.fixedDeltaTime);
        count += Time.fixedDeltaTime;
        if (count > 5f)
        {
            Destroy(gameObject);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.05f);
        for (int k = 0; k < colliders.Length; k++)
        {
            string Tag = colliders[k].gameObject.tag;
            if (Tag == "Player")
            {
                colliders[k].gameObject.GetComponent<PlayerChara>().Damaged(1);
                Destroy(gameObject);
            }
        }
    }
}
