using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private CharacterController controller;
    private Vector3 velocity;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Professional: Log component initialization
        if (showDebugInfo)
            Debug.Log($"[PlayerController] Initialized on {gameObject.name}");
    }
    
    void Update()
    {
        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * walkSpeed * Time.deltaTime);
        
        // Gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;
            
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // Debug visualization
        if (showDebugInfo && move.magnitude > 0.1f)
        {
            Debug.DrawRay(transform.position, move.normalized * 2f, Color.green);
        }
    }
    
    // Professional: Public method to check if player is moving (useful for animations later)
    public bool IsMoving()
    {
        return controller.velocity.magnitude > 0.1f;
    }
}