using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ActionMode{None, Metabolism, Procreation};

public class PlayerPopulationData {

    public int energy = 0;

    public int populationSize = 0;
    public int waste = 0;
    public int damage = 0;

    public PlayerPopulationData SubSample(int sample, out int realizedSample)
    {
        realizedSample = Mathf.Min(sample, populationSize);
        float fraction = sample / (float) populationSize;
        PlayerPopulationData sampleData = new PlayerPopulationData();
        sampleData.populationSize = sample;
        sampleData.energy = Mathf.FloorToInt(fraction * energy);
        sampleData.waste = Mathf.FloorToInt(fraction * waste);
        sampleData.damage = Mathf.FloorToInt(fraction * damage);
        
        return sampleData;
    }

    public void Add(PlayerPopulationData other)
    {
        if (other == null)
        {
            return;
        }
        populationSize += other.populationSize;
        energy += other.energy;
        waste += other.waste;
        damage += other.damage;
    }

    public void Remove(PlayerPopulationData other)
    {
        populationSize = Mathf.Max(0, populationSize - other.populationSize);
        energy = Mathf.Max(0, energy - other.energy);
        waste = Mathf.Max(0, waste - other.waste);
        damage = Mathf.Max(0, damage - other.damage);
    }
}

public class PlayerPopulation : AbsNutrientState {
    enum MetabolismOptions { ImportAA, ImportN, ImportC, ExportWaste, Maintenance };
    public ActionMode activeAction;

    public int playerId;
    PlayerPopulationData data = new PlayerPopulationData();

    public PlayerPopulationData GetDataSample(int sample, out int realizedSample)
    {
        if (sample <= 0)
        {
            realizedSample = 0;
            return new PlayerPopulationData();
        }
        else
        {
            return data.SubSample(sample, out realizedSample);
        }
    }

    Tile tile;

    public void Clear()
    {
        data.populationSize = 0;
        data.waste = 0;
        data.energy = 0;
        data.damage = 0;
        GetNutrient(Nutrients.AA).Empty();
        GetNutrient(Nutrients.C).Empty();
        GetNutrient(Nutrients.N).Empty();

    }

    public void SetFrom(PlayerPopulationData other)
    {
        data.damage = other.waste;
        data.populationSize = other.populationSize;
        data.waste = other.waste;
        data.energy = other.energy;
    }

    public int GetWasteState()
    {
        return data.waste;
    }

    public int GetEnergyState()
    {
        return data.energy;
    }

    public int GetDamageState()
    {
        return data.damage;
    }

    public int Size
    {
        get
        {
            return data.populationSize;
        }
    }

    public void AddMigrants(PlayerPopulationData migrants)
    {
        data.Add(migrants);
    }

    public void RemoveMigrants(PlayerPopulationData migrants)
    {
        data.Remove(migrants);
    }

    public void SetSize(int size)
    {
        data.populationSize = size;
        GetNutrient(Nutrients.C, true).SetMax(size * 80);
        GetNutrient(Nutrients.N, true).SetMax(size * 20);
        GetNutrient(Nutrients.AA, true).SetMax(size * 10);
    }

    public void SetEnergy(int energy)
    {
        data.energy = energy;
    }

    public int PlanningEnergy
    {
        get
        {
            return importAA + importC + importN + export + maintenance;
        }
    }

    public int RemainingEnergy
    {
        get
        {
            return data.energy - PlanningEnergy;
        }
    }

    public int TotalEnergy
    {
        get
        {
            return data.energy;
        }
    }

    int Budget(ref int planned, int requested, bool clampSelf  = true)
    {
        if (requested < 0)
        {
            requested = 0;
            planned = 0;
        } else if (requested > data.energy)
        {
            requested = data.energy;
            planned = 0;
        } else
        {
            planned = requested;
        }

        if (clampSelf && data.energy < PlanningEnergy)
        {
            int diff = data.energy - PlanningEnergy;
            requested += diff;
            planned += diff;
        }
        
        return requested;
    }

    void ClampOthers(MetabolismOptions me)
    {
        var values = System.Enum.GetNames(typeof(MetabolismOptions));
        var myName = System.Enum.GetName(typeof(MetabolismOptions), me);
        var otherNames = values.Where(e => e != myName).ToArray();
        int costOthers = 0;
        List<int> costPerOther = new List<int>();
        int costMe = 0;
        foreach (var item in values)
        {
            if (item == myName)
            {
                costMe = GetPlanningCostOf(item);
            } else
            {
                int costOther = GetPlanningCostOf(item);
                costOthers += costOther;
                costPerOther.Add(costOther);
            }
        }

        int surplus = (costMe + costOthers) - data.energy;
        while (surplus > 0)
        {
            //Recursive larger and zero test too
            int costPerItem = Mathf.CeilToInt(surplus / costPerOther.Count(e => e > 0));
            for (int i = 0; i<costPerOther.Count; i++)
            {
                int removal = Mathf.Min(costPerOther[i], costPerItem);
                surplus--;
                ReducePlanningOf(otherNames[i], removal);
            }        
        }
    }


    public int GetPlanningCostOf(string name)
    {
        switch (name)
        {
            case "ImportAA":
                return importAA;
            case "ImportC":
                return importC;
            case "ImportN":
                return importN;
            case "ExportWaste":
            case "Export":
                return export;
            case "Maintenance":
                return maintenance;
            default:
                throw new System.ArgumentException("Unknown planning cost: " + name);
        }
    }

