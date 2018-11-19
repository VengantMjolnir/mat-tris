using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour {
    public Camera viewCamera;
    public GridModel gridModel;
    public GameObject block;
    public GameObject settleEffect;
    public List<Material> blockMaterials = new List<Material>();
    public SoundCollection destructionSounds;
    public SoundCollection impactSounds;
    public SoundCollection spawnSounds;
    public int spawnCount = 2;
    public float matchSpeed = 0.5f;
    public float fallSpeed = 2f;

    private static MatchController _instance;
    public static MatchController Instance
    {
        get {
            if (_instance == null){
                _instance = FindObjectOfType<MatchController>();
            }
            return _instance;
        }
    }

    private List<Transform> blocks = new List<Transform>();
    private List<int> matches = new List<int>();
    private Transform cachedTransform;
    private bool ready = false;
    private float lastMatchTime;
    private float lastFallTime;
    private bool clearBlocks = false;

    // Use this for initialization
    void Start () {
        _instance = this;
        cachedTransform = transform;
        StartCoroutine(PopulateGrid());
	}

    public IEnumerator PopulateGrid()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        int count = 0;
        int soundCounter = 0;
        for (int y = 0; y < gridModel.h; ++y)
        {
            for (int x = 0; x < gridModel.w; ++x)
            {
                Vector3 pos = new Vector3(x + gridModel.offset.x, y + gridModel.offset.y, 0);
                GameObject go = Instantiate<GameObject>(block, pos, Quaternion.identity, cachedTransform);
                int type = Random.Range(0, blockMaterials.Count);
                MatchBlock mb = go.GetComponent<MatchBlock>();
                if (mb)
                {
                    mb.gridModel = gridModel;
                    mb.SetBockType(type, blockMaterials[type]);
                    mb.IsMatched = false;
                    mb.settled = true;
                }
                soundCounter++;
                if (soundCounter > 20)
                {
                    SFXSelector.Instance.PlayNextSound(spawnSounds);
                    soundCounter = 0;
                }
                count++;
                if (count >= spawnCount)
                {
                    count = 0;
                    yield return wait;
                }
            }
        }
        ready = true;
        TetrisController.Rules.ready = true;
        // Play dust!
        for (int y = 0; y < gridModel.h; ++y)
        {
            for (int x = 0; x < gridModel.w; ++x)
            {
                if (gridModel.grid[x, y] != null)
                {
                    MatchBlock mb = gridModel.grid[x, y].GetComponent<MatchBlock>();
                    mb.PlaySettleEffect(settleEffect);
                }
            }
        }
        SFXSelector.Instance.PlayRandomSound(impactSounds, 1f);
        lastFallTime = lastMatchTime = Time.time;
    }

    private bool CheckMatch(int x, int y, ref int currentType, ref List<MatchBlock> blocks)
    {
        if (gridModel.grid[x, y] == null)
        {
            blocks.Clear();
            return false;
        }

        MatchBlock mb = gridModel.grid[x, y].GetComponent<MatchBlock>();
        if (mb)
        {
            if (currentType != mb.BlockType || !mb.settled)
            {
                currentType = mb.BlockType;
                blocks.Clear();
            }

            if (mb.settled)
            {
                blocks.Add(mb);
            }

            if (blocks.Count == 3)
            {
                for (int i = blocks.Count - 1; i >= 0; --i)
                {
                    MatchBlock b = blocks[i];
                    b.IsMatched = true;
                }
                blocks.Clear();
                return true;
            }
        }
        return false;
    }

    public void CheckForMatches()
    {
        int currentType = -1;
        matches.Clear();
        List<MatchBlock> blocks = new List<MatchBlock>(3);
        // Vertical matches, top down
        for (int x = 0; x < gridModel.w; ++x)
        {
            blocks.Clear();
            currentType = -1;
            for (int y = gridModel.h - 1; y >= 0; --y)
            {
                if (CheckMatch(x, y, ref currentType, ref blocks))
                {
                    matches.Add(currentType);
                }
            }
        }
        // Horizontal matches, top down
        for (int y = gridModel.h - 1; y >= 0; --y)
        {
            blocks.Clear();
            currentType = -1;
            for (int x = 0; x < gridModel.w; ++x)
            {
                if (CheckMatch(x, y, ref currentType, ref blocks))
                {
                    matches.Add(currentType);
                }
            }
        }

    }

    public void ClearMatchesFromBoard()
    {
        bool removedAtLeastOne = false;
        for (int x = 0; x < gridModel.w; ++x)
        {
            for (int y = gridModel.h - 1; y >= 0; --y)
            {
                if (gridModel.grid[x, y] == null)
                {
                    continue;
                }
                MatchBlock mb = gridModel.grid[x, y].GetComponent<MatchBlock>();
                if (mb.IsMatched)
                {
                    Vector2 v = mb.GridPosition;
                    mb.PlaySettleEffect(settleEffect, true);
                    gridModel.deleteCell((int)v.x, (int)v.y);
                    removedAtLeastOne = true;
                }
            }
        }

        if (removedAtLeastOne)
        {
            SFXSelector.Instance.PlayRandomSound(destructionSounds);
        }
        // Send the matches to the rules object
        TetrisController.Rules.matchesMade.AddRange(matches);
    }

	void Update () {
        if (Input.GetButtonDown("Fire1") && viewCamera.pixelRect.Contains(Input.mousePosition))
            Debug.Log("Itsa mee, " + transform.name);

        if (!ready)
        {
            return;
        }

        float tickInterval = 1f / matchSpeed;
        if (Time.time - lastMatchTime > tickInterval)
        {
            if (!clearBlocks)
            {
                CheckForMatches();
            }
            else
            {
                ClearMatchesFromBoard();
            }
            clearBlocks = !clearBlocks;
            lastMatchTime = Time.time;
        }

        tickInterval = 1f / fallSpeed;
        if (Time.time - lastFallTime > tickInterval)
        {
            // Compact the grid, bottom up
            bool settledAtLeastOne = false;
            for (int y = 0; y < gridModel.h; ++y)
            {
                for (int x = 0; x < gridModel.w; ++x)
                {
                    if (gridModel.grid[x, y] != null)
                    {
                        MatchBlock mb = gridModel.grid[x, y].GetComponent<MatchBlock>();
                        if (mb)
                        {
                            if (mb.AttemptMove(Vector3.down, Vector3.up))
                            {
                                mb.settled = false;
                            }
                            else if (!mb.settled)
                            {
                                mb.settled = true;
                                settledAtLeastOne = true;
                            }
                        }
                    }
                }
                if (settledAtLeastOne)
                {
                    //SFXSelector.Instance.PlayRandomSound(impactSounds);
                }
            }

            lastFallTime = Time.time;
        }
	}

    public void PieceWasDragged(MatchBlock block)
    {
        if (block == null || block.dragDirection == MatchBlock.DragDirection.None)
        {
            return;
        }

        Vector2 gridPos = block.GridPosition;
        Vector2 otherPos = gridPos;
        switch(block.dragDirection)
        {
            case MatchBlock.DragDirection.Left:
                otherPos.x -= 1;
                break;
            case MatchBlock.DragDirection.Right:
                otherPos.x += 1;
                break;
            case MatchBlock.DragDirection.Up:
                otherPos.y += 1;
                break;
            case MatchBlock.DragDirection.Down:
                otherPos.y -= 1;
                break;
        }

        if (gridModel.insideBorder(otherPos))
        {
            Transform t = gridModel.grid[(int)otherPos.x, (int)otherPos.y];
            if (t != null)
            {
                Vector3 oldPos = block.transform.position;
                block.transform.position = t.position;
                t.position = oldPos;
                block.updateGrid();
                MatchBlock otherBlock = t.GetComponent<MatchBlock>();
                otherBlock.updateGrid();
            }
            else
            {
                block.transform.position = new Vector3(otherPos.x, otherPos.y) + gridModel.offset;
            }
        }
        block.dragDirection = MatchBlock.DragDirection.None;
        lastMatchTime = Time.time;
        clearBlocks = false;
        CheckForMatches();
    }
}
