using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetector : MonoBehaviour
{
    [SerializeField] private InputActionReference _dragDeltaAction;
    [SerializeField] private float _minSwipeDistance = 100f;
    [SerializeField] private GameObject _imageQuad;

    private Vector2 _accumulatedDelta;
    private Vector2 _swipeStartScreenPos;
    private bool _swipeDetected;
    private bool _isSwipeOverImageQuad;

    public static event Action<SwipeData> OnSwipe = delegate { };

    private void OnEnable()
    {
        _dragDeltaAction.action.performed += OnDragPerformed;
        _dragDeltaAction.action.started += OnSwipeStart;
        _dragDeltaAction.action.Enable();
    }

    private void OnDisable()
    {
        _dragDeltaAction.action.performed -= OnDragPerformed;
        _dragDeltaAction.action.started -= OnSwipeStart;
        _dragDeltaAction.action.Disable();
    }

    private void OnSwipeStart(InputAction.CallbackContext context)
    {
        _accumulatedDelta = Vector2.zero;
        _swipeDetected = false;
        _swipeStartScreenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        _isSwipeOverImageQuad = IsSwipeStartingOverImageQuad(_swipeStartScreenPos);
    }

    private void OnDragPerformed(InputAction.CallbackContext context)
    {
        if (!_isSwipeOverImageQuad) return;

        Vector2 delta = context.ReadValue<Vector2>();
        _accumulatedDelta += delta;

        if (_swipeDetected) return;

        if (_accumulatedDelta.magnitude >= _minSwipeDistance)
        {
            Vector2 swipe = _accumulatedDelta;
            _accumulatedDelta = Vector2.zero;
            _swipeDetected = true;

            SwipeDirection direction = Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y)
                ? (swipe.x > 0 ? SwipeDirection.Right : SwipeDirection.Left)
                : (swipe.y > 0 ? SwipeDirection.Up : SwipeDirection.Down);

            OnSwipe?.Invoke(new SwipeData
            {
                StartPosition = _swipeStartScreenPos,
                EndPosition = _swipeStartScreenPos + swipe,
                Direction = direction
            });

            Invoke(nameof(ResetSwipeDetection), 0.2f);
        }
    }

    private bool IsSwipeStartingOverImageQuad(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider != null && hit.collider.gameObject == _imageQuad;
        }
        return false;
    }

    private void ResetSwipeDetection()
    {
        _swipeDetected = false;
    }
}


public struct SwipeData
{
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public SwipeDirection Direction;
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right
}