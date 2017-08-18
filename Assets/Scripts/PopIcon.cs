using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopIcon : MonoBehaviour {

    Tile tile;

    private void Start()
    {
        tile = GetComponentInParent<Tile>();
    }

    private void OnMouseEnter()
    {
        ClickYeast();
    }

    public void ClickYeast()
    {
        TileCanvas.ShowFor(tile);
    }
}
