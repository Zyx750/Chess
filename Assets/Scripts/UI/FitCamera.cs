using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitCamera : MonoBehaviour
{
    private Camera cam;
    private float initialOrthographicSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        initialOrthographicSize = cam.orthographicSize;
        UpdateCameraSize();
    }

    void Update()
    {
        UpdateCameraSize();
    }

    private void UpdateCameraSize()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        
        if (currentAspect < 1)
        {
            // Portrait mode or narrower aspect ratio
            float scaleFactor = currentAspect;
            cam.orthographicSize = initialOrthographicSize / scaleFactor;
        }
        else
        {
            // Landscape mode or wider aspect ratio
            cam.orthographicSize = initialOrthographicSize;
        }
    }
}
