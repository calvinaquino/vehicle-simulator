using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VehiclePhysics {
    public struct SDriveForce {
        public float torque;
        public float reactionTorque;
        public float momentOfInertia;
        public float rpm => this.GetRPM();

        private float GetRPM() {
            float torquePerInertia = (torque - reactionTorque) / momentOfInertia;
            return torquePerInertia.RadToRPM();
        }
    }

    public class TorqueCurve {
        public AnimationCurve torqueCurve = new AnimationCurve();
        // rotations per minute
        public TorqueCurve(float[] rpms, float[] torques) {
            if (rpms.Length != torques.Length) {
                throw new UnityException("RPM array does not have the same size (" + rpms.Length + ") as the torques array (" + torques.Length + ").");
            }

            for (int i = 0; i < rpms.Length; i++) {
                this.torqueCurve.AddKey(rpms[i], torques[i]);
            }
        }

        // evaluates angular velocity, which is a more common physics measure
        public float EvaluateAV(float angularVelocity) {
            return this.torqueCurve.Evaluate(angularVelocity.RadToRPM());
        }

        // evaluates rotations per minute which is common for gearheads
        public float EvaluateRPM(float rpm) {
            return this.torqueCurve.Evaluate(rpm);
        }
    }

    [DisallowMultipleComponent]
    public class EngineTest : MonoBehaviour {
        public DahsboardDisplay dashboard;
        public Rotator engineRotator;
        public TorqueCurve torqueCurve;

        [HideInInspector]
        public SDriveForce driveForce;

        private float rotationalMass = 5f;
        private float rotationalRadius = 0.05f;
        private float rotationalFriction = 0.2f;
        private float rotationAcumulationEuler = 0;

        public float momentOfInertia => rotationalMass * rotationalRadius;
        private float maxRpm => rpms[rpms.Length - 1];

        private float rpm => driveForce.rpm;
        private float idleRpm = 1000f;

        private float accelerationInput;
        private float clutchInput;
        private bool starterInput;

        private bool limitting = false;
        private float limitThreshold = 500f;

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
            51,
            111,
            157,
            189,
            226,
            255,
            280,
            290,
            312,
            320,
            310,
            270,
            230,
            195,
        };

        // Functions

        void GetInputs() {
            accelerationInput = Input.GetAxis("Accelerate");
            clutchInput = Input.GetAxis("Clutch");
            starterInput = Input.GetKey(KeyCode.S);
        }

        public float GetClutchEngageRatio() => 1f - clutchInput;

        public float TorqueAtRPM(float rpm) {
            return torqueCurve.EvaluateRPM(rpm);
        }

        private float GetThrottle() {
            if (limitting) {
                return 0f;
            }
            float idleTarget = idleRpm / (1f - rotationalFriction);
            if (rpm < idleTarget) {
                float fullThrottleBelowRatio = 0.8f; // 80% of the low range will use full throttle
                float fullThrottleMax = fullThrottleBelowRatio * idleTarget;
                if (rpm > fullThrottleMax) {
                    float value = rpm - fullThrottleMax;
                    float delta = idleTarget - fullThrottleMax;
                    float throttleIdle = (delta - value) / delta;
                    return Mathf.Max(throttleIdle, accelerationInput);
                }
                return 1f;

            }
            return accelerationInput;
        }

        private float GetStarterEngineTorque() {
            return starterInput && rpm < idleRpm ? 10f : 0f;
        }

        private float GetEngineAccelerationFromTorque() {
            return driveForce.torque / momentOfInertia;
        }

        private void NormalizeRPM() {
            //if (rpm < 0) {
            //    driveForce.rpm = 0;
            //}
            //if (rpm > maxRpm) {
            //    limitting = true;
            //} else if (rpm < (maxRpm - limitThreshold)) {
            //    limitting = false;
            //}
        }

        private void GenerateTorque() {
            float throttle = GetThrottle();
            float frictionTorque = rpm.RPMToRad() * rotationalFriction;

            float starterTorque = GetStarterEngineTorque();

            driveForce.torque = TorqueAtRPM(rpm) * throttle + starterTorque - frictionTorque;
            driveForce.momentOfInertia = 
            //driveForce.rpm += GetEngineAccelerationFromTorque().RadToRPM() * Time.fixedDeltaTime;

            //NormalizeRPM();
        }

        void Start() {
            this.torqueCurve = new TorqueCurve(rpms, torques);
        }

        private void Update() {
            if (dashboard) {
                dashboard.rpm = rpm;
                dashboard.rpmMax = maxRpm;
                dashboard.RPMText.text = rpm.ToString();
            }
        }

        private void FixedUpdate() {
            GetInputs();
            GenerateTorque();
            engineRotator.angularVelocity = this.rpm.RPMToRad();
        }
    }
}