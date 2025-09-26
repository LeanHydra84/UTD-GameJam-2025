using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

[SelectionBase]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]
public class EyeController : MonoBehaviour
{

    public static EyeController Instance;

    const float Gravity = -9.8f;

    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float jumpForce = 1.0f;

    private PlayerInput pInput;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private float viewBobScale;
    private Vector3 defaultCameraPosition;

    [SerializeField] private Vector2 angleMinMax = new Vector2(-80, 80);

    private CharacterController cc;

    private Vector3 velocity;

    void Start()
    {
        Instance = this;
        pInput = GetComponent<PlayerInput>();
        cc = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        defaultCameraPosition = playerCamera.transform.localPosition;
    }

    void Update()
    {

        // Movement
        Vector3 translation = new Vector3(pInput.Movement.x, 0, pInput.Movement.y).normalized * (pInput.Sprint ? speed * sprintMultiplier : speed);
        translation = transform.TransformDirection(translation);

        // Gravity
        velocity.y += Gravity * Time.deltaTime;
        if (cc.isGrounded)
        {
            // Ground stop / Jump
            velocity = new Vector3(0, velocity.y, 0);
            if (velocity.y < 0) velocity.y = Gravity;
            if (pInput.Jump)
            {
                velocity.y = jumpForce;
                velocity += translation;
            }
        }
        else translation *= 0.25f;

        // Apply velocity
        cc.Move(translation * Time.deltaTime);
        cc.Move(velocity * Time.deltaTime);

        //View X
        Vector3 temp = transform.localEulerAngles;
        temp.y += pInput.Mouse.x;
        transform.localEulerAngles = temp;

        //View Y
        temp = playerCamera.transform.localEulerAngles;
        temp.x -= pInput.Mouse.y;
        temp.x = Extensions.ClampAngle(temp.x, angleMinMax.x, angleMinMax.y);
        playerCamera.transform.localEulerAngles = temp;

        // View bob
        if (translation != Vector3.zero)
        {
            Vector3 cameraOffset = new Vector3(Mathf.Sin(Time.time * 6), Mathf.Cos(Time.time * 12)) * viewBobScale;
            playerCamera.transform.localPosition = defaultCameraPosition + cameraOffset;
        }
        else if(playerCamera.transform.position != defaultCameraPosition)
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, defaultCameraPosition, 4 * Time.deltaTime);

    }


}
