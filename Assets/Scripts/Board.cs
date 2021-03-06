﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour {

    [SerializeField]
    NutrientState maxMedia;

    [SerializeField]
    NutrientState minMedia;

    NutrientState currentMedia;

    public void SetMediaComposition()
    {
        foreach (var media in GetComponents<NutrientState>())
        {
            if (media.name == "Current")
            {
                currentMedia = media;
                break;
            }
        }

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

    public void InitiateBatch(int nPlayers)
    {

        int totalSize;
        int[] totalSizes = TotalPlayerPopulations(out totalSize, nPlayers);
        int[] samples = GetSampleSizes(totalSizes, totalSize);
        PlayerPopulationData[] pops = new PlayerPopulationData[samples.Length];

        for (int i = 0; i < nPlayers; i++)
        {
            pops[i] = GetReplatePopulation(i, samples[i]);
        }
        RemovePopulations();
        for (int i = 0; i < nPlayers; i++)
        {
            startTiles[i].SetPopulationFromData(i, pops[i]);
        }

    }

    void RemovePopulations()
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {            

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
        int nPlayers = totalSizes.Length;
        float fraction = (startPop * nPlayers) / totalSize;
        int[] sample = new int[nPlayers];
        for (int i = 0; i < nPlayers; i++)
        {
            sample[i] = Mathf.FloorToInt(fraction * totalSizes[i]) + Random.Range(-startPop / 10, startPop / 10);
        }
        return sample;
    }

    public int[] TotalPlayerPopulations(out int totalSize, int nPlayers)
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

    public int[,] GetPlayerPopulationsPerTile()
    {
        Tile[] tiles = GetComponentsInChildren<Tile>();
        int nTiles = tiles.Length;
        int players = Match.TotalPlayers;
        int[,] pops = new int[nTiles, players];
        for (int idTile = 0; idTile < nTiles; idTile++)
        {
            for (int player = 0; player < players; player++)
            {
                pops[idTile, player] = tiles[idTile].GetPlayerPopulation(player).Size;
            }
        }

        return pops;
    }

    [SerializeField] Tile[] startTiles;

    public void SetupGame(int nPlayers)
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            for (int pId = 0; pId < nPlayers; pId++)
            {
                bool isStart = startTiles[pId] == tile;
                if (isStart)
                {
                    Debug.Log("Player " + pId + " starts at " + tile.name);
                }
                tile.SetPopulationSizeAndEnergy(pId, isStart ? startPop : 0, isStart ? startEnergy : 0);
            }
        }
    }

    public void EnactMetabolism()
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            foreach (int playerId in tile.GetPlayerIndexBySize())
            {
                if (tile.GetPlayerAction(playerId) == PlayerAction.Population)
                {
                    PlayerPopulation pop = tile.GetPlayerPopulation(playerId);
                    if (pop.activeAction == ActionMode.Metabolism)
                    {
                        Debug.Log("Metabolize " + playerId + " on tile " + tile.name);
                        pop.Metabolize();
                    }
                }
            }
        }
    }

    public void EnactProcreation()
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            foreach (int playerId in tile.GetPlayerIndexBySize())
            {
                if (tile.GetPlayerAction(playerId) == PlayerAction.Population || !Tile.allowProcreationAsAction)
                {
                    PlayerPopulation pop = tile.GetPlayerPopulation(playerId);
                    if (pop.activeAction == ActionMode.Procreation || !Tile.allowProcreationAsAction)
                    {
                        //Debug.Log("Procreate " + playerId + " on tile " + tile.name);
                        pop.Procreate();
                    }
                }
            }
        }
    }

    public void EnactMigration()
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>().OrderBy(e => -e.GetTotalPopulationSize()))
        {
            for (int i = 0, l = Match.TotalPlayers; i < l; i++)
            {
                if (tile.GetPlayerAction(i) == PlayerAction.Migration)
                {
                    tile.Migrate(i);
                }
            }
        }
    }

    public void EnactDiffusion()
    {
        if (Tile.allowDiffusionAsAction)
        {
            foreach (Tile tile in GetComponentsInChildren<Tile>().OrderBy(e => e.GetTotalPopulationSize()))
            {
                for (int i = 0, l = Match.TotalPlayers; i < l; i++)
                {
                    if (tile.GetPlayerAction(i) == PlayerAction.Diffusion)
                    {
                        tile.DiffuseMedia();
                    }
                }

            }
        } else
        {
            int diffusers = 2;
            foreach (Tile tile in GetComponentsInChildren<Tile>()
                .Select(e => new { rnd = Random.value, tile = e })
                .OrderBy(e => e.rnd)
                .Select(e => e.tile)
                .Take(diffusers))
            {
                tile.DiffuseMedia();
            }
        }        
    }

    public void SetDominionColors(Color playerOne, Color playerTwo)
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            tile.SetDominionColor(playerOne, playerTwo);
        }
    }

    public void RebalanceMetabolism()
    {
        for (int playerID = 0; playerID < Match.TotalPlayers; playerID++)
        {
            foreach (Tile tile in GetComponentsInChildren<Tile>())
            {
                if (tile.HasPopulation(playerID))
                {
                    UIPopAction.Rebalance(tile, playerID);
                }
            }
        }

    }
}
