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
        hovered = true;
    }

    private void OnMouseExit()
    {
        hovered = false;
    }

    public void ClickYeast()
    {
        UIPopAction.ShowFor(tile);
    }

    private void Update()
    {
        if (hovered && !UIPopAction.Showing && Input.GetMouseButtonDown(0))
        {
            ClickYeast();
        }
    }
}
