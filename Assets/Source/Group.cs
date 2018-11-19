using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour {
    public float fallMultiplier = 1f;
    public float lastFallTime;
    public GridModel gridModel;
    public bool frozen = false;

    private Transform _pivot;
    public Transform pivot 
    {
        get {
            if (_pivot == null)
            {
                _pivot = transform.GetChild(0);
            }
            return _pivot;
        }
    }

    private void Start()
    {
        if (gridModel && isValidGridPos())
        {
            updateGrid();
        }
    }

    public bool isValidGridPos() {
        foreach (Transform child in pivot)
        {
            Vector2 v = gridModel.roundVec2(child.position);

            // Not inside Border?
            if (!gridModel.insideBorder(v))
                return false;

            // Block in grid cell (and not part of same group)?
            if (gridModel.grid[(int)v.x, (int)v.y] != null &&
                gridModel.grid[(int)v.x, (int)v.y].parent != pivot)
                return false;
        }
        return true;
    }

    public void updateGrid() {
        for (int y = 0; y < gridModel.h; ++y) {
            for (int x = 0; x < gridModel.w; ++x) {
                if (gridModel.grid[x, y] != null) {
                    if (gridModel.grid[x, y].parent == pivot)
                    {
                        gridModel.grid[x, y] = null;
                    }
                }
            }
        }

        foreach (Transform child in pivot) {
            Vector2 v = gridModel.roundVec2(child.position);
            gridModel.grid[(int)v.x, (int)v.y] = child;
        }
    }

    private bool AttemptMove(Vector3 direction, Vector3 revert)
    {
        transform.position += direction;

        if (isValidGridPos())
        {
            updateGrid();
            return true;
        }
        else
        {
            transform.position += revert;
            return false;
        }
    }

    public bool AttemptMoveLeft()
    {
        return AttemptMove(Vector3.left, Vector3.right);
    }

    public bool AttemptMoveRight()
    {
        return AttemptMove(Vector3.right, Vector3.left);
    }

    public bool AttemptMoveDown()
    {
        return AttemptMove(Vector3.down * TetrisController.Rules.fallIncrement, Vector3.up * TetrisController.Rules.fallIncrement);
    }

    public bool AttemptRotate()
    {
        pivot.Rotate(0, 0, -90);

        if (isValidGridPos())
        {
            updateGrid();
            return true;
        }
        else
        {
            pivot.Rotate(0, 0, 90);
            return false;
        }
    }

    public void FreezeInPlace()
    {
        frozen = true;
    }

    public void Update()
    {
        if (frozen && pivot.childCount == 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void PlaySettleEffects(GameObject prefab, bool force = false)
    {
        foreach (Transform child in pivot)
        {
            bool playEffect = false;
            if (gridModel != null)
            {
                Vector2 v = gridModel.roundVec2(child.position);
                v.y -= 1;
                if (v.y < 0 || (gridModel.grid[(int)v.x, (int)v.y] != null &&
                                gridModel.grid[(int)v.x, (int)v.y].parent != pivot))
                {
                    playEffect = true;
                }
            }

            if (playEffect || force)
            {
                GameObject go = Instantiate<GameObject>(prefab);
                go.transform.position = child.position + (Vector3.down * 0.5f) + (Vector3.back * 0.5f);
                go.transform.localScale = Vector3.one * 2f;
            }
        }
    }
}
