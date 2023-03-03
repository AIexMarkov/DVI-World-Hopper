using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraMouseInputController : MonoBehaviour
{
	private ThirdPerson_Camera _thirdPersonCamera;
	public float MouseXSensitivity = 5f;                    // Mouse X sensitivity.
	public float MouseYSensitivity = 5f;                    // Mouse Y sensitivity.
	public float MouseWheelSensitivity = 5f;                // Mouse wheel/scroll sensitivity.

	private NewInputSystemScript inputSystem;

	InputAction look;
	InputAction freelook;
	InputAction zoom;

    private void OnEnable()
    {
		look = inputSystem.Player.Look;
		look.Enable();

		freelook = inputSystem.Player.Freelook;
		freelook.Enable();

		zoom = inputSystem.Player.Zoom;
		zoom.Enable();
    }

    private void OnDisable()
    {
		look.Disable();

		freelook.Disable();

		zoom.Disable();
    }

    private void Awake()
    {
        inputSystem = new NewInputSystemScript();
	}

	private void Start()
	{
		if (!_thirdPersonCamera)
			_thirdPersonCamera = GetComponent<ThirdPerson_Camera>();
	}

	private void LateUpdate()
	{
		HandlePlayerInput();
	}

	private void HandlePlayerInput()
	{
		var deadZone = 0.01f;

		// If right mouse button is down, get mouse axis input.
		//if (freelook.inProgress)
		//{
		_thirdPersonCamera.MouseX += look.ReadValue<Vector2>().x * MouseXSensitivity;
		_thirdPersonCamera.MouseY -= look.ReadValue<Vector2>().y * MouseYSensitivity;
		//}

		// Clamp (limit) mouse Y rotation. Uses thirdPersonCameraHelper.cs.
		_thirdPersonCamera.MouseY = ThirdPerson_Helper.ClampingAngle(_thirdPersonCamera.MouseY,
																	 _thirdPersonCamera.YMinLimit,
																	 _thirdPersonCamera.YMaxLimit
		);

		// Clamp (limit) mouse scroll wheel.
		if (zoom.ReadValue<Vector2>().y > deadZone || zoom.ReadValue<Vector2>().y < -deadZone)
		{
			_thirdPersonCamera.DesiredDistance = Mathf.Clamp(_thirdPersonCamera.Distance -
				zoom.ReadValue<Vector2>().y *
				MouseWheelSensitivity,
															 _thirdPersonCamera.DistanceMin,
															 _thirdPersonCamera.DistanceMax
			);
			_thirdPersonCamera.PreOccludedDistance = _thirdPersonCamera.DesiredDistance;
			_thirdPersonCamera.DistanceCameraSmooth = _thirdPersonCamera.DistanceSmooth;
		}
	}
}