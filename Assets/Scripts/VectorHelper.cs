using UnityEngine;

public static class VectorHelper
{
    public static bool ArePointsCollinear(Vector3 A, Vector3 B, Vector3 C, float epsilon = 1e-6f)
    {
        return Vector3.Cross(B - A, C - A).sqrMagnitude < epsilon;
    }

    public static bool IsOnGrid(Vector3 position)
    {
        return Mathf.Round(position.x * 2) % 2 == 0 && Mathf.Round(position.z * 2) % 2 == 0;
    }

    public static Vector3 RoundToHalf(Vector3 original)
    {
        return new Vector3(
            Mathf.Round(original.x * 2) / 2,
            original.y,
            Mathf.Round(original.z * 2) / 2
        );
    }
}
