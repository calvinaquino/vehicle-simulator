using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    [AddComponentMenu("VehiclePhysics/Vehicle", 1)]
    public class Vehicle : MonoBehaviour {
        [Header("Wheels")]
        public WheelController[] wheels; // 0 = FL, 1 = FR, 2 = RL, 3 = RR

        [Header("Body")]
        public GameObject body;
        public Vector3 centerOfMass = new Vector3(0, 0, 0);

        [Header("Camera")]
        public new Camera camera;

        // helper
        enum WheelPosition {
            FrontLeft = 0,
            FrontRight = 1,
            RearLeft = 2,
            RearRight = 3
        }

        private Engine engine;

        private WheelController frontLeft => wheels[(int)WheelPosition.FrontLeft];
        private WheelController frontRight => wheels[(int)WheelPosition.FrontRight];
        private WheelController rearLeft => wheels[(int)WheelPosition.RearLeft];
        private WheelController rearRight => wheels[(int)WheelPosition.RearRight];

        [Header("Turning")]
        public float turnRadius;

        public float mass => rigidbody.mass;

        private float wheelBase;
        private float steeringInput;
        private new Rigidbody rigidbody;



        void Start() {
            engine = GetComponentInChildren<Engine>();
            rigidbody = GetComponent<Rigidbody>();
        }

        void Update() {
            steeringInput = Input.GetAxis("Horizontal");
            if (Input.GetKey(KeyCode.Space)) {
                rigidbody.isKinematic = !rigidbody.isKinematic;
            }
            if (Input.GetKey(KeyCode.R)) {
                rigidbody.transform.position += rigidbody.transform.up * 1.5f * Time.deltaTime;
                rigidbody.transform.rotation = Quaternion.identity;
            }
            camera.transform.position = rigidbody.transform.position + new Vector3(6, 6, 0);
            camera.transform.LookAt(this.transform);
        }

        void FixedUpdate() {
            rigidbody.centerOfMass = this.centerOfMass;
            CalculateWheelTurnAngles();

            // float accel = Input.GetAxis("Vertical") * 10000;
            // rigidbody.AddForce(transform.forward * accel);
        }

        private void CalculateWheelTurnAngles() {
            wheelBase = frontLeft.transform.localPosition.z - rearLeft.transform.localPosition.z;
            if (frontLeft.turns) {
                frontLeft.turnAngle = CalculateAckermanAngle(rearLeft.transform.localPosition.x);
            }
            if (frontRight.turns) {
                frontRight.turnAngle = CalculateAckermanAngle(rearRight.transform.localPosition.x);
            }
            if (rearLeft.turns) {
                rearLeft.turnAngle = -CalculateAckermanAngle(-frontLeft.transform.localPosition.x);
            }
            if (rearRight.turns) {
                rearRight.turnAngle = -CalculateAckermanAngle(-frontRight.transform.localPosition.x);
            }
        }

        private float CalculateAckermanAngle(float oppositeTrack) {
            if (steeringInput > 0) {
                return Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - oppositeTrack)) * steeringInput;
            } else if (steeringInput < 0) {
                return Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + oppositeTrack)) * steeringInput;
            } else {
                return 0;
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Vector3 centerOfMassForGizmo = this.centerOfMass;
            if (Application.isPlaying) {
                centerOfMassForGizmo = rigidbody.centerOfMass;
            }
            Gizmos.DrawWireSphere(transform.TransformPoint(centerOfMassForGizmo), 0.1f);
        }
    }
}