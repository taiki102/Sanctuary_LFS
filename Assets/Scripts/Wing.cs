using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wing : MonoBehaviour
{
    bool Chaising = false;
    Transform player;

    float Rad = 1.5f;
    float Rad2 = 0.2f;

    float speed = 5.0f;
    void Update()
    {
        if (!Chaising)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, Rad);
            for (int k = 0; k < colliders.Length; k++)
            {
                string Tag = colliders[k].gameObject.tag;
                if (Tag == "Player")
                {
                    Chaising = true;
                    player = colliders[k].gameObject.transform;
                }
            }
        }
        else
        {
            if (player == null) return;
            Vector3 dir = (player.position - transform.position).normalized;
            transform.Translate(dir * speed * Time.deltaTime);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, Rad2);
            for (int k = 0; k < colliders.Length; k++)
            {
                string Tag = colliders[k].gameObject.tag;
                if (Tag == "Player")
                {
                    Chaising = false;
                    GameManager.instance.GetWing();
                    Destroy(gameObject);
                }
            }
        }
    }
}
