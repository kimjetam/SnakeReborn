using UnityEngine;

public static class VectorHelper
{
    public static bool ArePointsCollinear(Vector3 A, Vector3 B, Vector3 C, float epsilon = 1e-6f)
    {
        return Vector3.Cross(B - A, C - A).sqrMagnitude < epsilon;
    }

    public static bool TryGetCircleIntersectionBelow(Vector2 A, Vector2 C, Vector2 B, out Vector2 result)
    {
        result = Vector2.zero;

        float radius = Vector2.Distance(A, C);
        float d = Vector2.Distance(A, C);

        if (d > 2 * radius || d == 0f)
            return false; // Circles too far apart or same center

        Vector2 mid = (A + C) * 0.5f;
        float a = d / 2f;
        float h = Mathf.Sqrt(radius * radius - a * a);
        Vector2 dir = (C - A).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        Vector2 p1 = mid + perp * h;
        Vector2 p2 = mid - perp * h;

        // Calculate signed area / orientation function helper:
        float Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            return (q.x - p.x) * (r.y - p.y) - (q.y - p.y) * (r.x - p.x);
        }

        // Find orientation of triangle ABC
        float orientABC = Orientation(A, C, B);

        // Find orientation of intersection points relative to AC
        float orientP1 = Orientation(A, C, p1);
        float orientP2 = Orientation(A, C, p2);

        // Choose the point on the opposite side of B relative to AC
        if (Mathf.Sign(orientP1) != Mathf.Sign(orientABC))
            result = p1;
        else if (Mathf.Sign(orientP2) != Mathf.Sign(orientABC))
            result = p2;
        else
            // Both points are on the same side (should not happen if circles intersect properly)
            result = p1;

        return true;
    }

    public static bool TryGetClosestLineCircleIntersection(Vector3 linePoint3D, Vector3 lineTo3D, Vector3 circleCenter3D, float radius, out Vector3 intersection3D)
    {
        // Convert to 2D (XZ plane)
        Vector2 linePoint = new Vector2(linePoint3D.x, linePoint3D.z);
        Vector2 lineTo = new Vector2(lineTo3D.x, lineTo3D.z);
        Vector2 circleCenter = new Vector2(circleCenter3D.x, circleCenter3D.z);

        Vector2 dir = (lineTo - linePoint).normalized;
        Vector2 diff = linePoint - circleCenter;

        float a = Vector2.Dot(dir, dir); // Should be 1
        float b = 2 * Vector2.Dot(dir, diff);
        float c = Vector2.Dot(diff, diff) - radius * radius;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            intersection3D = Vector3.zero;
            return false;
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b - sqrtDiscriminant) / (2 * a);
        float t2 = (-b + sqrtDiscriminant) / (2 * a);

        // Find the closer t value (smaller positive t)
        float chosenT = (Mathf.Abs(t1) < Mathf.Abs(t2)) ? t1 : t2;

        Vector2 intersection2D = linePoint + dir * chosenT;

        // Convert back to 3D (XZ plane only, Y stays from original line point)
        intersection3D = new Vector3(intersection2D.x, linePoint3D.y, intersection2D.y);
        return true;
    }
}
