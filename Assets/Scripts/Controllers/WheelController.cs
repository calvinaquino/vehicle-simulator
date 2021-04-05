using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace VehiclePhysics {
    public struct WheelContact {
        public bool exists;
        public float distance;
        public Vector3 point;
        public Vector3 normal;
        public Vector3 relativeVelocity;
        public Collider collider;
    }

    [AddComponentMenu("VehiclePhysics/Drivetrain/WheelController", 5)]
    public class WheelController : MonoBehaviour {
        [Header("Suspension")]
        public float suspensionHeight;
        public float suspensionDamper;
        public float suspensionStiffness;

        [Header("Wheel")]
        public WheelRenderer wheel;

        [Header("Config")]
        public bool drives;
        public bool turns;

        [Header("Debug")]
        public WheelDebug wheelDebug;

        [HideInInspector]
        public float turnAngle = 0f;
        [HideInInspector]
        public float momentOfInertia => wheel.radius * wheel.weight;
        //[HideInInspector]
        //public float torque = 0f;
        [HideInInspector]
        public float angularVelocity = 0f;
        [HideInInspector]
        public float clutchEngageRatio = 0f;
        [HideInInspector]
        public WheelContact contact = new WheelContact();
        [HideInInspector]
        public float rawRPM = 0f;
        [HideInInspector]
        public float currentRPM => wheel.angularVelocity.RadToRPM();
        [HideInInspector]
        public float sidewaysSlip = 0f;
        [HideInInspector]
        public float forwardSlip = 0f;

        new private Rigidbody rigidbody;
        private DriveForce driveForce;
        private Vector3 wheelPosition;
        private Vector3 defaultWheelPosition => transform.position - (transform.up * suspensionHeight);

        private float driveRPM;
        private float driveTorque;


        public bool isFront => transform.position.z > 0;
        public bool isRear => transform.position.z < 0;
        public bool isLeft => transform.position.x < 0;
        public bool isRight => transform.position.x > 0;
        public bool isFrontLeft => isFront && isLeft;
        public bool isFrontRight => isFront && isRight;
        public bool isRearLeft => isRear && isLeft;
        public bool isRearRight => isRear && isRight;

        private Vector3 hitPointVelocity;

        private float suspensionCompression = 0;
        private float lastSuspensionCompression = 0;

        private Vector3 suspensionForce = Vector3.zero;
        private float forwardForce;
        private float lateralForce;

        private float maximumLateralForce;
        private float currentLateralForce;

        [HideInInspector]
        public Vector3 resultingForce = Vector3.zero;

        // from 0.006 to 0.015 are good values
        //private float rollingResistanceFactor = 0.01f;

        void Start() {
            driveForce = GetComponent<DriveForce>();
            driveTorque = driveForce.torque;
            driveRPM = driveForce.rpm;

            rigidbody = transform.root.GetComponent<Rigidbody>();
            wheelPosition = defaultWheelPosition;
            UpdateWheelPosition();
        }

        void FixedUpdate() {
            // rotate wheel to steering direction
            SteerWheel();
            // raycast, get contact info
            GetContact();

            // get current wheel angular vel
            GetRawRPM();

            ApplyDrive();

            // calculate and apply suspension forces and normals
            ApplySuspensionForce();

            GetSlip();
            ApplyFriction();
            UpdateWheelPosition();

            DebugForces();
        }

        private float Pacejka(float slip) {
            // constants
            float D = 1; //max and min
            float B = 1;
            float E = 1;
            float C = 1;
            return D * Mathf.Sin(C * Mathf.Atan(B * slip - E * (B * slip - Mathf.Atan(B * slip))));
        }

        private void UpdateWheelPosition() {
            if (wheel) {
                wheel.transform.position = wheelPosition;
            }
        }

        private void ApplySuspensionForce() {

            if (contact.exists) {
                // get wheel position
                wheelPosition = (contact.normal * wheel.radius) + contact.point;

                // calculate suspension compression
                lastSuspensionCompression = suspensionCompression;
                suspensionCompression = Mathf.Clamp(Vector3.Distance(wheelPosition, defaultWheelPosition), 0, suspensionHeight);

                float compressionRatio = 1 / suspensionHeight;
                float compressionVelocity = (lastSuspensionCompression - suspensionCompression) / Time.fixedDeltaTime;

                float damperForce = suspensionDamper * compressionVelocity;
                float springForce = suspensionStiffness * suspensionCompression;
                suspensionForce = (springForce - damperForce) * contact.normal * compressionRatio;
                rigidbody.AddForceAtPosition(suspensionForce, contact.point);
            } else {
                suspensionForce = Vector3.zero;
                lastSuspensionCompression = 0;
                suspensionCompression = 0;
                wheelPosition = defaultWheelPosition;
            }

        }

        //private void ApplyForces() {
        //    rigidbody.AddForceAtPosition((-lateralForce * transform.right), contact.point);
        //    if (drives) {
        //        rigidbody.AddForceAtPosition((forwardForce * transform.forward), contact.point);
        //    }
        //}

        void GetRawRPM() {
            if (contact.exists) {
                rawRPM = (contact.relativeVelocity.z / wheel.circumference * 60f );
            } else {
                rawRPM = wheel.rpm;
            }
        }

        void ApplyDrive() {
            float feedbackRPMBias = 0.5f;

        }

        // kinda DONE
        void GetSlip() {
            if (contact.exists) {
                sidewaysSlip = contact.relativeVelocity.x * 0.01f;
                forwardSlip = (0.01f * (rawRPM - currentRPM));
            } else {
                sidewaysSlip = 0;
                forwardSlip = 0;
            }
        }

        void ApplyFriction() {
            if (contact.exists) {

                //throw some forces
                float pushForce = 4000 * Input.GetAxis("Accelerate") - 4000 * Input.GetAxis("Brake");
                rigidbody.AddForceAtPosition(transform.forward * pushForce, contact.point);

                rigidbody.AddForceAtPosition(transform.right * -contact.relativeVelocity.x * rigidbody.mass, contact.point);
                wheel.angularVelocity = rawRPM.RPMToRad() * wheel.circumference;

                // Debug.DrawRay(contact.point, transform.right * -contact.relativeVelocity.x * rigidbody.mass, Color.red);

                //rigidbody.AddForceAtPosition(resultingForce, contact.point);
            }
        }

        void GetContact() {
            RaycastHit hit;
            contact.exists = Physics.SphereCast(transform.position, wheel.radius, -transform.up, out hit, suspensionHeight);
            if (contact.exists) {
                hitPointVelocity = rigidbody.GetPointVelocity(hit.point);

                contact.distance = hit.distance - wheel.radius;
                contact.normal = hit.normal;
                contact.point = hit.point;
                contact.relativeVelocity = transform.InverseTransformDirection(hitPointVelocity);
                contact.collider = hit.collider;
            } else {
                contact.distance = suspensionHeight;
                contact.normal = Vector3.zero;
                contact.point = Vector3.zero;
                contact.relativeVelocity = Vector3.zero;
                contact.collider = null;
            }
        }

        void SteerWheel() {
            transform.localRotation = Quaternion.Euler(Vector3.up * turnAngle);
        }

        private void OnValidate() {
            rigidbody = transform.root.GetComponent<Rigidbody>();
            wheelPosition = defaultWheelPosition;
            UpdateWheelPosition();
        }

        void DebugForces() {
            if (wheelDebug) {
                wheelDebug.normalForce.text = "NormalForce: " + suspensionForce.magnitude.ToString();
                wheelDebug.rightForce.text = "RightForce: " + lateralForce.ToString();
                wheelDebug.forwardForce.text = "ForwardForce: " + forwardForce.ToString();
            }
        }

        // GIZMOS
        void OnDrawGizmos() {
            // render suspension
            Vector3 wheelPos = transform.TransformPoint(Vector3.down * (suspensionHeight - suspensionCompression));
            float debugMass = 1500f;

            if (Application.isPlaying) {
                debugMass = rigidbody.mass;
            }
            float debugForceRatio = debugMass * Physics.gravity.magnitude / 4f;

            // debug axis forces
            Gizmos.color = Color.red;
            // Gizmos.DrawLine(wheelPos, wheelPos + (this.transform.forward * contact.relativeVelocity.z));
            // Gizmos.DrawLine(wheelPos, wheelPos + (this.transform.right * contact.relativeVelocity.x));
            // Gizmos.DrawLine(wheelPos, wheelPos + contact.relativeVelocity);

            // render suspension top link
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            // render wheel direction
            Gizmos.DrawLine(wheelPos, wheelPos + (this.transform.forward * wheel.radius));
            // render movement direction
            if (rigidbody) {
                Vector3 wheelVelocity = rigidbody.GetPointVelocity(contact.point);
                wheelVelocity.y = 0;
                Gizmos.DrawLine(wheelPos, wheelPos + (wheelVelocity.normalized * wheel.radius));
            }

            // render local velocity
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(contact.point, contact.point + (contact.relativeVelocity / debugForceRatio));

            // render suspension line
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, wheelPos);

            //// render wheel
            //Vector3 offset = -transform.up * (suspensionHeight - suspensionCompression);
            //GizmoDrawCircle(offset, wheel.width, wheel.radius);

            // render friction circle
            Vector3 circleCenter = transform.position - (transform.up * (suspensionHeight + wheel.radius - suspensionCompression) * 0.97f);
            if (contact.exists) {
                float oneG = rigidbody.mass * Physics.gravity.magnitude;
                float load = suspensionForce.magnitude;
                float halfCircle = wheel.width / 2f * (load / oneG) * 10f;
                GizmoDrawFlatCircle(circleCenter, halfCircle);
            } else {
                GizmoDrawFlatCircle(circleCenter, 0.01f);
            }

            // render forces
            bool debugForcesEnabled = true;
            if (debugForcesEnabled) {
                bool debugForcesSeparated = true;
                if (debugForcesSeparated) {
                    Gizmos.color = Color.blue;
                    // Debug Upwards Force
                    Gizmos.DrawLine(circleCenter, circleCenter + suspensionForce / debugForceRatio);
                    // Debug Forward Force
                    Gizmos.DrawLine(circleCenter, circleCenter + (forwardForce / debugForceRatio * transform.forward));


                    // Debug Lateral Force
                    if (Mathf.Abs(currentLateralForce) < maximumLateralForce) {
                        lateralForce = currentLateralForce;
                        Gizmos.color = Color.blue;
                    } else {
                        lateralForce = 0;
                        Gizmos.color = Color.red;
                    }
                    Gizmos.DrawLine(circleCenter, circleCenter + (currentLateralForce / debugForceRatio * transform.right));
                } else {
                    // Debug Resulting Force
                    Gizmos.color = Color.red;
                    Vector3 debugResultingForce = resultingForce;
                    debugResultingForce.y = 0;
                    Gizmos.DrawLine(circleCenter, circleCenter + debugResultingForce / debugForceRatio);
                }
            }
        }

        void GizmoDrawFlatCircle(Vector3 origin, float radius) {
            Gizmos.color = Color.white;
            Vector3 point1;
            Vector3 point0 = radius * new Vector3(Mathf.Sin(0), 0, Mathf.Cos(0));
            for (int i = 1; i <= 20; ++i) {
                point1 = radius * new Vector3(Mathf.Sin(i / 20.0f * Mathf.PI * 2.0f), 0, Mathf.Cos(i / 20.0f * Mathf.PI * 2.0f));
                Gizmos.DrawLine(origin + point0, origin + point1);
                point0 = point1;
            }
        }

        /////
        //private void CalcLongitudinalForces() {

        //    // calculate reaction torque from ground
        //    float groundReactionTorque = 0;
        //    if (contact.exists) {
        //        groundReactionTorque = contact.normal.y;
        //    }

        //    float netTorque = torque - groundReactionTorque;
        //    float angularAcceleration = netTorque / wheel.momentOfInertia;



        //    if (contact.exists) {
        //        // current wheel rotation
        //        float wheelAngularVelocity = wheel.angularVelocity;
        //        float newWheelAngularVelocity = angularVelocity;
        //        float clutchRatio = clutchEngageRatio;

        //        //float rawForwardForce = Input.GetAxis("Vertical") * 10000;
        //        // TODO find out speed of car according to wheel rotation

        //        // applied torque
        //        float engineTorque = torque;
        //        // wheel rotation
        //        float wheelAngularVel = wheel.angularVelocity;
        //        // wheel forward force
        //        float contactForce = torque * wheel.radius;


        //        forwardForce = rigidbody.mass * angularVelocity * wheel.radius * Mathf.Cos(Mathf.Deg2Rad * turnAngle);

        //        // debuggy local vel
        //        Debug.DrawLine(wheelPosition, wheelPosition + (hitPointVelocity / 10f), Color.white);
        //    }
        //}

        //private void CalcLateralForces() {
        //    if (contact.exists) {
        //        float turnAngleLateralForce = torque * Mathf.Sin(Mathf.Deg2Rad * turnAngle);

        //        // 1 lat g equals to mass * gravity
        //        maximumLateralForce = rigidbody.mass * Physics.gravity.magnitude;
        //        currentLateralForce = contact.relativeVelocity.x * rigidbody.mass - turnAngleLateralForce;

        //        if (Mathf.Abs(currentLateralForce) < maximumLateralForce) {
        //            lateralForce = currentLateralForce;
        //        } else {
        //            lateralForce = currentLateralForce * 0.2f;
        //        }
        //    }
        //}
    }
}