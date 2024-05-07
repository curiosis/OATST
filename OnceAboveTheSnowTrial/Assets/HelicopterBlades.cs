using UnityEngine;
public enum RotationAxis
{
    X,
    Y,
    Z
}
public class HelicopterBlades : MonoBehaviour
{
    // Serialize variables
    [Header("Speed Values")]
    [SerializeField]
    private float minSpeed = 0f;
    [SerializeField]
    private float maxSpeed = 1500f;

    [Header("Time Values")]
    [SerializeField]
    private float accelerationTime = 5f;

    [Header("Additional Values")]
    [SerializeField]
    private RotationAxis rotationAxis = RotationAxis.Y;

    // Private variables
    private bool increasePower = false;
    private bool stopPower = false;

    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float acceleration = 0f;
    private Vector3 rotationVector = Vector3.zero;

    void Start()
    {
        targetSpeed = maxSpeed;
        acceleration = (maxSpeed - minSpeed) / accelerationTime;

        switch (rotationAxis)
        {
            case RotationAxis.X:
                rotationVector = Vector3.right;
                break;
            case RotationAxis.Y:
                rotationVector = Vector3.up;
                break;
            case RotationAxis.Z:
                rotationVector = Vector3.forward;
                break;
        }
    }

    void Update()
    {
        UpdateRotors();
    }

    void UpdateRotors()
    {
        if (increasePower)
        {
            targetSpeed = maxSpeed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else if (stopPower)
        {
            targetSpeed = minSpeed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }

        transform.Rotate(rotationVector, currentSpeed * Time.deltaTime);
    }

    public void StartRotors()
    {
        stopPower = false;
        increasePower = true;
    }

    public void StopRotors()
    {
        increasePower = false;
        stopPower = true;
    }
}