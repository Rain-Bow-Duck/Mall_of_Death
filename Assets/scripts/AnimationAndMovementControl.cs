using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementControl : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    int isWalkingHash;
    int isRunningHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    bool isMovementPressed;
    bool isRunPressed;
    float rotationFactorPerFrame = 7.0f;
    float multiplier = 5.0f;

    void Awake() {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;

    }

    void onRun(InputAction.CallbackContext context){
        isRunPressed = context.ReadValueAsButton();
    }

    void handleRotation(){
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed){
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    void onMovementInput (InputAction.CallbackContext context){
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void handleAnimation(){
        // get parameters
        bool isWalking = animator.GetBool( isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if (isMovementPressed && !isWalking){
            animator.SetBool( isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking){
            animator.SetBool(isWalkingHash, false);
        }

        if((isMovementPressed && !isRunPressed) && !isRunning){
            animator.SetBool(isRunningHash, true);
        }
        else if ((!isMovementPressed || isRunPressed)&& isRunning){
            animator.SetBool(isRunningHash, false);
        }
    }

    void handleGravity(){
        if(characterController.isGrounded){
            float groundedGravity = -0.05f;
            currentMovement.y = groundedGravity;
        } else {
            float gravity = -0.3f;
            currentMovement.y += gravity;
        }
    }


    void Update()
    {
        handleGravity();
        handleRotation();
        handleAnimation();
        

        if (!isRunPressed){
            characterController.Move(currentMovement * multiplier * Time.deltaTime);
        } else {
            characterController.Move(currentMovement * Time.deltaTime);
        }
    }


    void OnEnable() {
        playerInput.CharacterControls.Enable();
    }

    void OnDisable() {
        playerInput.CharacterControls.Disable();
    }
}
