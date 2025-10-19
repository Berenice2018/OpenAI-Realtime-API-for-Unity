
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static ARInputActions;

public abstract class TouchInputBase : MonoBehaviour
{
    protected ARInputActions _inputActions;

    protected ARActionMapActions mapActions;
    protected Vector2 tapPosition,  primaryFingerPosition, secondaryFingerPosition, tertiaryFingerPosition;
    protected float firstPress, secondaryPress, tertiaryPress;


    protected virtual void Awake()
    {
        _inputActions = new ARInputActions();
        mapActions = _inputActions.ARActionMap;
    }



    protected virtual void OnEnable()
    {
        _inputActions.Enable();
        InputSystem.EnableDevice(Touchscreen.current);

        mapActions.Tap.started += OnTapStarted;
        mapActions.PrimaryTouchPress.performed += OnPrimaryPressPerformed;
        mapActions.PrimaryTouchHold.performed += OnPrimaryTouchPosition;

        mapActions.SecondaryTouchPress.performed += OnSecondaryPressPerformed;
        mapActions.SecondaryTouchHold.performed += OnSecondaryTouchPosition;

        mapActions.TertiaryTouchPress.performed += OnTertiaryPressPerformed;
        mapActions.TertiaryTouchHold.performed += OnTertiaryTouchPosition;
    }

    protected virtual void OnTapStarted(InputAction.CallbackContext ctx)
    {
        tapPosition = ctx.ReadValue<Vector2>();
        //Debug.Log("### OnTapStarted " + tapPosition);
    }

    protected virtual void OnPrimaryPressPerformed(InputAction.CallbackContext ctx)
    {
        firstPress = ctx.ReadValue<float>();
        //Debug.Log("### 1st Press performed ");
    }

    protected virtual void OnSecondaryPressPerformed(InputAction.CallbackContext ctx)
    {
        secondaryPress = ctx.ReadValue<float>();
        //Debug.Log("### 2nd Press performed " + secondaryPress);
    }

    protected virtual void OnTertiaryPressPerformed(InputAction.CallbackContext ctx)
    {
        tertiaryPress = ctx.ReadValue<float>();
        //Debug.Log("### 3rd Press performed " + tertiaryPress);
    }

    protected virtual void OnPrimaryTouchPosition(InputAction.CallbackContext context)
    {
        primaryFingerPosition = context.ReadValue<Vector2>();
        //Debug.Log("### 1st Touch Position. primaryFingerPosition " + primaryFingerPosition);
    }

    protected virtual void OnSecondaryTouchPosition(InputAction.CallbackContext ctx)
    {
        secondaryFingerPosition = ctx.ReadValue<Vector2>();
        //Debug.Log("### 2nd Touch Position performed " + secondaryFingerPosition);
    }
    protected virtual void OnTertiaryTouchPosition(InputAction.CallbackContext ctx)
    {
       tertiaryFingerPosition = ctx.ReadValue<Vector2>();
        //Debug.Log("### 3rd Touch Position performed. tertiaryFingerPosition " + tertiaryFingerPosition);
    }

    protected virtual void OnDisable()
    {
        mapActions.Tap.started -= OnTapStarted;
        mapActions.PrimaryTouchPress.performed -= OnPrimaryPressPerformed;
        mapActions.PrimaryTouchHold.performed -= OnPrimaryTouchPosition;
        mapActions.SecondaryTouchPress.performed -= OnSecondaryPressPerformed;
        mapActions.SecondaryTouchHold.performed -= OnSecondaryTouchPosition;
        mapActions.TertiaryTouchPress.performed -= OnTertiaryPressPerformed;
        mapActions.TertiaryTouchHold.performed -= OnTertiaryTouchPosition;

        _inputActions.Disable();
    }
}
