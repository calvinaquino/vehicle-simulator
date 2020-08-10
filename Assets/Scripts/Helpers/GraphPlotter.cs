using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphPlotter : MonoBehaviour {

    [Header("Options")]
    public float divisions = 10;

    [Header("Pacejka Inputs")]
    public int pRange = 50;
    public float B = 3; // sharpness, should be betwen 0.1 and 4
    public float C = 3; // fall off should be between 2 and 3.2
    public float D = 1; // factor, maximum y value
    public float E = 1; // fall off sharpness, shoud be less than 1

    [Header("Slip Values")]
    public int sRange = 10;
    public float lateralVelocity = 0;
    public float longitudinalVelocity = 10;
    public float distanceFromCG = 1.237f;
    public float angularVelocity = 0;

    // The factor D should be peak force, the return should be in degrees.

    Func<float, float, float, float, float, float> Pacejka = (x, b, c, d, e) => {
        return d * Mathf.Sin(c * Mathf.Atan(b * x - e * (b * x - Mathf.Atan(b * x))));
    };

    Func<float, float, float, float, float> SlipFront = (v_lat, w, b, v_long) => {
        return Mathf.Atan((v_lat + w * b) / v_long);
    };
    Func<float, float, float, float, float> SlipRear = (v_lat, w, c, v_long) => {
        return Mathf.Atan((v_lat + w * c) / v_long);
    };

    void Start() {
        for (int i = 0; i < rpms.Length; i++) {
            this.torqueCurve.AddKey(rpms[i], torques[i]);
        }
    }

    // Update is called once per frame
    void Update() {
        Plot((x) => Pacejka(x, B, C, D, E), Color.red, pRange);
        Plot((x) => SlipFront(lateralVelocity, x, distanceFromCG, longitudinalVelocity), Color.green, sRange);
        Plot((x) => torqueCurve.Evaluate(x * 100) / 10f, Color.blue, 70);
        //Plot((x) => SlipRear(0, x, 1.237f, 10), Color.blue);
    }

    void Plot(Func<float, float> function, Color color, int range) {
        int rightLimit = range;
        int leftLimit = -rightLimit;

        Vector3 lastPoint = new Vector3(leftLimit, 0);
        for (int x = leftLimit; x < rightLimit; x++) {
            for (int _x = 0; _x < divisions; _x++) {
                float stepX = x + (1 / divisions * _x);
                float stepY = function(stepX);

                Vector3 point = new Vector3(stepX, stepY);
                if (x == leftLimit && _x == 0) {
                    lastPoint = point;
                }
                Debug.DrawLine(lastPoint, point, color);
                lastPoint = point;
            }
        }
    }

    private float[] rpms = new float[] {
        0,
        500,
        1000,
        1500,
        2000,
        2500,
        3000,
        3500,
        4000,
        4500,
        5000,
        5500,
        6000,
        6500,
        7000,
    };

    private float[] torques = new float[] {
        0,
        51f,
        111f,
        157f,
        189f,
        226f,
        255f,
        280f,
        290f,
        312f,
        320f,
        310f,
        270f,
        230f,
        195f,
    };

    public AnimationCurve torqueCurve = new AnimationCurve();
}
//    public AnimationCurve torqueCurve = new AnimationCurve(new Keyframe[] {
//        new Keyframe(0,0),
//        new Keyframe(500,51),
//        new Keyframe(1000,111),
//        new Keyframe(1500,157),
//        new Keyframe(2000,189),
//        new Keyframe(2500,226),
//        new Keyframe(3000,255),
//        new Keyframe(3500,280),
//        new Keyframe(4000,290),
//        new Keyframe(4500,312),
//        new Keyframe(5000,320),
//        new Keyframe(5500,310),
//        new Keyframe(6000,270),
//        new Keyframe(6500,230),
//        new Keyframe(7000,196),
//    });
//}
