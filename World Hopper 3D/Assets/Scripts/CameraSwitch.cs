using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraSwitch : MonoBehaviour
{
    public CinemachineFreeLook freelookCamera;

    private NewInputSystemScript playerInputScript;
    private InputAction move;

    private float oldXSpeed;

    private void OnEnable()
    {
        move = playerInputScript.Player.Move;
        move.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
    }

    private void Awake()
    {
        playerInputScript = new NewInputSystemScript();
        oldXSpeed = freelookCamera.m_XAxis.m_MaxSpeed;
    }

    private void Update()
    {
        var moveDirection = new Vector3(move.ReadValue<Vector2>().x, 0f, move.ReadValue<Vector2>().y);

        if (moveDirection.magnitude >= 0.1f)
        {
            freelookCamera.m_XAxis.m_MaxSpeed = 0f;
        }
        else
        {
            freelookCamera.m_XAxis.m_MaxSpeed = oldXSpeed;
        }
    }
}
