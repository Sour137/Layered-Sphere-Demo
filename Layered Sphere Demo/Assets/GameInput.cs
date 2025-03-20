using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput
{
    Camera gameCamera;
    public NodeData nodeData;
    public bool flag_nodeData;

    public bool flag_enter;
    public bool flag_regionShading;
    public bool flag_heatmapShading;
    public bool flag_overlapShading;
    //public bool flag_resourceShading;

    public bool flag_zoomOut;
    public bool flag_zoomIn;

    public bool flag_escape;

    public bool flag_rightClick;
    public bool flag_rightClickUp;
    public Vector3 clickOrigin;
    public Vector3 clickDirection;

    public bool flag_up;
    public bool flag_down;
    public bool flag_left;
    public bool flag_right;

    public bool flag_resourceLookup;
    public bool flag_saveToFile;
    public bool flag_loadFile;

    public bool flag_debugControl;
    public bool flag_debugResources;

    public GameInput(Camera in_camera)
    {
        gameCamera = in_camera;
        Reset();
    }

    public bool Check()
    {
        bool toRet = false;
        Reset();

        if (Input.GetKeyUp(KeyCode.KeypadEnter))
            flag_enter = true;
        if (Input.GetKeyUp(KeyCode.F1))
            flag_regionShading = true;
        if (Input.GetKeyUp(KeyCode.F2))
            flag_heatmapShading = true;
        if (Input.GetKeyUp(KeyCode.F3))
            flag_overlapShading = true;
        if (Input.GetKeyUp(KeyCode.PageUp))
            flag_zoomOut = true;
        if (Input.GetKeyUp(KeyCode.PageDown))
            flag_zoomIn = true;
        if (Input.GetKeyUp(KeyCode.Escape))
            flag_escape = true;

        if (Input.GetKey(KeyCode.W))
            flag_up = true;
        if (Input.GetKey(KeyCode.S))
            flag_down = true;
        if (Input.GetKey(KeyCode.A))
            flag_left = true;
        if (Input.GetKey(KeyCode.D))
            flag_right = true;
        //if (Input.GetKeyUp(KeyCode.R))
        //    flag_resourceLookup = true;

        // control mutates
        if ((Input.GetKey(KeyCode.LeftControl)) || (Input.GetKey(KeyCode.RightControl)))
        {

            if (Input.GetKeyUp(KeyCode.R))
            {
                if (flag_debugControl)
                    flag_debugResources = true;
                else
                    flag_resourceLookup = true;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                if (flag_debugControl)
                    flag_debugControl = false;
                else
                    flag_debugControl = true;

                Debug.Log("Debug Control : " + flag_debugControl);
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                flag_saveToFile = true;
            }
            if (Input.GetKeyUp(KeyCode.L))
            {
                flag_loadFile = true;
            }
        }

        // Mouse Inputs
        if (Input.GetMouseButtonUp(0))
        {
            // not versatile enough yet
            flag_nodeData = true;
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
                if (hit.collider != null)
                    nodeData = hit.transform.GetComponent<NodeData>();

        }
        if (Input.GetMouseButtonDown(1))
        {
            flag_rightClick = true;
            clickOrigin = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            if (flag_rightClick)
                clickDirection = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            flag_rightClick = false;
            flag_rightClickUp = true;
        }
        if ((flag_enter) || (flag_regionShading) || (flag_nodeData) || (flag_zoomOut) || (flag_zoomIn) || (flag_escape) || (flag_rightClick) || (flag_up) || (flag_down) || (flag_left) || (flag_right) || flag_resourceLookup || flag_heatmapShading || flag_debugResources || flag_overlapShading || flag_saveToFile || flag_loadFile)
            toRet = true;

        return toRet;
    }

    private void Reset()
    {
        flag_nodeData = false;
        flag_enter = false;
        flag_regionShading = false;
        flag_zoomOut = false;
        flag_zoomIn = false;
        flag_escape = false;
        flag_up = false;
        flag_down = false;
        flag_left = false;
        flag_right = false;
        flag_rightClickUp = false;
        flag_resourceLookup = false;
        flag_heatmapShading = false;
        flag_debugResources = false;
        flag_overlapShading = false;
        flag_saveToFile = false;
        flag_loadFile = false;
        // excluded from reset
        // flag_debugControl is toggle
    }
}
