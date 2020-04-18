using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;

[RequireComponent(typeof(SpriteRenderer))]
public class InstancedSprite : MonoBehaviour
{
    private static MaterialPropertyBlock s_matPropBlock;
    private static int s_colorPropertyID = Shader.PropertyToID("_InstancedColor");
    private static int s_alphaPropertyID = Shader.PropertyToID("_InstancedAlpha");

    [SerializeField] private Color m_color = Color.white;
    [SerializeField] [Range(0, 1)] private float m_alpha = 1f;

    private void Awake()
    {
        SetColorToMesh();
    }

    private void OnValidate()
    {
        SetColorToMesh();
    }

    private void SetColorToMesh()
    {
        if(s_matPropBlock == null)
        {
            s_matPropBlock = new MaterialPropertyBlock();
        }

        SpriteRenderer spiteRenderer = GetComponent<SpriteRenderer>();

        spiteRenderer.GetPropertyBlock(s_matPropBlock);
        s_matPropBlock.SetColor(s_colorPropertyID, m_color);
        s_matPropBlock.SetFloat(s_alphaPropertyID, m_alpha);
        spiteRenderer.SetPropertyBlock(s_matPropBlock);
    }
}
