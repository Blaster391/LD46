using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IL2DLDirectLight
{
    float Intensity { get; set; }
    Color Color { get; set; }

    Camera ShadowCamera { get; }
    L2DLTextureSize ShadowMapSize { get; }
}
