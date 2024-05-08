using System.Collections;
using UnityEngine;

public class HelicopterLift : MonoBehaviour
{
    [Header("Helicopter Components Refs")]
    [SerializeField]
    private HelicopterBlades[] helicopterBlades;
    [SerializeField]
    private Transform[] targetPoints;

    [Header("Speed Values")]
    [SerializeField]
    private float movementSpeed = 1f;
    [SerializeField]
    private float rotationSpeed = 3f;
    [SerializeField]
    private float liftSpeed = 0.5f;

    [Header("Time Values")]
    [SerializeField]
    private float liftTime = 5f;
    [SerializeField]
    private float resetRotationXDurationTime = 1.75f;

    [Header("Additional Values")]
    [SerializeField]
    private float targetHeight = 10f;
    [SerializeField]
    private float maxRotationX = 15.0f;
    [SerializeField]
    private float landingForce = 0.15f;
    [SerializeField]
    private float swayForce = 20f;

    // Private variables
    private Rigidbody rb;
    private int currentTargetIndex = 0;
    private bool liftButtonClicked = false;
    private bool resetX = false;
    private bool isLanding = false;

    private float actualSpeed = 0;
    private bool isLifting = false;
    private readonly WaitForEndOfFrame waitForEndOfFrame = new();

    private float targetRotationX = -150f;
    private bool isRotating = false;

    private float lastSwayTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        lastSwayTime = Time.time;
    }

    void SwayBraker()
    {
        Vector3 swayDirection = new(Mathf.Sin(Time.time * 25f), 0f, Mathf.Cos(Time.time * 25f));
        rb.AddForce(swayDirection * swayForce, ForceMode.Impulse);
        Debug.LogError($"Sway: dir {swayDirection * swayForce}");
    }

    void Update()
    {
        if (isLifting)
        {
            

            if (isRotating)
            {
                float newRotationX = transform.rotation.eulerAngles.x;
                newRotationX -= Time.deltaTime;

                // Aktualizuj rotacjê helikoptera
                transform.rotation = Quaternion.Euler(newRotationX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

                // Jeœli osi¹gniêto docelow¹ rotacjê, zakoñcz rotacjê
                if (newRotationX <= targetRotationX)
                {
                    isRotating = false;
                }
            }
        }

        if (liftButtonClicked)
        {
            UpdateLift();
        }

        if (isLanding)
        {
            UpdateLand();
        }

        HeightAndRotationChecker();
    }

    void UpdateLift()
    {
        foreach (HelicopterBlades helicopterBlade in helicopterBlades)
        {
            helicopterBlade.StartRotors();
        }
        StartCoroutine(LiftAfterDelay());
        liftButtonClicked = false;
        return;
    }

    void UpdateLand()
    {
        rb.AddForce(-9.81f * landingForce * Vector3.up, ForceMode.Acceleration);
        if (transform.position.y <= 0.25f)
        {
            isLanding = false;
            rb.useGravity = true;
            foreach (HelicopterBlades helicopterBlade in helicopterBlades)
            {
                helicopterBlade.StopRotors();
            }
        }
        resetX = false;
        return;
    }

    void HeightAndRotationChecker()
    {
        if (transform.position.y >= 3.0f)
        {
            if (Time.time - lastSwayTime >= 1f)
            {
                SwayBraker();
                lastSwayTime = Time.time;
            }
        }

        if (transform.position.y >= 0.45f * targetHeight && !resetX)
        {
            RotateTowardsTarget();
        }

        if (transform.rotation.eulerAngles.x >= maxRotationX && transform.rotation.eulerAngles.x < 360.0f - maxRotationX)
        {
            transform.rotation = Quaternion.Euler(maxRotationX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        if (!resetX && CalculateDistance(transform.position, targetPoints[currentTargetIndex].position) <= 2.5f)
        {
            StartCoroutine(ResetRotationX());
        }
    }

    IEnumerator ResetRotationX()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);
        float elapsedTime = 0f;
        float duration = resetRotationXDurationTime;
        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return waitForEndOfFrame;
        }
        isLanding = true;
        resetX = true;
        transform.rotation = targetRotation;
    }

    public float CalculateDistance(Vector3 point1, Vector3 point2)
    {
        Vector2 point1XZ = new(point1.x, point1.z);
        Vector2 point2XZ = new(point2.x, point2.z);
        float distance = Vector2.Distance(point1XZ, point2XZ);

        return distance;
    }

    IEnumerator MoveToTargetAfterDelay()
    {
        yield return new WaitForSeconds(15.00f);
        MoveToTargetPosition();
    }

    IEnumerator LiftAfterDelay()
    {
        yield return new WaitForSeconds(liftTime);
        Lift();
    }

    void Lift()
    {
        isRotating = true;

        isLifting = true;
        rb.useGravity = false;

        float liftForce = Mathf.Sqrt(liftSpeed * Mathf.Abs(Physics.gravity.y) * targetHeight);
        rb.AddForce(Vector3.up * liftForce, ForceMode.VelocityChange);
        Debug.LogError("Lift");
    }

    void RotateTowardsTarget()
    {
        Vector3 targetDirection = targetPoints[currentTargetIndex].position - transform.position;
        float step = rotationSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
        isLifting = false;
        StartCoroutine(MoveToTargetAfterDelay());
    }

    void MoveToTargetPosition()
    {
        float newRotationX = transform.rotation.eulerAngles.x;
        newRotationX += 10.0f * Time.deltaTime;

        transform.rotation = Quaternion.Euler(newRotationX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        Vector3 targetPosition = targetPoints[currentTargetIndex].position;
        targetPosition.y = targetHeight;

        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        actualSpeed += 2.0f * Time.deltaTime;
        if (actualSpeed > movementSpeed)
        {
            actualSpeed = movementSpeed;
        }

        Vector3 desiredVelocity = direction * actualSpeed;
        Vector3 currentVelocity = rb.velocity;
        Vector3 velocityChange = desiredVelocity - currentVelocity;

        rb.AddForce(velocityChange, ForceMode.Acceleration);

        float swayForce = 0.35f;
        Vector3 swayDirection = new Vector3(Mathf.Sin(Time.time * 1f), 0f, Mathf.Cos(Time.time * 1f));
        rb.AddForce(swayDirection * swayForce, ForceMode.Acceleration);
        
        float tiltAmount = Mathf.Clamp(Vector3.Dot(swayDirection, transform.right), -1f, 1f);
        float swayAmount = 25f * tiltAmount;
        Vector3 rotationAxis = Vector3.forward;

        transform.Rotate(rotationAxis, swayAmount * Time.deltaTime);
    }

    void NextTarget()
    {
        currentTargetIndex = (currentTargetIndex + 1) % targetPoints.Length;
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Lift Helicopter"))
        {
            actualSpeed = 0.0f;
            liftButtonClicked = true;
        }
    }
#endif
}
