using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

public class TobiiTracking : MonoBehaviour
{
    // How many milliseconds between each check?
    public int milliseconds = 100;
    // Distance from monitor in meters
    public float distanceFromScreen = 0.3f;
    // Adjust view offset (TODO: Add auto calibration when staring at center of monitor)
    public float viewOffsetX = -0.45f;
    public float viewOffsetY = -0.5f;
    private float timeSinceLastCheck;
    private GameObject player;
    private Transform leftEye;
    private Transform rightEye;
    private GameObject head;
    private Vector3 lookAtPos;

    // Start is called before the first frame update
    void Start()
    {
        head = transform.GetChild(0).gameObject;
        player = GameObject.FindGameObjectWithTag("Player");
        leftEye = player.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftEye);
        rightEye = player.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightEye);
        timeSinceLastCheck = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastCheck += Time.deltaTime;
        if(Mathf.FloorToInt(timeSinceLastCheck * 1000) >= milliseconds)
        {
            timeSinceLastCheck = 0;
            // Keep track of user head
            HeadPose headPose = TobiiAPI.GetHeadPose();
            if (headPose.IsRecent())
            {
                head.transform.eulerAngles = new Vector3(headPose.Rotation.eulerAngles.x, headPose.Rotation.eulerAngles.y + 180, headPose.Rotation.eulerAngles.z);
            }

            if (Settings.Instance.eyeTracking)
            {
                // Keep track of user eyes
                GazePoint gazePoint = TobiiAPI.GetGazePoint();
                if (gazePoint.IsRecent())
                {
                    lookAtPos = new Vector3(
                        (leftEye.position.x + rightEye.position.x) / 2 + gazePoint.Viewport.x + viewOffsetX,
                        leftEye.position.y + gazePoint.Viewport.y + viewOffsetY,
                        leftEye.position.z - 1.0f);
                    leftEye.LookAt(lookAtPos);
                    rightEye.LookAt(lookAtPos);
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
