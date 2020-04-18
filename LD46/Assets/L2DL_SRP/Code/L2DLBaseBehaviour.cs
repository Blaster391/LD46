using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace L2DL
{
    [ExecuteInEditMode]
    public class L2DLBaseBehaviour<TRendererType> : MonoBehaviour
    where TRendererType : Renderer
    {
        [Header("Lighting ")]
        [Range(0, 5)] public float Emission;
        [Range(0, 5)] public float Occlusion;
        [Range(0, 1)] public float Reflectance = 0.5f;
        [Range(0, 1)] public float LightBleed = 0.5f;

        [Tooltip("Set custom lighting data for this object. If that data is linked intrisicly to the sprite used then you can add this texture as a _Lighting secondary texture on that sprite instead.")]
        public Texture LightingTexture = null;

        [Header("Animation")]
        [Tooltip("Replaces LightingTexture with specific sprites/frames, usually set via an animation to animate purely the lighting data.")]
        public Sprite lightingFrame = null;

        protected TRendererType l2DLRenderer;
        protected TRendererType L2DLRenderer
        {
            get
            {
                if(l2DLRenderer == null)
                {
                    l2DLRenderer = GetComponent<TRendererType>();
                }
                return l2DLRenderer;
            }
        }

        private MaterialPropertyBlock m_workingMaterialPropertyBlock;
        protected MaterialPropertyBlock WorkingMaterialPropertyBlock
        {
            get
            {
                if (m_workingMaterialPropertyBlock == null)
                {
                    m_workingMaterialPropertyBlock = new MaterialPropertyBlock();
                }
                return m_workingMaterialPropertyBlock;
            }
        }

        // --------------------------------------------------------------------
        protected virtual void Start()
        {
            l2DLRenderer = GetComponent<TRendererType>();

            if (l2DLRenderer == null)
            {
                Debug.LogError("Failed to find a Renderer on this GameObject. A Renderer is required, " +
                    "there's likely something wrong with this L2DL Behaviour. Contact support please!");
            }

            UpdateMaterialParameters();
        }

        // --------------------------------------------------------------------
        void LateUpdate()
        {
            UpdateMaterialParameters();
        }

        // --------------------------------------------------------------------
        protected virtual void OnValidate()
        {
            UpdateMaterialParameters();
        }

        // --------------------------------------------------------------------
        protected virtual void UpdateMaterialParameters()
        {
            // Even though the material property block stuff isn't working for instancing we should still use it to avoid creating a new material for every object
            // It might break batching and cause a bunch of problems but there doesn't seem to be a better alternative, at least that I know of right now
            // Once instancing is 'fixed' this should allow it to render all objects with the same sprite together in one call so bit of a future investment

            L2DLRenderer.GetPropertyBlock(WorkingMaterialPropertyBlock, 0);
            WorkingMaterialPropertyBlock.SetFloat("_Emission", Emission);
            WorkingMaterialPropertyBlock.SetFloat("_Occlusion", Occlusion);
            WorkingMaterialPropertyBlock.SetFloat("_Reflectance", Reflectance);
            WorkingMaterialPropertyBlock.SetFloat("_LightBleed", LightBleed);
            if(LightingTexture)
            {
                WorkingMaterialPropertyBlock.SetTexture("_Lighting", LightingTexture);
            }

            if (lightingFrame != null)
            {
                WorkingMaterialPropertyBlock.SetTexture("_Lighting", lightingFrame.texture);

                Vector2 textureSize = new Vector2(lightingFrame.texture.width, lightingFrame.texture.height);
                WorkingMaterialPropertyBlock.SetVector("_LightingStartUV", lightingFrame.rect.min / textureSize);
                WorkingMaterialPropertyBlock.SetVector("_LightingEndUV", lightingFrame.rect.max / textureSize);
            }
            else
            {
                WorkingMaterialPropertyBlock.SetVector("_LightingStartUV", new Vector2(0f, 0f));
                WorkingMaterialPropertyBlock.SetVector("_LightingEndUV", new Vector2(1f, 1f));
            }

            UpdateMaterialPropertyBlockParameters(WorkingMaterialPropertyBlock);
            
            l2DLRenderer.SetPropertyBlock(WorkingMaterialPropertyBlock, 0);
        }

        protected virtual void UpdateMaterialPropertyBlockParameters(MaterialPropertyBlock materialPropertyBlock)
        {
        }
    }
}