using Leap.Unity;
using RootMotion.FinalIK;
using RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.XR;
using Valve.VR;

public class Tracking : Singleton<Tracking>
{
    [System.Serializable]
    public enum TrackingType {
        None,
        OpenCV,
        SteamVR,
        TobiiEyeTracker,
        LeapMotion
    }

    public struct TrackedObject
    {
        public int index;
        public string name;
    }

    public TrackingType headTracking;
    public TrackingType handTracking;
    public VRIK ik;
    public GameObject noneObject;
    public GameObject steamVRObject;
    public GameObject openCVObject;
    public GameObject tobiiObject;
    public GameObject leapMotionObject;
    public SerializableVector3 headPositionOffset;
    public SerializableVector3 headRotationOffset;
    public SerializableVector3 leftHandPositionOffset;
    public SerializableVector3 leftHandRotationOffset;
    public SerializableVector3 rightHandPositionOffset;
    public SerializableVector3 rightHandRotationOffset;
    public RuntimeAnimatorController gestureAnimator;
    private List<TrackedObject> trackedObjects;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FindAvatar()
    {
        ik = GameObject.FindGameObjectWithTag("Player").GetComponent<VRIK>();
    }

    public void SetHead(TrackingType type)
    {
        FindAvatar();
        headTracking = type;
        switch (headTracking)
        {
            case TrackingType.None:
                noneObject.SetActive(true);
                ik.solver.spine.headTarget = noneObject.transform.GetChild(2);
                break;
            case TrackingType.OpenCV:
                openCVObject.SetActive(true);
                ik.solver.spine.headTarget = openCVObject.transform.GetChild(2);
                break;
            case TrackingType.SteamVR:
                steamVRObject.SetActive(true);
                ik.solver.spine.headTarget = steamVRObject.transform.GetChild(2).GetChild(0);
                break;
            case TrackingType.TobiiEyeTracker:
                tobiiObject.SetActive(true);
                ik.solver.spine.headTarget = tobiiObject.transform.GetChild(0);
                break;
        }
        //Settings.Instance.headTracker = type.ToString();
        //Settings.Instance.SaveSettings();
    }

    public void SetHands(TrackingType type)
    {
        FindAvatar();
        handTracking = type;
        switch (handTracking)
        {
            case TrackingType.None:
                noneObject.SetActive(true);
                ik.solver.leftArm.target = noneObject.transform.GetChild(0);
                ik.solver.rightArm.target = noneObject.transform.GetChild(1);
                FindObjectOfType<LeapXRServiceProvider>().enabled = false;
                ik.gameObject.GetComponent<Animator>().runtimeAnimatorController = null;
                break;
            case TrackingType.OpenCV:
                openCVObject.SetActive(true);
                ik.solver.leftArm.target = openCVObject.transform.GetChild(0);
                ik.solver.rightArm.target = openCVObject.transform.GetChild(1);
                ik.gameObject.GetComponent<Animator>().runtimeAnimatorController = gestureAnimator;
                break;
            case TrackingType.SteamVR:
                steamVRObject.SetActive(true);
                ik.solver.leftArm.target = steamVRObject.transform.GetChild(0).GetChild(0);
                ik.solver.rightArm.target = steamVRObject.transform.GetChild(1).GetChild(0);
                FindObjectOfType<LeapXRServiceProvider>().enabled = false;
                ik.gameObject.GetComponent<Animator>().runtimeAnimatorController = null;
                break;
            case TrackingType.LeapMotion:
                leapMotionObject.SetActive(true);
                ik.solver.leftArm.target = leapMotionObject.transform.GetChild(0);
                ik.solver.rightArm.target = leapMotionObject.transform.GetChild(1);
                ik.gameObject.GetComponent<Animator>().runtimeAnimatorController = null;
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<LeapXRServiceProvider>().enabled = true;
                break;
        }
        //Settings.Instance.handTracker = type.ToString();
        //Settings.Instance.SaveSettings();
    }

