using UnityEngine;
using UnityEngine.XR;

public class VRCaptureFromSkybox : MonoBehaviour
{
    public static VRCaptureFromSkybox Instance { get; private set; }


    public Transform xrRig; // Main camera (assign in Inspector)
    public Camera eyeCamera; // VR eye camera (assign in Inspector)
    public Camera desktopCamera; // Camera to capture the desktop view (assign in Inspector)
    public RenderTexture skyboxRenderTexture; // Assigned in Inspector (360 skybox source)
    private RenderTexture flattenedRenderTexture; // Texture we will actually send

    private Material skyboxMaterial;
    private GameObject quad;
    private int unwrapLayer;

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
        XRSettings.eyeTextureResolutionScale = 1.5f; // Up to 2.0
        Debug.Log("[VRCaptureFromSkybox] Start() called");

        if (eyeCamera == null || desktopCamera == null || skyboxRenderTexture == null)
        {
            Debug.LogError("[VRCaptureFromSkybox] One or more required components are not assigned!");
            return;
        }

        Debug.Log("[VRCaptureFromSkybox] All components assigned.");

        // Create a separate root for the desktop camera to avoid hierarchy interference
        GameObject desktopRoot = new GameObject("DesktopViewRoot");
        desktopCamera.transform.SetParent(desktopRoot.transform, false);

        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.SetParent(desktopCamera.transform);
        quad.transform.localPosition = new Vector3(0, 0, 999999); // Adjusted to a reasonable distance
        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3(1, 1, 1);
    //    Debug.Log("[VRCaptureFromSkybox] Quad created and positioned.");

        flattenedRenderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        flattenedRenderTexture.Create();
        desktopCamera.targetTexture = flattenedRenderTexture;
     //   Debug.Log("[VRCaptureFromSkybox] Assigned flattenedRenderTexture to desktopCamera.");

        unwrapLayer = LayerMask.NameToLayer("UnwrapQuad");
        if (unwrapLayer == -1)
        {
          //  Debug.LogWarning("[VRCaptureFromSkybox] Layer 'UnwrapQuad' not found! Please create it manually.");
        }
        else
        {
            quad.layer = unwrapLayer;
          //  Debug.Log($"[VRCaptureFromSkybox] Assigned quad to 'UnwrapQuad' layer {unwrapLayer}.");
        }

        Shader unwrapShader = Shader.Find("Unlit/EquirectangularUnwrap");
        if (unwrapShader == null || !unwrapShader.isSupported)
        {
           // Debug.LogError("[VRCaptureFromSkybox] Unwrap shader not found or unsupported!");
            skyboxMaterial = new Material(Shader.Find("Unlit/Color"));
            skyboxMaterial.color = Color.red;
        }
        else
        {
            skyboxMaterial = new Material(unwrapShader);
            skyboxMaterial.SetTexture("_SkyboxTex", skyboxRenderTexture);
            skyboxMaterial.SetFloat("_FOV", desktopCamera.fieldOfView);
           // Debug.Log("[VRCaptureFromSkybox] Shader and material set up.");
        }

        quad.GetComponent<MeshRenderer>().material = skyboxMaterial;
       // Debug.Log("[VRCaptureFromSkybox] Material applied to quad.");
      //  Debug.Log($"[VRCaptureFromSkybox] flattenedRenderTexture set: {flattenedRenderTexture.width}x{flattenedRenderTexture.height}");

        IsInitialized = true;
        Debug.Log("[VRCaptureFromSkybox] Initialization complete.");
    }

    void LateUpdate()
    {
        if (IsInitialized && eyeCamera != null && desktopCamera != null)
        {
            // Update desktop camera rotation based on eye camera without affecting VR hierarchy
            desktopCamera.transform.rotation = eyeCamera.transform.rotation;
            // Ensure desktop camera position is fixed (e.g., at origin) to avoid movement interference
            desktopCamera.transform.position = Vector3.zero;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (quad != null) Destroy(quad);
        if (skyboxMaterial != null) Destroy(skyboxMaterial);
        if (flattenedRenderTexture != null) flattenedRenderTexture.Release();
      //  Debug.Log("[VRCaptureFromSkybox] Cleaned up.");
    }

    public RenderTexture GetFlattenedRenderTexture()
    {
        return flattenedRenderTexture;
    }
}