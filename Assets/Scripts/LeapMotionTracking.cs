using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class LeapMotionTracking : MonoBehaviour
{
    public RiggedHand leftLeapHand;
    public RiggedHand rightLeapHand;
    public GameObject leftHand;
    public GameObject rightHand;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (leftLeapHand.IsTracked)
        {
            leftHand.transform.localPosition = new Vector3(-leftLeapHand.GetWristPosition().x,
            leftLeapHand.GetWristPosition().y + (float)Settings.Instance.handHeight*3,
            leftLeapHand.GetWristPosition().z);

            Quaternion leftHandRotation = leftLeapHand.GetPalmRotation() * Quaternion.Euler(new Vector3(-90.0f, 0, 0));
            leftHand.transform.localRotation = leftHandRotation;
        }
        else
        {
            leftHand.transform.position = Tracking.Instance.noneObject.transform.GetChild(0).position;
            leftHand.transform.rotation = Tracking.Instance.noneObject.transform.GetChild(0).rotation;
        }

        if (rightLeapHand.IsTracked)
        {
            rightHand.transform.localPosition = new Vector3(-rightLeapHand.GetWristPosition().x,
            rightLeapHand.GetWristPosition().y + (float)Settings.Instance.handHeight*3,
            rightLeapHand.GetWristPosition().z);

            Quaternion rightHandRotation = rightLeapHand.GetPalmRotation() * Quaternion.Euler(new Vector3(-90.0f, 0, 0));
            rightHand.transform.localRotation = rightHandRotation;
        }
        else
        {
            rightHand.transform.position = Tracking.Instance.noneObject.transform.GetChild(1).position;
            rightHand.transform.rotation = Tracking.Instance.noneObject.transform.GetChild(1).rotation;
        }
    }
}
