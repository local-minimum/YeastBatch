using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Nutrients { C, N, AA};

[System.Serializable]
public class MediaNutrient
{
    public string name;
    public Nutrients nutrient;

    [SerializeField]
    int maxValue;

    [SerializeField]
    int currentValue;

    public int CurrentValue
    {
        get
        {
            return currentValue;
        }
    }

    public int MaxValue
    {
        get
        {
            return maxValue;
        }
    }

    public float saturation
    {
        get
        {
            return currentValue / maxValue;
        }
    }

    public void Saturate()
    {
        currentValue = maxValue;
    }

    public int Extract(int volume)
    {
        volume = Mathf.Min(currentValue, volume);
        currentValue -= volume;
        return volume;
    }

    public int Extract(float volume)
    {
        return Extract(Mathf.FloorToInt(volume * currentValue));
    }

    public void Deposit(int volume, out int surplus)
    {
        surplus = Mathf.Max(0, volume - (maxValue - currentValue));
        currentValue = Mathf.Min(maxValue, currentValue + volume);
    }

    public void Deposit(int volume)
    {
        currentValue = Mathf.Min(maxValue, currentValue + volume);
    }

    public void SetMax(int max)
    {
        this.maxValue = max;
    }

}

public abstract class AbsNutrientState : MonoBehaviour
{
    public string name;

    public static string NutrientToName(Nutrients nutrient)
    {
        switch (nutrient)
        {
            case Nutrients.AA:
                return "Amino Acids";
            case Nutrients.C:
                return "Carbon";
            case Nutrients.N:
                return "Nitrogen";
        }
        return "";
    }

    [SerializeField]
    protected List<MediaNutrient> nutrients = new List<MediaNutrient>();

    protected MediaNutrient GetNutrient(Nutrients nutrient, bool addIfMissing = false)
    {
        for (int i = 0, l = nutrients.Count; i < l; i++)
        {
            if (nutrients[i].nutrient == nutrient)
            {
                return nutrients[i];
            }
        }

        if (addIfMissing)
        {
            MediaNutrient mediaNutrient = new MediaNutrient();
            mediaNutrient.nutrient = nutrient;
            mediaNutrient.name = NutrientToName(nutrient);
            nutrients.Add(mediaNutrient);
            return mediaNutrient;
        }

        return null;
    }

    public void CopyMax(NutrientState media)
    {
        for (int i = 0, l = media.nutrients.Count; i < l; i++)
        {
            MediaNutrient template = media.nutrients[i];
            MediaNutrient target = GetNutrient(template.nutrient, true);
            target.SetMax(template.MaxValue);
        }
    }

    public int GetMax(Nutrients nutrient)
    {
        return GetNutrient(nutrient).MaxValue;
    }

    public void Saturate()
    {
        for (int i = 0, l = nutrients.Count; i < l; i++)
        {
            nutrients[i].Saturate();
        }
    }

    public void SetMaxAndCurrent(Nutrients nutrient, int max)
    {
        MediaNutrient mediaNutrient = GetNutrient(nutrient, true);
        mediaNutrient.SetMax(max);
        mediaNutrient.Saturate();
    }
}

public class NutrientState : AbsNutrientState {

    #region Extraction

    public int Extract(Nutrients nutrient, int energy)
    {
        MediaNutrient mediaNutrient = GetNutrient(nutrient);
        int extraction = GetExtractionVolume(mediaNutrient, energy);
        return mediaNutrient.Extract(extraction);
    }

    int GetExtractionVolume(MediaNutrient nutrient, int energy)
    {
        return Mathf.FloorToInt(Mathf.Min(energy * nutrient.saturation, nutrient.CurrentValue));
    }

    #endregion

    #region Deposition

    public void Deposit(Nutrients nutrient, int volume)
    {
        GetNutrient(nutrient).Deposit(volume);
    }
    #endregion

    #region Diffusion

    [SerializeField, Range(0, 1)]
    float diffusionFactor = 0.2f;

    public void CopyDiffusion(NutrientState media)
    {
        diffusionFactor = media.diffusionFactor;
    }

    public void CauseDiffusion(NutrientState[] others)
    {
        int nNutrients = nutrients.Count;
        int nOthers = others.Length;

        for (int i = 0; i < nNutrients; i++)
        {
            int own = nutrients[i].Extract(diffusionFactor);
            int ownPerOther = own / 6;
            for (int j = 0; j < nOthers; j++)
            {
                MediaNutrient otherNutrient = others[j].GetNutrient(nutrients[i].nutrient);
                int surplus = 0;
                nutrients[i].Deposit(otherNutrient.Extract(diffusionFactor / 6), out surplus);
                otherNutrient.Deposit(ownPerOther + surplus, out surplus);
            }
        }
    }

    #endregion
}