    public void SetFinalIKStuff()
    {
        FindAvatar();
        //ik.solver.leftArm
        ik.solver.plantFeet = false;
        ik.solver.spine.pelvisTarget = noneObject.transform.GetChild(3);
        ik.solver.spine.pelvisPositionWeight = 1f;
        ik.solver.leftLeg.target = noneObject.transform.GetChild(4);
        ik.solver.leftLeg.positionWeight = 1f;
        ik.solver.rightLeg.target = noneObject.transform.GetChild(5);
        ik.solver.rightLeg.positionWeight = 1f;
    }

    public void UpdateTrackerList()
    {
        if (XRSettings.enabled)
        {
            // For testing steamvr stuff
            steamVRObject.GetComponent<SteamVRTracking>().SetAllTrackersToNone();
            List<TrackedObject> trackerList = new List<TrackedObject>();
            ETrackedPropertyError error = new ETrackedPropertyError();
            TrackedObject trackedObject = new TrackedObject();
            trackedObject.index = -1;
            trackedObject.name = "None";
            trackerList.Add(trackedObject);

            for (uint i = 0; i < 17; i++)
            {
                string trackedDeviceType = OpenVR.System.GetTrackedDeviceClass(i).ToString();
                if (trackedDeviceType == "HMD" || trackedDeviceType == "Controller" || trackedDeviceType == "GenericTracker")
                {
                    trackedObject = new TrackedObject();
                    trackedObject.index = (int)i;
                    System.Text.StringBuilder result = new System.Text.StringBuilder((int)64);
                    OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
                    trackedObject.name = result.ToString();
                    trackerList.Add(trackedObject);
                }
            }
            trackedObjects = trackerList;
        }
    }

    public List<TrackedObject> GetTrackerList()
    {
        return trackedObjects;
    }

    public int GetVRIndexFromIndex(int index)
    {
        return trackedObjects[index].index;
    }

    public void SetSteamVRTracker(string tracker, int index)
    {
        int vrIndex = GetVRIndexFromIndex(index);
        if (tracker == "head")
        {
            steamVRObject.transform.GetChild(2).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(vrIndex);
            Settings.Instance.SteamVRHeadTracker = vrIndex;
        }
        else if(tracker == "leftHand")
        {
            steamVRObject.transform.GetChild(0).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(vrIndex);
            Settings.Instance.SteamVRLeftHandTracker = vrIndex;
        }
        else if(tracker == "rightHand")
        {
            steamVRObject.transform.GetChild(1).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(vrIndex);
            Settings.Instance.SteamVRRightHandTracker = vrIndex;
        }
    }

    public void SetSteamVRTrackerVRIndex(string tracker, int vrIndex)
    {
        if (tracker == "head")
        {
            steamVRObject.transform.GetChild(2).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(vrIndex);
            Settings.Instance.SteamVRHeadTracker = vrIndex;
        }
        else if (tracker == "leftHand")
        {
            steamVRObject.transform.GetChild(0).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(vrIndex);
            Settings.Instance.SteamVRLeftHandTracker = vrIndex;
        }
        else if (tracker == "rightHand")
        {
            steamVRObject.transform.GetChild(1).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(vrIndex);
            Settings.Instance.SteamVRRightHandTracker = vrIndex;
        }
    }

    public void CalibrateTracking()
    {
        VRIKCalibrator.Settings settings = new VRIKCalibrator.Settings();
        RootMotion.Demos.VRIKCalibrationController calibrationController = steamVRObject.GetComponent<RootMotion.Demos.VRIKCalibrationController>();
        calibrationController.headTracker = ik.solver.spine.headTarget.gameObject.transform;
        //calibrationController.bodyTracker = ik.solver.spine.pelvisTarget.gameObject.transform;
        calibrationController.leftHandTracker = ik.solver.leftArm.target.gameObject.transform.parent;
        calibrationController.rightHandTracker = ik.solver.rightArm.target.gameObject.transform.parent;
        //calibrationController.leftFootTracker = ik.solver.leftLeg.target.gameObject.transform;
        //calibrationController.rightFootTracker = ik.solver.rightLeg.target.gameObject.transform;
        calibrationController.data = VRIKCalibrator.Calibrate(ik, settings, calibrationController.headTracker, calibrationController.bodyTracker, calibrationController.leftHandTracker, calibrationController.rightHandTracker, calibrationController.leftFootTracker, calibrationController.rightFootTracker);
        //SetFinalIKStuff();
        //VRIKCalibrator.Settings settings = new VRIKCalibrator.Settings();
        //VRIKCalibrator.Calibrate(ik, settings, ik.solver.spine.headTarget, ik.solver.spine.pelvisTarget, ik.solver.leftArm.target, ik.solver.rightArm.target, ik.solver.leftLeg.target, ik.solver.rightLeg.target);
    }

