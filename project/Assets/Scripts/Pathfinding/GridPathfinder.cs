using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridPathfinder : MonoBehaviour
{
    public static GridPathfinder Instance { get; private set; }

    public bool showDebugGrid = true;

    bool[,] _walkable;
    int _gridWidth;
    int _gridHeight;
    int _originX;
    int _originY;
    bool _ready;
    bool[,] _walkableOverridden;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        BuildGrid();
    }

    void BuildGrid()
    {
        List<Tilemap> obstacleMaps = new List<Tilemap>();

        GameObject wallsAndFloors = GameObject.Find("WallsAndFloors");
        if (wallsAndFloors != null)
        {
            foreach (Tilemap tm in wallsAndFloors.GetComponentsInChildren<Tilemap>())
            {
                if (tm.gameObject.name == "Walls")
                    obstacleMaps.Add(tm);
            }
        }

        GameObject bordersAndFurnitures = GameObject.Find("BordersAndFurnitures");
        if (bordersAndFurnitures != null)
        {
            foreach (Tilemap tm in bordersAndFurnitures.GetComponentsInChildren<Tilemap>())
                obstacleMaps.Add(tm);
        }

        if (obstacleMaps.Count == 0)
        {
            return;
        }

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (Tilemap tm in obstacleMaps)
        {
            BoundsInt b = tm.cellBounds;
            if (b.xMin < minX) minX = b.xMin;
            if (b.yMin < minY) minY = b.yMin;
            if (b.xMax > maxX) maxX = b.xMax;
            if (b.yMax > maxY) maxY = b.yMax;
        }

        _originX = minX;
        _originY = minY;
        _gridWidth = maxX - minX;
        _gridHeight = maxY - minY;

        _walkable = new bool[_gridWidth, _gridHeight];

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                _walkable[x, y] = true;
            }
        }

        foreach (Tilemap tm in obstacleMaps)
        {
            BoundsInt b = tm.cellBounds;
            int tilesBlocked = 0;
            for (int cx = b.xMin; cx < b.xMax; cx++)
            {
                for (int cy = b.yMin; cy < b.yMax; cy++)
                {
                    if (tm.GetTile(new Vector3Int(cx, cy, 0)) != null)
                    {
                        int gx = cx - _originX;
                        int gy = cy - _originY;
                        if (gx >= 0 && gx < _gridWidth && gy >= 0 && gy < _gridHeight)
                        {
                            _walkable[gx, gy] = false;
                            tilesBlocked++;
                        }
                    }
                }
            }
        }

        int tileBlockedCount = 0;
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                if (!_walkable[x, y])
                {
                    tileBlockedCount++;
                }
            }
        }

