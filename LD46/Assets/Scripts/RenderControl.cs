using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderControl : MonoBehaviour
{    
    void Update()
    {
        if (L2DLCameraRenderer.CameraRenderInstance != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                L2DLCameraRenderer.CameraRenderInstance.TextureToView = L2DLBufferTextures.None;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                L2DLCameraRenderer.CameraRenderInstance.TextureToView = L2DLBufferTextures.Colour;
            }
        }
    }
}
