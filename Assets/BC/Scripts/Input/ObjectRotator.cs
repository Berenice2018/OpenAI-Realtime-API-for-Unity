using UnityEngine;

public class ObjectRotator : TouchInputBase
{
    public Transform targetToTransform;
    [SerializeField] bool enableRotation = true;
    [SerializeField] bool enableScaling = false;

    [Header("Config")]
    public float rotationSpeed = 0.2f;
    public float scaleSpeed = 0.01f;
    public float scaleRangeMin = 0.8f;
    public float scaleRangeMax = 4f;

    private Vector2 prevMidPoint;
    private bool isGestureActive = false;

    private Vector3 initialScale;
    private bool _scaledForFirstTime, _rotatedForFirstTime;

    public void Init()
    {
        initialScale = targetToTransform.localScale;
    }

    void Update()
    {
        if (tertiaryPress > 0 && secondaryPress > 0 && firstPress > 0)
            return;
        if (!targetToTransform) 
            return;
        
        if (IsDoubleFingerPress())// && !CanvasHitDetector.IsPointerOverUI()
        {
            Vector2 midPoint = (primaryFingerPosition + secondaryFingerPosition) / 2f;

            if (!isGestureActive)
            {
                prevMidPoint = midPoint;
                isGestureActive = true;
                return; // wait for next frame to calculate deltas
            }

            Vector2 delta = midPoint - prevMidPoint;

            // ROTATION: horizontal movement
            if (enableRotation)
            {
                float horizontalDelta = delta.x;
                targetToTransform.Rotate(0f, horizontalDelta * rotationSpeed, 0f, Space.World);
            }

            // SCALING: vertical movement
            if (enableScaling)
            {
                float verticalDelta = delta.y;
                float newScale = targetToTransform.localScale.x + verticalDelta * scaleSpeed;
                newScale = Mathf.Clamp(newScale, scaleRangeMin, scaleRangeMax);
                targetToTransform.localScale = initialScale * (newScale / initialScale.x);
                
                //did we scale enough?
                //var scaleDelta = initialScale.x - _draggable.localScale.x;
            }
            prevMidPoint = midPoint;
        }
        else
        {
            isGestureActive = false;
        }
    }

    private bool IsDoubleFingerPress()
    {
        return tertiaryPress < 1 && secondaryPress > 0 && firstPress > 0;
    }
}
