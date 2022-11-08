using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Variables
    [Header("Player Movement")]
    [Tooltip("The speed the player moves")]
    [SerializeField]
    private float moveSpeed;

    [Tooltip("The smoothness of player rotation, keep low")]
    [SerializeField] [Range (0f, 1f)]
    private float smoothRotationTime = 0.1f;

    [Tooltip("The force of player jump")]
    [SerializeField]
    private float jumpForce;

    [Tooltip("The player uses it's own, independent gravity, and doesn't interact with Rigidbodies")]
    [SerializeField]
    private float gravity;

    [Tooltip("The amount of jumps the player has")]
    [Range (1, 5)]
    public int jumpsAvailable = 2;

    [Tooltip("The Feet position, relevant for ground checks and jumping")]
    [SerializeField]
    private Transform feet;

    [Tooltip("How Far Do The Legs check for ground")]
    [SerializeField]
    private float groundCheckRadius = 0.4f;

    [Tooltip("What Layer does the player consider to be ground")]
    [SerializeField]
    private LayerMask groundLayer;

    [Tooltip("Related to the camera aim")]
    [Range(2, 100)] 
    [SerializeField] 
    private float cameraTargetDivider;

    [Tooltip("The Distance the player travels when dashing")]
    [SerializeField]
    private float dashDistance;

    [Tooltip("The time the player needs to wait for the dash to be available again")]
    [SerializeField]
    private float dashCooldown;

    //privats
    private NewInputSystemScript playerInputScript; 
    
    //Vector3s
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 verticalVelocity = Vector3.zero;
    
    //other components
    private CharacterController controller;
    private Camera mainCamera;

    //Transforms
    private Transform mainCameraTransform;
    private Transform cameraAimAt;
    
    //floats
    private float turnSmoothVelocity;
    
    //ints
    private int originalNumberOfJumpsAvailable;
    
    //bools
    private bool grounded = true;
    private bool canDash = true;

    //input actions
    private InputAction move;
    private InputAction look;
    private InputAction dash;
    private InputAction jump;


    //Methods
    private void OnEnable()
    {
        move = playerInputScript.Player.Move;
        move.Enable();

        look = playerInputScript.Player.Look;
        look.Enable();

        dash = playerInputScript.Player.Dash;
        dash.Enable();
        dash.performed += Dash;

        jump = playerInputScript.Player.Jump;
        jump.Enable();
        jump.performed += Jump;
    }

    private void OnDisable()
    {
        move.Disable();
        look.Disable();
        dash.Disable();
        jump.Disable(); 
    }

    private void Awake()
    {
        playerInputScript = new NewInputSystemScript();
        controller = GetComponent<CharacterController>();
        mainCameraTransform = GameObject.Find("Main Camera").transform;
        mainCamera = mainCameraTransform.gameObject.GetComponent<Camera>();
        cameraAimAt = GameObject.Find("LookAtMe").transform;
    }

    private void Start()
    {
        originalNumberOfJumpsAvailable = jumpsAvailable;

        Cursor.visible = false;
    }

    private void Update()
    {
        //Where does the camera look at?
        var mousePosition = mainCamera.ScreenToWorldPoint(look.ReadValue<Vector2>());
        var cameraPosition = (mousePosition + (cameraTargetDivider - 1) * transform.position) / cameraTargetDivider;
        cameraAimAt.position = cameraPosition;

        JumpingAndGravity();
        Moving();
    }


    private void Dash(InputAction.CallbackContext context)
    {
        //Reminder: add dash time
        if (canDash)
        {
            canDash = false;
            StartCoroutine(DashCooldown());
            Vector3 dashDirection = mainCameraTransform.forward;
            dashDirection.y = 0f;
            controller.Move(dashDirection.normalized * dashDistance);
            jumpsAvailable = originalNumberOfJumpsAvailable;
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (jumpsAvailable > 0)
        {
            verticalVelocity.y = jumpForce;
            controller.Move(verticalVelocity * Time.deltaTime);
            jumpsAvailable--;
        }
    }

    private void JumpingAndGravity()
    {
        grounded = Physics.CheckSphere(feet.position, groundCheckRadius, groundLayer);

        if (grounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -10f;
            jumpsAvailable = originalNumberOfJumpsAvailable;
        }
        else
        {
            verticalVelocity.y -= gravity * Time.deltaTime;
        }
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void Moving()
    {
        moveDirection = new Vector3(move.ReadValue<Vector2>().x, 0f, move.ReadValue<Vector2>().y).normalized;
        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothRotationTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }
    }

    //Coroutines

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}