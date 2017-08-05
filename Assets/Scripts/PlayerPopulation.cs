using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPopulation : AbsNutrientState {

    public int playerId;

    int energy = 0;
    
    int populationSize = 0;
    int waste = 0;
    int damage = 0;

    Tile tile;

    public int Size
    {
        get
        {
            return populationSize;
        }
    }

    public void SetSize(int size)
    {
        populationSize = size;
    }

    public void SetEnergy(int energy)
    {
        this.energy = energy;
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
        if (this.energy < PlanningEnergy)
        {
            int diff = this.energy - PlanningEnergy;
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

        energy = Mathf.Clamp(energy - damage, 0, populationSize);
        populationSize += energy / 10;
        energy /= 4;

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
        waste += C.Extract(C.CurrentValue / 4);

        MediaNutrient N = GetNutrient(Nutrients.N);
        waste += N.Extract(C.CurrentValue / 3);

        MediaNutrient AA = GetNutrient(Nutrients.AA);
        waste += AA.Extract(AA.CurrentValue / 2);
    }

    void ExportWaste()
    {
        waste = Mathf.Max(0, waste - export);
        export = 0;
    }

    int wasteToDamage = 3;

    void CalculateDamage()
    {
        damage += waste / wasteToDamage;
        damage += populationSize / 100;
        damage = Mathf.Max(0, damage - maintenance);
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

        energy += production;        
    }

    void ImportNutrients()
    {
        ImportNutrient(GetNutrient(Nutrients.AA), ref importAA);
        ImportNutrient(GetNutrient(Nutrients.C), ref importC);
        ImportNutrient(GetNutrient(Nutrients.N), ref importN);
    }

    void ImportNutrient(MediaNutrient nutrient, ref int energy)
    {
        int extracted = tile.nutrientState.Extract(nutrient.nutrient, energy * populationSize / 80);
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
