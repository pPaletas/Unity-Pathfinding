using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public interface IGridCell
{
    public Vector2Int Index { get; set; }
}

public class ForgotGrid<T> where T : struct, IGridCell
{
    private int _columns; //x
    private int _rows; //y
    private float _cellSize;
    private Vector3 _position;
    private T[,] _grid;
    private T _defaultValue;

    private bool _debug;
    private TextMesh[,] _debugObjects;

    public int Columns { get => _columns; }
    public int Rows { get => _rows; }
    public float CellSize { get => _cellSize; }

    public ForgotGrid(int columns, int rows, bool debug = false, float cellSize = 1f, Vector3 position = default, T defaultValue = default)
    {
        _columns = columns;
        _rows = rows;
        _cellSize = cellSize;
        _position = position;
        _defaultValue = defaultValue;

        _grid = new T[_columns, _rows];
        SetDefaultToAllCells(defaultValue);

        if (debug)
        {
            _debug = debug;
            _debugObjects = new TextMesh[_columns, _rows];
            DisplayGrid();
        }

    }

    #region Public Methods

    public T GetValue(int column, int row)
    {
        if (IsInBoundaries(column, row))
        {
            return _grid[column, row];
        }

        return default;
    }

    public T GetValue(Vector3 worldPos)
    {
        int c;
        int r;

        WorldToCoordinates(worldPos, out c, out r);

        return GetValue(c, r);
    }

    public Vector3 CoordinatesToWorld(int column, int row)
    {
        if (IsInBoundaries(column, row))
        {
            // coordenada * size
            // Restar floor de mitad de todo el grid - 1
            float scaledCoorX = column * _cellSize;
            float scaledCoorY = row * _cellSize;

            float minuOneHalfX = (_columns - 1) * _cellSize * 0.5f;
            float minuOneHalfY = (_rows - 1) * _cellSize * 0.5f;

            Vector3 pos = new Vector3(scaledCoorX - minuOneHalfX, scaledCoorY - minuOneHalfY);
            pos += _position;

            return pos;
        }

        return default;
    }

    public bool IsValid(int column, int row)
    {
        bool isColumnsInBounds = column >= 0f && column < _grid.GetLength(0);
        bool isRowInBounds = row >= 0f && row < _grid.GetLength(1);

        bool inBounds = isColumnsInBounds && isRowInBounds;

        return inBounds;
    }

    public bool IsValid(Vector3 worldPos)
    {
        int c;
        int r;

        WorldToCoordinates(worldPos, out c, out r);

        return IsValid(c, r);
    }

    public void CleanUpGrid()
    {
        for (int x = 0; x < _columns; x++)
        {
            for (int y = 0; y < _rows; y++)
            {
                _defaultValue.Index = new Vector2Int(x, y);
                _grid[x, y] = _defaultValue;
            }
        }
    }

    public void WorldToCoordinates(Vector3 worldPos, out int column, out int row)
    {
        worldPos -= _position;

        float normalizedX = worldPos.x / _cellSize;
        float normalizedY = worldPos.y / _cellSize;

        column = Mathf.FloorToInt(normalizedX + (_columns * 0.5f));
        row = Mathf.FloorToInt(normalizedY + (_rows * 0.5f));
    }

    public void SetValue(int column, int row, T value)
    {
        if (IsInBoundaries(column, row))
        {
            _grid[column, row] = value;


            _debugObjects[column, row].text = value.ToString();
        }
    }

    public void SetValue(Vector3 worldPos, T value)
    {
        int c;
        int r;

        WorldToCoordinates(worldPos, out c, out r);

        SetValue(c, r, value);
    }

    #endregion

    #region Private Methods

    private bool IsInBoundaries(int column, int row)
    {
        bool inBounds = IsValid(column, row);

        if (!inBounds)
        {
            Debug.LogWarning($"Position ({column}, {row}) is outside of the boundaries of the grid");
        }

        return inBounds;
    }

    private void SetDefaultToAllCells(T value)
    {
        for (int x = 0; x < _columns; x++)
        {
            for (int y = 0; y < _rows; y++)
            {
                value.Index = new Vector2Int(x, y);
                _grid[x, y] = value;
            }
        }
    }

    private void DisplayGrid()
    {
        for (int x = 0; x < _columns; x++)
        {
            for (int y = 0; y < _rows; y++)
            {
                Vector3 worldPos = CoordinatesToWorld(x, y);

                Vector3 start = worldPos - Vector3.one * (_cellSize * 0.5f);

                Vector3 endX = start + Vector3.right * _cellSize;
                Debug.DrawLine(start, endX, Color.white, 1000f);

                Vector3 endY = start + Vector3.up * _cellSize;
                Debug.DrawLine(start, endY, Color.white, 1000f);

                _debugObjects[x, y] = UtilsClass.CreateWorldText(_grid[x, y].ToString(), null, worldPos, 33, Color.white, TextAnchor.MiddleCenter);
            }
        }
    }
    #endregion
}