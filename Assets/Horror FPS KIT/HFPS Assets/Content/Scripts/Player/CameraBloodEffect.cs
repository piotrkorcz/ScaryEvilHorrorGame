using UnityEngine;

public class CameraBloodEffect : MonoBehaviour {

    public Texture2D bloodTexture;
    public Texture2D bloodNormalMap;

    [Range(0.0f, 1.0f)]
    public float bloodAmount = 0.0f;

    [Range(0.0f, 1.0f)]
    public float distortion = 1.0f;

    [SerializeField]
    public Shader bloodShader = null;

    private Material material = null;

    HealthManager healthScript;

    void Start()
    {
        healthScript = GetComponentInParent<HealthManager>();
    }

    void Update()
    {
        //float difference = healthScript.maximumHealth - healthScript.Health;
        //bloodAmount = difference / 100f;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (bloodShader == null) return;
        if (material == null)
        {
            material = new Material(bloodShader);
        }
        if (material == null) return;
        if (bloodTexture != null)
            material.SetTexture("_BloodTex", bloodTexture);

        if (bloodNormalMap != null)
            material.SetTexture("_BloodBump", bloodNormalMap);

        material.SetFloat("_Distortion", distortion);
        material.SetFloat("_BloodAmount", bloodAmount);
        Graphics.Blit(src, dest, material);
    }
}