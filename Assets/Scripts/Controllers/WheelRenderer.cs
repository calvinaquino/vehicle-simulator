using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    [AddComponentMenu("VehiclePhysics/Wheel", 1)]
    public class WheelRenderer : MonoBehaviour {
        public float rimSizeInches = 20f;
        public float radius = 0.33f;
        public float width = 0.2f;
        public float rimWidth = 0.25f;
        public float weight = 15f;

        public float angularVelocity = 0;
        public float rpm {
            set { angularVelocity = value.RPMToRad(); }
            get { return angularVelocity.RadToRPM(); }
        }

        public float momentOfInertia => weight * radius * radius / 2f;
        public float reactionTorque => momentOfInertia * angularVelocity; //?
        public float circumference => 2 * Mathf.PI * radius;

        [Header("Rendering")]
        public float renderDivisions = 36f;
        public int wireLinkDistance = 4;

        private float rotationAcumulationEuler = 0;


        void Update() {
            Rotate();
        }

        private void Rotate() {
            float eulerAcceleration = Mathf.Rad2Deg * angularVelocity * Time.deltaTime;
            rotationAcumulationEuler = (rotationAcumulationEuler + eulerAcceleration) % 360f;

            transform.localRotation = Quaternion.Euler(Vector3.right * rotationAcumulationEuler);

        }

        void OnDrawGizmos() {
            float rimSizeMeters = rimSizeInches * 0.025f;
            float rimSizeRadius = rimSizeMeters / 2f;
            float tireInnerRadius = rimSizeRadius;
            float rimOuterRadius = tireInnerRadius * 0.95f;
            float rimInnerRadius = rimOuterRadius * 0.4f;

            Vector3 center = transform.position;

            Gizmos.color = Color.black;
            GizmoDrawCircle(center, width, radius);
            GizmoDrawCircle(center, width, tireInnerRadius);
            Gizmos.color = Color.grey;
            GizmoDrawCircle(center, rimWidth, rimOuterRadius);
            GizmoDrawCircle(center, rimWidth, rimInnerRadius, true);
        }

        void GizmoDrawCircle(Vector3 origin, float width, float radius, bool drawMarker = false) {
            Vector3 offsetLeft = origin - (transform.right * width / 2);
            Vector3 offsetRight = origin + (transform.right * width / 2);

            Vector3 point1;
            Vector3 point0 = transform.TransformDirection(radius * CirclePoint(0));

            for (int i = 1; i <= renderDivisions; ++i) {
                point1 = transform.TransformDirection(radius * CirclePoint(i / renderDivisions));
                Gizmos.DrawLine(offsetLeft + point0, offsetLeft + point1);
                Gizmos.DrawLine(offsetRight + point0, offsetRight + point1);

                // link edges
                if (i % wireLinkDistance == 0) {
                    Gizmos.DrawLine(offsetRight + point0, offsetLeft + point0);
                }


                point0 = point1;
            }

        }

        private Vector3 CirclePoint(float rangeFactor) {
            return new Vector3(0, Mathf.Sin(rangeFactor * Mathf.PI * 2.0f), Mathf.Cos(rangeFactor * Mathf.PI * 2.0f));
        }
    }
}