    void ReducePlanningOf(string name, int ammount)
    {
        switch (name)
        {
            case "ImportAA":
                importAA -= ammount;
                break;
            case "ImportC":
                importC -= ammount;
                break;
            case "ImportN":
                importN -= ammount;
                break;
            case "ExportWaste":
                export -= ammount;
                break;
            case "Maintenance":
                maintenance -= ammount;
                break;
            default:
                throw new System.ArgumentException("Unknown planning cost item: " + name);
        }
    }

    int importC;
    int importAA;
    int importN;

    public enum ClampMode { Self, Others}
    public int PlanImport(Nutrients nutrient, int requested, ClampMode mode)
    {
        int allowed;
        MetabolismOptions meOp;
        if (nutrient == Nutrients.AA)
        {            
            allowed = Budget(ref importAA, requested, mode == ClampMode.Self);
            meOp = MetabolismOptions.ImportAA;
        } else if (nutrient == Nutrients.C)
        {
            allowed = Budget(ref importC, requested, mode == ClampMode.Self);
            meOp = MetabolismOptions.ImportC;
        } else
        {
            allowed = Budget(ref importN, requested, mode == ClampMode.Self);
            meOp = MetabolismOptions.ImportN;
        }

        if (mode == ClampMode.Others)
        {
            ClampOthers(meOp);
        }
        return allowed;
    }

    int export;
    public int PlanExportWaste(int requested, ClampMode mode)
    {
        int allowed = Budget(ref export, requested, mode == ClampMode.Self);
        if (mode == ClampMode.Others)
        {
            ClampOthers(MetabolismOptions.ExportWaste);
        }
        return allowed;
    }

    public void Metabolize()
    {
        ImportNutrients();

        CreateEnergy();

        MakeWaste();
        ExportWaste();

        CalculateDamage();
    }

    int maintenance;

    public int PlanMaintenance(int requested, ClampMode mode)
    {
        int allowed = Budget(ref maintenance, requested, mode == ClampMode.Self);
        if (mode == ClampMode.Others)
        {
            ClampOthers(MetabolismOptions.Maintenance);
        }
        return allowed;
    }

    public void Procreate()
    {
        CalculateDamage();
        int energyToIndiviuals = 5;
        int energy = Mathf.Clamp(data.energy - data.damage, 0, data.populationSize * energyToIndiviuals);
        data.populationSize += energy / energyToIndiviuals;
        data.energy = Mathf.Max(0, data.energy - energy / 2);

        MakeWaste();

    }


    void MakeWaste()
    {
        MediaNutrient C = GetNutrient(Nutrients.C);
        data.waste += C.Extract(C.CurrentValue / 4);

        MediaNutrient N = GetNutrient(Nutrients.N);
        data.waste += N.Extract(C.CurrentValue / 3);

        MediaNutrient AA = GetNutrient(Nutrients.AA);
        data.waste += AA.Extract(AA.CurrentValue / 2);
    }

    void ExportWaste()
    {
        data.waste = Mathf.RoundToInt(Mathf.Max(0, data.waste - export * exportPerMaint));

        export = 0;
    }

    float exportPerMaint = 0.3f;
    float dmgReductionPerMaint = 0.8f;

    int wasteToDamage = 3;

    void CalculateDamage()
    {
        data.damage += Mathf.RoundToInt(data.waste / (float) wasteToDamage);
        data.damage += Mathf.RoundToInt(data.populationSize / 100f);
        data.damage = Mathf.RoundToInt(Mathf.Max(0, data.damage - maintenance * dmgReductionPerMaint));
    }

    void CreateEnergy()
    {
        MediaNutrient C = GetNutrient(Nutrients.C, true);
        MediaNutrient N = GetNutrient(Nutrients.N, true);
        MediaNutrient AA = GetNutrient(Nutrients.AA, true);
        int cFactor = 5;
        int nFactor = 2;
        int aFactor = 1;

        int production = Mathf.Min(
            C.CurrentValue / cFactor,
            N.CurrentValue / nFactor,
            AA.CurrentValue / aFactor);

        C.Extract(production * cFactor);
        N.Extract(production * nFactor);
        AA.Extract(production * aFactor);

        data.energy += production * 10;        
    }

    void ImportNutrients()
    {
        ImportNutrient(GetNutrient(Nutrients.AA), ref importAA);
        ImportNutrient(GetNutrient(Nutrients.C), ref importC);
        ImportNutrient(GetNutrient(Nutrients.N), ref importN);
    }

    void ImportNutrient(MediaNutrient nutrient, ref int energy)
    {   if (energy == 0)
        {
            return;
        }
        int extracted = tile.nutrientState.Extract(nutrient.nutrient, Mathf.RoundToInt(energy * (1 + data.populationSize / 80f)));
        int surplus;
        nutrient.Deposit(extracted, out surplus);
        tile.nutrientState.Deposit(nutrient.nutrient, surplus);
        Debug.Log(string.Format(
            "Import of {0} using {1} energy, extracted {2} which when deposited caused {3} surplus",
            nutrient, energy, extracted, surplus));
    }

    private void Start()
    {
        tile = GetComponent<Tile>();
    }
}
