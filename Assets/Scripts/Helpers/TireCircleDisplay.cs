using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    public class TireCircleDisplay : MonoBehaviour {
        public RectTransform dot;
        public WheelController wheel;

        //void Start() {
        //    dot = GetComponentInChildren<RectTransform>();
        //}

        void Update() {
            float factor = 1;// needs real car mass and gravity
            Vector2 origin = dot.anchoredPosition;
            origin.x = wheel.resultingForce.x / factor;
            origin.y = wheel.resultingForce.z / factor;
        }
    }
}
