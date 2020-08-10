using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    public class WireframeGizmo : MonoBehaviour {
        private void OnDrawGizmos() {
            // make points
            Transform parent = transform.parent;
            Vector3 measures = transform.localScale / 2f;
            Vector3 offset = transform.localPosition;

            Vector3 topFrontLeft = parent.TransformPoint(new Vector3(-measures.x, measures.y, measures.z) + offset);
            Vector3 topFrontRight = parent.TransformPoint(new Vector3(measures.x, measures.y, measures.z) + offset);
            Vector3 topRearLeft = parent.TransformPoint(new Vector3(-measures.x, measures.y, -measures.z) + offset);
            Vector3 topRearRight = parent.TransformPoint(new Vector3(measures.x, measures.y, -measures.z) + offset);
            Vector3 bottomFrontLeft = parent.TransformPoint(new Vector3(-measures.x, -measures.y, measures.z) + offset);
            Vector3 bottomFrontRight = parent.TransformPoint(new Vector3(measures.x, -measures.y, measures.z) + offset);
            Vector3 bottomRearLeft = parent.TransformPoint(new Vector3(-measures.x, -measures.y, -measures.z) + offset);
            Vector3 bottomRearRight = parent.TransformPoint(new Vector3(measures.x, -measures.y, -measures.z) + offset);

            Gizmos.color = Color.green;
            // top
            Gizmos.DrawLine(topFrontLeft, topFrontRight);
            Gizmos.DrawLine(topFrontRight, topRearRight);
            Gizmos.DrawLine(topRearRight, topRearLeft);
            Gizmos.DrawLine(topRearLeft, topFrontLeft);
            // middle
            Gizmos.DrawLine(topFrontLeft, bottomFrontLeft);
            Gizmos.DrawLine(topFrontRight, bottomFrontRight);
            Gizmos.DrawLine(topRearRight, bottomRearRight);
            Gizmos.DrawLine(topRearLeft, bottomRearLeft);
            // bottom
            Gizmos.DrawLine(bottomFrontLeft, bottomFrontRight);
            Gizmos.DrawLine(bottomFrontRight, bottomRearRight);
            Gizmos.DrawLine(bottomRearRight, bottomRearLeft);
            Gizmos.DrawLine(bottomRearLeft, bottomFrontLeft);
        }
    }
}