using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
}

public class PlayerPopulation : AbsNutrientState {

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
            return importAA + importC + importN + export;
        }
    }

    int Budget(ref int param, int energy)
    {
        param += energy;
        if (param < 0)
        {
            energy += param;
            param = 0;
        }
        if (data.energy < PlanningEnergy)
        {
            int diff = data.energy - PlanningEnergy;
            energy += diff;
            param += diff;
        }
        
        return energy;
    }

    int importC;
    int importAA;
    int importN;
    public int PlanImport(Nutrients nutrient, int energy)
    {
        if (nutrient == Nutrients.AA)
        {            
            return Budget(ref importAA, energy);
        } else if (nutrient == Nutrients.C)
        {
            return Budget(ref importC, energy);
        } else
        {
            return Budget(ref importN, energy);
        }
    }

    int export;
    public int PlanExportWaste(int energy)
    {
        return Budget(ref export, energy);
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

    public int PlanMaintenance(int energy)
    {
        return Budget(ref maintenance, energy);
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

    int componentsToWaste = 4;
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
