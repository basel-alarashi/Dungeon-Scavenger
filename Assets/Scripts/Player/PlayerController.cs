using UnityEngine;

namespace DungeonScavenger.Player
{
    /// <summary>
    /// Handles player movement including walking, gravity, and jumping.
    /// Uses CharacterController for collision detection.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private bool enableSprint = false;

        [Header("Jump Settings")]
        [SerializeField] private bool enableJump = true;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.2f;
        [SerializeField] private LayerMask groundMask = -1; // Default to "Everything"

        [Header("Boundaries (Optional)")]
        [SerializeField] private bool limitMovementArea = false;
        [SerializeField] private float minX = -18f;
        [SerializeField] private float maxX = 18f;
        [SerializeField] private float minZ = -18f;
        [SerializeField] private float maxZ = 18f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        #endregion

        #region Private Variables

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float currentSpeed;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            controller = GetComponent<CharacterController>();

            // Validate required components
            if (enableJump && groundCheck == null)
            {
                Debug.LogWarning("[PlayerController] Jump enabled but Ground Check not assigned. Using controller.isGrounded instead.");
            }

            if (showDebugInfo)
                Debug.Log($"[PlayerController] Initialized. Jump: {enableJump}, Speed: {walkSpeed}");
        }

        private void Update()
        {
            HandleGroundCheck();
            HandleMovement();
            HandleJump();
            ApplyGravity();
            ApplyBoundaries();
        }

        #endregion

        #region Movement Handlers

        /// <summary>
        /// Checks if the player is touching the ground.
        /// Uses a sphere cast for more accurate detection than built-in isGrounded.
        /// </summary>
        private void HandleGroundCheck()
        {
            if (groundCheck != null)
            {
                // Sphere cast for precise ground detection
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }
            else
            {
                // Fallback to CharacterController's built-in ground check
                isGrounded = controller.isGrounded;
            }
        }

        /// <summary>
        /// Processes WASD input and moves the player.
        /// </summary>
        private void HandleMovement()
        {
            // Get input axes
            float horizontal = Input.GetAxis("Horizontal");  // A/D or Left/Right arrows
            float vertical = Input.GetAxis("Vertical");      // W/S or Up/Down arrows

            // Calculate movement direction relative to player's rotation
            Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

            // Normalize to prevent faster diagonal movement
            if (moveDirection.magnitude > 1f)
                moveDirection.Normalize();

            // Determine speed (walk or sprint)
            currentSpeed = walkSpeed;
            if (enableSprint && Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = sprintSpeed;
            }

            // Apply movement
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            // Debug visualization
            if (showDebugInfo && moveDirection.magnitude > 0.1f)
            {
                Debug.DrawRay(transform.position, moveDirection * 2f, Color.green);
            }
        }

        /// <summary>
        /// Handles jump input and applies vertical velocity.
        /// </summary>
        private void HandleJump()
        {
            if (!enableJump) return;

            // Jump when on ground and Space is pressed
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                // Physics formula: v = sqrt(2 * g * h)
                // Negative gravity means we use absolute value
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (showDebugInfo)
                    Debug.Log($"[PlayerController] Jump! Velocity: {velocity.y}");
            }
        }

        /// <summary>
        /// Applies gravity to the player's vertical velocity.
        /// </summary>
        private void ApplyGravity()
        {
            // Reset velocity if grounded and falling (prevents accumulating downward velocity)
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;

            // Apply vertical movement
            controller.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Optional: Restricts player movement to a defined area.
        /// </summary>
        private void ApplyBoundaries()
        {
            if (!limitMovementArea) return;

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.z = Mathf.Clamp(position.z, minZ, maxZ);
            transform.position = position;
        }

        #endregion

        #region Public Properties (Useful for other scripts)

        /// <summary>
        /// Returns true if the player is currently on the ground.
        /// </summary>
        public bool IsGrounded => isGrounded;

        /// <summary>
        /// Returns the player's current horizontal speed (ignores vertical).
        /// </summary>
        public float CurrentHorizontalSpeed => new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        /// <summary>
        /// Returns true if the player is currently moving.
        /// </summary>
        public bool IsMoving => CurrentHorizontalSpeed > 0.1f;

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Visualize ground check sphere
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }

            // Visualize movement boundaries
            if (limitMovementArea)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3((minX + maxX) / 2f, transform.position.y, (minZ + maxZ) / 2f);
                Vector3 size = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
                Gizmos.DrawWireCube(center, size);
            }
        }
#endif

        #endregion
    }
}