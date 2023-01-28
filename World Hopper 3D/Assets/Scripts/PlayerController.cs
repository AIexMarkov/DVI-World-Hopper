using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    class AnimatorController
    {
        //Variables
        Transform modelTransform;
        Animator modelAnimator;

        //Constructor
        public AnimatorController(Transform transform)
        {
            modelTransform = transform;
            modelAnimator = transform.gameObject.GetComponent<Animator>();
        }

        //Methods
        public void RotateModel(Vector3 rotation)
        {
            modelTransform.localRotation = Quaternion.Euler(rotation);
        }

        public void SetSpeed(float speed)
        {
            modelAnimator.SetFloat("Speed", speed);
        }

        public void SetDirection(float direction)
        {
            modelAnimator.SetFloat("Direction", direction);
        }

        public void SetJumpTrigger()
        {
            modelAnimator.SetTrigger("Jump");
        }


        public void SetJumpHeight(float jumpHeight)
        {
            modelAnimator.SetFloat("JumpHeight", jumpHeight);
        }

        public void SetGravityControl(float gravityControl)
        {
            modelAnimator.SetFloat("GravityControl", gravityControl);
        }
    }
    
    //Variables
    [Header("Player Movement")]

    [Tooltip("The speed the player moves")]
    [SerializeField]
    private float moveSpeed;

    [Tooltip("The smoothness of player rotation, keep low")]
    [SerializeField] [Range (0f, 1f)]
    private float smoothRotationTime = 0.1f;

    [Space(10)]

    [Header("Player Jumping")]

    [Tooltip("The speed of player jump")]
    [SerializeField]
    private float jumpSpeed;

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

    [Space(10)]

    [Header("Camera Variables")]

    [Tooltip("Related to the camera aim")]
    [Range(2, 100)] 
    [SerializeField] 
    private float cameraTargetDivider;

    [Space(10)]

    [Header("Player Dash")]

    [Tooltip("The speed the player travels when dashing")]
    [SerializeField]
    private float dashSpeed;

    [Tooltip("The distance the player travels when dashing")]
    [SerializeField]
    private float dashDistance;

    [Tooltip("The time it takes after dashing to slow down")]
    [SerializeField] [Range (1.5f, 5f)]
    private float dashSlowdown;

    [Tooltip("The slowest speed of a dash before stopping")]
    [SerializeField]
    [Range(0.1f, 1f)]
    private float dashSpeedLimit;

    [Tooltip("The time the player needs to wait for the dash to be available again if on ground")]
    [SerializeField]
    private float dashCooldown;

    [Space(10)]

    [Header("Player Sliding")]

    [Tooltip("The amount of distance in meters the player moves when sliding")]
    [SerializeField]
    private float slidingDistance;

    [Tooltip("The speed of player movement when sliding")]
    [SerializeField]
    private float slidingSpeed;

    [Tooltip("The maximum amount of momentum the player gains from sliding")]
    [SerializeField]
    private float maximumSlideMomentum;

    [Tooltip("The amount of seconds it needs to pass for the player to slide again")]
    [SerializeField]
    private float slideCooldown;

    //placeholders
    public Image jumpImage;
    public Image dashImage;
    public Image slideImage;
    public TextMeshProUGUI jumpCountText;
    public TextMeshProUGUI dashSecondsText;

    //privats
    private NewInputSystemScript playerInputScript; 
    
    //Vector3s
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 verticalVelocity = Vector3.zero;
    private Vector3 dashingVector = Vector3.zero;
    private Vector3 startSlidePos = Vector3.zero;
    private Vector3 startDashPos = Vector3.zero;

    //other components
    private CharacterController controller;
    private Camera mainCamera;
    private AnimatorController animatorController;
    private CheckpointManager checkpointManager;

    //Transforms
    private Transform mainCameraTransform;
    private Transform cameraAimAt;

    //floats
    private float turnSmoothVelocity;
    private float momentum = 0f;
    private float moveValueForAnimator;
    private float jumpHeightValueForAnimator;
    
    //ints
    private int originalNumberOfJumpsAvailable;
    private int dashSeconds;
    
    //bools
    private bool grounded = true;
    private bool canDash = true;
    private bool dashing = false;
    private bool jumpBoolForAnimator = false;
    private bool canSlide = true;
    private bool sliding = false;
    private bool canRefreshDash = false;

    //input actions
    private InputAction move;
    private InputAction look;
    private InputAction dash;
    private InputAction jump;
    private InputAction slide;
    private InputAction killPlayer;


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

        slide = playerInputScript.Player.Slide;
        slide.Enable();
        slide.performed += Slide;

        killPlayer = playerInputScript.Player.KillPlayer;
        killPlayer.Enable();
        killPlayer.performed += KillPlayer;
    }

    private void OnDisable()
    {
        move.Disable();
        look.Disable();
        dash.Disable();
        jump.Disable(); 
        slide.Disable();
        killPlayer.Disable();
    }

    private void Awake()
    {
        playerInputScript = new NewInputSystemScript();
        controller = GetComponent<CharacterController>();
        mainCameraTransform = GameObject.Find("Main Camera").transform;
        mainCamera = mainCameraTransform.gameObject.GetComponent<Camera>();
        cameraAimAt = GameObject.Find("LookAtMe").transform;
        animatorController = new AnimatorController(GameObject.Find("Animated Model").transform);
        checkpointManager = FindObjectOfType<CheckpointManager>();
    }

    private void Start()
    {
        originalNumberOfJumpsAvailable = jumpsAvailable;
        dashSeconds = Mathf.RoundToInt(dashCooldown);

        Cursor.visible = false;
    }

    private void Update()
    {
        //Where does the camera look at?
        var mousePosition = mainCamera.ScreenToWorldPoint(look.ReadValue<Vector2>());
        var cameraPosition = (mousePosition + (cameraTargetDivider - 1) * transform.position) / cameraTargetDivider;
        cameraAimAt.position = cameraPosition;

        JumpingAndGravity();

        if (!sliding)
        {
            Moving();
            momentum = 0f;
        }
        else if (sliding) Sliding();

        animatorController.SetSpeed(moveDirection.magnitude);

        if (dashing) Dashing();

        //icon placeholders
        if (jumpsAvailable > 0)
        {
            jumpImage.color = new Color(1f, 1f, 1f, 1f);
            jumpCountText.text = jumpsAvailable.ToString();
        }
        else
        {
            jumpImage.color = new Color(1f, 1f, 1f, 0.3f);
            jumpCountText.text = "0";
        }

        if (canDash)
        {
            dashImage.color = new Color(1f, 1f, 1f, 1f);
            dashSecondsText.text = "";
        }
        else
        {
            dashImage.color = new Color(1f, 1f, 1f, 0.3f);
            dashSecondsText.text = dashSeconds.ToString();
        }

        if (canSlide && grounded)
        {
            slideImage.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            slideImage.color = new Color(1f, 1f, 1f, 0.3f);
        }
    }

    private void KillPlayer(InputAction.CallbackContext context)
    {
        checkpointManager.RespawnPlayer();
    }

    private void Dash(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            if (sliding)
            {
                sliding = false;
                animatorController.RotateModel(new Vector3(0f, 0f, 0f));
                canDash = false;
                dashing = true;
                verticalVelocity = Vector3.zero;

                StartCoroutine(DashCooldown());
                Vector3 dashDirection = mainCameraTransform.forward;
                dashDirection.y = 0f;

                dashingVector = dashDirection.normalized * (dashDistance + momentum);
                dashingVector.y = 0f;
                jumpsAvailable = originalNumberOfJumpsAvailable;
            }
            else
            {
                canDash = false;
                dashing = true;
                verticalVelocity = Vector3.zero;

                StartCoroutine(DashCooldown());
                Vector3 dashDirection = mainCameraTransform.forward;
                dashDirection.y = 0f;

                dashingVector = dashDirection.normalized * dashDistance;
                dashingVector.y = 0f;
                jumpsAvailable = originalNumberOfJumpsAvailable;
            }

            startDashPos = transform.position;
            startDashPos.y = 0f;
        }
    }

    private void Dashing()
    {
        Vector3 playerPosNoY = new Vector3(transform.position.x, 0f, transform.position.z);
        var dashTravelled = playerPosNoY - startDashPos;

        if (dashTravelled.magnitude <= dashDistance + momentum)
        {
            controller.Move(dashingVector * dashSpeed * Time.deltaTime);
            //dashingVector = Vector3.Lerp(dashingVector, Vector3.zero, dashSlowdown * Time.deltaTime);
        }
        else
        {
            dashingVector = Vector3.zero;
            dashing = false;
        }
    }

    public void ResetDash()
    {
        dashSeconds = Mathf.RoundToInt(dashCooldown);
        canDash = true;
    }

    private void Slide(InputAction.CallbackContext context)
    {
        if (canSlide && grounded)
        {
            sliding = true;
            canSlide = false;
            StartCoroutine(SlideCooldown());
            startSlidePos = transform.position;
        }
    }

    private void Sliding()
    {
        Vector3 stirDirection = new Vector3(move.ReadValue<Vector2>().x / 3f, 0f, 0f);

        transform.forward = mainCamera.transform.forward;
        controller.Move((transform.forward + stirDirection).normalized * slidingSpeed * Time.deltaTime);
        momentum = Mathf.Lerp(0f, maximumSlideMomentum, ((transform.position - startSlidePos).magnitude) / slidingDistance);
        animatorController.RotateModel(new Vector3(-60f, 0f, 0f));

        if ((transform.position - startSlidePos).magnitude >= slidingDistance)
        {
            sliding = false;
            animatorController.RotateModel(new Vector3(0f, 0f, 0f));
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (jumpsAvailable > 0)
        {
            if (sliding)
            {
                sliding = false;
                animatorController.RotateModel(new Vector3(0f, 0f, 0f));
                verticalVelocity.y = jumpSpeed + momentum;
                controller.Move(verticalVelocity * Time.deltaTime);
                jumpsAvailable--;
                jumpBoolForAnimator = true;
            }
            else
            {
                verticalVelocity.y = jumpSpeed;
                controller.Move(verticalVelocity * Time.deltaTime);
                jumpsAvailable--;
                jumpBoolForAnimator = true;
            }
            
            if (originalNumberOfJumpsAvailable - jumpsAvailable == 1) animatorController.SetJumpTrigger();
        }
    }

    private void JumpingAndGravity()
    {
        grounded = Physics.CheckSphere(feet.position, groundCheckRadius, groundLayer);

        if (grounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -10f;
            jumpsAvailable = originalNumberOfJumpsAvailable;
            if (canRefreshDash)
            {
                canRefreshDash = false;
                dashSeconds = Mathf.RoundToInt(dashCooldown);
                canDash = true;
            }
            //jumpBoolForAnimator = false;
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
            moveValueForAnimator = (moveDir.normalized * moveSpeed * Time.deltaTime).magnitude;
        }
    }

    public bool ReturnDashing()
    {
        return dashing;
    }

    //Coroutines

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(1f);
        dashSeconds--;
        if (dashSeconds > 0)
        {
            StartCoroutine(DashCooldown());
        }
        else if (!canDash)
        {
            canRefreshDash = true;
        }
    }

    IEnumerator SlideCooldown()
    {
        yield return new WaitForSeconds(slideCooldown);
        canSlide = true;
    }
}