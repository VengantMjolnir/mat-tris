using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Tetronimo : ScriptableObject {
    public float queueRotation;
    public Vector3 queueOffset;
    public float spawnRotation;
    public GameObject group;
}
