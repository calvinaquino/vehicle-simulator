using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    [AddComponentMenu("VehiclePhysics/Drivetrain/DriveForce", 1)]
    public class DriveForce : MonoBehaviour {
        [HideInInspector]
        public Vehicle vehicle;
        [HideInInspector]
        public float torque = 0;
        [HideInInspector]
        public float rpm = 0;
        [HideInInspector]
        public float feedbackRPM = 0;

        private void Start() {
            vehicle = transform.root.GetComponent<Vehicle>();
        }
    }
}