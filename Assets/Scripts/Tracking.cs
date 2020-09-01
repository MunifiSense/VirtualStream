using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                ik.solver.spine.headTarget = steamVRObject.transform.GetChild(2);
                break;
            case TrackingType.TobiiEyeTracker:
                tobiiObject.SetActive(true);
                ik.solver.spine.headTarget = tobiiObject.transform.GetChild(0);
                break;
        }
        Settings.Instance.headTracker = type.ToString();
        Settings.Instance.SaveSettings();
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
                break;
            case TrackingType.SteamVR:
                steamVRObject.SetActive(true);
                ik.solver.leftArm.target = steamVRObject.transform.GetChild(0);
                ik.solver.rightArm.target = steamVRObject.transform.GetChild(1);
                break;
            case TrackingType.LeapMotion:
                leapMotionObject.SetActive(true);
                ik.solver.leftArm.target = leapMotionObject.transform.GetChild(0);
                ik.solver.rightArm.target = leapMotionObject.transform.GetChild(1);
                break;
        }
        Settings.Instance.handTracker = type.ToString();
        Settings.Instance.SaveSettings();
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
        ik.solver.leftLeg.positionWeight = 1f;
    }

    public void UpdateTrackerList()
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
        if(tracker == "head")
        {
            steamVRObject.transform.GetChild(2).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(GetVRIndexFromIndex(index));
        }
        else if(tracker == "leftHand")
        {
            steamVRObject.transform.GetChild(0).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(GetVRIndexFromIndex(index));
        }
        else if(tracker == "rightHand")
        {
            steamVRObject.transform.GetChild(1).GetComponent<SteamVR_TrackedObject>().SetDeviceIndex(GetVRIndexFromIndex(index));
        }
    }

    public void CalibrateTracking()
    {
        VRIKCalibrator.Settings settings = new VRIKCalibrator.Settings();
        RootMotion.Demos.VRIKCalibrationController calibrationController = steamVRObject.GetComponent<RootMotion.Demos.VRIKCalibrationController>();
        calibrationController.headTracker = ik.solver.spine.headTarget.gameObject.transform;
        //calibrationController.bodyTracker = ik.solver.spine.pelvisTarget.gameObject.transform;
        calibrationController.leftHandTracker = ik.solver.leftArm.target.gameObject.transform;
        calibrationController.rightHandTracker = ik.solver.rightArm.target.gameObject.transform;
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
        GameObject head = ik.solver.spine.headTarget.gameObject;
        GameObject leftArm = ik.solver.leftArm.target.gameObject;
        GameObject rightArm = ik.solver.rightArm.target.gameObject;
        head.GetComponent<MeshRenderer>().enabled = true;
        leftArm.GetComponent<MeshRenderer>().enabled = true;
        rightArm.GetComponent<MeshRenderer>().enabled = true;
        head.layer = 8;
        leftArm.layer = 8;
        rightArm.layer = 8;
    }

    public void DisableGizmoEditingOnTrackers()
    {
        GameObject head = ik.solver.spine.headTarget.gameObject;
        GameObject leftArm = ik.solver.leftArm.target.gameObject;
        GameObject rightArm = ik.solver.rightArm.target.gameObject;
        head.GetComponent<MeshRenderer>().enabled = false;
        leftArm.GetComponent<MeshRenderer>().enabled = false;
        rightArm.GetComponent<MeshRenderer>().enabled = false;
        head.layer = 0;
        leftArm.layer = 0;
        rightArm.layer = 0;
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
}
