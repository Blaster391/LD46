using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(L2DLSpotLight))]
public class L2DLSpotLightPositioningAssistance : MonoBehaviour
{
    L2DLSpotLight spotLight;

    private void Awake()
    {
        spotLight = GetComponent<L2DLSpotLight>();
    }
    
    void LateUpdate()
    {
        if(spotLight == null)
        {
            spotLight = GetComponent<L2DLSpotLight>();
        }

        Vector3 localPosition = transform.localPosition;
        localPosition.y = -spotLight.Range / 2f;
        transform.localPosition = localPosition;
    }
}
