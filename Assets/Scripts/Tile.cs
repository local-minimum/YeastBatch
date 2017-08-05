using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    [HideInInspector]
    public Brick brick;

    [HideInInspector]
    public NutrientState nutrientState;

    private void Start()
    {
        brick = GetComponentInChildren<Brick>();
        nutrientState = GetComponent<NutrientState>();
    }

    [SerializeField]
    Tile[] neighbours;

    [SerializeField, Range(0, 10)]
    float neighbourRadius = 1;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, neighbourRadius);
        Gizmos.color = Color.red;
        for (int i=0; i<neighbours.Length; i++)
        {
            Gizmos.DrawLine(transform.position, neighbours[i].transform.position);
        }
    }

    public void SetNeighbours() {
        List<Tile> neighbours = new List<Tile>();
        foreach (Tile tile in transform.parent.GetComponentsInChildren<Tile>())
        {
            if (tile == this)
            {
                continue;
            }

            if (Vector3.Distance(transform.position, tile.transform.position) < neighbourRadius)
            {
                neighbours.Add(tile);
            }
        }

        this.neighbours = neighbours.ToArray();
    }

    public void SetMediaComposition(NutrientState media)
    {
        if (nutrientState == null)
        {
            nutrientState = gameObject.AddComponent<NutrientState>();
        }
        nutrientState.CopyMax(media);
        nutrientState.CopyDiffusion(media);
    }

    public void SaturateMedia()
    {
        nutrientState.Saturate();
    }
}
