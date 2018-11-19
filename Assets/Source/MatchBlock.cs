using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBlock : MonoBehaviour 
{
    public enum DragDirection
    {
        None = 0,
        Left,
        Right,
        Up,
        Down
    }
    private static int counter = 0;

    public GridModel gridModel;
    private int type;
    public bool settled = false;
    public Material matchMaterial;
    public DragDirection dragDirection = DragDirection.None;
    
    private bool matched = false;
    private Vector3 clickPosition;
    private Renderer _renderer;
    private Renderer CachedRenderer
    {
        get {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }
            return _renderer;
        }
    }
    private Material typeMaterial;

    public int BlockType
    {
        get { return type; }
    }

    public bool IsMatched
    {
        get { return matched; }
        set {
            matched = value;
            if (matched)
            {
                CachedRenderer.material = matchMaterial;
            }
            else
            {
                CachedRenderer.material = typeMaterial;
            }
        }
    }

	// Use this for initialization
	void Start () {

        if (gridModel && isValidGridPos())
        {
            updateGrid();
        }
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        this.name = "Block " + counter.ToString();
        counter++;
    }

    public void SetBockType(int type, Material mat)
    {
        typeMaterial = mat;
        CachedRenderer.material = mat;
        this.type = type;
    }

    public bool AttemptMove(Vector3 direction, Vector3 revert)
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

    public Vector2 GridPosition
    {
        get { return gridModel.roundVec2(transform.position - gridModel.offset); }
    }

    public bool isValidGridPos()
    {
        Vector2 v = GridPosition;

        // Not inside Border?
        if (!gridModel.insideBorder(v))
            return false;

        // Block in grid cell (and not part of same group)?
        if (gridModel.grid[(int)v.x, (int)v.y] != null &&
            gridModel.grid[(int)v.x, (int)v.y] != transform)
        {
            return false;
        }

        return true;
    }

    public void updateGrid()
    {
        for (int y = 0; y < gridModel.h; ++y)
        {
            for (int x = 0; x < gridModel.w; ++x)
            {
                if (gridModel.grid[x, y] == transform)
                {
                    gridModel.grid[x, y] = null;
                }
            }
        }

        Vector2 v = GridPosition;
        gridModel.grid[(int)v.x, (int)v.y] = transform;
    }

    public void PlaySettleEffect(GameObject prefab, bool useBlockColor = false)
    {
        Vector2 v = gridModel.roundVec2(transform.position);
        GameObject go = Instantiate<GameObject>(prefab);
        go.transform.position = transform.position + (Vector3.down * 0.5f) + (Vector3.back * 0.5f);
        go.transform.localScale = Vector3.one * 2f;

        if (useBlockColor)
        {
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startColor = typeMaterial.color;
        }
    }

    private void OnMouseDown()
    {
        clickPosition = Input.mousePosition;
    }

    private void OnMouseUp()
    {
        dragDirection = DragDirection.None;
        Vector3 direction = Input.mousePosition - clickPosition;
        direction.Normalize();

        if(direction.x > 0.8f)
        {
            dragDirection = DragDirection.Right;
        }
        else if (direction.x < -0.8f)
        {
            dragDirection = DragDirection.Left;
        }
        else if (direction.y > 0.8f)
        {
            dragDirection = DragDirection.Up;
        }
        else if (direction.y < -0.8f)
        {
            dragDirection = DragDirection.Down;
        }
        Debug.Log("Drag direction: " + dragDirection.ToString());
        MatchController.Instance.PieceWasDragged(this);
    }
}
