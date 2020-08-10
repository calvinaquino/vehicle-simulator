using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    [AddComponentMenu("VehiclePhysics/Drivetrain/Transmission", 3)]
    [RequireComponent(typeof(DriveForce))]
    public class Transmission : MonoBehaviour {

        public DahsboardDisplay dashboard;

        private DriveForce driveForce;

        //public float inputShaft = 0;
        //public float outputShaft = 0;
        //public float outputTorque = 0;
        //public float clucthReactionTorque = 0;

        private bool isShifting = false;
        private float shiftTime = 0.3f;
        private float shiftStart = 0f;
        private int nextShift = 0;

        [Header("Ratios")]
        public float[] gearRatios = new float[] {
        -3.83f, // reverse
        3.83f,
        2.36f,
        1.69f,
        1.31f,
        1.0f,
        0.79f,
    };
        public int gear = 0;

        public void ShiftUp() {
            if (isShifting) {
                return;
            }
            if (gear >= -1 && gear < gearRatios.Length - 1) {
                shiftStart = Time.time;
                isShifting = true;
                nextShift = gear + 1;
            }
        }

        public void ShiftDown() {
            if (isShifting) {
                return;
            }
            if (gear <= gearRatios.Length - 1 && gear > -1) {
                shiftStart = Time.time;
                isShifting = true;
                nextShift = gear - 1;
            }
        }

        public float GetCurrentRatio() {
            if (isShifting) {
                return 0f;
            }
            if (gear > 0) {
                return gearRatios[gear];
            } else if (gear == -1) {
                return gearRatios[0];
            }
            // neutral
            return 0f;
        }

        public bool IsNeutral() {
            return gear == 0 || isShifting;
        }

        void GetInput() {
            if (isShifting) {
                return;
            }
            if (Input.GetKey(KeyCode.A)) {
                ShiftUp();
            }
            if (Input.GetKey(KeyCode.Z)) {
                ShiftDown();
            }
        }

        private void Start() {
            driveForce = transform.root.GetComponentInChildren<DriveForce>();
        }

        private void FixedUpdate() {
            GetInput();

            if (isShifting) {
                if (shiftStart + shiftTime < Time.time) {
                    isShifting = false;
                    gear = nextShift;
                }
            }
            if (dashboard) {
                string text = gear.ToString();
                if (gear == 0) {
                    text = "N";
                } else if (gear == -1) {
                    text = "R";
                }
                dashboard.gearIndicator.text = isShifting ? "-" : text;
            }

            float currentGearRatio = GetCurrentRatio();
            driveForce.torque *= currentGearRatio;
            driveForce.feedbackRPM = driveForce.rpm / currentGearRatio;
        }
    }
}