using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Variables
    [Header("Player Movement")]
    [Tooltip("The speed the player moves")]
    public float moveSpeed;

    [Tooltip("The force of player jump")]
    public float jumpForce;

    [Tooltip("Force Mode the player will use for movement")]
    public ForceMode playerForceMode;

    [Header("Other Components")]
    [Tooltip("The new Input System Script")]
    public NewInputSystemScript playerInputScript;

    enum GroundLayer { Default, TransparentFX, IgnoreRaycast, NotGround, Water, UI}
    [Tooltip("What does the player consider as ground")]
    [SerializeField]
    private GroundLayer groundLayer;

    private Vector2 moveDirection = Vector2.zero;
    private Rigidbody rb;

    bool grounded = true;

    //input actions
    private InputAction move;
    private InputAction fire;
    private InputAction jump;


    //Methods
    private void OnEnable()
    {
        move = playerInputScript.Player.Move;
        move.Enable();

        fire = playerInputScript.Player.Fire;
        fire.Enable();
        fire.performed += Fire;

        jump = playerInputScript.Player.Jump;
        jump.Enable();
        jump.performed += Jump;
    }

    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
        jump.Disable(); 
    }

    private void Awake()
    {
        playerInputScript = new NewInputSystemScript();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        moveDirection = move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(moveDirection.x * moveSpeed, 0f, moveDirection.y * moveSpeed), ForceMode.Impulse);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == ((int)groundLayer))
        {
            grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == ((int)groundLayer))
        {
            grounded = false;
        }
    }

    private void Fire(InputAction.CallbackContext context)
    {
        Debug.Log("We Fired");
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (grounded) 
        {
            grounded = false;   
            rb.AddForce(new Vector3(0f, jumpForce, 0f), playerForceMode); 
        }
    }
}