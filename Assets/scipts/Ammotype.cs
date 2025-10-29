using UnityEngine;

public enum AmmoType
{
    HE,
    APHE,
    AP,
    APCR,
    HEAT
}



public static class AmmoData
{
    /// <summary>
    /// Returns damage for a specific ammo type and target tank type
    /// </summary>
    public static int GetDamage(AmmoType type, TankType targetType)
    {
        switch (type)
        {
            case AmmoType.HE: return CalculateHE(targetType);
            case AmmoType.APHE: return CalculateAPHE(targetType);
            case AmmoType.AP: return CalculateAP(targetType);
            case AmmoType.APCR: return CalculateAPCR(targetType);
            case AmmoType.HEAT: return CalculateHEAT(targetType);
            default: return 0;
        }
    }

    private static int CalculateHE(TankType t)
    {
        float dmg = Random.value <= 0.2f ? Random.Range(400f, 500f) : Random.Range(200f, 300f);

        switch (t)
        {
            case TankType.Light: dmg *= 1.5f; break;   // +50%
            case TankType.Armoured: dmg *= 0.9f; break; // -10%
            case TankType.Heavy: dmg *= 0.7f; break;   // -30%
        }
        return Mathf.RoundToInt(dmg);
    }

    private static int CalculateAPHE(TankType t)
    {
        float dmg = Random.value <= 0.35f ? Random.Range(600f, 700f) : Random.Range(300f, 550f);

        switch (t)
        {
            case TankType.Light: dmg *= 0.9f; break;   // -10%
            case TankType.Armoured: dmg *= 1.2f; break; // +20%
            case TankType.Heavy: dmg *= 1.4f; break;    // +40%
        }
        return Mathf.RoundToInt(dmg);
    }

    private static int CalculateAP(TankType t)
    {
        float dmg = Random.value <= 0.4f ? Random.Range(500f, 600f) : Random.Range(250f, 400f);

        switch (t)
        {
            case TankType.Light: dmg *= 0.7f; break;    // -30%
            case TankType.Armoured: dmg *= 1.1f; break; // +10%
            case TankType.Heavy: dmg *= 1.3f; break;    // +30%
        }
        return Mathf.RoundToInt(dmg);
    }

    private static int CalculateAPCR(TankType t)
    {
        float dmg = Random.value <= 0.25f ? Random.Range(500f, 550f) : Random.Range(300f, 500f);

        switch (t)
        {
            case TankType.Light: dmg *= 0.6f; break;    // -40%
            case TankType.Armoured: dmg *= 1.2f; break; // +20%
            case TankType.Heavy: dmg *= 1.4f; break;    // +40%
        }
        return Mathf.RoundToInt(dmg);
    }

    private static int CalculateHEAT(TankType t)
    {
        float dmg = Random.value <= 0.5f ? Random.Range(300f, 350f) : Random.Range(350f, 700f);

        switch (t)
        {
            case TankType.Light: dmg *= 0.5f; break;    // -50%
            case TankType.Armoured: dmg *= 1.2f; break; // +20% 
            case TankType.Heavy: dmg *= 1.5f; break;    // +50%
        }
        return Mathf.RoundToInt(dmg);
    }
}