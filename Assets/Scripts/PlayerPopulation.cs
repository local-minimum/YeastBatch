using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ActionMode{Metabolism, Procreation};

public class PlayerPopulationData {

    public int energy = 0;

    public int populationSize = 0;
    public int waste = 0;
    public int damage = 0;

    public PlayerPopulationData SubSample(int sample, out int realizedSample)
    {
        realizedSample = Mathf.Min(sample, populationSize);
        float fraction = sample / populationSize;
        PlayerPopulationData sampleData = new PlayerPopulationData();
        sampleData.populationSize = sample;
        sampleData.energy = Mathf.FloorToInt(fraction * energy);
        sampleData.waste = Mathf.FloorToInt(fraction * waste);
        sampleData.damage = Mathf.FloorToInt(fraction * damage);

        return sampleData;
    }

    public void Add(PlayerPopulationData other)
    {
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
        return data.SubSample(sample, out realizedSample);
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
        CalculateDamage();

        CreateEnergy();

        MakeWaste();
        ExportWaste();

        ImportNutrients();

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

        data.energy = Mathf.Clamp(data.energy - data.damage, 0, data.populationSize);
        data.populationSize += data.energy / 10;
        data.energy /= 4;

        MakeWaste();

        export = 0;
        importAA = 0;
        importC = 0;
        importN = 0;
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
        data.waste = Mathf.Max(0, data.waste - export);
        export = 0;
    }

    int wasteToDamage = 3;

    void CalculateDamage()
    {
        data.damage += data.waste / wasteToDamage;
        data.damage += data.populationSize / 100;
        data.damage = Mathf.Max(0, data.damage - maintenance);
        maintenance = 0;
    }

    void CreateEnergy()
    {
        MediaNutrient C = GetNutrient(Nutrients.C);
        MediaNutrient N = GetNutrient(Nutrients.N);
        MediaNutrient AA = GetNutrient(Nutrients.AA);

        int production = Mathf.Min(
            C.CurrentValue / 20,
            N.CurrentValue / 3,
            AA.CurrentValue / 1);

        C.Extract(production * 20);
        N.Extract(production * 3);
        AA.Extract(production);

        data.energy += production;        
    }

    void ImportNutrients()
    {
        ImportNutrient(GetNutrient(Nutrients.AA), ref importAA);
        ImportNutrient(GetNutrient(Nutrients.C), ref importC);
        ImportNutrient(GetNutrient(Nutrients.N), ref importN);
    }

    void ImportNutrient(MediaNutrient nutrient, ref int energy)
    {
        int extracted = tile.nutrientState.Extract(nutrient.nutrient, energy * (1 + data.populationSize / 80));
        int surplus;
        nutrient.Deposit(extracted, out surplus);
        tile.nutrientState.Deposit(nutrient.nutrient, surplus);
        energy = 0;
    }

    private void Start()
    {
        tile = GetComponent<Tile>();
    }
}
