using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Prop : MonoBehaviour
{
    public string name;
    public SerializableVector3 scale;
    public SerializableVector3 positionOffset;
    public SerializableVector3 rotationOffset;
    public HumanBodyBones attachedBone;
    public bool attachedToSomething;
    private Animator player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateProp()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        // If the avatar is found, move to the attached bone.
        if (player != null && attachedToSomething)
        {
            transform.parent = player.GetComponent<Animator>().GetBoneTransform(attachedBone);
            transform.localPosition = positionOffset;
            transform.localEulerAngles = rotationOffset;
            transform.localScale = scale;
        }
        else
        {
            transform.position = positionOffset;
            transform.eulerAngles = rotationOffset;
            transform.localScale = scale;
        }
    }
}
