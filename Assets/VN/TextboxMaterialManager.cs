using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utilities.Swizzle;

public class TextboxMaterialManager : MonoBehaviour
{

    [SerializeField] Material OutputTextboxMaterial;
    [SerializeField] RectTransform TextboxSpriteTransform;

    private void Awake()
    {
        OutputTextboxMaterial.SetVector("_Dimensions", TextboxSpriteTransform.localScale.xyzw());
    }

}
