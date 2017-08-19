﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction { None, Population, Migration, Diffusion};

public class Tile : MonoBehaviour {

    [HideInInspector]
    public Brick brick;

    [HideInInspector]
    public NutrientState nutrientState;

    List<PlayerPopulation> playerPopulations = new List<PlayerPopulation>();

    public PlayerPopulation GetPlayerPopulation(int playerId)
    {
        for (int i = 0, l = playerPopulations.Count; i < l; i++)
        {
            if (playerPopulations[i].playerId == playerId)
            {
                return playerPopulations[i];
            }
        }
        return null;
    }

    public int GetPopulationSize(int playerId)
    {
        PlayerPopulation pop = GetPlayerPopulation(playerId);
        return pop ? pop.Size : 0;
    }

    public bool HasPopulation(int playerId)
    {
        PlayerPopulation pop = GetPlayerPopulation(playerId);
        return pop ? pop.Size > 0 : false;
    }

    public void ShowSelectedPopAction(int playerId)
    {
        if (GetPlayerAction(playerId) == PlayerAction.Population)
        {
            var pop = GetPlayerPopulation(playerId);
            brick.ShowPopAction(pop == null ? ActionMode.None : pop.activeAction);
        }
    }

    public PlayerPopulationData GetSubSample(int playerId, int sample, out int realizedSample)
    {
        PlayerPopulation pop = GetPlayerPopulation(playerId);
        if (pop && pop.Size > 0)
        {
            return pop.GetDataSample(sample, out realizedSample);
        }
        realizedSample = 0;
        return null;

    }

    public void SetPopulationFromData(int playerId, PlayerPopulationData data)
    {
        PlayerPopulation pop = GetPlayerPopulation(playerId);
        pop.Clear();
        pop.SetFrom(data);
    }

    public void ClearPopulations()
    {
        foreach (PlayerPopulation pop in playerPopulations)
        {
            pop.Clear();
        }
    }

    public void SetPopulationSizeAndEnergy(int playerId, int size, int energy)
    {
        PlayerPopulation pop = GetPlayerPopulation(playerId);
        if (pop == null)
        {
            pop = gameObject.AddComponent<PlayerPopulation>();
            playerPopulations.Add(pop);
        }

        pop.SetSize(size);
        pop.SetEnergy(energy);
    }

    private void Start()
    {
        brick = GetComponentInChildren<Brick>();
        nutrientState = GetComponent<NutrientState>();
        SetNeighbours();
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

    bool HasNeighbour(Tile other)
    {
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] == other)
            {
                return true;
            }
        }
        return false;
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

    List<PlayerAction> selectedActions = new List<PlayerAction>();

    public void SetPlayerAction(int playerId, PlayerAction action)
    {
        while (selectedActions.Count <= playerId)
        {
            selectedActions.Add(PlayerAction.None);
        }
        selectedActions[playerId] = action;
    }

    public PlayerAction GetPlayerAction(int playerId)
    {
        if (selectedActions.Count <= playerId)
        {
            return PlayerAction.None;
        } else
        {
            return selectedActions[playerId];
        }
    }

    public void PlanDiffusion()
    {
        SetPlayerAction(Match.ActivePlayer, PlayerAction.Diffusion);
    }

    Tile migrationTarget;
    public void PlanMigration(Tile target)
    {
        SetPlayerAction(Match.ActivePlayer, PlayerAction.Migration);
        migrationTarget = target;
    }

    public void ClearPlan()
    {
        SetPlayerAction(Match.ActivePlayer, PlayerAction.None);
    }

    [SerializeField]
    int migrationFraction = 6;

    public void Migrate(Tile destination)
    {
        if (!HasNeighbour(destination))
        {
            throw new System.ArgumentException("Destination is not a neighbour");
        }

        for (int pId = 0, l = playerPopulations.Count; pId < l; pId++)
        {
            PlayerPopulation sourcePop = GetPlayerPopulation(pId);
            PlayerPopulation targetPop = destination.GetPlayerPopulation(pId);
            int migration = (sourcePop.Size - targetPop.Size) / migrationFraction;
            if (migration > 0)
            {
                int realizedSample;
                var subPop = sourcePop.GetDataSample(migration, out realizedSample);

                sourcePop.RemoveMigrants(subPop);
                targetPop.AddMigrants(subPop);
            }
        }
    }
}
