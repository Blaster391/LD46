using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace L2DL
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class L2DLSpriteBehaviour : L2DLBaseBehaviour<SpriteRenderer>
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/L2DL/Object/L2DL Sprite", false, 10)]
        static void CreateL2DLSpriteInWorld(MenuCommand menuCommand)
        {
            var newL2DLSprite = new GameObject("L2DLSprite");
            GameObjectUtility.SetParentAndAlign(newL2DLSprite, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(newL2DLSprite, "Create " + newL2DLSprite.name);
            Selection.activeObject = newL2DLSprite;
            var spriteRenderer = newL2DLSprite.AddComponent<SpriteRenderer>();
            spriteRenderer.sharedMaterial = Resources.Load("Materials/L2DL Sprite", typeof(Material)) as Material;
            newL2DLSprite.AddComponent<L2DLSpriteBehaviour>();
        }
#endif

        protected override void UpdateMaterialPropertyBlockParameters(MaterialPropertyBlock materialPropertyBlock)
        {
        }
    }
}
