using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    [AddComponentMenu("VehiclePhysics/Drivetrain/Differential", 4)]
    [RequireComponent(typeof(DriveForce))]
    public class Differential : MonoBehaviour {
        public WheelController[] drivingWheels = new WheelController[] { };
        private DriveForce driveForce;
        public float finalRatio = 3.27f;

        private void Start() {
            driveForce = transform.root.GetComponentInChildren<DriveForce>();
        }

        private void FixedUpdate() {
            int drivingWheelsCount = drivingWheels.Length;
            driveForce.torque *= finalRatio / (float)drivingWheelsCount;
            driveForce.feedbackRPM /= finalRatio;
        }
    }
}
