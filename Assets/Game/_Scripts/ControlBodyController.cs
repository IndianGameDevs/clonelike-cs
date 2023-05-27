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

    private bool IsSprinting;
    private bool IsJumping;


    private InputDataInfo m_ServerInput;

    private Vector3 m_Movement;
    private Vector3 m_VerticalMovement;

    [SerializeField, Range(0.1f, 10.0f)] private float m_CharacterSpeed = 1.7f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!this.IsLocalPlayer)
        {
            m_Camera.gameObject.SetActive(false);
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
        if (!IsServer) return;
        m_ServerInput.Movement.Normalize();
        m_Movement = transform.right * m_ServerInput.Movement.x + transform.forward * m_ServerInput.Movement.y;
        m_CharacterController.Move(m_CharacterSpeed * Time.fixedDeltaTime * m_Movement);
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

    #region ServerRPC's
    [ServerRpc]
    private void SetInput_ServerRpc(InputDataInfo info)
    {
        m_ServerInput = info;
    }
    #endregion
}
