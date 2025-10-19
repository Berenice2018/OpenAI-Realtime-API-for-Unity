
using System.Collections;

namespace BC.Scripts.AR
{
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    public class ModelPlacer : MonoBehaviour
    {
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] Transform _targetToPlace;
        private Transform _camTF;
        private bool _modelPlaced, _isReady;

        void OnEnable()
        {
            _camTF = Camera.main.transform;
            
            planeManager.planesChanged += OnPlanesChanged;
        }

        void OnDisable()
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }

        void Start()
        {
            StartCoroutine(WaitForARReady());
        }

        IEnumerator WaitForARReady()
        {
            while (ARSession.state < ARSessionState.SessionTracking)
                yield return null;
            _isReady = true;
        }

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            if (_modelPlaced || !_isReady) return;

            foreach (var plane in args.added)
            {
                // Check if it’s a horizontal plane (floor or table)
                if (plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    PlaceModelOnFloor(plane);
                    _modelPlaced = true;
                    break;
                }
            }
        }

        private void PlaceModelOnFloor(ARPlane floorPlane)
        {
            // Calculate position 2 meters in front of camera
            Vector3 forward = _camTF.forward;
            forward.y = -floorPlane.transform.position.y; // flatten so we stay level with the floor
            forward.Normalize();

            Vector3 position = _camTF.position + forward * 2f;

            // Snap the y-position to the detected plane height
            position.y = floorPlane.transform.position.y;

            _targetToPlace.position = position;
                
            // Get the plane's normal vector in world space
            Vector3 planeNormal = floorPlane.transform.up;

            // Create a rotation that aligns the object's up-axis (Y) with the plane normal,
            // and makes it face the camera direction projected onto the plane.
            Vector3 projectedForward = Vector3.ProjectOnPlane(-forward, planeNormal).normalized;
            Quaternion rotation = Quaternion.LookRotation(projectedForward, planeNormal);

            _targetToPlace.rotation = rotation;
            _targetToPlace.gameObject.SetActive(true);
        }
    }

}