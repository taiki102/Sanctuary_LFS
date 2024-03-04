using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUN : MonoBehaviour
{
    [SerializeField]
    GameObject Bullet;

    public void shoot()
    {
        GameObject bullet = Instantiate(Bullet, transform.position, transform.rotation);
    }
}