    public void EnableGizmoEditingOnObject(GameObject ikObject)
    {
        ikObject.GetComponent<MeshRenderer>().enabled = true;
        ikObject.layer = 8;
    }

    public void DisableGizmoEditingOnObject(GameObject ikObject)
    {
        ikObject.GetComponent<MeshRenderer>().enabled = false;
        ikObject.layer = 0;
    }

    public void EnableGizmoEditingOnTrackers()
    {
        Camera.main.GetComponent<TransformGizmo>().enabled = true;
        if(Settings.Instance.headTracker == "SteamVR")
        {
            GameObject head = ik.solver.spine.headTarget.gameObject;
            GameObject headReference = steamVRObject.transform.GetChild(5).gameObject;
            head.GetComponent<MeshRenderer>().enabled = true;
            headReference.GetComponent<MeshRenderer>().enabled = true;
            head.layer = 8;
            headReference.layer = 8;
        }

        if(Settings.Instance.handTracker == "SteamVR")
        {
            GameObject leftArm = ik.solver.leftArm.target.gameObject;
            GameObject rightArm = ik.solver.rightArm.target.gameObject;
            GameObject leftHandReference = steamVRObject.transform.GetChild(3).gameObject;
            GameObject rightHandReference = steamVRObject.transform.GetChild(4).gameObject;
            leftArm.GetComponent<MeshRenderer>().enabled = true;
            rightArm.GetComponent<MeshRenderer>().enabled = true;
            leftHandReference.GetComponent<MeshRenderer>().enabled = true;
            rightHandReference.GetComponent<MeshRenderer>().enabled = true;
            leftArm.layer = 8;
            rightArm.layer = 8;
            leftHandReference.layer = 8;
            rightHandReference.layer = 8;
        }
    }

    public void DisableGizmoEditingOnTrackers()
    {
        Camera.main.GetComponent<TransformGizmo>().enabled = false;
        GameObject head = steamVRObject.transform.GetChild(2).GetChild(0).gameObject;
        GameObject leftArm = steamVRObject.transform.GetChild(0).GetChild(0).gameObject;
        GameObject rightArm = steamVRObject.transform.GetChild(1).GetChild(0).gameObject;
        GameObject leftHandReference = steamVRObject.transform.GetChild(3).gameObject;
        GameObject rightHandReference = steamVRObject.transform.GetChild(4).gameObject;
        GameObject headReference = steamVRObject.transform.GetChild(5).gameObject;
        head.GetComponent<MeshRenderer>().enabled = false;
        leftArm.GetComponent<MeshRenderer>().enabled = false;
        rightArm.GetComponent<MeshRenderer>().enabled = false;
        headReference.GetComponent<MeshRenderer>().enabled = false;
        leftHandReference.GetComponent<MeshRenderer>().enabled = false;
        rightHandReference.GetComponent<MeshRenderer>().enabled = false;
        head.layer = 0;
        leftArm.layer = 0;
        rightArm.layer = 0;
        leftHandReference.layer = 0;
        rightHandReference.layer = 0;
        headReference.layer = 0;
    }

    public void ApplySteamVROffsets()
    {
        // Apply offsets to the final ik object
        steamVRObject.transform.GetChild(2).GetChild(0).localPosition = headPositionOffset;
        steamVRObject.transform.GetChild(2).GetChild(0).localEulerAngles = headRotationOffset;
        steamVRObject.transform.GetChild(0).GetChild(0).localPosition = leftHandPositionOffset;
        steamVRObject.transform.GetChild(0).GetChild(0).localEulerAngles = leftHandRotationOffset;
        steamVRObject.transform.GetChild(1).GetChild(0).localPosition = rightHandPositionOffset;
        steamVRObject.transform.GetChild(1).GetChild(0).localEulerAngles = rightHandRotationOffset;
    }

