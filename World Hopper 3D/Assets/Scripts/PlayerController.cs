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

        bool isInAir = false;

        //Constructor
        public AnimatorController(Transform transform)
        {
            modelTransform = transform;
            modelAnimator = transform.gameObject.GetComponent<Animator>();
        }

        //Methods
        public void ReadPlayerGroundInput(Vector2 moveValue)
        {
            if (moveValue.magnitude <= 0.1f)
            {
                modelAnimator.SetBool("Start Running", false);
                modelAnimator.SetBool("Go Idle", true);
            }
            else
            {
                modelAnimator.SetBool("Go Idle", false);
                modelAnimator.SetBool("Start Running", true);
            }
        }

        public void AnimateJump()
        {
            modelAnimator.SetTrigger("Jump");
            modelAnimator.SetBool("Go Idle", false);
            modelAnimator.SetBool("Start Running", false);
            isInAir = true;
        }

        public void AnimateFall(bool value)
        {
            modelAnimator.SetBool("Fall", value);
        }

        public void GroundedTrigger(bool value)
        {
            modelAnimator.SetBool("Grounded", value);
            isInAir = false;
        }

        public bool IsInAir()
        {
            return isInAir;
        }

        public void DashAnim()
        {
            modelAnimator.SetTrigger("Dash");
        }

        public void StartSliding()
        {
            modelAnimator.SetTrigger("Slide");
            modelAnimator.SetBool("Sliding", true);
        }

        public void StopSliding()
        {
            modelAnimator.SetBool("Sliding", false);
        }
    }

    //Variables
    [Header("Player Health")]

    [Tooltip("The number of hits the player takes before dying")]
    [SerializeField]
    private int playerHitPoints = 1;

    [Space(10)]

    [Header("Player Movement")]

    [Tooltip("The speed the player moves")]
    [SerializeField]
    private float moveSpeed;

    [Tooltip("The smoothness of player rotation, keep low")]
    [SerializeField] [Range(0f, 1f)]
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
    [Range(1, 5)]
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

    //[Tooltip("Related to the camera aim")]
    //[Range(2, 100)] 
    //[SerializeField] 
    //private float cameraTargetDivider;
    [Tooltip("How fast the player and the camera turn left and right")]
    [SerializeField]
    private float turningSpeedX = 0.3f;

    [Tooltip("How fast the player and the camera turn up and down")]
    [SerializeField]
    private float turningSpeedY = 0.2f;

    [Space(10)]

    [Header("Player Dash")]

    [Tooltip("The speed the player travels when dashing")]
    [SerializeField]
    private float dashSpeed;

    [Tooltip("The distance the player travels when dashing")]
    [SerializeField]
    private float dashDistance;

    [Tooltip("The time it takes after dashing to slow down")]
    [SerializeField] [Range(1.5f, 5f)]
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
    public Image hpImage;
    public TextMeshProUGUI jumpCountText;
    public TextMeshProUGUI dashSecondsText;
    public TextMeshProUGUI hpText;

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
    private Transform cameraPivot;

    //floats
    private float turnSmoothVelocity;
    private float momentum = 0f;
    private float moveValueForAnimator;
    private float jumpHeightValueForAnimator;


    //ints
    private int originalNumberOfJumpsAvailable;
    private int dashSeconds;
    private int maxHP;

    //bools
    private bool grounded = true;
    private bool canDash = true;
    private bool dashing = false;
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
    private InputAction freelook;


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

        freelook = playerInputScript.Player.Freelook;
        freelook.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        look.Disable();
        dash.Disable();
        jump.Disable();
        slide.Disable();
        killPlayer.Disable();
        freelook.Disable();
    }

    private void Awake()
    {
        playerInputScript = new NewInputSystemScript();

        controller = GetComponent<CharacterController>();

        mainCameraTransform = GameObject.Find("Main Camera").transform;
        mainCamera = mainCameraTransform.gameObject.GetComponent<Camera>();
        cameraPivot = transform.Find("CameraFreelookPivot");

        animatorController = new AnimatorController(GameObject.Find("Animated Model").transform);

        checkpointManager = FindObjectOfType<CheckpointManager>();

        maxHP = playerHitPoints;
    }

    private void Start()
    {
        originalNumberOfJumpsAvailable = jumpsAvailable;
        dashSeconds = Mathf.RoundToInt(dashCooldown);

        Cursor.visible = false;

        hpImage.color = new Color(1f, 0.35f, 0.24f, playerHitPoints / maxHP);
        hpText.text = playerHitPoints.ToString();
    }

    private void Update()
    {
        JumpingAndGravity();

        if (!sliding)
        {
            Moving();
            momentum = 0f;
        }
        else if (sliding) Sliding();

        if (dashing) Dashing();

        #region UI Icons Temp
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
        #endregion

        var moveValue = move.ReadValue<Vector2>();

        if (grounded)
        {
            animatorController.ReadPlayerGroundInput(moveValue);
            animatorController.GroundedTrigger(true);
        }
        else
        {
            animatorController.GroundedTrigger(false);
        }
    }

    private void KillPlayer(InputAction.CallbackContext context)
    {
        checkpointManager.RespawnPlayer();
    }

    public void TakeDamage(int damage)
    {
        playerHitPoints -= damage;
        if (playerHitPoints <= 0)
        {
            playerHitPoints = maxHP;
            if(checkpointManager != null) checkpointManager.RespawnPlayer();
        }

        float playersHpDivided = maxHP / playerHitPoints;
        hpImage.color = new Color(1f, 0.35f, 0.24f, playersHpDivided);
        hpText.text = playerHitPoints.ToString();
    }

    private void Dash(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            if (sliding)
            {
                sliding = false;
                canDash = false;
                dashing = true;
                verticalVelocity = Vector3.zero;

                StartCoroutine(DashCooldown());
                Vector3 dashDirection = transform.forward;
                dashDirection.y = 0f;

                dashingVector = dashDirection.normalized * (dashDistance + momentum);
                dashingVector.y = 0f;
                jumpsAvailable = originalNumberOfJumpsAvailable;

                animatorController.DashAnim();
                animatorController.StopSliding();
            }
            else
            {
                canDash = false;
                dashing = true;
                verticalVelocity = Vector3.zero;

                StartCoroutine(DashCooldown());
                Vector3 dashDirection = transform.forward;
                dashDirection.y = 0f;

                dashingVector = dashDirection.normalized * dashDistance;
                dashingVector.y = 0f;
                jumpsAvailable = originalNumberOfJumpsAvailable;

                animatorController.DashAnim();
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

            animatorController.StartSliding();
        }
    }

    private void Sliding()
    {
        Vector3 stirDirection = new Vector3(move.ReadValue<Vector2>().x / 3f, 0f, 0f);

        controller.Move((transform.forward + stirDirection).normalized * slidingSpeed * Time.deltaTime);
        momentum = Mathf.Lerp(0f, maximumSlideMomentum, ((transform.position - startSlidePos).magnitude) / slidingDistance);

        if ((transform.position - startSlidePos).magnitude >= slidingDistance)
        {
            sliding = false;
            animatorController.StopSliding();
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (jumpsAvailable > 0)
        {
            if (sliding)
            {
                sliding = false;
                verticalVelocity.y = jumpSpeed + momentum;
                controller.Move(verticalVelocity * Time.deltaTime);
                jumpsAvailable--;

                animatorController.AnimateJump();
                animatorController.StopSliding();
            }
            else
            {
                verticalVelocity.y = jumpSpeed;
                controller.Move(verticalVelocity * Time.deltaTime);
                jumpsAvailable--;

                animatorController.AnimateJump();
            }
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

            animatorController.AnimateFall(false);
        }
        else
        {
            verticalVelocity.y -= gravity * Time.deltaTime;

            if (!animatorController.IsInAir() && jumpsAvailable == originalNumberOfJumpsAvailable) 
            { 
                animatorController.AnimateFall(true);
            }
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