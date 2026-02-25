using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // TODO: tweak default values
    [SerializeField]
    private float moveForce = 1.5f; 

    [SerializeField]
    private float jumpForce = 5.0f;

    [SerializeField]
    private float fallOffTime = 1f;

    private InputAction moveAction;
    private InputAction jumpAction;

    private Rigidbody playerRigidBody;
    private Camera playerCamera;

    private Vector3 finalMoveVector;
    private bool shouldJump;
    private float jumpTimer = 0;
    private bool grounded;

    void Start()
    {
        // Fetch our inputs from the new Unity input system
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        // Could be extended to proper camera system
        playerCamera = Camera.main;
        playerRigidBody = GetComponent<Rigidbody>();

        // Lock our cursor and turn in invisible when in the game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Compute the move vector and store it for the physics step
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        finalMoveVector = transform.forward * moveValue.y + transform.right * moveValue.x;
        grounded = isGrounded();

        // TODO: add wall run

        // TODO: attach to wall + rotate = "gravity boots"

        // Variable for fall off time
        jumpTimer += Time.deltaTime;

        // Toggle jump if button press & on ground
        if (jumpAction.WasPressedThisFrame() && (grounded || jumpTimer < fallOffTime))
        {
            shouldJump = true;
        }
    }

    private void FixedUpdate()
    {
        // Rotate our player body to match where the camera is looking
        transform.eulerAngles = new Vector3(0, playerCamera.transform.eulerAngles.y, 0);

        // Compute more accurate velocity change
        Vector3 velocity = playerRigidBody.linearVelocity;
        Vector3 targetVelocity = finalMoveVector.normalized * moveForce;
        Vector3 appliedVelocity = new Vector3(
                targetVelocity.x - velocity.x,
                0,
                targetVelocity.z - velocity.z
        );

        float moveControlMultiplier = grounded ? 1 : 0.3f; // TODO: add slider in inspector
        playerRigidBody.AddForce(appliedVelocity * moveControlMultiplier, ForceMode.VelocityChange);

        if (shouldJump)
        {
            playerRigidBody.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
            shouldJump = false;
        }
    }

    bool isGrounded()
    {
        RaycastHit hit;
        int layerMask = ~(1 << 3);

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f, layerMask)) // TODO: add height check/config
        {
            Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.red); // TODO: Make Debug toggle
            jumpTimer = 0;
            return true;
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down * 1000, Color.white); // TODO: Make Debug toggle
            return false;
        }

    }
}
