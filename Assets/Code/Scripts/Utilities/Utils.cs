using System.Reflection;
using UnityEngine;

public static class Utils
{
    /// <summary>
    /// Rotates a transform to look at a target m_position, but only on the allowed axes.
    /// </summary>
    /// <param name="transform">Transform to rotate.</param>
    /// <param name="targetPosition">Target m_position.</param>
    /// <param name="allowedAxes">The axes allowed for movement (e.g. Vector3(1, 1, 0) for X and Y only).</param>
    public static void LookAtWithAxes(Transform transform, Vector3 targetPosition, Vector3 allowedAxes)
    {
        // Calculate the direction
        Vector3 direction = targetPosition - transform.position;

        // Limit the direction to the allowed axes
        direction = Vector3.Scale(direction, allowedAxes);

        // If the direction is too small, don't rotate
        if (direction.sqrMagnitude < Mathf.Epsilon) return;

        // Calculate the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }

    /// <summary>
    /// Clears the log entries
    /// </summary>
    public static void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
        var type = assembly.GetType("UnityEditorInternal.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    /// <summary>
    /// Normalizes the angle to be between 0 and 360 degrees
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }

    /// <summary>
    /// Calculates the intersection of a ray and a radial vector
    /// </summary>
    /// <param name="nodeRay"></param>
    /// <param name="origin"></param>
    /// <param name="radialVector"></param>
    /// <returns></returns>
    public static Vector3 CalculateRayIntersection(Ray nodeRay, Vector3 origin, Vector3 radialVector)
    {
        Vector3 radialDirection = radialVector.normalized;

        float t = Vector3.Dot((origin - nodeRay.origin), radialDirection) / Vector3.Dot(nodeRay.direction, radialDirection);

        Vector3 intersection = nodeRay.origin + nodeRay.direction * t;

        return intersection;
    }
}
