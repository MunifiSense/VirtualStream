using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSCameraController : MonoBehaviour
{
    public float scrollSpeed = 1f;
    public float rotateSpeed = 3f;
    public float panSpeed = 6f;
    private float x;
    private float y;
    private float x2;
    private float y2;

    // Update is called once per frame
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        //transform.Translate(0, 0, scroll * scrollSpeed, Space.Self);

        // Rotate around camera axis with Right Mouse
        if (Input.GetMouseButton(1))
        {
            //transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * rotateSpeed, -Input.GetAxis("Mouse X") * rotateSpeed, 0));
            //x = transform.rotation.eulerAngles.x;
            //y = transform.rotation.eulerAngles.y;
            //transform.rotation = Quaternion.Euler(x, y, 0);
            x += rotateSpeed * Input.GetAxis("Mouse X");
            y -= rotateSpeed * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(y, x, 0f);
        }

        // Pan around with Middle Mouse
        if (Input.GetMouseButton(2))
        {
            transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * panSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * panSpeed, 0, Space.Self);

        }

        transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * scrollSpeed, Space.Self);
    }
}
