using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTest : MonoBehaviour {
    public Rotator wheelRotator;

    public float rpm;

    void Update() {
        
    }

    private void FixedUpdate() {
        wheelRotator.rpm = this.rpm;
    }
}
