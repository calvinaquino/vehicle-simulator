using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransmissionTest : MonoBehaviour {
    public EngineTest engine;
    public WheelTest wheels;

    public float clutchInput = 0f;

    [Range(0f, 1f)]
    private float clutchEngagement = 1f;

    private void FixedUpdate() {
        clutchEngagement = 1f - clutchInput;
    }
}
