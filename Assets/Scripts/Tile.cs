using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerAction { None, Population, Migration, Diffusion};

public class Tile : MonoBehaviour {

    static public bool allowDiffusionAsAction = false;
    static public bool allowProcreationAsAction = false;

    [HideInInspector]
    public Brick brick;

    NutrientState _nutrientState;

    public NutrientState nutrientState
    {
        get
        {
            if (_nutrientState == null)
            {
                _nutrientState = GetComponent<NutrientState>();
            }
            return _nutrientState;
        }
    }

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
        playerPopulations.AddRange(
            GetComponents<PlayerPopulation>()
                .Where(e => !playerPopulations.Contains(e)));
        PlayerPopulation pop = playerPopulations.FirstOrDefault(e => e.playerId == playerId);
        if (pop == null)
        {
            pop = gameObject.AddComponent<PlayerPopulation>();
            pop.playerId = playerId;
            playerPopulations.Add(pop);
        }
        return pop;
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

    public List<int> GetPlayerIndexBySize()
    {
        List<KeyValuePair<int, int>> sizes = new List<KeyValuePair<int, int>>(); 
        for (int i=0,l=Match.TotalPlayers; i<l; i++)
        {
            if (HasPopulation(i))
            {
                sizes.Add(new KeyValuePair<int, int>(i, GetPlayerPopulation(i).Size));
            }
        }
        return sizes.OrderBy(e => e.Value).Select(e => e.Key).ToList();
    }

    public int GetTotalPopulationSize()
    {
        int total = 0;
        for (int i = 0, l = Match.TotalPlayers; i < l; i++)
        {
            total += GetPopulationSize(i);
        }
        return total;
    }

    public void ShowSelectedPopAction(int playerId)
    {
        var playerAction = GetPlayerAction(playerId);
        if (playerAction == PlayerAction.Population)
        {
            var pop = GetPlayerPopulation(playerId);
            brick.ShowPopAction(pop == null ? ActionMode.None : pop.activeAction);
        }
        else if (playerAction == PlayerAction.Diffusion)
        {
            brick.ShowDiffusion();
        }
        else if (playerAction == PlayerAction.Migration)
        {
            brick.ShowMigration(GetMigrationTarget(playerId));
        } else
        {
            brick.ClearPlanIllustration();
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
        SetNeighbours();
    }

    [SerializeField]
    Tile[] neighbours;

    [SerializeField, Range(0, 10)]
    float neighbourRadius = 1;

    public bool IsNeighbour(Tile tile)
    {
        return neighbours.Contains(tile);
    }

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
            _nutrientState = gameObject.AddComponent<NutrientState>();
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

    public void DiffuseMedia()
    {
        nutrientState.CauseDiffusion(neighbours.Select(n => n.nutrientState).ToArray());
    }

    List<Tile> migrationTargets = new List<Tile>();
    public void PlanMigration(Tile target)
    {
        if (!IsNeighbour(target))
        {
            return;
        }

        SetPlayerAction(Match.ActivePlayer, PlayerAction.Migration);
        while (Match.ActivePlayer >= migrationTargets.Count)
        {
            migrationTargets.Add(null);
        }
        migrationTargets[Match.ActivePlayer] = target;
    }

    public Tile GetMigrationTarget(int playerId)
    {
        if (playerId < migrationTargets.Count)
        {
            return migrationTargets[playerId];
        }
        return null;
    }

    public void ClearPlan()
    {
        SetPlayerAction(Match.ActivePlayer, PlayerAction.None);
    }

    [SerializeField]
    int migrationFraction = 6;

    public void Migrate(int playerId)
    {
        Tile migrationTarget = GetMigrationTarget(playerId);

        if (!HasNeighbour(migrationTarget) || migrationTarget == null)
        {
            throw new System.ArgumentException(string.Format("Destination {0} is not a neighbour for player {1}", migrationTarget, playerId));
        }

        for (int pId = 0, l = playerPopulations.Count; pId < l; pId++)
        {
            PlayerPopulation sourcePop = GetPlayerPopulation(pId);
            PlayerPopulation targetPop = migrationTarget.GetPlayerPopulation(pId);
            int migration = (sourcePop.Size - targetPop.Size) / migrationFraction;
            if (migration > 0)
            {
                int realizedSample;
                var subPop = sourcePop.GetDataSample(migration, out realizedSample);

                sourcePop.RemoveMigrants(subPop);
                targetPop.AddMigrants(subPop);
                targetPop.SetSize(targetPop.Size);

                //Debug.Log("Migrated " + subPop.populationSize + " (" + migration + "/" + realizedSample + ") units for Player " + pId + " from " + name + " to " + migrationTarget.name);
            }
        }
    }

    public void SetDominionColor(Color playerOne, Color playerTwo)
    {
        int popOne = GetPopulationSize(0);
        int popTwo = GetPopulationSize(1);

        Color dominionColor;
        if (popOne == 0 && popTwo == 0)
        {
            dominionColor = Color.white;
        } else if (popOne == popTwo)
        {
            dominionColor = Color.gray;
        } else if (popOne > popTwo)
        {
            dominionColor = playerOne;
        } else
        {
            dominionColor = playerTwo;
        }
        brick.SetDominionColor(dominionColor);
    }
}
 