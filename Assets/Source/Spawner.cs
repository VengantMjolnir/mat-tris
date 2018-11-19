using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Group SpawnTetronimo(Tetronimo piece)
    {
        GameObject go = Instantiate(piece.group,
                    transform.position,
                    Quaternion.Euler(0, 0, piece.spawnRotation)
        ) as GameObject;

        if (go != null)
        {
            Group group = go.GetComponent<Group>();
            return group;
        }
        return null;
    }
}
