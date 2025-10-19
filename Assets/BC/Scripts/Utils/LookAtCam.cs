using UnityEngine;

namespace BC.Scripts.Utils
{
    public class LookAtCam : MonoBehaviour
    {
        public static Quaternion FaceCameraAndKeepHorizontalRotation(
            Transform camTf, Transform tfToRotate, Vector3 eulerOffset)
        {
            // Calculate the direction to the camera, ignoring vertical component
            var directionToCamera = (camTf.position - tfToRotate.position).normalized;
            directionToCamera.y = 0; // Keep only horizontal rotation
    
            // Calculate the rotation to face the camera
            var targetRotation = Quaternion.LookRotation(-directionToCamera);
    
            // Apply the Euler offset to the target rotation
            targetRotation *= Quaternion.Euler(eulerOffset);

            return targetRotation;
        }

    }
}
