using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Test : MonoBehaviour
{
    public Pathfinding _pathFinding;

    private Vector3? start;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (start == null)
            {
                start = UtilsClass.GetMouseWorldPosition();
            }
            else
            {
                Vector3 target = UtilsClass.GetMouseWorldPosition();
                _pathFinding.FindPath(start.Value, target, true);
                start = null;
            }
        }
        else if (Input.GetMouseButton(1))
        {
            _pathFinding.SetWalkable(UtilsClass.GetMouseWorldPosition(), false);
        }
    }
}
