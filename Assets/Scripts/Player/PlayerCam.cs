using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform cameraPosition;
    public Transform orientation;
    public Camera cam;

    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensX * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.fixedDeltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.localRotation = Quaternion.Euler(0, yRotation, 0);

        transform.position = cameraPosition.transform.position;
    }

    public void ChangeFOV(float fov)
    {
        StopAllCoroutines();
        StartCoroutine(SmoothlyChangeFOV(fov));
    }

    private IEnumerator SmoothlyChangeFOV(float fov)
    {
        float time = 0;
        float startFOV = cam.fieldOfView;
        float difference = Mathf.Abs(startFOV - fov);

        while (time < difference)
        {
            cam.fieldOfView = Mathf.Lerp(startFOV, fov, time / difference);
            time += Time.deltaTime * 15f;
            yield return null;
        }

        cam.fieldOfView = fov;
    }
}
