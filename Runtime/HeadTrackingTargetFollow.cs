using UnityEngine;

namespace UnityEssentials
{
    public class HeadTrackingTargetFollow : MonoBehaviour
    {
        [Tooltip("How much we mirror the position when directly behind")]
        [Range(0f, 1f)] public float _weight = 1f;
        [Tooltip("Smoothing speed for position updates")]
        [Range(0f, 1f)] public float _smoothness = 1f;

        [Foldout("Settings")]
        [Tooltip("The target we want to follow")]
        [SerializeField] private Transform _sourceObject;
        [Tooltip("The position and look direction of the source")]
        [SerializeField] private Transform _sourcePosition;
        [SerializeField] private Vector3 _offset;

        [Header("Vertical Constraints")]
        [Tooltip("Minimum vertical angle (degrees)")]
        [Range(-90f, 0f)] public float minVerticalAngle = -30f;
        [Tooltip("Maximum vertical angle (degrees)")]
        [Range(0f, 90f)] public float maxVerticalAngle = 30f;

        private float _smoothingSpeed = 5;
        private Vector3 _smoothedPosition;

        public void Update()
        {
            if (_sourcePosition == null || _sourceObject == null)
                return;

            // Get world position and direction references
            Vector3 worldPos = _sourcePosition.position + _offset;
            Vector3 lookDir = _sourcePosition.forward;

            // Calculate target direction in local space (relative to world position)
            Vector3 toTarget = _sourceObject.position - worldPos;
            Vector3 localTargetPos = toTarget;

            // Calculate horizontal components
            Vector3 toTargetFlat = Vector3.ProjectOnPlane(toTarget, Vector3.up).normalized;
            Vector3 lookDirFlat = Vector3.ProjectOnPlane(lookDir, Vector3.up).normalized;

            // Calculate how much the target is behind us
            float dot = Vector3.Dot(lookDirFlat, toTargetFlat);

            // Start with original position as default
            Vector3 finalLocalPos = localTargetPos;

            // If target is behind (dot < threshold), calculate mirrored position
            const float threshold = 0f;
            if (dot < threshold)
            {
                float mirrorAmount = Mathf.InverseLerp(threshold, -1f, dot) * _weight;
                Vector3 mirroredLocalPos = Vector3.Reflect(localTargetPos, lookDirFlat);
                finalLocalPos = Vector3.Lerp(localTargetPos, mirroredLocalPos, mirrorAmount);
            }

            // Apply vertical constraints before distance normalization
            finalLocalPos = ClampVerticalAngle(finalLocalPos);

            // Normalize distance if too close (using constrained position)
            const float minDistance = 1f;
            if (finalLocalPos.magnitude < minDistance)
                finalLocalPos = finalLocalPos.normalized * minDistance;

            // Convert to world space and apply smoothing
            Vector3 finalWorldPos = worldPos + finalLocalPos;
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, finalWorldPos, _smoothingSpeed * Time.deltaTime);

            transform.position = Vector3.Lerp(_smoothedPosition, finalWorldPos, 1 - _smoothness);
        }

        private Vector3 ClampVerticalAngle(Vector3 localPos)
        {
            // Special case for directly above/below
            if (Mathf.Approximately(localPos.x, 0) && Mathf.Approximately(localPos.z, 0))
            {
                float clampedY = Mathf.Clamp(
                    localPos.y,
                    Mathf.Tan(minVerticalAngle * Mathf.Deg2Rad),
                    Mathf.Tan(maxVerticalAngle * Mathf.Deg2Rad)
                );
                return new Vector3(0, clampedY, 0).normalized * localPos.magnitude;
            }

            // Calculate current angle
            Vector3 horizontal = new Vector3(localPos.x, 0, localPos.z);
            float currentAngle = Mathf.Atan2(localPos.y, horizontal.magnitude) * Mathf.Rad2Deg;

            // Clamp the angle
            float clampedAngle = Mathf.Clamp(currentAngle, minVerticalAngle, maxVerticalAngle);

            // If no adjustment needed
            if (Mathf.Approximately(currentAngle, clampedAngle))
                return localPos;

            // Calculate new position with clamped angle
            float newY = Mathf.Tan(clampedAngle * Mathf.Deg2Rad) * horizontal.magnitude;
            return new Vector3(localPos.x, newY, localPos.z);
        }
    }
}