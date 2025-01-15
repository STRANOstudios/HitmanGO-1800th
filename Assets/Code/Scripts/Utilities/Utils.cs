using PathSystem;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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
        return (angle + 360f) % 360f;
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

    /// <summary>
    /// Calculates the slope between two points
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <returns></returns>
    public static float CalculateSlope(Vector3 origin, Vector3 point)
    {
        // normalize
        point -= origin;
        float alpha = Mathf.Atan2(point.z, point.x) * Mathf.Rad2Deg;

        return (alpha + 360f) % 360f;
    }

    /// <summary>
    /// Checks if an angle is within a range
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool IsAngleInRange(float angle, float a, float b)
    {
        angle = NormalizeAngle(angle);

        if (a <= b)
        {
            return angle >= a && angle < b;
        }
        else
        {
            return (angle >= a && angle < 360f) || (angle >= 0f && angle < b);
        }
    }

    /// <summary>
    /// Interacts with a node
    /// </summary>
    /// <param name="currentNode"></param>
    /// <param name="targetNode"></param>
    /// <param name="gameObject"></param>
    public static void NodeInteraction(Node currentNode, Node targetNode, GameObject gameObject)
    {
        currentNode.Storages.Remove(gameObject);
        targetNode.Storages.Add(gameObject);
    }

    /// <summary>
    /// Checks which objects from the provided list are inside a defined 3D box.
    /// </summary>
    /// <typeparam name="T">The type of objects being checked, must inherit from MonoBehaviour.</typeparam>
    /// <param name="boxCenter">The center of the box.</param>
    /// <param name="boxSize">The size (dimensions) of the box.</param>
    /// <param name="objects">The list of objects to check.</param>
    /// <returns>A list of objects that are inside the box.</returns>
    public static List<T> CheckGameObjectsInBox<T>(Vector3 boxCenter, Vector3 boxSize, List<T> objects) where T : MonoBehaviour
    {
        // Initialize the list to store objects inside the box
        List<T> objectsInBox = new();

        // Create bounds based on the given center and size
        Bounds boxBounds = new(boxCenter, boxSize);

        // Iterate through the provided objects
        foreach (var obj in objects)
        {
            // Check if the object's position is within the bounds
            if (boxBounds.Contains(obj.transform.position))
            {
                objectsInBox.Add(obj);
            }
        }

        // Return the list of objects found inside the box
        return objectsInBox;
    }
}
