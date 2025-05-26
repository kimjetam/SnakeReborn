using UnityEngine;

public static class VectorHelper
{
    public static bool ArePointsCollinear(Vector3 A, Vector3 B, Vector3 C, float epsilon = 1e-6f)
    {
        return Vector3.Cross(B - A, C - A).sqrMagnitude < epsilon;
    }

    public static bool TryGetCircleIntersectionBelow(Vector3 A3, Vector3 B3, Vector3 C3, out Vector3 result)
    {
        result = Vector3.zero;

        // Project into XZ plane
        Vector2 A = new Vector2(A3.x, A3.z);
        Vector2 B = new Vector2(B3.x, B3.z);
        Vector2 C = new Vector2(C3.x, C3.z);

        float d = Vector2.Distance(A, C);      // distance between centers
        float radius = d; // radius from A to B

        if (d > 2f * radius || d == 0f)
            return false; // Circles too far apart or same center

        Vector2 mid = (A + C) * 0.5f;
        float a = d / 2f;
        float h = Mathf.Sqrt(radius * radius - a * a);
        Vector2 dir = (C - A).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        Vector2 p1 = mid + perp * h;
        Vector2 p2 = mid - perp * h;

        float Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            return (q.x - p.x) * (r.y - p.y) - (q.y - p.y) * (r.x - p.x);
        }

        float orientABC = Orientation(A, C, B);
        float orientP1 = Orientation(A, C, p1);
        float orientP2 = Orientation(A, C, p2);

        Vector2 chosen2D = (Mathf.Sign(orientP1) != Mathf.Sign(orientABC)) ? p1 :
                           (Mathf.Sign(orientP2) != Mathf.Sign(orientABC)) ? p2 :
                           p1;

        // Interpolate Y from A3 to C3 using projection factor t
        Vector2 AC = C - A;
        Vector2 AP = chosen2D - A;
        float t = (AC.sqrMagnitude > 0f) ? Vector2.Dot(AP, AC) / AC.sqrMagnitude : 0f;
        t = Mathf.Clamp01(t);

        float y = Mathf.Lerp(A3.y, C3.y, t);
        result = new Vector3(chosen2D.x, y, chosen2D.y);

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
        float chosenT;
        if (t1 >= 0 && (t1 <= t2 || t2 < 0))
            chosenT = t1;
        else if (t2 >= 0)
            chosenT = t2;
        else
        {
            intersection3D = Vector3.zero;
            return false; // both behind
        }

        Vector2 intersection2D = linePoint + dir * chosenT;

        // Convert back to 3D (XZ plane only, Y stays from original line point)
        intersection3D = new Vector3(intersection2D.x, linePoint3D.y, intersection2D.y);
        return true;
    }
}
