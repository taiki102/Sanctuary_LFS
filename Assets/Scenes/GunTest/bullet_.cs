using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet_ : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.forward * 10.0f * Time.deltaTime);
    }
}
