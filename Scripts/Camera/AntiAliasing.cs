/**
 * Applies the Anti Aliasing shader. See shaders/AntiAliasing
 */

using UnityEngine;

public class AntiAliasing : MonoBehaviour
{
    public Material AA_Material;

    [ExecuteInEditMode]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, AA_Material);
    }
}
