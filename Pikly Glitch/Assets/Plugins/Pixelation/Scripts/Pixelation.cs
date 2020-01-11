using Assets.Pixelation.Example.Scripts;
using UnityEngine;

namespace Assets.Pixelation.Scripts
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Pixelation")]
    public class Pixelation : ImageEffectBase
    {
        [Range(32.0f, 1024.0f)] public float BlockCount = 128;
        [Range(0.1f, 5f)]
        public float countMultiplierX = 1, countMultiplierY = 1;


        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            float k = Camera.main.aspect;
            Vector2 count = new Vector2(BlockCount, BlockCount/k);
            Vector2 size = new Vector2(countMultiplierX/count.x, countMultiplierY/count.y);
            //
            material.SetVector("BlockCount", count);
            material.SetVector("BlockSize", size);
            //destination.antiAliasing = 0;
            Graphics.Blit(source, destination, material);
        }
    }
}