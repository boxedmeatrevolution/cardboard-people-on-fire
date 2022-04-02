using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;

    [Header("Movement")]
    [SerializeField] public float walkingSpeed = 7.5f;
    [SerializeField] public float runningSpeed = 11.5f;
    [SerializeField] public float jumpSpeed = 8.0f;
    [SerializeField] public float gravity = 20.0f;
    [SerializeField] public float lookSpeed = 2.0f;
    [SerializeField] public float lookXLimit = 45.0f;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode interactionKey = KeyCode.Mouse1;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(sprintKey);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // Interaction stuff
        HandleInteractionCheck();
        HandleInteractionInput();
    }

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;

    private void HandleInteractionCheck()
    {
        Interactable newInteractable = null;
        bool shouldEndCurrentInteraction = false;
        Ray ray = playerCamera.ViewportPointToRay(interactionRayPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject.layer == 9)
            {
                hit.collider.TryGetComponent(out Interactable ihit);
                if (currentInteractable == null || ihit.gameObject.GetInstanceID() != currentInteractable.gameObject.GetInstanceID()) 
                {
                    newInteractable = ihit;
                    shouldEndCurrentInteraction = true;
                }
            }
            else if (hit.collider.gameObject.layer != 9 && currentInteractable) {
                shouldEndCurrentInteraction = true;
            }
        }
        else 
        {
            shouldEndCurrentInteraction = true;
        }

        if (shouldEndCurrentInteraction && currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }

        if (newInteractable) 
        {
            currentInteractable = newInteractable;
            currentInteractable.OnFocus();
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
        {
            currentInteractable.OnInteract();
        }
    }

}