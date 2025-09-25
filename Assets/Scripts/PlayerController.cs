using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[SelectionBase]
public class PlayerController : MonoBehaviour
{

    public static PlayerController instance;

    [SerializeField] Transform cameraTransform;

    private CharacterController cc;
    private PlayerInput pInput;

    [Header("Speeds")]
    [SerializeField] float sensitivity = 1;
    [SerializeField] float moveSpeed = 1;

    [Header("Angle Clamp")]
    [SerializeField] float minAngle;
    [SerializeField] float maxAngle;

    void Start()
    {
        instance = this;
        cc = GetComponent<CharacterController>();
        pInput = GetComponent<PlayerInput>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Movement
        if (CanMove)
        {
            Vector3 movement = (new Vector3
            {
                x = pInput.Movement.x,
                y = 0,
                z = pInput.Movement.y,
            }).normalized * moveSpeed;

            movement.y -= 10f * Time.deltaTime;
            movement = transform.TransformDirection(movement);
            cc.Move(movement * Time.deltaTime);
        }


        // Mouse
        if (CanLook)
        {
            Vector3 euler = transform.localEulerAngles;
            euler.y += pInput.Mouse.x * sensitivity;
            transform.localEulerAngles = euler;

            euler = cameraTransform.eulerAngles;
            euler.x -= pInput.Mouse.y * sensitivity;
            euler.x = Extensions.ClampAngle(euler.x, minAngle, maxAngle);
            cameraTransform.eulerAngles = euler;
        }

    }

    public bool CanMove = true;
    public bool CanLook = true;

}
