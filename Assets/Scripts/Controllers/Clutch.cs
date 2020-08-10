using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    [AddComponentMenu("VehiclePhysics/Drivetrain/Clutch", 2)]
    [RequireComponent(typeof(DriveForce))]
    public class Clutch : MonoBehaviour {
        private float clutchInput;
        private DriveForce driveForce;
        public float GetClutchEngageRatio() => 1f - clutchInput;

        /*
         * In my implementation the clutch computes the torque that keeps itself locked. 
         * If any torque coming from either side is greater than that value, or the torques 
         * have opposite sign with a difference larger than it, then the clutch slips. 
         * This requires knowing the reaction torque at both sides, with involves knowing 
         * the reaction torque at the wheels.
         */

        private void Start() {
            driveForce = transform.root.GetComponentInChildren<DriveForce>();
        }

        private void FixedUpdate() {
            clutchInput = Input.GetAxis("Clutch");
            // update DriveForce?
        }
    }
}