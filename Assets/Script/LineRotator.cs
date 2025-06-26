using UnityEngine;
using UnityEngine.XR;

public class LineRotator : MonoBehaviour
{
    public static LineRotator Instance { get; private set; }
    public float rotationAngle = 5f;
    public float inputThreshold = 0.5f;
    public float cooldown = 0.2f;
    public float rotationSpeed = 180f; // degrees per second

    private float lastInputTime;
    private Quaternion targetRotation;

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
        targetRotation = transform.rotation;
    }

    void Update()
    {
        Vector2 input;

        if (TryGetCombinedJoystickInput(out input))
        {
            if (Time.time - lastInputTime > cooldown)
            {
                if (input.x > inputThreshold)
                {
                    RotateRight();
                    lastInputTime = Time.time;
                }
                else if (input.x < -inputThreshold)
                {
                    RotateLeft();
                    lastInputTime = Time.time;
                }
            }
        }

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    bool TryGetCombinedJoystickInput(out Vector2 combinedAxis)
    {
        combinedAxis = Vector2.zero;

        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        Vector2 leftAxis = Vector2.zero;
        Vector2 rightAxis = Vector2.zero;
        bool gotLeft = leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftAxis);
        bool gotRight = rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightAxis);

        // Use the axis with the stronger horizontal input
        if (gotLeft && gotRight)
        {
            combinedAxis = Mathf.Abs(leftAxis.x) > Mathf.Abs(rightAxis.x) ? leftAxis : rightAxis;
            return true;
        }
        else if (gotLeft)
        {
            combinedAxis = leftAxis;
            return true;
        }
        else if (gotRight)
        {
            combinedAxis = rightAxis;
            return true;
        }

        return false;
    }

    public void RotateLeft()
    {
        targetRotation *= Quaternion.Euler(0f, 0f, rotationAngle);
    }

    public void RotateRight()
    {
        targetRotation *= Quaternion.Euler(0f, 0f, -rotationAngle);
    }
}
