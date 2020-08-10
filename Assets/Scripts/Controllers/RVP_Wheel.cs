using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics {
    [RequireComponent(typeof(DriveForce))]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Drivetrain/Wheel", 1)]

    //Class for the wheel
    public class Wheel : MonoBehaviour {
        [System.NonSerialized]
        public Transform tr;
        Rigidbody rb;
        [System.NonSerialized]
        public Vehicle vp;
        //[System.NonSerialized]
        //public Suspension suspensionParent;
        [System.NonSerialized]
        public Transform rim;
        Transform tire;
        Vector3 localVel;

        //[Tooltip("Generate a sphere collider to represent the wheel for side collisions")]
        //public bool generateHardCollider = true;
        //SphereCollider sphereCol;//Hard collider
        //Transform sphereColTr;//Hard collider transform

        [Header("Rotation")]

        [Tooltip("Bias for feedback RPM lerp between target RPM and raw RPM")]
        [Range(0, 1)]
        public float feedbackRpmBias;

        [Tooltip("Curve for setting final RPM of wheel based on driving torque/brake force, x-axis = torque/brake force, y-axis = lerp between raw RPM and target RPM")]
        public AnimationCurve rpmBiasCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("As the RPM of the wheel approaches this value, the RPM bias curve is interpolated with the default linear curve")]
        public float rpmBiasCurveLimit = Mathf.Infinity;

        [Range(0, 10)]
        public float axleFriction;

        [Header("Friction")]

        [Range(0, 1)]
        public float frictionSmoothness = 0.5f;
        public float forwardFriction = 1;
        public float sidewaysFriction = 1;
        public float forwardRimFriction = 0.5f;
        public float sidewaysRimFriction = 0.5f;
        public float forwardCurveStretch = 1;
        public float sidewaysCurveStretch = 1;
        Vector3 frictionForce = Vector3.zero;

        [Tooltip("X-axis = slip, y-axis = friction")]
        public AnimationCurve forwardFrictionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("X-axis = slip, y-axis = friction")]
        public AnimationCurve sidewaysFrictionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [System.NonSerialized]
        public float forwardSlip;
        [System.NonSerialized]
        public float sidewaysSlip;
        //public enum SlipDependenceMode { dependent, forward, sideways, independent };
        //public SlipDependenceMode slipDependence = SlipDependenceMode.sideways;
        //[Range(0, 2)]
        //public float forwardSlipDependence = 2;
        //[Range(0, 2)]
        //public float sidewaysSlipDependence = 2;

        [Tooltip("Adjusts how much friction the wheel has based on the normal of the ground surface. X-axis = normal dot product, y-axis = friction multiplier")]
        public AnimationCurve normalFrictionCurve = AnimationCurve.Linear(0, 1, 1, 1);

        //[Tooltip("How much the suspension compression affects the wheel friction")]
        //[Range(0, 1)]
        //public float compressionFrictionFactor = 0.5f;

        [Header("Size")]

        public float tireRadius;
        public float rimRadius;
        public float tireWidth;
        public float rimWidth;

        [System.NonSerialized]
        public float actualRadius;

        [Header("Tire")]

        [Tooltip("Requires deform shader")]

        float currentRPM;
        [System.NonSerialized]
        public DriveForce targetDrive;
        [System.NonSerialized]
        public float rawRPM;//RPM based purely on velocity
        [System.NonSerialized]
        public WheelContact contactPoint = new WheelContact();
        [System.NonSerialized]
        public bool grounded;
        [System.NonSerialized]
        public float travelDist;
        Vector3 upDir;//Up direction
        float circumference;

        [System.NonSerialized]
        public Vector3 contactVelocity;//Velocity of contact point
        float actualEbrake;
        float actualTargetRPM;
        float actualTorque;

        [System.NonSerialized]
        public Vector3 forceApplicationPoint;//Point at which friction forces are applied

        [Tooltip("Apply friction forces at ground point")]
        public bool applyForceAtGroundContact;

        void Start() {
            tr = transform;
            rb = tr.GetTopmostParentComponent<Rigidbody>();
            vp = tr.GetTopmostParentComponent<Vehicle>();

            targetDrive = GetComponent<DriveForce>();
            currentRPM = 0;
        }

        void FixedUpdate() {
            upDir = tr.up;
            actualRadius = tireRadius;
            circumference = Mathf.PI * actualRadius * 2;
            localVel = rb.GetPointVelocity(forceApplicationPoint);

            //Get proper inputs
            actualEbrake = 0;
            actualTargetRPM = targetDrive.rpm;
            actualTorque = targetDrive.torque;

            //if (getContact) {
                GetWheelContact();
            //} else if (grounded) {
                contactPoint.point += localVel * Time.fixedDeltaTime;
            //}

            //airTime = grounded ? 0 : airTime + Time.fixedDeltaTime;
            //forceApplicationPoint = applyForceAtGroundContact ? contactPoint.point : tr.position;

            GetRawRPM();
            ApplyDrive();

            //Get travel distance
            //travelDist = suspensionParent.compression < travelDist || grounded ? suspensionParent.compression : Mathf.Lerp(travelDist, suspensionParent.compression, suspensionParent.extendSpeed * Time.fixedDeltaTime);
            travelDist = 0.5f;

            PositionWheel();

            GetSlip();
            ApplyFriction();
        }

        void Update() {
            RotateWheel();

            if (!Application.isPlaying) {
                PositionWheel();
            }
        }

        void GetWheelContact() {
            //float castDist = Mathf.Max(suspensionParent.suspensionDistance * Mathf.Max(0.001f, suspensionParent.targetCompression) + actualRadius, 0.001f);
            //RaycastHit[] wheelHits = Physics.RaycastAll(suspensionParent.maxCompressPoint, suspensionParent.springDirection, castDist, GlobalControl.wheelCastMaskStatic);
            //RaycastHit hit;
            //int hitIndex = 0;
            bool validHit = false;
            //float hitDist = Mathf.Infinity;

            //if (connected) {
            //    //Loop through raycast hits to find closest one
            //    for (int i = 0; i < wheelHits.Length; i++) {
            //        if (!wheelHits[i].transform.IsChildOf(vp.tr) && wheelHits[i].distance < hitDist) {
            //            hitIndex = i;
            //            hitDist = wheelHits[i].distance;
            //            validHit = true;
            //        }
            //    }
            //} else {
            //    validHit = false;
            //}

            RaycastHit hit;
            //validHit = Physics.SphereCast(transform.position, wheel.radius, -transform.up, out hit, suspensionHeight);
            validHit = Physics.SphereCast(transform.position, 0.33f, -transform.up, out hit, 1);

            //Set contact point variables
            if (validHit) {
                grounded = true;
                contactPoint.distance = hit.distance - actualRadius;
                contactPoint.point = hit.point + localVel * Time.fixedDeltaTime;
                contactPoint.exists = true;
                contactPoint.normal = hit.normal;
                contactPoint.relativeVelocity = tr.InverseTransformDirection(localVel);
                contactPoint.collider = hit.collider;

                if (hit.collider.attachedRigidbody) {
                    contactVelocity = hit.collider.attachedRigidbody.GetPointVelocity(contactPoint.point);
                    contactPoint.relativeVelocity -= tr.InverseTransformDirection(contactVelocity);
                } else {
                    contactVelocity = Vector3.zero;
                }
            } else {
                grounded = false;
                //contactPoint.distance = suspensionParent.suspensionDistance;
                contactPoint.point = Vector3.zero;
                contactPoint.exists = false;
                contactPoint.normal = upDir;
                contactPoint.relativeVelocity = Vector3.zero;
                contactPoint.collider = null;
                contactVelocity = Vector3.zero;
            }
        }

        void GetRawRPM() {
            float fakeBrakeForce = 0;
            float fakeBrakeInput = 0;
            float fakeEBrakeInput = 0;

            if (grounded) {
                rawRPM = (contactPoint.relativeVelocity.x / circumference) * (Mathf.PI * 100);
            } else {
                rawRPM = Mathf.Lerp(rawRPM, actualTargetRPM, (actualTorque + fakeBrakeForce * fakeBrakeInput + actualEbrake * fakeEBrakeInput) * Time.timeScale);
            }
        }

        void GetSlip() {
            if (grounded) {
                sidewaysSlip = (contactPoint.relativeVelocity.z * 0.1f) / sidewaysCurveStretch;
                forwardSlip = (0.01f * (rawRPM - currentRPM)) / forwardCurveStretch;
            } else {
                sidewaysSlip = 0;
                forwardSlip = 0;
            }
        }

        void ApplyFriction() {
            if (grounded) {
                float forwardSlipFactor = forwardSlip - sidewaysSlip;
                float sidewaysSlipFactor = sidewaysSlip - forwardSlip;
                float forwardSlipDependenceFactor = Mathf.Clamp01(0.5f - Mathf.Clamp01(Mathf.Abs(sidewaysSlip)));
                float sidewaysSlipDependenceFactor = Mathf.Clamp01(0.5f - Mathf.Clamp01(Mathf.Abs(forwardSlip)));

                //
                float forwardFrictionForce = forwardFrictionCurve.Evaluate(Mathf.Abs(forwardSlipFactor));
                float sidewaysFrictionForce = sidewaysFrictionCurve.Evaluate(Mathf.Abs(sidewaysSlipFactor));
                //float normalFrictionForce = normalFrictionCurve.Evaluate(Mathf.Clamp01(Vector3.Dot(contactPoint.normal, GlobalControl.worldUpDir)));

                //frictionForce = Vector3.Lerp(frictionForce, tr.TransformDirection(forwardFrictionForce * forwardFriction * forwardSlipDependenceFactor, 0, sidewaysFrictionForce * sidewaysFriction * sidewaysSlipDependenceFactor * normalFrictionForce))
                //        * ((1 - compressionFrictionFactor) + (1 - suspensionParent.compression) * compressionFrictionFactor * Mathf.Clamp01(Mathf.Abs(suspensionParent.tr.InverseTransformDirection(localVel).z) * 10)) * contactPoint.surfaceFriction
                //    , 1 - frictionSmoothness);

                rb.AddForceAtPosition(frictionForce, forceApplicationPoint);

                //If resting on a rigidbody, apply opposing force to it
                if (contactPoint.collider.attachedRigidbody) {
                    contactPoint.collider.attachedRigidbody.AddForceAtPosition(-frictionForce, contactPoint.point);
                }

                //throw some forces
                //float pushForce = 10000 * Input.GetAxis("Accelerate");
                //rigidbody.AddForceAtPosition(transform.forward * pushForce, rigidbody.centerOfMass);
            }
        }

        void ApplyDrive() {
            float brakeForce = 0;

            float fakeBrakeInput = 0;
            float fakeEBrakeInput = 0;

            //Set brake force
            float fakeBrakeForce = 100;
            brakeForce = fakeBrakeForce * fakeBrakeInput;

            brakeForce += axleFriction * 0.1f * (Mathf.Approximately(actualTorque, 0) ? 1 : 0);

            //if (targetDrive.rpm != 0) {
            //    brakeForce *= (1 - vp.burnout);
            //}

            //Set final RPM
            bool validTorque = (!(Mathf.Approximately(actualTorque, 0) && Mathf.Abs(actualTargetRPM) < 0.01f) && !Mathf.Approximately(actualTargetRPM, 0)) || brakeForce + actualEbrake * fakeEBrakeInput > 0;

            currentRPM = Mathf.Lerp(rawRPM,
                Mathf.Lerp(
                Mathf.Lerp(rawRPM, actualTargetRPM, validTorque ? EvaluateTorque(actualTorque) : actualTorque)
                , 0, Mathf.Max(brakeForce, actualEbrake * fakeEBrakeInput))
            , validTorque ? EvaluateTorque(actualTorque + brakeForce + actualEbrake * fakeEBrakeInput) : actualTorque + brakeForce + actualEbrake * fakeEBrakeInput);

            targetDrive.feedbackRPM = Mathf.Lerp(currentRPM, rawRPM, feedbackRpmBias);

        }

        //Extra method for evaluating torque to make the ApplyDrive method more readable
        float EvaluateTorque(float t) {
            float torque = Mathf.Lerp(rpmBiasCurve.Evaluate(t), t, rawRPM / (rpmBiasCurveLimit * Mathf.Sign(actualTargetRPM)));
            return torque;
        }

        void PositionWheel() {
            //
        }

        void RotateWheel() {
            // 
        }
    }


    //Contact point class
    //public class WheelContact {
    //    public bool grounded;//Is the contact point grounded?
    //    public Collider col;//The collider of the contact point
    //    public Vector3 point;//The position of the contact point
    //    public Vector3 normal;//The normal of the contact point
    //    public Vector3 relativeVelocity;//Relative velocity between the wheel and the contact point object
    //    public float distance;//Distance from the suspension to the contact point minus the wheel radius
    //    public float surfaceFriction;//Friction of the contact surface
    //    public int surfaceType;//The surface type identified by the surface types array of GroundSurfaceMaster
    //}
}