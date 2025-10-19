using UnityEngine;
using UnityEngine.InputSystem;

public class ModelDragger : TouchInputBase
{
    [SerializeField] private Camera _cam;
    [SerializeField] private float _dragPixelThreshold = 6f;   // minimal movement in pixels to begin drag
    public float minDistance = 0.15f;
    public float maxDistance = 10.0f;

    private Transform _objectToDrag;

    private bool _armedForDrag = false;
    private bool _isDragging = false;
    private Vector2 _tapScreenPos;
    private Vector3 _modelStartPos;
    private Plane _dragPlane;
    private Vector3 _dragStartWorldOnPlane;

    private bool _fingerWasDownLastFrame = false;

    public void SetObjectToDrag(Transform tfToDrag)
    {
        _objectToDrag = tfToDrag;
    }

    protected override void OnTapStarted(InputAction.CallbackContext ctx)
    {
        base.OnTapStarted(ctx);
        TryArmDrag(tapPosition);
    }

    private void TryArmDrag(Vector2 screenPos)
    {
        _isDragging = false;
        _armedForDrag = false;

        if (!_objectToDrag) return;
        if (!_cam ) _cam = Camera.main;
        if (CanvasHitDetector.IsPointerOverUI()) return;
        if (secondaryPress >= 1f) return; // ignore if multitouch

        Ray ray = _cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, maxDistance))
        {
            if ((hit.transform == _objectToDrag || hit.transform.IsChildOf(_objectToDrag)) && hit.distance >= minDistance)
            {
                _tapScreenPos = screenPos;
                _modelStartPos = _objectToDrag.position;
                _dragPlane = new Plane(Vector3.up, new Vector3(0f, _modelStartPos.y, 0f));

                if (RayToPlane(screenPos, out _dragStartWorldOnPlane))
                    _armedForDrag = true;
            }
        }
    }

    private void Update()
    {
        if (!_objectToDrag) return;

        bool singleFingerDown = firstPress > 0f && secondaryPress < 1f;

        // Detect new touch-down after releasing
        if (singleFingerDown && !_fingerWasDownLastFrame)
        {
            TryArmDrag(primaryFingerPosition);
        }

        // Detect release
        if (!singleFingerDown && _fingerWasDownLastFrame)
        {
            CancelDrag();
        }

        // Cancel when UI touched or second finger appears
        if (CanvasHitDetector.IsPointerOverUI() || secondaryPress >= 1f)
        {
            CancelDrag();
            _fingerWasDownLastFrame = singleFingerDown;
            return;
        }

        if (_armedForDrag)
        {
            if (!_isDragging)
            {
                float pixelDelta = (primaryFingerPosition - _tapScreenPos).magnitude;
                if (pixelDelta >= _dragPixelThreshold)
                    _isDragging = true;
            }

            if (_isDragging && RayToPlane(primaryFingerPosition, out var currentWorldOnPlane))
            {
                Vector3 delta = currentWorldOnPlane - _dragStartWorldOnPlane;
                _objectToDrag.position = _modelStartPos + delta;
            }
        }

        _fingerWasDownLastFrame = singleFingerDown;
    }

    private bool RayToPlane(Vector2 screenPos, out Vector3 worldPoint)
    {
        worldPoint = default;
        if (!_cam) _cam = Camera.main;

        Ray ray = _cam.ScreenPointToRay(screenPos);
        if (_dragPlane.Raycast(ray, out float dist) && dist <= maxDistance)
        {
            worldPoint = ray.GetPoint(dist);
            return true;
        }
        return false;
    }

    private void CancelDrag()
    {
        _isDragging = false;
        _armedForDrag = false;
    }
}
