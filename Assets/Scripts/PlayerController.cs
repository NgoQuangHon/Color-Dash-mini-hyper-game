using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float forwardSpeed = 8f;
    [SerializeField, Min(0f)] private float lateralSpeed = 6f;
    [SerializeField, Min(0.1f)] private float laneRange = 3f;

    [Header("Difficulty")]
    [SerializeField, Min(0f)] private float speedIncreasePerSecond = 0.25f;
    [SerializeField, Min(0f)] private float maxForwardSpeed = 18f;

    private Rigidbody rb;
    private float horizontalInput;
    private float elapsedPlayingTime;
    private float initialForwardSpeed;
    private bool isGameRunning;

    // Shared with LevelGenerator so lane width / reachability stay consistent.
    public float LaneRange => laneRange;
    public float LateralSpeed => lateralSpeed;
    public float MaxForwardSpeed => maxForwardSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialForwardSpeed = forwardSpeed;
    }

    private void Update()
    {
        if (!CanMove())
        {
            horizontalInput = 0f;
            return;
        }

        elapsedPlayingTime += Time.deltaTime;
        forwardSpeed = Mathf.Min(
            initialForwardSpeed + elapsedPlayingTime * speedIncreasePerSecond,
            maxForwardSpeed
        );

        horizontalInput = ReadHorizontalInput();
    }

    private void FixedUpdate()
    {
        if (!CanMove())
            return;

        Vector3 nextPosition = rb.position;
        nextPosition.z += forwardSpeed * Time.fixedDeltaTime;
        nextPosition.x += horizontalInput * lateralSpeed * Time.fixedDeltaTime;
        nextPosition.x = Mathf.Clamp(nextPosition.x, -laneRange, laneRange);
        rb.MovePosition(nextPosition);
    }

    /// <summary>
    /// Keyboard (A/D, arrows) and touch (hold left / right half of the screen).
    /// Works with the new Input System, the old Input Manager, or both.
    /// </summary>
    private static float ReadHorizontalInput()
    {
        float input = 0f;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                input -= 1f;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                input += 1f;
        }

        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            float x = touchscreen.primaryTouch.position.ReadValue().x;
            input += x < Screen.width * 0.5f ? -1f : 1f;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            input -= 1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            input += 1f;

        if (Input.touchCount > 0)
        {
            float x = Input.GetTouch(0).position.x;
            input += x < Screen.width * 0.5f ? -1f : 1f;
        }
#endif

        return Mathf.Clamp(input, -1f, 1f);
    }

    private bool CanMove()
    {
        if (!isGameRunning)
            return false;

        return GameManager.Instance == null || GameManager.Instance.IsPlaying;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Obstacle"))
            return;

        StopGame();

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayCollision();

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Coin"))
            return;

        Coin coin = other.GetComponent<Coin>();
        if (coin != null)
            coin.Collect();
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
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ResetRun()
    {
        elapsedPlayingTime = 0f;
        forwardSpeed = initialForwardSpeed;
        StopGame();
    }
}
