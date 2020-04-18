using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace L2DL
{
    [RequireComponent(typeof(ParticleSystem))]
    public class L2DLParticleSystemBehaviour : L2DLBaseBehaviour<ParticleSystemRenderer>
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/L2DL/Object/L2DL Particle System", false, 11)]
        static void CreateL2DLSpriteInWorld(MenuCommand menuCommand)
        {
            var newL2DLParticleSystem = new GameObject("L2DLParticleSystem");
            GameObjectUtility.SetParentAndAlign(newL2DLParticleSystem, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(newL2DLParticleSystem, "Create " + newL2DLParticleSystem.name);
            Selection.activeObject = newL2DLParticleSystem;

            var particleSystem = newL2DLParticleSystem.AddComponent<ParticleSystem>();
            var textureSheetAnimationModule = particleSystem.textureSheetAnimation;
            textureSheetAnimationModule.enabled = true;
            textureSheetAnimationModule.mode = ParticleSystemAnimationMode.Sprites;
            
            var particleSystemRenderer = newL2DLParticleSystem.GetComponent<ParticleSystemRenderer>();
            particleSystemRenderer.sharedMaterial = Resources.Load("Materials/L2DL Sprite", typeof(Material)) as Material;
            particleSystemRenderer.sortMode = ParticleSystemSortMode.Distance;
            particleSystemRenderer.alignment = ParticleSystemRenderSpace.World;
            particleSystemRenderer.minParticleSize = 0f;
            particleSystemRenderer.maxParticleSize = 100f;
            
            newL2DLParticleSystem.AddComponent<L2DLParticleSystemBehaviour>();
        }
#endif

        protected override void UpdateMaterialPropertyBlockParameters(MaterialPropertyBlock materialPropertyBlock)
        {
        }
    }
}
