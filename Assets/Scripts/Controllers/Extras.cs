using UnityEngine;
using System.Collections;

namespace VehiclePhysics {
    //Static class with extra functions
    public static class F {
        //Returns the number with the greatest absolute value
        public static float MaxAbs(params float[] nums) {
            float result = 0;

            for (int i = 0; i < nums.Length; i++) {
                if (Mathf.Abs(nums[i]) > Mathf.Abs(result)) {
                    result = nums[i];
                }
            }

            return result;
        }

        //Returns the topmost parent of a Transform with a certain component
        public static T GetTopmostParentComponent<T>(this Transform tr) where T : Component {
            T getting = null;

            while (tr.parent != null) {
                if (tr.parent.GetComponent<T>() != null) {
                    getting = tr.parent.GetComponent<T>();
                }

                tr = tr.parent;
            }

            return getting;
        }

        // RPM <-> Radians helpers
        public static float RPMToRad(this float rpm) {
            const float RadToRPMFactor = 9.549f;
            return rpm / RadToRPMFactor;
        }

        public static float RadToRPM(this float rad) {
            const float RadToRPMFactor = 9.549f;
            return rad * RadToRPMFactor;
        }
    }
}