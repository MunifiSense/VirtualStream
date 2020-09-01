using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using RootMotion.FinalIK;

public class SteamVRTracking : MonoBehaviour
{
    public SteamVR_TrackedObject head;
    public SteamVR_TrackedObject leftHand;
    public SteamVR_TrackedObject rightHand;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAllTrackersToNone()
    {
        head.SetDeviceIndex(-1);
        leftHand.SetDeviceIndex(-1);
        rightHand.SetDeviceIndex(-1);
    }
}
