using UnityEngine;

public class FishForwardMovement : MonoBehaviour
{
  [Header("Movement Settings")]
    [SerializeField] private float speed = 0.5f;        // Forward swim speed
    [SerializeField] private float turnSpeed = 1.0f;    // How fast the fish can turn

    [Header("Wall Avoidance")]
    [SerializeField] private float wallCheckDistance = 0.5f; // How far to "feel" for a wall
    [SerializeField] private LayerMask wallLayerMask;        // Set this in the Inspector to your "Wall" layer

    [Header("Behavioral Randomness")]
    [SerializeField] private float minChangeDirTime = 2.0f; // Min time (sec) before picking a new direction
    [SerializeField] private float maxChangeDirTime = 5.0f; // Max time (sec) before picking a new direction
    [SerializeField] private float randomTurnArc = 20.0f;   // How sharply it can randomly turn (in degrees)

    private Rigidbody rb;
    private Vector3 targetDirection;
    private float timeToChangeDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Start facing a random horizontal direction
        transform.Rotate(0, Random.Range(0, 360), 0);
        targetDirection = transform.forward;
        
        // Stagger the first turn
        ScheduleNextDirectionChange();
    }

    void FixedUpdate()
    {
        // 1. Decide where to go
        UpdateTargetDirection();

        // 2. Smoothly rotate towards that direction
        RotateFish();

        // 3. Always move forward (based on new rotation)
        MoveFish();
    }

    void UpdateTargetDirection()
    {
        // --- Wall Avoidance (Priority #1) ---
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, wallCheckDistance, wallLayerMask))
        {
            // A wall is detected!
            // We must turn *immediately* to avoid moving through it.

            // --- THIS IS THE FIX ---

            // Option 1: A natural-looking "bounce" (Recommended)
            // This calculates the reflection angle, like a pool ball.
            Vector3 reflectDir = Vector3.Reflect(transform.forward, hit.normal).normalized;
            
            // Instantly set the rotation. This is not a 'target' anymore, it's a command.
            transform.rotation = Quaternion.LookRotation(reflectDir, Vector3.up);
            
            // We *must* also update targetDirection so the smooth rotator doesn't
            // try to "correct" our instant turn.
            targetDirection = reflectDir;


            // Option 2: The simple 180-degree "turn back" (Your original idea)
            // If you prefer the simple 180-degree flip, use this instead of Option 1.
            /*
            Vector3 turnBackDir = -transform.forward;
            transform.rotation = Quaternion.LookRotation(turnBackDir, Vector3.up);
            targetDirection = turnBackDir;
            */
            
            // -------------------------

            // Reset the random timer so it doesn't try to turn again right away
            ScheduleNextDirectionChange();
            return; // Exit early, wall avoidance is most important
        }

        // --- Random Behavior (Priority #2) ---
        // (This part is the same as before and will only run if NO wall is detected)
        timeToChangeDirection -= Time.fixedDeltaTime;

        if (timeToChangeDirection <= 0)
        {
            // Time to pick a new random direction
            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(-randomTurnArc, randomTurnArc), // Pitch
                Random.Range(-randomTurnArc, randomTurnArc), // Yaw
                0                                            // Roll
            );
            
            // Apply this random rotation to our current forward direction
            // This becomes the *target* for the smooth Slerp turn.
            targetDirection = (randomRotation * transform.forward).normalized;

            // Schedule the next change
            ScheduleNextDirectionChange();
        }
    }

    void RotateFish()
    {
        // Create the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // Smoothly interpolate from our current rotation to the target rotation
        // The 'turnSpeed' controls how fast this Slerp happens.
        rb.MoveRotation(Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        ));
    }

    void MoveFish()
    {
        // We always move along our *actual* forward vector (which is being rotated by RotateFish)
        rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
    }

    void ScheduleNextDirectionChange()
    {
        // Reset the timer with a new random value
        timeToChangeDirection = Random.Range(minChangeDirTime, maxChangeDirTime);
    }
}