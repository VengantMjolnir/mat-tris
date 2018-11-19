using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTimer : MonoBehaviour {

    public List<Renderer> blocks = new List<Renderer>();
    public Material on;
    public Material off;
    public float tickInterval = 0.2f;
    public bool dropNow = false;

    private float lastTickTime;
    private int index;

    // Use this for initialization
    void Start () {
        ResetTimer();
	}

    public void ResetTimer()
    {
        foreach (Renderer block in blocks)
        {
            block.material = off;
        }

        index = 0;
        lastTickTime = Time.time;
        dropNow = false;
    }

	// Update is called once per frame
	void Update () {
        if (Time.time - lastTickTime > tickInterval)
        {
            if (index < blocks.Count)
            {
                blocks[index].material = on;
                lastTickTime = Time.time;
                index += 1;
            }
            else
            {
                dropNow = true;
            }
        }
	}
}
