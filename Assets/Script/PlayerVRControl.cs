using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.XR;


public class PlayerVRControl : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public LineRotator lineRotator;

    public GameObject VideoPlayerObjects;
    public GameObject VerticaleOptiqueObjects;

    public VideoClip[] videoClips; // Assign via Inspector
    private int currentClipIndex = 0;

    void Start()
    {
        Debug.Log("[PlayerVRControl] Start() called from " + SceneManager.GetActiveScene().name);

        if (videoPlayer == null)
        {
            Debug.LogWarning("[PlayerVRControl] No VideoPlayer found in the scene.");
        }

        if (videoClips.Length > 0)
        {
            videoPlayer.clip = videoClips[currentClipIndex];
        }

        VRClientWebSocket.OnMessageReceived += HandleCommand;
    }

    private void OnDestroy()
    {
        VRClientWebSocket.OnMessageReceived -= HandleCommand;
    }

    public void HandleCommand(string command)
    {
        if (string.IsNullOrEmpty(command)) return;

        string normalizedCommand = command.Trim().ToLower();
        Debug.Log($"[PlayerVRControl] Normalized command: '{normalizedCommand}'");

        switch (normalizedCommand)
        {
            case "play":
                Play();
                break;

            case "next":
                PlayNextClip();
                break;

            case "prev":
                PlayPreviousClip();
                break;

            case "recenter":
                PerformRecenter();
                break;

            case "vosr":
                Debug.Log("vosr sent");
                VideoPlayerObjects.SetActive(false);
                VerticaleOptiqueObjects.SetActive(true);
                videoPlayer.Pause();
                break;

            case "vpr":
                Debug.Log("vpr sent");
                VideoPlayerObjects.SetActive(true);
                VerticaleOptiqueObjects.SetActive(false);
                videoPlayer.Play();
                break;

            case "rlr":
                Debug.Log("rlr sent");
                lineRotator.RotateRight();
                break;

            case "rll":
                Debug.Log("vll sent");
                lineRotator.RotateLeft();
                break;

            default:
                Debug.LogWarning($"[PlayerVRControl] Unknown command: {command}");
                break;
        }
    }

    public void PerformRecenter()
    {
        Transform rig = VRCaptureFromSkybox.Instance.xrRig;
        Transform head = VRCaptureFromSkybox.Instance.eyeCamera.transform;
        Debug.Log($"[Recenter] head.localPosition: {head.localPosition}, rig.position before: {rig.position}");

        // Oculus native recenter
        OVRManager.display.RecenterPose();

        // Optional manual correction (only needed if you're doing additional world-offset logic)
     

        Vector3 offset = new Vector3(head.localPosition.x, 0f, head.localPosition.z);
        rig.position -= offset;

        Debug.Log($"[Recenter] rig.position after: {rig.position}");
    }

    private void Play()
    {
        if (videoPlayer != null)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
                videoPlayer.Play();

            Debug.Log("[PlayerVRControl] Play/Pause toggled");
        }
    }

    private void PlayNextClip()
    {
        if (videoClips.Length == 0) return;

        currentClipIndex = (currentClipIndex + 1) % videoClips.Length;
        videoPlayer.clip = videoClips[currentClipIndex];
        videoPlayer.Play();

        Debug.Log($"[PlayerVRControl] Playing next clip: {videoClips[currentClipIndex].name}");
    }

    private void PlayPreviousClip()
    {
        if (videoClips.Length == 0) return;

        currentClipIndex = (currentClipIndex - 1 + videoClips.Length) % videoClips.Length;
        videoPlayer.clip = videoClips[currentClipIndex];
        videoPlayer.Play();

        Debug.Log($"[PlayerVRControl] Playing previous clip: {videoClips[currentClipIndex].name}");
    }

    private void SkipForward()
    {
        if (videoPlayer != null)
        {
            videoPlayer.time += 5;
            Debug.Log("[PlayerVRControl] Skip Forward 5s");
        }
    }

    private void SkipBackward()
    {
        if (videoPlayer != null)
        {
            videoPlayer.time = Mathf.Max(0, (float)videoPlayer.time - 5);
            Debug.Log("[PlayerVRControl] Skip Backward 5s");
        }
    }
}
