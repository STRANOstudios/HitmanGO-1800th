using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class OverlayCamera : MonoBehaviour
{
    public bool debug = false;

    void Start()
    {
        // Trova la Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            if (debug) Debug.LogError("Main Camera not found!");
            return;
        }

        // Assicura che la Main Camera abbia il componente UniversalAdditionalCameraData
        var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        if (mainCameraData == null)
        {
            mainCameraData = mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }

        // Configura la Main Camera come Base Camera
        mainCameraData.renderType = CameraRenderType.Base;

        // Trova l'Overlay Camera (che deve essere presente nella scena con un tag o nome specifico)
        Camera overlayCamera = GameObject.FindWithTag("Loading")?.GetComponent<Camera>();
        if (overlayCamera == null)
        {
            if (debug) Debug.LogError("Overlay Camera not found!");
            return;
        }

        // Assicura che l'Overlay Camera abbia il componente UniversalAdditionalCameraData
        var overlayCameraData = overlayCamera.GetUniversalAdditionalCameraData();
        if (overlayCameraData == null)
        {
            overlayCameraData = overlayCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }

        // Configura l'Overlay Camera come Overlay
        overlayCameraData.renderType = CameraRenderType.Overlay;

        // Aggiungi l'Overlay Camera alla stack della Main Camera
        if (!mainCameraData.cameraStack.Contains(overlayCamera))
        {
            mainCameraData.cameraStack.Add(overlayCamera);
            if (debug) Debug.Log("Overlay Camera added to the Main Camera stack.");
        }
        else
        {
            if (debug) Debug.Log("Overlay Camera is already in the stack.");
        }
    }
}
