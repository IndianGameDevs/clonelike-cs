using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// This controller is supposed to have both FPS and Remote Body.
/// Disable Mesh for Remote Body if Local Player, else disable the FPS module
/// </summary>
public class ControlBodyController : NetworkBehaviour
{
    public Animator fpsAnimator;
    public RemoteBodyController remoteBodyController;

    [SerializeField] private CharacterController m_CharacterController;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private Transform m_GroundCheck;

    private bool IsSprinting;
    private bool IsJumping;
    private bool IsCharacterGrounded;


    private InputDataInfo m_ServerInput;

    private Vector3 m_Movement;
    private Vector3 m_VerticalMovement;

    [SerializeField, Range(0.1f, 10.0f)] private float m_CharacterSpeed = 10;
    [SerializeField] private float m_Gravity = 9.8f;

    public Transform verticalHinge;
    public Transform horizontalHinge;

    private float m_Yaw, m_Pitch;
    private float mouseSensitivity = 1000f;
    private Collider[] m_GrouncColliders = new Collider[8];

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!this.IsLocalPlayer)
        {
            m_Camera.gameObject.SetActive(false);
            remoteBodyController.ForceMesh(true);
            // Need to disable FPS Mesh as well.
        }
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            ReadClientInput();
        }
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            ReadMouseInput();
        }
        if (!IsServer) return;

        IsCharacterGrounded = IsGrounded();

        if (!IsCharacterGrounded)
        {
            MovementInAir();
        }
        else
        {
            MovementOnGround();
        }
        fpsAnimator.SetFloat("Movement", m_ServerInput.Movement.magnitude);
        fpsAnimator.SetBool("IsSprinting", Input.GetKey(KeyCode.LeftShift));

        m_CharacterController.Move(m_VerticalMovement * Time.fixedDeltaTime);
    }

    private void MovementInAir()
    {
        m_VerticalMovement.y -= m_Gravity * Time.fixedDeltaTime;
    }

    private void MovementOnGround()
    {
        m_ServerInput.Movement.Normalize();
        m_Movement = verticalHinge.right * m_ServerInput.Movement.x + verticalHinge.forward * m_ServerInput.Movement.y;
        m_CharacterController.Move(m_CharacterSpeed * Time.fixedDeltaTime * m_Movement);
        m_VerticalMovement.y = -0.2f;
    }

    private void ReadClientInput()
    {
        InputDataInfo info = new InputDataInfo()
        {
            Movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")),
            CameraDirection = m_Camera.transform.forward,
            IsJumpPressed = Input.GetKey(KeyCode.Space),
            IsSprinting =Input.GetKey(KeyCode.LeftShift),
            IsShootPressed = Input.GetMouseButton(0)
        };

        SetInput_ServerRpc(info); 
    }

    private void ReadMouseInput()
    {
        float mouseInputX = Input.GetAxis("Mouse X");
        float mouseInputY = Input.GetAxis("Mouse Y");

        m_Yaw += mouseInputX * Time.fixedDeltaTime * mouseSensitivity;
        m_Pitch -= mouseInputY * Time.fixedDeltaTime * mouseSensitivity;
        m_Pitch = Mathf.Clamp(m_Pitch, -90.0f, 90.0f);

        verticalHinge.localRotation = Quaternion.Euler(0, m_Yaw, 0);
        horizontalHinge.localRotation = Quaternion.Euler(m_Pitch, 0, 0);

    }

    private bool IsGrounded()
    {
        Physics.OverlapSphereNonAlloc(m_GroundCheck.position, 0.1f, m_GrouncColliders);
        return m_GrouncColliders.Length > 0;
    }

    #region ServerRPC's
    [ServerRpc]
    private void SetInput_ServerRpc(InputDataInfo info)
    {
        m_ServerInput = info;
    }
    #endregion

    #region DrawGizmos
    private void OnDrawGizmos()
    {
        if (m_GroundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_GroundCheck.position, 0.1f);
        }
    }

    #endregion
}
