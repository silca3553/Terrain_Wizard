using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowGrid : MonoBehaviour
{
    public Material highlightMaterial;
    public Transform player;
    public Camera topCamera;

    private Transform curGrid;
    private Material originalMaterial;
    private bool isMouseOver = false;

    private void Start()
    {
        curGrid = GetComponent<Transform>().transform;
        originalMaterial = GetComponent<Renderer>().material;
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        if (IsSelectableGrid() && topCamera.enabled)
            ApplyEffect();
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        RemoveEffect();
    }

    private void ApplyEffect()
    {
        if (highlightMaterial != null)
        {
            GetComponent<Renderer>().material = highlightMaterial;
        }
    }

    private void RemoveEffect()
    {
        if (!isMouseOver)
        {
            GetComponent<Renderer>().material = originalMaterial;
        }
    }
    bool IsSelectableGrid()
    {
        Mesh curGridMesh = curGrid.GetComponent<MeshFilter>().sharedMesh;
        Vector3 curGridSize = curGridMesh.bounds.size;
        float curGridWidth = curGridSize.x;

        Vector3 playerPosition2d = new Vector3(player.position.x, 0, player.position.z);
        Vector3 curGridPosition2d = new Vector3(curGrid.position.x, 0, curGrid.position.z);
        bool closeX = (Mathf.Abs(player.position.x - curGrid.position.x) < 1.5 * curGridWidth);
        bool closeZ = (Mathf.Abs(player.position.z - curGrid.position.z) < 1.5 * curGridWidth);
        if (closeX && closeZ) return true;
        else return false;
    }
}
