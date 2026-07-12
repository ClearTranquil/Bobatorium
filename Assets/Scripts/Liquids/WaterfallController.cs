using UnityEngine;

public class TeaPourVFX : MonoBehaviour
{
    [SerializeField] private MachineLever lever;
    [SerializeField] private Renderer[] waterfallRenderers;

    private Material[] materials;
    private static readonly int GlobalAlphaID = Shader.PropertyToID("_GlobalAlpha");

    private void Awake()
    {
        materials = new Material[waterfallRenderers.Length];

        for (int i = 0; i < waterfallRenderers.Length; i++)
        {
            materials[i] = waterfallRenderers[i].material;
        }
    }

    private void Update()
    {
        float alpha = Mathf.Clamp01(lever.PullAmount);

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(GlobalAlphaID, alpha);
        }
    }
}