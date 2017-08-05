using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour {

    [SerializeField]
    NutrientState maxMedia;

    [SerializeField]
    NutrientState minMedia;

    NutrientState currentMedia;

    public void SetMediaComposition()
    {
        if (currentMedia == null)
        {
            currentMedia = gameObject.AddComponent<NutrientState>();
            currentMedia.name = "Current";
        }
        SetMediaComposition(Nutrients.C);
        SetMediaComposition(Nutrients.N);
        SetMediaComposition(Nutrients.AA);
    }

    void SetMediaComposition(Nutrients nutrient)
    {
        currentMedia.SetMaxAndCurrent(nutrient,
            Random.Range(
                minMedia.GetMax(nutrient),
                maxMedia.GetMax(nutrient)
            ));
    }

    public void CastMedia()
    {
        foreach(Tile tile in GetComponentsInChildren<Tile>())
        {
            tile.SetMediaComposition(currentMedia);
            tile.SaturateMedia();
        }
    }

    [SerializeField]
    int startPop = 20;
    [SerializeField]
    int startEnergy = 200;

    public void InitiateBatch()
    {

    }

    [SerializeField] Tile startTile;

    public void StartGame(int nPlayers)
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            bool isStart = tile == startTile;
            for (int pId = 0; pId < nPlayers; pId++)
            {
                tile.SetPopulationSizeAndEnergy(0, isStart ? startPop : 0, isStart ? startEnergy : 0);
            }
        }
    }

    private void Start()
    {
        StartGame(2);
    }
}
