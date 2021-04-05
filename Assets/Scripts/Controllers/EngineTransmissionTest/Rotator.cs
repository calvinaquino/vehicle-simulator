using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Rotator : MonoBehaviour {

    public float rate = 0.05f;
    public float angularVelocity = 0;

    private void FixedUpdate() {
        float eulerAcceleration = Mathf.Rad2Deg * angularVelocity * Time.fixedDeltaTime * rate;
        transform.Rotate(Vector3.up * eulerAcceleration);
    }
}
