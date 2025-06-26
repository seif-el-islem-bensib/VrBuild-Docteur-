using UnityEngine;

public class SkyboxCaptureManager : MonoBehaviour
{
    public static SkyboxCaptureManager Instance { get; private set; }

    [Header("References")]
    public Camera captureCamera; // Dedicated, disabled camera
    public RenderTexture captureRenderTexture; // Assigned in Inspector or created
    public GameObject videoSphere; // Optional: 360 video sphere (hidden from main camera)

    [Header("Unwrap")]
    public Material unwrapMaterial;
    public GameObject quadOutput; // Quad in front of captureCamera

    public bool IsInitialized { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        Debug.Log("[SkyboxCaptureManager] Starting setup...");

        if (!captureCamera || !captureRenderTexture || !unwrapMaterial || !quadOutput)
        {
            Debug.LogError("[SkyboxCaptureManager]  Missing required references!");
            return;
        }

        captureCamera.enabled = false;
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = Color.black;
        captureCamera.cullingMask = LayerMask.GetMask("SkyboxCapture");

        captureCamera.targetTexture = captureRenderTexture;

        quadOutput.GetComponent<MeshRenderer>().material = unwrapMaterial;
        unwrapMaterial.SetTexture("_SkyboxTex", captureRenderTexture);

        if (videoSphere)
        {
            videoSphere.layer = LayerMask.NameToLayer("SkyboxCapture");
        }

        IsInitialized = true;
        Debug.Log("[SkyboxCaptureManager]  Initialized.");
    }

    void LateUpdate()
    {
        if (!IsInitialized) return;

        captureCamera.Render();

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log("[SkyboxCaptureManager]  Captured frame.");
        }
    }

    public RenderTexture GetFlattenedRenderTexture()
    {
        return captureRenderTexture;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