_walkableOverridden = new bool[_gridWidth, _gridHeight];
        int overrideCount = 0;

        GameObject walkablePathGO = GameObject.Find("WalkablePath");
        Tilemap walkablePath = walkablePathGO != null ? walkablePathGO.GetComponent<Tilemap>() : null;

        if (walkablePath != null)
        {
            TilemapRenderer walkableRenderer = walkablePath.GetComponent<TilemapRenderer>();
            if (walkableRenderer != null)
            {
                walkableRenderer.enabled = false;
            }

            BoundsInt wb = walkablePath.cellBounds;
            for (int cx = wb.xMin; cx < wb.xMax; cx++)
            {
                for (int cy = wb.yMin; cy < wb.yMax; cy++)
                {
                    if (walkablePath.GetTile(new Vector3Int(cx, cy, 0)) != null)
                    {
                        int gx = cx - _originX;
                        int gy = cy - _originY;
                        if (gx >= 0 && gx < _gridWidth && gy >= 0 && gy < _gridHeight)
                        {
                            if (!_walkable[gx, gy])
                            {
                                _walkable[gx, gy] = true;
                                _walkableOverridden[gx, gy] = true;
                                overrideCount++;
                            }
                        }
                    }
                }
            }

        }

        PunchCollisionHoles(obstacleMaps);

        _ready = true;
    }

    void PunchCollisionHoles(List<Tilemap> obstacleMaps)
    {
        HashSet<CompositeCollider2D> compositesToRebake = new HashSet<CompositeCollider2D>();

        foreach (Tilemap tm in obstacleMaps)
        {
            TilemapCollider2D tilemapCollider = tm.GetComponent<TilemapCollider2D>();
            if (tilemapCollider == null || !tilemapCollider.usedByComposite) continue;

            CompositeCollider2D composite = tm.GetComponent<CompositeCollider2D>();

            int holesCount = 0;
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (!_walkableOverridden[x, y]) continue;

                    Vector3Int cellPos = new Vector3Int(x + _originX, y + _originY, 0);
                    if (tm.GetTile(cellPos) == null) continue;

                    Tile visualOnly = ScriptableObject.CreateInstance<Tile>();
                    visualOnly.sprite = tm.GetSprite(cellPos);
                    visualOnly.color = tm.GetColor(cellPos);
                    visualOnly.transform = tm.GetTransformMatrix(cellPos);
                    visualOnly.colliderType = Tile.ColliderType.None;

                    tm.SetTile(cellPos, visualOnly);
                    holesCount++;
                }
            }

            if (holesCount > 0 && composite != null)
            {
                compositesToRebake.Add(composite);
            }
        }

        foreach (CompositeCollider2D composite in compositesToRebake)
        {
            composite.GenerateGeometry();
        }
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 end, Vector2? dynamicObstacle = null)
    {
        if (!_ready)
        {
            return new List<Vector2> { start, end };
        }

        Vector2Int startCell = WorldToGrid(start);
        Vector2Int endCell = WorldToGrid(end);

startCell.x = Mathf.Clamp(startCell.x, 0, _gridWidth - 1);
        startCell.y = Mathf.Clamp(startCell.y, 0, _gridHeight - 1);
        endCell.x = Mathf.Clamp(endCell.x, 0, _gridWidth - 1);
        endCell.y = Mathf.Clamp(endCell.y, 0, _gridHeight - 1);

        bool startWalkable = _walkable[startCell.x, startCell.y];
        bool endWalkable = _walkable[endCell.x, endCell.y];

        if (!startWalkable)
        {
            startCell = FindNearestWalkable(startCell);
        }

        if (!endWalkable)
        {
            endCell = FindNearestWalkable(endCell);
        }

        bool dynamicBlocked = false;
        Vector2Int obstacleCell = new Vector2Int(-1, -1);
        if (dynamicObstacle.HasValue)
        {
            obstacleCell = WorldToGrid(dynamicObstacle.Value);
            obstacleCell.x = Mathf.Clamp(obstacleCell.x, 0, _gridWidth - 1);
            obstacleCell.y = Mathf.Clamp(obstacleCell.y, 0, _gridHeight - 1);
            bool isStartOrEnd = (obstacleCell == startCell) || (obstacleCell == endCell);
            if (!isStartOrEnd && _walkable[obstacleCell.x, obstacleCell.y])
            {
                _walkable[obstacleCell.x, obstacleCell.y] = false;
                dynamicBlocked = true;
            }
        }

        List<Vector2> rawPath = RunAStar(startCell, endCell);

        List<Vector2> simplified = SimplifyPath(rawPath);

        if (startWalkable && simplified.Count > 0)
        {
            simplified[0] = start;
        }

        if (endWalkable && simplified.Count > 1)
        {
            simplified[simplified.Count - 1] = end;
        }

        if (simplified.Count == 2)
        {
        }

        if (dynamicBlocked)
        {
            _walkable[obstacleCell.x, obstacleCell.y] = true;
        }

        return simplified;
    }

    List<Vector2> RunAStar(Vector2Int start, Vector2Int end)
    {
        Node[,] nodes = new Node[_gridWidth, _gridHeight];
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                nodes[x, y] = new Node(x, y);
            }
        }

        List<Node> openSet = new List<Node>();

        Node startNode = nodes[start.x, start.y];
        startNode.gCost = 0f;
        startNode.hCost = Heuristic(start, end);
        openSet.Add(startNode);
        startNode.inOpen = true;

        int[] dx = { -1,  0,  1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1,  0, 0,  1, 1, 1 };
        float[] stepCost = { 1.414f, 1f, 1.414f, 1f, 1f, 1.414f, 1f, 1.414f };

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < current.FCost || (openSet[i].FCost == current.FCost && openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }

            openSet.Remove(current);
            current.inOpen = false;
            current.inClosed = true;

            if (current.x == end.x && current.y == end.y)
                return ReconstructPath(current);

            for (int i = 0; i < 8; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx < 0 || nx >= _gridWidth || ny < 0 || ny >= _gridHeight) continue;
                if (!_walkable[nx, ny]) continue;

                bool isDiagonal = dx[i] != 0 && dy[i] != 0;
                if (isDiagonal)
                {
                    if (!_walkable[current.x + dx[i], current.y]) continue;
                    if (!_walkable[current.x, current.y + dy[i]]) continue;
                }

                Node neighbor = nodes[nx, ny];
                if (neighbor.inClosed) continue;

                float newG = current.gCost + stepCost[i];
                if (newG < neighbor.gCost)
                {
                    neighbor.gCost = newG;
                    neighbor.hCost = Heuristic(new Vector2Int(nx, ny), end);
                    neighbor.parent = current;

                    if (!neighbor.inOpen)
                    {
                        openSet.Add(neighbor);
                        neighbor.inOpen = true;
                    }
                }
            }
        }

        return new List<Vector2> { GridToWorld(start.x, start.y), GridToWorld(end.x, end.y) };
    }

    List<Vector2> ReconstructPath(Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = endNode;
        while (current != null)
        {
            path.Add(GridToWorld(current.x, current.y));
            current = current.parent;
        }
        path.Reverse();
        return path;
    }

    List<Vector2> SimplifyPath(List<Vector2> path)
    {
        if (path.Count <= 2)
            return path;

        List<Vector2> simplified = new List<Vector2>();
        simplified.Add(path[0]);

        int from = 0;
        while (from < path.Count - 1)
        {
            int to = path.Count - 1;
            while (to > from + 1)
            {
                if (HasLineOfSight(path[from], path[to]))
                    break;
                to--;
            }
            from = to;
            simplified.Add(path[from]);
        }

        return simplified;
    }

    bool HasLineOfSight(Vector2 from, Vector2 to)
    {
        Vector2Int fromCell = WorldToGrid(from);
        Vector2Int toCell = WorldToGrid(to);

        int x = fromCell.x;
        int y = fromCell.y;
        int x1 = toCell.x;
        int y1 = toCell.y;

        int dx = Mathf.Abs(x1 - x);
        int dy = Mathf.Abs(y1 - y);
        int sx = x < x1 ? 1 : -1;
        int sy = y < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight) return false;
            if (!_walkable[x, y]) return false;
            if (x == x1 && y == y1) return true;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
    }

    float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) + (1.414f - 2f) * Mathf.Min(dx, dy);
    }

    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x) - _originX,
            Mathf.FloorToInt(worldPos.y) - _originY
        );
    }

    Vector2 GridToWorld(int gx, int gy)
    {
        return new Vector2(_originX + gx + 0.5f, _originY + gy + 0.5f);
    }

    Vector2Int FindNearestWalkable(Vector2Int cell)
    {
        for (int radius = 1; radius < Mathf.Max(_gridWidth, _gridHeight); radius++)
        {
            for (int ox = -radius; ox <= radius; ox++)
            {
                for (int oy = -radius; oy <= radius; oy++)
                {
                    if (Mathf.Abs(ox) != radius && Mathf.Abs(oy) != radius) continue;
                    int nx = cell.x + ox;
                    int ny = cell.y + oy;
                    if (nx >= 0 && nx < _gridWidth && ny >= 0 && ny < _gridHeight && _walkable[nx, ny])
                        return new Vector2Int(nx, ny);
                }
            }
        }
        return cell;
    }

    void OnDrawGizmos()
    {
        if (!showDebugGrid || !_ready) return;

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                Vector3 center = new Vector3(_originX + x + 0.5f, _originY + y + 0.5f, 0f);

                if (_walkableOverridden != null && _walkableOverridden[x, y])
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
                    Gizmos.DrawCube(center, Vector3.one);
                    Gizmos.color = new Color(0f, 1f, 0f, 0.9f);
                    Gizmos.DrawWireCube(center, Vector3.one);
                }
                else if (!_walkable[x, y])
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
                    Gizmos.DrawCube(center, Vector3.one);
                    Gizmos.color = new Color(1f, 0f, 0f, 0.9f);
                    Gizmos.DrawWireCube(center, Vector3.one);
                }
            }
        }
    }

    class Node
    {
        public int x;
        public int y;
        public float gCost = float.MaxValue;
        public float hCost;
        public float FCost { get { return gCost + hCost; } }
        public Node parent;
        public bool inOpen;
        public bool inClosed;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
