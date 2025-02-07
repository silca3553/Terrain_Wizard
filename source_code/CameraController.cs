using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera topCamera;
    public Camera playerCamera;
    public Camera[] gridCameras;
    public Transform player;

    private Camera currentCamera;
    public bool isGridActive = false;

    void Start()
    {
        // init topCamera
        topCamera.transform.position = new Vector3(0, 200, 0);
        topCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        topCamera.orthographic = true;

        // init playerCamera
        playerCamera.transform.localPosition = new Vector3(0, 5, -5);
        playerCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

        // init gridCamera
        for (int i = 0; i < 4; i++)
        {
            gridCameras[i].transform.position = topCamera.transform.position;
            gridCameras[i].transform.rotation = topCamera.transform.rotation;
            Vector3 offset = Vector3.zero;
            switch (i)
            {
                case 0: // Front
                    offset = new Vector3(0, 0, 5); break;
                case 1: // Left
                    offset = new Vector3(-5, 0, 0); break;
                case 2: // Back
                    offset = new Vector3(0, 0, -5); break;
                case 3: // Right
                    offset = new Vector3(5, 0, 0); break;
            }
            gridCameras[i].transform.position += offset;
        }

        // activate initial camera (playerCamera)
        currentCamera = playerCamera;
        ActivateCamera(currentCamera);
    }

    // Update is called once per frame
    void Update()
    {
        playerCamera.transform.position = player.position + new Vector3(0, 10, -10);
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (currentCamera == topCamera)
                currentCamera = playerCamera;
            else currentCamera = topCamera;
            ActivateCamera(currentCamera);
        }

        if (currentCamera == topCamera)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                isGridActive = !isGridActive;
                if (isGridActive) ActivateGridMode();
                else DeactivateGridMode();
            }

            if (isGridActive)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow)) // going left
                {
                    if (gridCameras[0].enabled) ActivateGridCamera(gridCameras[1]);
                    else if (gridCameras[1].enabled) ActivateGridCamera(gridCameras[2]);
                    else if (gridCameras[2].enabled) ActivateGridCamera(gridCameras[3]);
                    else if (gridCameras[3].enabled) ActivateGridCamera(gridCameras[0]);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow)) // going right
                {
                    if (gridCameras[0].enabled) ActivateGridCamera(gridCameras[3]);
                    else if (gridCameras[1].enabled) ActivateGridCamera(gridCameras[0]);
                    else if (gridCameras[2].enabled) ActivateGridCamera(gridCameras[1]);
                    else if (gridCameras[3].enabled) ActivateGridCamera(gridCameras[2]);
                }

                else if (Input.GetKeyDown(KeyCode.UpArrow)) // going up
                {
                    for (int i = 0; i < 4; i++)
                        if (gridCameras[i].enabled) gridCameras[i].transform.position += new Vector3(0, 2, 0);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow)) // going down
                {
                    for (int i = 0; i < 4; i++)
                        if (gridCameras[i].enabled) gridCameras[i].transform.position += new Vector3(0, -2, 0);
                }
                
            }
        }
    }

    void ActivateCamera(Camera camera)
    {
        topCamera.enabled = false;
        playerCamera.enabled = false;
        camera.enabled = true;
    }

    void ActivateGridMode() // activate grid cameras
    {
        gridCameras[0].enabled = true;
        gridCameras[1].enabled = false;
        gridCameras[2].enabled = false;
        gridCameras[3].enabled = false;
    }

    void DeactivateGridMode()
    {
        gridCameras[0].enabled = false;
        gridCameras[1].enabled = false;
        gridCameras[2].enabled = false;
        gridCameras[3].enabled = false;
        ActivateCamera(playerCamera);
    }

    void ActivateGridCamera(Camera gridCamera)
    {
        gridCameras[0].enabled = false;
        gridCameras[1].enabled = false;
        gridCameras[2].enabled = false;
        gridCameras[3].enabled = false;
        gridCamera.enabled = true;
    }
}
