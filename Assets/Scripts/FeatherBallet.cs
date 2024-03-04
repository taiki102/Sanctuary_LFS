using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatherBallet : MonoBehaviour
{
    float acceleration = 35.0f;
    float maxSpeed = 50.0f;
    float currentSpeed = 15f;
    void FixedUpdate()
    {
        currentSpeed += acceleration * Time.fixedDeltaTime;
        transform.Translate(Vector3.right * currentSpeed * Time.fixedDeltaTime);
        if (currentSpeed > maxSpeed)
        {
            Destroy(gameObject);
        }
    }
}
