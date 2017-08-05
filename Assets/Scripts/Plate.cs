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
        int totalSize;
        int[] totalSizes = TotalPlayerPopulations(out totalSize);
        int[] samples = GetSampleSizes(totalSizes, totalSize);

    }

    public int[] GetSampleSizes(int[] totalSizes, int totalSize)
    {
        float fraction = (startPop * nPlayers) / totalSize;
        int[] sample = new int[nPlayers];
        for (int i = 0; i < nPlayers; i++)
        {
            sample[i] = Mathf.FloorToInt(fraction * totalSizes[i]) + Random.Range(-startPop / 10, startPop / 10);
        }
        return sample;
    }

    public int[] TotalPlayerPopulations(out int totalSize)
    {
        totalSize = 0;
        int[] totalSizes = new int[nPlayers];
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            for (int pId = 0; pId < nPlayers; pId++)
            {
                totalSizes[pId] += tile.GetPopulationSize(pId);
                totalSize += totalSizes[pId];
            }
        }
        return totalSizes;
    }

    [SerializeField] Tile startTile;
    int nPlayers;
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
        this.nPlayers = nPlayers;
    }

    private void Start()
    {
        StartGame(2);
    }
}
