using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public TrackingType headTracking;
    public TrackingType handTracking;
    public VRIK ik;
    public GameObject noneObject;
    public GameObject steamVRObject;
    public GameObject openCVObject;
    public GameObject tobiiObject;
    public GameObject leapMotionObject;
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
                ik.solver.spine.headTarget = steamVRObject.transform.GetChild(2).transform.GetChild(0);
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
                ik.solver.leftArm.target = steamVRObject.transform.GetChild(0).transform.GetChild(0);
                ik.solver.rightArm.target = steamVRObject.transform.GetChild(1).transform.GetChild(0);
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

    public void SetOtherStuff()
    {
        FindAvatar();
        ik.solver.plantFeet = false;
        ik.solver.spine.pelvisTarget = noneObject.transform.GetChild(3);
        ik.solver.spine.pelvisPositionWeight = 1f;
        ik.solver.leftLeg.target = noneObject.transform.GetChild(4);
        ik.solver.leftLeg.positionWeight = 1f;
        ik.solver.rightLeg.target = noneObject.transform.GetChild(5);
        ik.solver.leftLeg.positionWeight = 1f;
    }
}
