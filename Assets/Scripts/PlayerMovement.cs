using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of the player's movement
    public float rotationSpeed = 10f; // Speed of the smooth rotation

    private Vector2 movement; // Stores the player's input
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private float targetAngle; // Target angle for smooth rotation

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input from the player
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // Determine the target angle based on input
        if (movement != Vector2.zero)
        {
            targetAngle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f; // Calculate angle in degrees
        }
    }

    void FixedUpdate()
    {
        // Move the player
        rb.velocity = movement.normalized * moveSpeed;

        // Smoothly rotate the player
        float currentAngle = rb.rotation; // Get the current rotation angle
        float angle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime); // Smooth transition
        rb.rotation = angle; // Apply the new rotation
    }
}
