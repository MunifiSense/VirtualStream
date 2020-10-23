using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;
using System;

public class TobiiTracking : MonoBehaviour
{
    // How many milliseconds between each check?
    public int milliseconds = 50;
    // Distance from monitor in meters
    public float distanceFromScreen = 0.3f;
    // Adjust view offset (TODO: Add auto calibration when staring at center of monitor)
    public float viewOffsetX = -0.45f;
    public float viewOffsetY = -0.5f;
    public GameObject player;
    private float timeSinceLastCheck;
    private Transform leftEye;
    private Transform rightEye;
    private GameObject head;
    private Vector3 lookAtPos;

    // Start is called before the first frame update
    void Start()
    {
        head = transform.GetChild(0).gameObject;
        player = GameObject.FindGameObjectWithTag("Player");
        //leftEye = player.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftEye);
        //rightEye = player.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightEye);
        timeSinceLastCheck = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        timeSinceLastCheck += Time.deltaTime;
        if(Mathf.FloorToInt(timeSinceLastCheck * 1000) >= milliseconds)
        {
            timeSinceLastCheck = 0;
            // Keep track of user head
            HeadPose headPose = TobiiAPI.GetHeadPose();
            if (headPose.IsRecent())
            {
                head.transform.localEulerAngles = new Vector3(headPose.Rotation.eulerAngles.x, headPose.Rotation.eulerAngles.y + 180, -headPose.Rotation.eulerAngles.z);
            }

            if (Settings.Instance.eyeTracking)
            {
                // Keep track of user eyes
                GazePoint gazePoint = TobiiAPI.GetGazePoint();
                if (gazePoint.IsRecent())
                {
                    leftEye = player.GetComponent<VS_AvatarDescriptor>().LeftEye;
                    rightEye = player.GetComponent<VS_AvatarDescriptor>().RightEye;
                    // Position between eyes
                    lookAtPos = new Vector3(
                        (leftEye.position.x + rightEye.position.x) / 2 + gazePoint.Viewport.x + viewOffsetX,
                        leftEye.position.y + gazePoint.Viewport.y + viewOffsetY,
                        leftEye.position.z);
                    // In front of eyes
                    lookAtPos += player.transform.forward;
                    leftEye.LookAt(lookAtPos);
                    rightEye.LookAt(lookAtPos);
                }
            }

            if (Settings.Instance.eyeBlinking)
            {
                // Keep track of user eyes
                GazePoint gazePoint = TobiiAPI.GetGazePoint();
                if (gazePoint.IsRecent(0.01f))
                {
                    VS_AvatarDescriptor avatarDescriptor = player.GetComponent<VS_AvatarDescriptor>();
                    avatarDescriptor.FaceMesh.SetBlendShapeWeight(avatarDescriptor.FaceMesh.sharedMesh.GetBlendShapeIndex(avatarDescriptor.Blink), 0.0f);
                }
                else if(gazePoint.IsRecent())
                {
                    VS_AvatarDescriptor avatarDescriptor = player.GetComponent<VS_AvatarDescriptor>();
                    avatarDescriptor.FaceMesh.SetBlendShapeWeight(avatarDescriptor.FaceMesh.sharedMesh.GetBlendShapeIndex(avatarDescriptor.Blink), 100.0f);
                }
            }
        }
    }

    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lookAtPos, 0.1f);
    }*/
}
