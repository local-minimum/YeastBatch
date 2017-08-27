using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopIcon : MonoBehaviour {

    Tile tile;

    private void Start()
    {
        tile = GetComponentInParent<Tile>();
    }

    bool hovered = false;

    private void OnMouseEnter()
    {
        //ClickYeast();
        hovered = true;
    }

    private void OnMouseExit()
    {
        hovered = false;
    }

    public void ClickYeast()
    {
        //TileCanvas.ShowFor(tile);
        //Brick.SetLeftSelect(tile.brick);
        Debug.Log("Click");
    }

    private void Update()
    {
        if (hovered && Input.GetMouseButtonDown(0))
        {
            ClickYeast();
        }
    }
}
