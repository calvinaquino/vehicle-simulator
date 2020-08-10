using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VehiclePhysics {
    public class InputDisplay : MonoBehaviour {

        public RectTransform accelerator;
        public RectTransform brake;

        public RectTransform steeringRange;
        public RectTransform steeringIndicator;

        private float acceleratorHeight;
        private float brakeHeight;
        private float indicatorCenter;

        void Start() {
            acceleratorHeight = accelerator.sizeDelta.y;
            brakeHeight = brake.sizeDelta.y;
            indicatorCenter = steeringRange.anchoredPosition.x;
        }

        void Update() {
            accelerator.sizeDelta = new Vector2(accelerator.sizeDelta.x, acceleratorHeight * Input.GetAxis("Accelerate"));
            brake.sizeDelta = new Vector2(brake.sizeDelta.x, brakeHeight * Input.GetAxis("Brake"));

            float range = steeringRange.sizeDelta.x / 2f;
            Vector2 steeringIndicatorPosition = steeringIndicator.anchoredPosition;
            steeringIndicatorPosition.x = indicatorCenter + range * Input.GetAxis("Horizontal");
            steeringIndicator.anchoredPosition = steeringIndicatorPosition;
        }
    }
}