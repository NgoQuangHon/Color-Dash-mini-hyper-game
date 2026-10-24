
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private float lateralSpeed = 6f;
    [SerializeField] private float laneRange = 3f;

    [Header("Difficulty Settings")]
    [SerializeField] private float speedIncreasePerSecond = 0.25f;
    [SerializeField] private float maxForwardSpeed = 18f;

    private float initialForwardSpeed;

    private Rigidbody rb;
    private float horizontalInput;
    private bool isGameRunning = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialForwardSpeed = forwardSpeed;
    }

    private void Update()
    {
        if (!isGameRunning)
        {
            horizontalInput = 0f;
            return;
        }

        forwardSpeed = Mathf.Min(
            initialForwardSpeed +
            Time.timeSinceLevelLoad * speedIncreasePerSecond,
            maxForwardSpeed
        );

        horizontalInput = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            horizontalInput -= 1f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            horizontalInput += 1f;
    }

    private void FixedUpdate()
    {
        if (!isGameRunning)
            return;

        Vector3 nextPosition = rb.position;
        nextPosition.z += forwardSpeed * Time.fixedDeltaTime;
        nextPosition.x += horizontalInput
                          * lateralSpeed
                          * Time.fixedDeltaTime;

        nextPosition.x = Mathf.Clamp(
            nextPosition.x, -laneRange, laneRange
        );

        rb.MovePosition(nextPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            StopGame();

            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            Coin coin = other.GetComponent<Coin>();

            if (coin != null)
                coin.Collect();
        }
    }

    public void StartGame()
    {
        isGameRunning = true;
    }

    public void StopGame()
    {
        isGameRunning = false;
        horizontalInput = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
