using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathNode : IGridCell
{
    public int gCost;
    public int hCost;
    public bool isWalkable;
    public Vector2Int lastNodeIndex;

    private int fCost;

    public Vector2Int Index { get; set; }

    public int GetFCost() => fCost;
    public void CalculateFCost() => fCost = gCost + hCost;

    public override string ToString()
    {
        return $"{Index.x} \n {Index.y}";
    }
}

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Vector2Int _size;
    [SerializeField] private float _scale;
    [SerializeField] private bool _diagonalNeighbours = true;

    private HashSet<PathNode> _closedList;
    private List<PathNode> _openList;

    private ForgotGrid<PathNode> _grid;
    private GameObject[,] _tiles;

    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;

    #region Public Methods

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, bool debug = false)
    {
        if (!_grid.IsValid(startPos) || !_grid.IsValid(targetPos)) return null;

        CleanUpGrid();

        PathNode startNode = _grid.GetValue(startPos);
        PathNode currentNode = startNode;
        PathNode targetNode = _grid.GetValue(targetPos);

        _tiles[startNode.Index.x, startNode.Index.y].GetComponent<SpriteRenderer>().color = Color.red;

        currentNode.gCost = 0;
        currentNode.hCost = GetDistance(currentNode, targetNode);
        currentNode.CalculateFCost();
        _grid.SetValue(startNode.Index.x, startNode.Index.y, currentNode);

        _openList = new List<PathNode> { currentNode };
        _closedList = new HashSet<PathNode>();

        bool targetFound = false;

        while (_openList.Count > 0)
        {
            _openList.Remove(currentNode);
            _closedList.Add(currentNode);
            // En caso de que la celda start, sea el mismo target
            targetFound = RevealNeighbours(currentNode, targetNode) || currentNode.Index == targetNode.Index;

            if (!targetFound)
            {
                PathNode nextNode = GetLowestFNode();
                currentNode = nextNode;
            }
            else break;
        }

        if (targetFound)
        {
            var path = RecalculatePath(targetNode.Index, startNode);
            if (debug) DisplayPath(path);

            return path;
        }

        return null;
    }

    public void SetWalkable(Vector3 pos, bool walkable)
    {
        if (!_grid.IsValid(pos)) return;

        PathNode node = _grid.GetValue(pos);
        node.isWalkable = walkable;
        _grid.SetValue(node.Index.x, node.Index.y, node);

        _tiles[node.Index.x, node.Index.y].GetComponent<SpriteRenderer>().color = Color.black;
    }
    #endregion

    #region Private Methods

    private PathNode GetLowestFNode()
    {
        if (_openList.Count <= 0) return default;

        PathNode lowestCostNode = _openList[0];

        for (int i = 1; i < _openList.Count; i++)
        {
            if (_openList[i].GetFCost() < lowestCostNode.GetFCost())
            {
                lowestCostNode = _openList[i];
            }
        }

        _tiles[lowestCostNode.Index.x, lowestCostNode.Index.y].GetComponent<SpriteRenderer>().color = Color.magenta;
        return lowestCostNode;
    }

    private List<PathNode> GetNeighbours(PathNode currentNode)
    {
        List<PathNode> neighbours = new List<PathNode>();
        Vector2Int currentIndex = currentNode.Index;

        bool rightAvailable = currentIndex.x + 1 < _grid.Columns;
        bool leftAvailable = currentIndex.x - 1 >= 0;
        bool upAvailable = currentIndex.y + 1 < _grid.Rows;
        bool downAvailable = currentIndex.y - 1 >= 0;

        // Right
        if (rightAvailable)
        {
            neighbours.Add(_grid.GetValue(currentIndex.x + 1, currentIndex.y));

            // Right Up
            if (upAvailable && _diagonalNeighbours) neighbours.Add(_grid.GetValue(currentIndex.x + 1, currentIndex.y + 1));
            // Right Down
            if (downAvailable && _diagonalNeighbours) neighbours.Add(_grid.GetValue(currentIndex.x + 1, currentIndex.y - 1));
        }
        // Left
        if (leftAvailable)
        {
            neighbours.Add(_grid.GetValue(currentIndex.x - 1, currentIndex.y));

            // Right Up
            if (upAvailable && _diagonalNeighbours) neighbours.Add(_grid.GetValue(currentIndex.x - 1, currentIndex.y + 1));
            // Right Down
            if (downAvailable && _diagonalNeighbours) neighbours.Add(_grid.GetValue(currentIndex.x - 1, currentIndex.y - 1));
        }

        // Up
        if (upAvailable) neighbours.Add(_grid.GetValue(currentIndex.x, currentIndex.y + 1));
        // Down
        if (downAvailable) neighbours.Add(_grid.GetValue(currentIndex.x, currentIndex.y - 1));

        return neighbours;
    }

    private int GetDistance(PathNode startNode, PathNode targetNode)
    {
        int xDif = Math.Abs(targetNode.Index.x - startNode.Index.x);
        int yDif = Math.Abs(targetNode.Index.y - startNode.Index.y);

        int straightSteps = Mathf.Abs(xDif - yDif);
        int diagonalSteps = Mathf.Min(xDif, yDif);

        return straightSteps * STRAIGHT_COST + diagonalSteps * DIAGONAL_COST;
    }

    // Returns a value indicating if target is a neighbour
    private bool RevealNeighbours(PathNode currentNode, PathNode targetNode)
    {
        List<PathNode> neighbours = GetNeighbours(currentNode);

        if (neighbours.Contains(targetNode))
        {
            targetNode.lastNodeIndex = currentNode.Index;
            _grid.SetValue(targetNode.Index.x, targetNode.Index.y, targetNode);
            return true;
        }

        foreach (PathNode n in neighbours)
        {
            if (_closedList.Contains(n) || !n.isWalkable) { continue; }

            int gCostFromCurrent = currentNode.gCost + GetDistance(currentNode, n);

            if (gCostFromCurrent < n.gCost)
            {
                PathNode newValues = n;
                newValues.gCost = gCostFromCurrent;
                newValues.hCost = GetDistance(n, targetNode);
                newValues.CalculateFCost();
                newValues.lastNodeIndex = currentNode.Index;

                // _tiles[n.Index.x, n.Index.y].GetComponent<SpriteRenderer>().color = Color.blue;
                _grid.SetValue(n.Index.x, n.Index.y, newValues);
                if (!_openList.Contains(newValues)) _openList.Add(newValues);
            }
        }

        return false;
    }

    private List<Vector3> RecalculatePath(Vector2Int targetNodeIndex, PathNode startNode)
    {
        Vector2Int currentPathIndex = targetNodeIndex;

        List<Vector3> path = new List<Vector3>();

        // Repetir hasta que lleguemos al inicio
        while (currentPathIndex != startNode.Index)
        {
            Vector3 pos = _grid.CoordinatesToWorld(currentPathIndex.x, currentPathIndex.y);
            path.Add(pos);
            currentPathIndex = _grid.GetValue(currentPathIndex.x, currentPathIndex.y).lastNodeIndex;
        }
        // Agregar el Start
        path.Add(_grid.CoordinatesToWorld(startNode.Index.x, startNode.Index.y));

        path.Reverse();

        return path;
    }

    private void DisplayPath(List<Vector3> path)
    {
        GameObject tile;

        // Limpiar primero
        for (int x = 0; x < _grid.Columns; x++)
        {
            for (int y = 0; y < _grid.Rows; y++)
            {
                Color color = _grid.GetValue(x, y).isWalkable ? Color.white : Color.black;

                _tiles[x, y].GetComponent<SpriteRenderer>().color = color;
            }
        }

        foreach (Vector3 pos in path)
        {
            int x;
            int y;

            _grid.WorldToCoordinates(pos, out x, out y);

            tile = _tiles[x, y];

            tile.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    private void CreateGrid()
    {
        PathNode defaultValue = new PathNode
        {
            gCost = int.MaxValue,
            hCost = int.MaxValue,
            isWalkable = true,
        };
        _grid = new ForgotGrid<PathNode>(_size.x, _size.y, true, _scale, defaultValue: defaultValue);
    }

    private void DisplayGrid()
    {
        _tiles = new GameObject[_size.x, _size.y];

        for (int x = 0; x < _grid.Columns; x++)
        {
            for (int y = 0; y < _grid.Rows; y++)
            {
                GameObject tile = Instantiate(_tilePrefab);
                tile.transform.position = _grid.CoordinatesToWorld(x, y);
                _tiles[x, y] = tile;
            }
        }
    }

    private void CleanUpGrid()
    {
        for (int x = 0; x < _grid.Columns; x++)
        {
            for (int y = 0; y < _grid.Rows; y++)
            {
                PathNode node = _grid.GetValue(x, y);
                node.gCost = int.MaxValue;
                node.hCost = int.MaxValue;
                node.lastNodeIndex = default;
                node.CalculateFCost();

                _grid.SetValue(x, y, node);
            }
        }
    }

    #endregion

    private void Awake()
    {
        CreateGrid();
        DisplayGrid();
    }
}