using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveController : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private Vector3 m_CurrentVelocity;
    private float m_SpeedVertical = 0f;
    private float m_SpeedHorizontal = 0f;
    float translateSensitivity = 2f;
    float rotationSpeed = 1f;
    float heightSpeed = 2f;
    private float m_Gravity = -9.8f;

    private CharacterController m_CharacterController;

    // Start is called before the first frame update
    void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleHead();
        CalculateMovement();
        HandleHeight();        
    }

    private void HandleHead()
    {
        float mouseHorizontal = Input.GetAxis("Mouse X");
        float rotationY = mouseHorizontal * rotationSpeed + transform.eulerAngles.y;

        float mouseVertical = Input.GetAxis("Mouse Y");
        float rotationX = -mouseVertical * rotationSpeed + transform.eulerAngles.x;

        if (Input.GetMouseButton(2))
        {
            transform.eulerAngles = new Vector3(rotationX, rotationY, transform.eulerAngles.z);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void CalculateMovement()
    {
        m_CurrentVelocity = Vector3.zero;

        // 头朝向哪边，运动的方向就指向哪边
        Vector3 orientationEuler = new Vector3(0, transform.eulerAngles.y, 0);
        Quaternion orientation = Quaternion.Euler(orientationEuler);


        // 确定速度矢量的X分量和Z分量
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        m_SpeedHorizontal = horizontalInput * translateSensitivity;
        m_SpeedVertical = verticalInput * translateSensitivity;

        Vector3 CurrentVelocityXZHorizontal = orientation * (m_SpeedHorizontal * Vector3.right);
        Vector3 CurrentVelocityXZVertical = orientation * (m_SpeedVertical * Vector3.forward);
        m_CurrentVelocity.x = CurrentVelocityXZHorizontal.x + CurrentVelocityXZVertical.x;
        m_CurrentVelocity.z = CurrentVelocityXZHorizontal.z + CurrentVelocityXZVertical.z;

        // 通过重力，确定速度矢量的Y分量
        if (m_CharacterController.isGrounded)  // 如果在地上，则Y轴上速度为零
        {
            m_CurrentVelocity.y = 0;
        }
        else
        {
            m_CurrentVelocity.y = m_CurrentVelocity.y + m_Gravity * Time.deltaTime;
        }

        // moving
        m_CharacterController.Move(m_CurrentVelocity * Time.deltaTime);
    }

    private void HandleHeight()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            this.transform.Translate(Vector3.up * Time.deltaTime * heightSpeed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            this.transform.Translate(-Vector3.up * Time.deltaTime * heightSpeed);
        }
    }
}
