using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceQueue : MonoBehaviour {

    public List<QueuedPiece> pieces = new List<QueuedPiece>();
    public SoundCollection whooshSounds;
    public Transform selector;
    public GameObject addEffect;
    public int selectorIndex = 0;
    public float spacing = 3f;
    public bool dropNow = false;

    private void Start()
    {
        selector.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        if (!TetrisController.Rules.ready)
        {
            return;
        }

        if (TetrisController.Rules.matchesMade.Count > 0)
        {
            Debug.Log("Found matches: " + TetrisController.Rules.matchesMade.Count);

            for (int i = 0; i < TetrisController.Rules.matchesMade.Count; ++i)
            {
                int index = TetrisController.Rules.matchesMade[i];
                if (index > TetrisController.Rules.tetronimos.Length || index == -1)
                {
                    index = Random.Range(0, TetrisController.Rules.tetronimos.Length);
                }
                Tetronimo tetronimo = TetrisController.Rules.tetronimos[index];
                AddPiece(tetronimo);
            }
            TetrisController.Rules.matchesMade.Clear();
        }

        LayoutPieces();

        if (selector)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) && selectorIndex < (pieces.Count - 1))
            {
                MoveSelectorDown();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) && selectorIndex > 0)
            {
                MoveSelectorUp();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                dropNow = true;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                RemoveSelectedPiece();
            }
        }
	}

    private void MoveSelectorUp()
    {
        selector.localPosition += Vector3.up * spacing;
        selectorIndex -= 1;
        SFXSelector.Instance.PlayRandomSound(whooshSounds);
    }

    private void MoveSelectorDown()
    {
        selector.localPosition += Vector3.down * spacing;
        selectorIndex += 1;
        SFXSelector.Instance.PlayRandomSound(whooshSounds);
    }

    public void AddPiece(Tetronimo piece)
    {
        if (pieces.Count >= TetrisController.Rules.maxPiecesInQueue)
        {
            return;
        }
        GameObject go = Instantiate<GameObject>(piece.group, this.transform);
        go.transform.Rotate(0, 0, piece.queueRotation);
        QueuedPiece t = new QueuedPiece();
        t.transform = go.transform;
        t.tetronimo = piece;
        pieces.Add(t);

        selector.gameObject.SetActive(true);

        LayoutPieces();

        // Play effect AFTER layout so they are in the correct spot
        Group group = go.GetComponent<Group>();
        if (group)
        {
            group.PlaySettleEffects(addEffect, true);
        }
    }

    public Tetronimo GetSelectedPiece()
    {
        if (selectorIndex < pieces.Count)
        {
            return pieces[selectorIndex].tetronimo;
        }
        return null;
    }

    public void RemoveSelectedPiece()
    {
        if (selectorIndex < pieces.Count)
        {
            Destroy(pieces[selectorIndex].transform.gameObject);
            pieces.RemoveAt(selectorIndex);
            LayoutPieces();
            if (selectorIndex >= pieces.Count)
            {
                if (pieces.Count == 0)
                {
                    selector.gameObject.SetActive(false);
                }
                else
                {
                    MoveSelectorUp();
                }
            }
        }
    }

    public void LayoutPieces()
    {
        int count = 0;
        Vector3 current = new Vector3();
        foreach (QueuedPiece piece in pieces)
        {
            if (count > 0)
            {
                current += Vector3.down * spacing;
            }
            piece.transform.localPosition = current + piece.tetronimo.queueOffset;
            count++;
        }

        if (count > 0)
        {
            selector.gameObject.SetActive(true);
        }
    }

    public class QueuedPiece
    {
        public Transform transform;
        public Tetronimo tetronimo;
    }
}
