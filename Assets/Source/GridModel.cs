using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridModel : MonoBehaviour
{
    public int w = 10;
    public int h = 24;
    public Transform[,] grid;
    public Vector3 offset = Vector3.zero;

    public void Start()
    {
        grid = new Transform[w, h];
    }

    public Vector2 roundVec2(Vector2 v) {
        return new Vector2(Mathf.Round(v.x),
                           Mathf.Round(v.y));
    }

    public bool insideBorder(Vector2 pos) {
        return ((int)pos.x >= 0 &&
                (int)pos.x < w &&
                (int)pos.y >= 0);
    }

    public void deleteRow(int y) {
        for (int x = 0; x < w; ++x) {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    public void deleteCell(int x, int y) {
        Destroy(grid[x, y].gameObject);
        grid[x, y] = null;
    }

    public void decreaseCell(int x, int y) {
        if (grid[x, y] != null)
        {
            grid[x, y - 1] = grid[x, y];
            grid[x, y] = null;
            grid[x, y - 1].position += Vector3.down;
        }
    }

    public void decreaseCellsAbove(int x, int y) {
        for (int i = y; i < h; ++i) {
            decreaseCell(x, i);
        }
    }

    public void decreaseRow(int y) {
        for (int x = 0; x < w; ++x) {
            if (grid[x, y] != null) {
                grid[x, y - 1] = grid[x, y];
                grid[x, y] = null;

                grid[x, y - 1].position += Vector3.down;
            }
        }
    }

    public void decreaseRowsAbove(int y) {
        for (int i = y; i < h; ++i) {
            decreaseRow(i);
        }
    }

    public void increaseRow(int y)
    {
        for (int x = 0; x < w; ++x)
        {
            if (grid[x, y] != null)
            {
                grid[x, y + 1] = grid[x, y];
                grid[x, y] = null;

                grid[x, y + 1].position += Vector3.up;
            }
        }
    }

    public void increaseAllRows()
    {
        for (int i = h - 2; i >= 0; --i)
        {
            increaseRow(i);
        }
    }

    public bool isRowFull(int y) {
        for (int x = 0; x < w; ++x)
        {
            if (grid[x, y] == null) {
                return false;
            }
            else if (grid[x, y].tag == "Block")
            {
                return false;
            }
        }
        return true;
    }

    public void deleteFullRows() {
        for (int y = 0; y < h; ++y) {
            if (isRowFull(y)) {
                deleteRow(y);
                decreaseRowsAbove(y + 1);
                --y;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (grid == null)
        {
            return;
        }

        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                Gizmos.color = Color.white;
                if (grid[x, y] != null){
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawWireCube(offset + new Vector3(x, y, 0), Vector3.one * 0.85f);
            }
        }
    }
}
