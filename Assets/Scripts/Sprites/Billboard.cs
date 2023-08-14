using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool lockY = true;
    private Camera camera;

    private Transform turnPos;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //transform.LookAt(camera.transform);

        transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position);

        if (lockY)
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }
}