    public void SetOffsets()
    {
        headPositionOffset = steamVRObject.transform.GetChild(2).GetChild(0).localPosition;
        headRotationOffset = steamVRObject.transform.GetChild(2).GetChild(0).localEulerAngles;
        leftHandPositionOffset = steamVRObject.transform.GetChild(0).GetChild(0).localPosition;
        leftHandRotationOffset = steamVRObject.transform.GetChild(0).GetChild(0).localEulerAngles;
        rightHandPositionOffset = steamVRObject.transform.GetChild(1).GetChild(0).localPosition;
        rightHandRotationOffset = steamVRObject.transform.GetChild(1).GetChild(0).localEulerAngles;

        // Also save it to settings
        Settings.Instance.headPosition = headPositionOffset;
        Settings.Instance.headRotation = headRotationOffset;
        Settings.Instance.leftHandPosition = leftHandPositionOffset;
        Settings.Instance.leftHandRotation = leftHandRotationOffset;
        Settings.Instance.rightHandPosition = rightHandPositionOffset;
        Settings.Instance.rightHandRotation = rightHandRotationOffset;
    }

    public void ResetOffsets()
    {
        steamVRObject.transform.GetChild(2).GetChild(0).localPosition = Vector3.zero;
        steamVRObject.transform.GetChild(2).GetChild(0).localEulerAngles = Vector3.zero;
        steamVRObject.transform.GetChild(0).GetChild(0).localPosition = Vector3.zero;
        steamVRObject.transform.GetChild(0).GetChild(0).localEulerAngles = Vector3.zero;
        steamVRObject.transform.GetChild(1).GetChild(0).localPosition = Vector3.zero;
        steamVRObject.transform.GetChild(1).GetChild(0).localEulerAngles = Vector3.zero;
    }

    public void SetLegHeight(double height)
    {
        AdjustHeight(noneObject.transform.GetChild(4), height);
        AdjustHeight(noneObject.transform.GetChild(5), height);
    }

    public void SetHandHeight(double height)
    {
        AdjustHeight(noneObject.transform.GetChild(0), height);
        AdjustHeight(noneObject.transform.GetChild(1), height);
        AdjustHeight(openCVObject.transform.GetChild(0), height);
        AdjustHeight(openCVObject.transform.GetChild(1), height);
        AdjustHeight(steamVRObject.transform.GetChild(0), height);
        AdjustHeight(steamVRObject.transform.GetChild(1), height);
        AdjustHeight(steamVRObject.transform.GetChild(3), height);
        AdjustHeight(steamVRObject.transform.GetChild(4), height);
        AdjustHeight(leapMotionObject.transform.GetChild(0), height);
        AdjustHeight(leapMotionObject.transform.GetChild(1), height);
    }

    public void AdjustHeight(Transform heightObject, double yAxis)
    {
        heightObject.position = new Vector3(heightObject.localPosition.x, (float)yAxis, heightObject.localPosition.z);
    }

    public void AdjustSpread(Transform leftObject, Transform rightObject, double spread)
    {
        leftObject.position = new Vector3((float)spread / 2, leftObject.localPosition.y, leftObject.localPosition.z);
        rightObject.position = new Vector3(-(float)spread / 2, rightObject.localPosition.y, rightObject.localPosition.z);
    }

    public void SetArmSpread(double spread)
    {
        AdjustSpread(noneObject.transform.GetChild(0), noneObject.transform.GetChild(1), spread);
        AdjustSpread(openCVObject.transform.GetChild(0), openCVObject.transform.GetChild(1), spread);
        AdjustSpread(steamVRObject.transform.GetChild(0), steamVRObject.transform.GetChild(1), spread);
        AdjustSpread(steamVRObject.transform.GetChild(3), steamVRObject.transform.GetChild(4), spread);
        AdjustSpread(leapMotionObject.transform.GetChild(0), leapMotionObject.transform.GetChild(1), spread);
    }
}
