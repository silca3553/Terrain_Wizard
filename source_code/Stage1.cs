using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage1 : MonoBehaviour
{
    public Transform[] grids;
    public Camera topCamera;
    public Camera playerCamera;
    public Camera[] gridCameras;
    public Transform player;

    //public float transitionDuration = 1.0f;
    private Transform[] alreadySelectedGrids;
    private Transform playerGrid;
    private Camera currentCamera;
    private bool isMovingGrid = false;
    public bool isGridActive = false;

    void Start()
    {
        // init topCamera
        topCamera.transform.position = new Vector3(0, 200, 0);
        topCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        topCamera.orthographic = true;

        // init playerCamera
        playerCamera.transform.localPosition = new Vector3(0, 2, -2);
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





    void Update()
    {
        playerCamera.transform.position = player.position + new Vector3(0, 2, -2);
        HandleCamera();
    }

    // camera
    void HandleCamera()
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
            if (Input.GetMouseButtonDown(0))
            {
                playerGrid = GetPlayerGrid();
                Debug.Log(playerGrid.name); // #####
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = topCamera.ScreenPointToRay(mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log(hit.collider.name);
                    Transform selectedGrid = GetClosestGrid(hit.point);
                    if (IsSelectableGrid(selectedGrid, playerGrid))
                    {
                        Debug.Log("Selectable!"); // #####
                        MoveGridCameras(topCamera, selectedGrid);
                    }
                }
            }
            HandleGridCameras();
        }
    }
    void ActivateCamera(Camera camera)
    {
        topCamera.enabled = false;
        playerCamera.enabled = false;
        camera.enabled = true;
    }

    void DeactivateGridMode()
    {
        gridCameras[0].enabled = false;
        gridCameras[1].enabled = false;
        gridCameras[2].enabled = false;
        gridCameras[3].enabled = false;
    }
    void ActivateGridCamera(Camera gridCamera)
    {
        gridCameras[0].enabled = false;
        gridCameras[1].enabled = false;
        gridCameras[2].enabled = false;
        gridCameras[3].enabled = false;
        gridCamera.enabled = true;
    }

    // grid
    Transform GetPlayerGrid()
    {
        Vector3 playerPosition2d = new Vector3(player.position.x, 0, player.position.z);
        Transform playerGrid = null;
        float distance = Mathf.Infinity;
        float curDistance = Mathf.Infinity;
        foreach (Transform grid in grids)
        {
            Vector3 gridPosition2d = new Vector3(grid.position.x, 0, grid.position.z);
            curDistance = Vector3.Distance(playerPosition2d, gridPosition2d);
            if (curDistance < (5 * Mathf.Sqrt(2)))
            {
                if (playerGrid == null)
                {
                    distance = curDistance;
                    playerGrid = grid;
                }
                if (playerGrid != null)
                {
                    if (curDistance < distance)
                    {
                        distance = curDistance;
                        playerGrid = grid;
                    }
                }
            }
        }
        return playerGrid;
    }

    bool IsSelectableGrid(Transform grid, Transform playerGrid)
    {
        if (Vector3.Distance(grid.position, playerGrid.position) <= 15) return true;
        else return false;
    }

    Transform GetClosestGrid(Vector3 position)
    {
        Transform closestGrid = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform grid in grids)
        {
            float distance = Vector3.Distance(position, grid.position);
            if (distance < closestDistance)
            {
                closestGrid = grid;
                closestDistance = distance;
            }
        }
        return closestGrid;
    }

    /*
    void StartTransition(Camera camera)
    {
        initialCameraPosition = new Vector3[gridCameras.Length];
        initialCameraRotation = new Quaternion[gridCameras.Length];

        for (int i = 0; i < gridCameras.Length; i++)
        {
            initialCameraPosition[i] = gridCameras[i].transform.position;
            initialCameraRotation[i] = gridCameras[i].transform.rotation;
        }

        transitioning = true;
        transitionStartTime = Time.time;
    }
    void FinishTransition()
    {
        transitioning = false;
        targetGrid = null;
    }
    */

    void MoveGridCameras(Camera topCamera, Transform grid)
    {
        Vector3 gridPosition = new Vector3(grid.position.x, player.position.y, grid.position.z);
        gridCameras[0].transform.position = gridPosition + new Vector3(0, 10, -10); // front
        gridCameras[1].transform.position = gridPosition + new Vector3(-10, 10, 0); // left
        gridCameras[2].transform.position = gridPosition + new Vector3(0, 10, 10); // back
        gridCameras[3].transform.position = gridPosition + new Vector3(10, 10, 0); // right

        for (int i = 0; i < 4; i++)
        {
            gridCameras[i].transform.LookAt(gridPosition);
        }
        
        isGridActive = true;
        StartCoroutine(SmoothTopToGrid(topCamera, gridCameras[0], 1.0f));
    }

    IEnumerator SmoothTopToGrid(Camera topCamera, Camera gridCamera, float duration)
    {
        isMovingGrid = true;
        float elapsedTime = 0;
        Vector3 startingPos = topCamera.transform.position;
        Vector3 targetPos = gridCamera.transform.position;
        Quaternion startingRot = topCamera.transform.rotation;
        Quaternion targetRot = gridCamera.transform.rotation;
        while (elapsedTime <= duration)
        {
            topCamera.transform.position = Vector3.Lerp(startingPos, targetPos, (elapsedTime / duration));
            topCamera.transform.rotation = Quaternion.Lerp(startingRot, targetRot, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        topCamera.transform.position = targetPos;
        topCamera.transform.rotation = targetRot;
        gridCamera.transform.position = targetPos;
        gridCamera.transform.rotation = targetRot;
        ActivateGridCamera(gridCamera);
        topCamera.enabled = false;
        topCamera.transform.position = startingPos;
        topCamera.transform.rotation = startingRot;
        isMovingGrid = false;
    }

    IEnumerator SmoothChangeGridCamera(Camera fromCamera, Camera toCamera, float duration)
    {
        isMovingGrid = true;
        float elapsedTime = 0;
        Vector3 startingPos = fromCamera.transform.position;
        Vector3 targetPos = toCamera.transform.position;
        Quaternion startingRot = fromCamera.transform.rotation;
        Quaternion targetRot = toCamera.transform.rotation;
        Debug.Log("NowCamera: " + fromCamera + " / TargetCamera: " + toCamera);
        while (elapsedTime <= duration)
        {
            fromCamera.transform.position = Vector3.Lerp(startingPos, targetPos, (elapsedTime / duration));
            fromCamera.transform.rotation = Quaternion.Lerp(startingRot, targetRot, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fromCamera.transform.position = targetPos;
        fromCamera.transform.rotation = targetRot;
        toCamera.transform.position = targetPos;
        toCamera.transform.rotation = targetRot;
        ActivateGridCamera(toCamera);
        fromCamera.transform.position = startingPos;
        fromCamera.transform.rotation = startingRot;
        isMovingGrid = false;
    }

    IEnumerator SmoothMoveGridCamera(Camera camera, float y, float duration)
    {
        isMovingGrid = true;
        float elapsedTime = 0;
        Vector3 startingPos = camera.transform.position;
        Vector3 targetPos = camera.transform.position + new Vector3 (0, y, 0);
        while (elapsedTime < duration)
        {
            camera.transform.position = Vector3.Lerp(startingPos, targetPos, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isMovingGrid = false;
    }

    void HandleGridCameras()
    {
        if (isGridActive)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) // going left
            {
                if (gridCameras[0].enabled && !isMovingGrid)
                    StartCoroutine(SmoothChangeGridCamera(gridCameras[0], gridCameras[1], 1.0f));
                else if (gridCameras[1].enabled && !isMovingGrid) 
                    StartCoroutine(SmoothChangeGridCamera(gridCameras[1], gridCameras[2], 1.0f));
                else if (gridCameras[2].enabled && !isMovingGrid)
                    StartCoroutine(SmoothChangeGridCamera(gridCameras[2], gridCameras[3], 1.0f));
                else if (gridCameras[3].enabled && !isMovingGrid)
                    StartCoroutine(SmoothChangeGridCamera(gridCameras[3], gridCameras[0], 1.0f));
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) // going right
            {
                if (gridCameras[0].enabled && !isMovingGrid) 
                    StartCoroutine(SmoothChangeGridCamera(gridCameras[0], gridCameras[3], 1.0f));
                else if (gridCameras[1].enabled && !isMovingGrid)
                    StartCoroutine(SmoothChangeGridCamera(gridCameras[1], gridCameras[0], 1.0f));
                else if (gridCameras[2].enabled && !isMovingGrid)
                    StartCoroutine(SmoothChangeGridCamera(gridCameras[2], gridCameras[1], 1.0f));
                else if (gridCameras[3].enabled && !isMovingGrid)
                    StartCoroutine(SmoothChangeGridCamera(gridCameras[3], gridCameras[2], 1.0f));
                
            }

            else if (Input.GetKeyDown(KeyCode.UpArrow)) // going up
            {
                if (!isMovingGrid)
                    for (int i = 0; i < 4; i++)
                        StartCoroutine(SmoothMoveGridCamera(gridCameras[i], 2, 0.5f));
                //gridCameras[i].transform.position += new Vector3(0, 2, 0);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow)) // going down
            {
                if (!isMovingGrid)
                    for (int i = 0; i < 4; i++)
                        StartCoroutine(SmoothMoveGridCamera(gridCameras[i], -2, 0.5f));
                //gridCameras[i].transform.position += new Vector3(0, -2, 0);
            }
        }
    }
}