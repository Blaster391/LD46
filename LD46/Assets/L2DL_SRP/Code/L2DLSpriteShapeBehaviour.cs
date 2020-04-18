using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.U2D;

namespace L2DL
{
    [RequireComponent(typeof(SpriteShapeRenderer))]
    public class L2DLSpriteShapeBehaviour : L2DLBaseBehaviour<SpriteShapeRenderer>
    {
        [Header("Edge Settings")]
        [Range(0, 5)] public float EdgeEmissivePower = 0f;
        [Range(0, 5)] public float EdgeOcclusion = 0f;
        [Range(0, 1)] public float EdgeReflectance = 0.5f;
        [Range(0, 1)] public float EdgeLightBleed = 0.5f;

        public Texture EdgeLightingTexture = null;

        protected override void UpdateMaterialParameters()
        {
            base.UpdateMaterialParameters();

            L2DLRenderer.GetPropertyBlock(WorkingMaterialPropertyBlock, 1);
            WorkingMaterialPropertyBlock.SetFloat("_Emission", EdgeEmissivePower);
            WorkingMaterialPropertyBlock.SetFloat("_Occlusion", EdgeOcclusion);
            WorkingMaterialPropertyBlock.SetFloat("_Reflectance", EdgeReflectance);
            WorkingMaterialPropertyBlock.SetFloat("_LightBleed", EdgeLightBleed);
            if(EdgeLightingTexture)
            {
                WorkingMaterialPropertyBlock.SetTexture("_Lighting", EdgeLightingTexture);
            }

            UpdateMaterialPropertyBlockParameters(WorkingMaterialPropertyBlock);
            
            l2DLRenderer.SetPropertyBlock(WorkingMaterialPropertyBlock, 1);
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/L2DL/Object/L2DLSpriteShape", false, 11)]
        static void CreateL2DLSpriteShapeInWorld(MenuCommand menuCommand)
        {
            var newL2DLSpriteShape = new GameObject("L2DLSpriteShape");
            GameObjectUtility.SetParentAndAlign(newL2DLSpriteShape, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(newL2DLSpriteShape, "Create " + newL2DLSpriteShape.name);
            Selection.activeObject = newL2DLSpriteShape;
            var spriteShapeRenderer = newL2DLSpriteShape.AddComponent<SpriteShapeRenderer>();
            spriteShapeRenderer.sharedMaterial = Resources.Load("Materials/L2DL SpriteShape", typeof(Material)) as Material;
            newL2DLSpriteShape.AddComponent<L2DLSpriteShapeBehaviour>();
        }
#endif

        protected override void UpdateMaterialPropertyBlockParameters(MaterialPropertyBlock materialPropertyBlock)
        {
        }
    }
}
