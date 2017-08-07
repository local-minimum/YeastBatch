using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour {

    public void SetPlayerDominion(int player, float dominon)
    {

    }

    Tile tile;

    private void Start()
    {
        tile = GetComponentInParent<Tile>();
    }

    private void OnMouseEnter()
    {
        
    }

    private void OnMouseExit()
    {
        
    }
}
