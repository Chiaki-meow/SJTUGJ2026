using System.Collections.Generic;

public class DiceModifierModel
{
    public string modifierId;
    public string modifierName;
    public DiceModifierType modifierType;
    public List<DiceModifierTag> modifierTags = new List<DiceModifierTag>();
    public bool needTargetDice;
    public int maxUsePerCheck = 1;
    public int usedCount;
    public DiceModifierCostType costType;
    public int costAmount;
    public string costTargetId;
    public Dictionary<string, int> intParams = new Dictionary<string, int>();
    public Dictionary<string, string> stringParams = new Dictionary<string, string>();

    public bool CanUse()
    {
        return maxUsePerCheck <= 0 || usedCount < maxUsePerCheck;
    }

    public void MarkUsed()
    {
        usedCount++;
    }

    public void ResetUsedCount()
    {
        usedCount = 0;
    }

    public int GetIntParam(string key, int defaultValue)
    {
        return intParams != null && intParams.TryGetValue(key, out int value) ? value : defaultValue;
    }

    public string GetStringParam(string key, string defaultValue)
    {
        return stringParams != null && stringParams.TryGetValue(key, out string value) ? value : defaultValue;
    }
}
