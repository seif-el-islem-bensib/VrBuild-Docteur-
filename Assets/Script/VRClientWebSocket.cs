using System;
using System.Collections;
using UnityEngine;
using NativeWebSocket;

public class VRClientWebSocket : MonoBehaviour
{
    private WebSocket websocket;
    public float sendInterval = 0.1f; // Seconds between sending frames
    public static event System.Action<string> OnMessageReceived;

    void Start()
    {
        Debug.Log("[VRClientWebSocket] Start() called");
        StartCoroutine(StartWebSocketAfterSkyboxReady());
    }

    private IEnumerator StartWebSocketAfterSkyboxReady()
    {
        Debug.Log("[VRClientWebSocket] Waiting for VRCaptureFromSkybox to initialize...");
        yield return new WaitUntil(() => VRCaptureFromSkybox.Instance != null && VRCaptureFromSkybox.Instance.IsInitialized);

        Debug.Log("[VRClientWebSocket] VRCaptureFromSkybox initialized, starting WebSocket.");
        ConnectWebSocket();
    }

    private void ConnectWebSocket()
    {
     
            //192.168.1.15
        websocket = new WebSocket("ws://192.168.100.5:8080"); // Replace with your desktop IP/port

        websocket.OnOpen += () =>
        {
            Debug.Log("[VRClientWebSocket] Connected to server.");
            //here
         //   MenuLoader.Instance.StartCoroutine(MenuLoader.Instance.ConnectedToServerFeedback());
            StartCoroutine(SendFrames());
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"[VRClientWebSocket] Error: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("[VRClientWebSocket] Connection closed.");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("[VRClientWebSocket] Received: " + message);

            // Broadcast the message to any listeners (e.g., PlayerVRControl)
            OnMessageReceived?.Invoke(message);
        };

        websocket.Connect();
    }


    private IEnumerator SendFrames()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendInterval);

            var texture = VRCaptureFromSkybox.Instance.GetFlattenedRenderTexture();
            if (texture == null)
            {
                Debug.LogWarning("[VRClientWebSocket] Flattened RenderTexture is null.");
                continue;
            }

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = texture;

            Texture2D tex2D = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            tex2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            tex2D.Apply();

            RenderTexture.active = currentRT;

            byte[] imageBytes = tex2D.EncodeToJPG(75); // Quality 75%
            Destroy(tex2D);

            if (websocket.State == WebSocketState.Open)
            {
                websocket.Send(imageBytes);
            }
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    private void OnApplicationQuit()
    {
        if (websocket != null)
        {
            websocket.Close();
        }
    }
}
