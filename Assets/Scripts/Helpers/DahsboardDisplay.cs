using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VehiclePhysics {
    public class DahsboardDisplay : MonoBehaviour {

        [Header("References")]
        public Text RPMText;
        public RectTransform RPMIndicator;
        public RectTransform RPMRange;
        public Text gearIndicator;

        [HideInInspector]
        public float rpm = 0;
        [HideInInspector]
        public float rpmMax = 0;

        void Update() {
            float rpmRatio = rpm / rpmMax;
            float gaugeOrigin = RPMRange.anchoredPosition.x;
            float gaugeWidth = RPMRange.sizeDelta.x;
            float indicatorPosition = Mathf.Lerp(gaugeOrigin, gaugeWidth, rpmRatio);

            Vector2 RPMIndicatorPosition = RPMIndicator.anchoredPosition;
            RPMIndicatorPosition.x = gaugeOrigin + indicatorPosition - (gaugeWidth / 2f);
            RPMIndicator.anchoredPosition = RPMIndicatorPosition;
        }
    }
}