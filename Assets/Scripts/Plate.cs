using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PlayerTurn(Player player, int pId);

public class Plate : MonoBehaviour {

    public static PlayerTurn OnPlayerTurn;

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

        for (int i = 0; i < nPlayers; i++)
        {
            PlayerPopulationData pop = GetReplatePopulation(i, samples[i]);
            startTile.SetPopulationFromData(i, pop);
        }

        RemovePopulationsFromNonStart();
    }

    void RemovePopulationsFromNonStart()
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            if (tile == startTile)
            {
                continue;
            }

            tile.ClearPopulations();
        }
    }

    PlayerPopulationData GetReplatePopulation(int player, int sampleSize)
    {
        PlayerPopulationData pop = new PlayerPopulationData();
        float perTile = sampleSize / 16f;
        int soFar = 0;
        Tile[] tiles = GetComponentsInChildren<Tile>();
        while (soFar < sampleSize)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                int sample = Mathf.Min(Mathf.FloorToInt(perTile), sampleSize - soFar);
                int resultingSample;
                pop.Add(tiles[i].GetSubSample(player, sample, out resultingSample));
                soFar += resultingSample;
                if (soFar >= sampleSize)
                {
                    break;
                }
            }
        }
        return pop;
    }
    
    int[] GetSampleSizes(int[] totalSizes, int totalSize)
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
        if (OnPlayerTurn != null)
        {
            OnPlayerTurn(players[0], activePlayer);

        }
    }

    int activePlayer;
    Player[] players;

    private void Start()
    {
        players = Player.GetPlayers();
        StartGame(2);
    }
}
