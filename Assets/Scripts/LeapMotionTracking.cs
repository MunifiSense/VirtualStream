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
        leftHand.transform.position = leftLeapHand.GetWristPosition();
        leftHand.transform.rotation = leftLeapHand.GetPalmRotation() * Quaternion.Euler(new Vector3(-90.0f, 0, 0));
        rightHand.transform.position = rightLeapHand.GetWristPosition();
        rightHand.transform.rotation = rightLeapHand.GetPalmRotation() * Quaternion.Euler(new Vector3(-90.0f, 0, 0));
    }

    void SetPositionOffset(Vector3 offset)
    {
        positionOffset = offset;
        transform.position  = offset;
    }

    void SetRotationOffset(Vector3 offset)
    {
        rotationOffset = offset;
        transform.eulerAngles = offset;
    }
}
