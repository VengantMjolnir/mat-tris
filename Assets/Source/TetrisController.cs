using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisController : MonoBehaviour
{

    public List<Group> activeGroups = new List<Group>(4);
    public GameObject solidBlock;
    public Transform bottomRow;
    public Spawner spawner;
    public PieceQueue queue;
    public GridModel gridModel;
    public DropTimer timer;
    public GameRules rules;
    public SoundCollection whooshSounds;
    public SoundCollection rotateSounds;
    public SoundCollection impactSounds;
    public SoundCollection selectSounds;
    public SoundCollection fallSounds;
    public SoundCollection penaltySounds;
    public int overrideIndex = -1;

    private static GameRules _rules;
    public static GameRules Rules
    {
        get { return _rules; }
    }

    private float fallSpeed;
    private float fallBoost;

    // Use this for initialization
    void Start()
    {
        if (spawner == null)
        {
            enabled = false;
            return;
        }

        fallSpeed = rules.startingFallSpeed;
        fallBoost = 1f;
        _rules = rules;
    }

    public void AddPiece(Tetronimo piece)
    {
        Group group = spawner.SpawnTetronimo(piece);
        group.lastFallTime = Time.time;
        group.gridModel = gridModel;
        activeGroups.Add(group);
    }

    public bool SpawnRandom()
    {
        int i = UnityEngine.Random.Range(0, Rules.tetronimos.Length);
        if (overrideIndex >= 0 && overrideIndex < Rules.tetronimos.Length)
        {
            i = overrideIndex;
            overrideIndex++;
        }

        AddPiece(Rules.tetronimos[i]);
        return true;
    }

    public bool SpawnSelected()
    {
        Tetronimo piece = queue.GetSelectedPiece();
        if (piece)
        {
            AddPiece(piece);
            queue.RemoveSelectedPiece();
            return true;
        }
        return false;
    }

    public bool AddBlockerRow()
    {
        if (solidBlock && bottomRow)
        {
            gridModel.increaseAllRows();
            GameObject go = Instantiate<GameObject>(solidBlock, bottomRow.position, Quaternion.identity);
            Group group = go.GetComponent<Group>();
            if (group)
            {
                group.gridModel = gridModel;
            }
        }
        return true;
    }

    public void HandleInput(Group currentGroup)
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentGroup.AttemptMoveLeft();
            SFXSelector.Instance.PlayRandomSound(whooshSounds);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            currentGroup.AttemptMoveRight();
            SFXSelector.Instance.PlayRandomSound(whooshSounds);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            currentGroup.AttemptRotate();
            SFXSelector.Instance.PlayRandomSound(rotateSounds);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (!currentGroup.AttemptMoveDown())
            {
                currentGroup.FreezeInPlace();

                gridModel.deleteFullRows();
                activeGroups.Remove(currentGroup);
                SFXSelector.Instance.PlayRandomSound(impactSounds);
            }
            else
            {
                SFXSelector.Instance.PlayRandomSound(whooshSounds);
            }
            currentGroup.lastFallTime = Time.time;
        }

        // Check for speed up for the first group
        if (Input.GetKey(KeyCode.S))
        {
            currentGroup.fallMultiplier = rules.fallBoost;
        }
        else
        {
            currentGroup.fallMultiplier = 1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Rules.ready)
        {
            return;
        }

        // Check if a new one should spawn
        if (timer.dropNow)
        {
            bool didSpawn = SpawnSelected();
            if (!didSpawn)
            {
                AddBlockerRow();
                SFXSelector.Instance.PlayRandomSound(penaltySounds);
            }
            timer.ResetTimer();
            queue.dropNow = false;
        }

        // Bail if there are no active pieces
        if (activeGroups.Count == 0)
        {
            return;
        }

        // Try to spawn a new one if there is space allowed
        if (queue.dropNow)
        {
            bool canSpawn = true;
            foreach (Group group in activeGroups)
            {
                Vector2 gridPos = gridModel.roundVec2(group.transform.localPosition);
                if (gridPos.y > rules.dropHeightLimit)
                {
                    canSpawn = false;
                    break;
                }
            }
            if (canSpawn)
            {
                SpawnSelected();
                SFXSelector.Instance.PlayRandomSound(selectSounds);
                timer.ResetTimer();
            }
            queue.dropNow = false;
        }

        // Update first group (should be lowest one?)
        Group currentGroup = activeGroups[0];
        HandleInput(currentGroup);

        // Update all the other groups
        bool atLeastOneFell = false;
        for (int i = 0; i < activeGroups.Count; ++i)
        {
            Group group = activeGroups[i];
            float fallInterval = 1f / (fallSpeed * group.fallMultiplier);
            if ((Time.time - group.lastFallTime) >= fallInterval)
            {
                if (!group.AttemptMoveDown())
                {
                    group.FreezeInPlace();
                    group.PlaySettleEffects(rules.settleEffect);
                    SFXSelector.Instance.PlayRandomSound(impactSounds);

                    gridModel.deleteFullRows();
                    activeGroups.Remove(group);
                    i--;
                }
                else
                {
                    atLeastOneFell = true;
                }
                group.lastFallTime = Time.time;
            }
        }
        if (atLeastOneFell)
        {
            SFXSelector.Instance.PlayRandomSound(fallSounds);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    [Serializable]
    public class GameRules
    {
        public float startingFallSpeed = 1f;
        public float fallBoost = 2f;
        public float fallIncrement = 0.5f;
        public int dropHeightLimit = 16;
        public int maxPiecesInQueue = 10;
        public GameObject settleEffect;

        public Tetronimo[] tetronimos;
        public List<int> matchesMade = new List<int>();
        public bool ready = false;
    }
}
