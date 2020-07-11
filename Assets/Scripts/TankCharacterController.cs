using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankCharacterController : MonoBehaviour
{
    public static TankCharacterController Instance;

    [Header("Setup")]
    public GameObject m_CameraObject;
    public GameObject m_VisualElements;

    [Header("Movement")]
    public float m_MovementSpeed = 10f;
    public float m_Acceleration = 1f;
    public float m_VisualRotationSpeed = 20f;

    [Header("Camera")]
    public float m_CameraSensitivity = 5f;
    public float m_CameraSmoothing = 2f;
    public float m_CameraZoomSensitivity = 2f;
    public float m_CameraZoomSmoothing = 2f;
    public float m_MinZoom = 1f;
    public float m_MaxZoom = 5f;
    public float m_CameraMaxPitch = 89f;
    public float m_CameraMinPitch = 25;

    private Quaternion m_CurrentVisualRotationTarget;
    private Vector3 m_InputMovementVector;
    private Vector2 m_CameraAngle = new Vector2(45f, 0f);
    private Vector2 m_CameraTargetAngle;
    private Quaternion m_CameraRotation;
    private Vector3 m_CameraVector;
    private float m_CameraZoom;
    private float m_CameraTargetZoom;
    private Quaternion m_CurrentVisualRotation;

    private Rigidbody m_Rigidbody;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        m_Rigidbody = this.GetComponent<Rigidbody>();
        m_CameraZoom = (m_MinZoom + m_MaxZoom) * 0.35f;
        m_CameraTargetZoom = m_CameraZoom;
        m_CameraTargetAngle = m_CameraAngle;
    }

    private void Update()
    {
        this.ProcessInput();
        this.ProcessCamera();
        this.ProcessVisualElements();
    }

    private void FixedUpdate()
    {
        this.ProcessMovement();
    }

    private void ProcessInput()
    {
        var forward = Input.GetAxis("Vertical");
        var right = Input.GetAxis("Horizontal");

        var cameraRotation2D = Quaternion.Euler(new Vector3(0f, m_CameraAngle.y, 0f));
        var forwardVector = cameraRotation2D * Vector3.forward;
        var rightVector = cameraRotation2D * Vector3.right;

        m_InputMovementVector = forwardVector * forward + rightVector * right;
        m_InputMovementVector = m_InputMovementVector.normalized;
        m_InputMovementVector.Scale(new Vector3(m_MovementSpeed, m_MovementSpeed, m_MovementSpeed));

        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");
        var mouseScroll = -Input.GetAxis("Mouse ScrollWheel");

        m_CameraTargetAngle.x -= mouseY * m_CameraSensitivity;
        m_CameraTargetAngle.y += mouseX * m_CameraSensitivity;
        m_CameraTargetZoom = Mathf.Clamp(m_CameraTargetZoom + mouseScroll * m_CameraZoomSensitivity, m_MinZoom, m_MaxZoom);
        m_CameraZoom = Mathf.Lerp(m_CameraZoom, m_CameraTargetZoom, Time.deltaTime * m_CameraZoomSmoothing);

        m_CameraTargetAngle.x = Mathf.Clamp(m_CameraTargetAngle.x, m_CameraMinPitch, m_CameraMaxPitch);
        m_CameraAngle = Vector2.Lerp(m_CameraAngle, m_CameraTargetAngle, Time.deltaTime * m_CameraSmoothing);
    }

    private void ProcessCamera()
    {
        m_CameraRotation = Quaternion.Euler(new Vector3(m_CameraAngle.x, m_CameraAngle.y, 0f));
        m_CameraVector = m_CameraRotation * Vector3.forward;

        m_CameraObject.transform.localPosition = -m_CameraVector * m_CameraZoom;
        m_CameraObject.transform.rotation = m_CameraRotation;
    }

    private void ProcessVisualElements()
    {
        var direction = m_Rigidbody.velocity;
        direction.y = 0f;
        direction = direction.normalized;

        if (direction.magnitude > 0f)
        {
            m_CurrentVisualRotationTarget = Quaternion.LookRotation(direction);
        }

        if (m_CurrentVisualRotation == m_CurrentVisualRotationTarget)
        {
            return;
        }

        var degreeDiff = Quaternion.Angle(m_CurrentVisualRotationTarget, m_CurrentVisualRotation);

        m_CurrentVisualRotation = Quaternion.RotateTowards(m_CurrentVisualRotation, m_CurrentVisualRotationTarget, degreeDiff * Time.deltaTime * m_VisualRotationSpeed);
        m_VisualElements.transform.rotation = m_CurrentVisualRotation;
    }

    private void ProcessMovement()
    {
        var velocity = m_Rigidbody.velocity;
        var diff = m_InputMovementVector - velocity;

        var adjustAmount = Mathf.Min(diff.magnitude, m_Acceleration);
        var adjustVelocity = diff.normalized * adjustAmount;

        m_Rigidbody.AddForce(adjustVelocity, ForceMode.VelocityChange);
    }
}
