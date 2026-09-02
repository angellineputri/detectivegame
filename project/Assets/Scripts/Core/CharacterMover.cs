using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterMover
{
    public const float MinSegDuration = 0.3f;

    public struct Obstacle
    {
        public readonly Vector2 Position;
        public readonly float Radius;

        public Obstacle(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public static Obstacle FromTransform(Transform t, float radius)
        {
            if (t != null)
            {
                return new Obstacle((Vector2)t.position, radius);
            }
            return new Obstacle(new Vector2(999999f, 999999f), radius);
        }
    }

    public static IEnumerator Walk(Transform obj, Vector3 destination, float speed, params Obstacle[] obstacles)
    {
        Vector3[] path = ComputePath(obj.position, destination, obstacles);
        yield return ExecutePath(obj, path, speed);
    }

    public static IEnumerator Walk(Transform obj, Vector3 destination, float speed, Transform[] obstacleTransforms, float obstacleRadius)
    {
        yield return Walk(obj, destination, speed, FromTransforms(obstacleTransforms, obstacleRadius));
    }

    public static IEnumerator WalkAndDeactivate(Transform obj, Vector3 destination, float speed, params Obstacle[] obstacles)
    {
        yield return Walk(obj, destination, speed, obstacles);
        obj.gameObject.SetActive(false);
    }

    public static IEnumerator WalkPath(Transform obj, List<Vector2> path, float speed)
    {
        if (path == null || path.Count < 2)
            yield break;

        Vector3[] waypoints = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
            waypoints[i] = new Vector3(path[i].x, path[i].y, obj.position.z);

        float startDelta = Vector2.Distance((Vector2)obj.position, path[0]);
        Debug.Log("[CharacterMover] WalkPath '" + obj.name + "': actual=" + (Vector2)obj.position + " first waypoint=" + path[0] + " delta=" + startDelta.ToString("F3") + "u");

        yield return ExecutePath(obj, waypoints, speed);
    }

    public static IEnumerator WalkPathAndDeactivate(Transform obj, List<Vector2> path, float speed)
    {
        yield return WalkPath(obj, path, speed);
        obj.gameObject.SetActive(false);
    }

    public static Obstacle[] FromTransforms(Transform[] transforms, float radius)
    {
        if (transforms == null) return new Obstacle[0];

        Obstacle[] result = new Obstacle[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            result[i] = Obstacle.FromTransform(transforms[i], radius);
        }
        return result;
    }

    static IEnumerator ExecutePath(Transform obj, Vector3[] waypoints, float speed)
    {
        Debug.Log("[CharacterMover] ExecutePath '" + obj.name + "' — " + (waypoints.Length - 1) + " segment(s)");

        Animator animator = obj.GetComponent<Animator>();
        Vector2 lastDirection = Vector2.zero;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 from = waypoints[i];
            Vector3 to   = waypoints[i + 1];
            float dist   = Vector3.Distance(from, to);

            if (dist < 0.001f) continue;

            Vector2 direction = ((Vector2)(to - from)).normalized;
            lastDirection = direction;

            float dur     = Mathf.Max(dist / speed, MinSegDuration);
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                obj.position = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / dur));
                if (animator != null)
                {
                    animator.SetFloat("MoveX", direction.x);
                    animator.SetFloat("MoveY", direction.y);
                    animator.SetFloat("Speed", 1f);
                }
                yield return null;
            }

            obj.position = to;
        }

        if (animator != null)
        {
            if (lastDirection != Vector2.zero)
            {
                animator.SetFloat("MoveX", lastDirection.x);
                animator.SetFloat("MoveY", lastDirection.y);
            }
            animator.SetFloat("Speed", 0f);
        }

        Debug.Log("[CharacterMover] ExecutePath '" + obj.name + "' done.");
    }

    static Vector3[] ComputePath(Vector3 start, Vector3 destination, Obstacle[] obstacles)
    {
        if (obstacles == null || obstacles.Length == 0 || IsSegmentClear(start, destination, obstacles))
        {
            return new Vector3[] { start, destination };
        }

        Vector3 hWaypoint = new Vector3(destination.x, start.y,       start.z);
        Vector3 vWaypoint = new Vector3(start.x,       destination.y, start.z);

        Vector3[] hFirst = new Vector3[] { start, hWaypoint, destination };
        Vector3[] vFirst = new Vector3[] { start, vWaypoint, destination };

        bool hOk = IsPathClear(hFirst, obstacles);
        bool vOk = IsPathClear(vFirst, obstacles);

        if (hOk && vOk)
        {
            if (PathLength(hFirst) <= PathLength(vFirst))
                return hFirst;
            return vFirst;
        }
        if (hOk) return hFirst;
        if (vOk) return vFirst;

        return Detour3(start, destination, obstacles);
    }

    static Vector3[] Detour3(Vector3 start, Vector3 destination, Obstacle[] obstacles)
    {
        const float margin = 0.5f;

        float yHi = float.MinValue;
        float yLo = float.MaxValue;
        float xHi = float.MinValue;
        float xLo = float.MaxValue;

        foreach (Obstacle obs in obstacles)
        {
            yHi = Mathf.Max(yHi, obs.Position.y + obs.Radius + margin);
            yLo = Mathf.Min(yLo, obs.Position.y - obs.Radius - margin);
            xHi = Mathf.Max(xHi, obs.Position.x + obs.Radius + margin);
            xLo = Mathf.Min(xLo, obs.Position.x - obs.Radius - margin);
        }

        int n = obstacles.Length;
        float[] byYs = new float[4 + n * 2];
        float[] byXs = new float[4 + n * 2];

        byYs[0] = yHi;  byYs[1] = yLo;  byYs[2] = start.y;  byYs[3] = destination.y;
        byXs[0] = xHi;  byXs[1] = xLo;  byXs[2] = start.x;  byXs[3] = destination.x;

        for (int i = 0; i < n; i++)
        {
            float r = obstacles[i].Radius + margin;
            byYs[4 + i * 2]     = obstacles[i].Position.y + r;
            byYs[4 + i * 2 + 1] = obstacles[i].Position.y - r;
            byXs[4 + i * 2]     = obstacles[i].Position.x + r;
            byXs[4 + i * 2 + 1] = obstacles[i].Position.x - r;
        }

        Vector3[] best = null;
        float bestLen  = float.MaxValue;

        foreach (float byY in byYs)
        {
            Vector3[] path = new Vector3[]
            {
                start,
                new Vector3(start.x,       byY, start.z),
                new Vector3(destination.x, byY, start.z),
                destination
            };

            if (IsPathClear(path, obstacles))
            {
                float len = PathLength(path);
                if (len < bestLen)
                {
                    bestLen = len;
                    best = path;
                }
            }
        }

        foreach (float byX in byXs)
        {
            Vector3[] path = new Vector3[]
            {
                start,
                new Vector3(byX, start.y,       start.z),
                new Vector3(byX, destination.y, start.z),
                destination
            };

            if (IsPathClear(path, obstacles))
            {
                float len = PathLength(path);
                if (len < bestLen)
                {
                    bestLen = len;
                    best = path;
                }
            }
        }

        foreach (Obstacle obs in obstacles)
        {
            if (DistToSegment(obs.Position, start, destination) >= obs.Radius) continue;

            float side = obs.Radius + margin;
            Vector3[] bypassPoints = new Vector3[]
            {
                new Vector3(obs.Position.x - side, start.y,               start.z),
                new Vector3(obs.Position.x + side, start.y,               start.z),
                new Vector3(start.x,               obs.Position.y + side, start.z),
                new Vector3(start.x,               obs.Position.y - side, start.z),
            };

            foreach (Vector3 bp in bypassPoints)
            {
                if (!IsSegmentClear(start, bp, obstacles)) continue;

                Vector3[] tail = SubPath(bp, destination, obstacles);
                if (tail == null) continue;

                Vector3[] path = new Vector3[tail.Length + 1];
                path[0] = start;
                for (int i = 0; i < tail.Length; i++)
                {
                    path[i + 1] = tail[i];
                }

                if (IsPathClear(path, obstacles))
                {
                    float len = PathLength(path);
                    if (len < bestLen)
                    {
                        bestLen = len;
                        best = path;
                    }
                }
            }
        }

        if (best != null) return best;

        Vector3[] leastBad  = null;
        float     bestScore = float.MinValue;

        foreach (float byY in byYs)
        {
            Vector3[] path = new Vector3[]
            {
                start,
                new Vector3(start.x,       byY, start.z),
                new Vector3(destination.x, byY, start.z),
                destination
            };

            float score = MinClearance(path, obstacles);
            if (score > bestScore)
            {
                bestScore = score;
                leastBad = path;
            }
        }

        foreach (float byX in byXs)
        {
            Vector3[] path = new Vector3[]
            {
                start,
                new Vector3(byX, start.y,       start.z),
                new Vector3(byX, destination.y, start.z),
                destination
            };

            float score = MinClearance(path, obstacles);
            if (score > bestScore)
            {
                bestScore = score;
                leastBad = path;
            }
        }

        Debug.LogWarning("[CharacterMover] No clear path found — using best available.");

        if (leastBad != null) return leastBad;
        return new Vector3[] { start, destination };
    }

    static float MinClearance(Vector3[] path, Obstacle[] obstacles)
    {
        float min = float.MaxValue;
        for (int i = 0; i < path.Length - 1; i++)
        {
            foreach (Obstacle obs in obstacles)
            {
                float clearance = DistToSegment(obs.Position, path[i], path[i + 1]) - obs.Radius;
                if (clearance < min)
                {
                    min = clearance;
                }
            }
        }
        return min;
    }

    static Vector3[] SubPath(Vector3 a, Vector3 b, Obstacle[] obstacles)
    {
        if (IsSegmentClear(a, b, obstacles)) return new Vector3[] { a, b };

        Vector3[] hF = new Vector3[] { a, new Vector3(b.x, a.y, a.z), b };
        Vector3[] vF = new Vector3[] { a, new Vector3(a.x, b.y, a.z), b };

        bool hOk = IsPathClear(hF, obstacles);
        bool vOk = IsPathClear(vF, obstacles);

        if (hOk && vOk)
        {
            if (PathLength(hF) <= PathLength(vF)) return hF;
            return vF;
        }
        if (hOk) return hF;
        if (vOk) return vF;
        return null;
    }

    static bool IsSegmentClear(Vector3 a, Vector3 b, Obstacle[] obstacles)
    {
        foreach (Obstacle obs in obstacles)
        {
            if (DistToSegment(obs.Position, a, b) < obs.Radius)
            {
                return false;
            }
        }
        return true;
    }

    static bool IsPathClear(Vector3[] path, Obstacle[] obstacles)
    {
        for (int i = 0; i < path.Length - 1; i++)
        {
            if (!IsSegmentClear(path[i], path[i + 1], obstacles))
            {
                return false;
            }
        }
        return true;
    }

    static float PathLength(Vector3[] points)
    {
        float len = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            len += Vector3.Distance(points[i], points[i + 1]);
        }
        return len;
    }

    static float DistToSegment(Vector2 point, Vector3 segA3, Vector3 segB3)
    {
        Vector2 segA  = (Vector2)segA3;
        Vector2 segB  = (Vector2)segB3;
        Vector2 seg   = segB - segA;
        float   lenSq = seg.sqrMagnitude;

        if (lenSq < 0.0001f) return Vector2.Distance(point, segA);

        float t = Mathf.Clamp01(Vector2.Dot(point - segA, seg) / lenSq);
        return Vector2.Distance(point, segA + t * seg);
    }
}
