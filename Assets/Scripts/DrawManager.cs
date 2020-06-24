using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawManager : MonoBehaviour
{

    public GameObject drawPrefab;
    public GameObject colourPickerPanel;
    public float brushStartWidth;
    
    private GameObject theTrail;
    private Plane planeObj;
    private Vector3 startPos;
    private TrailRenderer trailRenderer;
    private FlexibleColorPicker fcp;
    private Color currentBrushColour;


    // Awake is called before Start
    void Awake()
    {
        // create a new plane in front of the main camera
        planeObj = new Plane(Camera.main.transform.forward * -1, this.transform.position);

        trailRenderer = drawPrefab.GetComponent<TrailRenderer>();
        fcp = colourPickerPanel.GetComponent<FlexibleColorPicker>();

        // set starting width and colour
        trailRenderer.startWidth = brushStartWidth;
        trailRenderer.startColor = fcp.startingColor;
        trailRenderer.endColor = fcp.startingColor;
        currentBrushColour = fcp.startingColor;

        colourPickerPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            // if someone touches the screen
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began
                || Input.GetMouseButtonDown(0))
            {
                // if not touching UI
                if (!IsPointerOverUI())
                {
                    Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    float dist;
                    if (planeObj.Raycast(mouseRay, out dist))
                    {
                        startPos = mouseRay.GetPoint(dist);
                    }
                    theTrail = Instantiate(drawPrefab, startPos, Quaternion.identity);
                }
                else
                {
                    Debug.Log("Touch UI");
                }

            }
            // there is a prolonged touch
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved
                || Input.GetMouseButton(0))
            {
                // if not touching UI
                if (!IsPointerOverUI())
                {
                    Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    float dist;
                    if (planeObj.Raycast(mouseRay, out dist))
                    {
                        theTrail.transform.position = mouseRay.GetPoint(dist);
                        startPos = mouseRay.GetPoint(dist);
                    }
                }
                else
                {
                    theTrail = null;
                }
            }
            else
            {
                theTrail = null;
            }
        } 
        catch (NullReferenceException)
        {
            Debug.Log("null trail");
        }
        
    }

    public void SetBrushSize(float brushSize)
    {
        if (brushSize > 0)
        {
            trailRenderer.startWidth = brushSize;
        }
    }

    public void UpdateBrushColour()
    {
        Color toUpdate = fcp.color;
        ChangeBrushColour(toUpdate);
    }

    public void SetBrushColour(Color colour)
    {
        colourPickerPanel.SetActive(true);
        fcp.color = colour;
        ChangeBrushColour(colour);
        colourPickerPanel.SetActive(false);
    }

    public void ChangeBrushColour(Color colour)
    {
        trailRenderer.startColor = colour;
        trailRenderer.endColor = colour;
        currentBrushColour = colour;
    }

    public Color GetBrushColour()
    {
        return currentBrushColour;
    }

    private static bool IsPointerOverUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    // clears all strokes in the scene
    public void Clear()
    {
        GameObject[] brushLines;
        brushLines = GameObject.FindGameObjectsWithTag("Brush Line");

        foreach (GameObject line in brushLines)
        {
            Destroy(line);
        }
    }

} 