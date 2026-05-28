using System;
using System.Collections.Generic;

public class DiceManager
{
    private readonly List<DiceModel> currentDiceList = new List<DiceModel>();
    private DiceCheckType currentCheckType;
    private int currentDifficulty;
    private DiceCompareRule currentCompareRule;
    private bool currentAllowModifier;
    private readonly List<DiceModifierModel> usedModifierList = new List<DiceModifierModel>();
    private readonly List<DiceModifierTag> allowedModifierTags = new List<DiceModifierTag>();
    private bool isCheckFinished;
    private int rawTotalValue;
    private int finalTotalValue;
    private readonly Random random = new Random();

    public Action onCheckStarted;
    public Action<List<DiceModel>, int> onDiceRolled;
    public Action<List<DiceModifierModel>> onModifierOptionsGenerated;
    public Action<DiceCheckResultModel> onCheckFinished;

    public void StartCheck(DiceCheckType checkType, int diceCount, int difficulty, DiceCompareRule compareRule, bool allowModifier)
    {
        InitCheckData(checkType, difficulty, compareRule, allowModifier);
        CreateDicePool(diceCount, checkType);
        NotifyCheckStarted();
        RollAllDices();

        if (currentAllowModifier)
        {
            NotifyModifierOptionsGenerated(GetAvailableModifiers());
        }
    }

    public void StartOmenCheck(int omenCount, int diceCount)
    {
        StartCheck(DiceCheckType.Omen, diceCount, omenCount, DiceCompareRule.GreaterOrEqual, true);
    }

    public void InitCheckData(DiceCheckType checkType, int difficulty, DiceCompareRule compareRule, bool allowModifier)
    {
        ClearCurrentCheckData();
        currentCheckType = checkType;
        currentDifficulty = difficulty;
        currentCompareRule = compareRule;
        currentAllowModifier = allowModifier;
        isCheckFinished = false;
    }

    public void ClearCurrentCheckData()
    {
        currentDiceList.Clear();
        usedModifierList.Clear();
        allowedModifierTags.Clear();
        isCheckFinished = false;
        rawTotalValue = 0;
        finalTotalValue = 0;
    }

    public DiceCheckResultModel ConfirmCheckResult()
    {
        RefreshFinalTotalValue();
        DiceCheckResultModel result = GenerateCheckResult();
        isCheckFinished = true;
        NotifyCheckFinished(result);
        return result;
    }

    public void CancelCurrentCheck()
    {
        ClearCurrentCheckData();
    }

    public void CreateDicePool(int diceCount, DiceCheckType checkType)
    {
        CreateDicePool(diceCount, 1, 6, checkType);
    }

    public void CreateDicePool(int diceCount, int minValue, int maxValue, DiceCheckType checkType)
    {
        currentDiceList.Clear();
        string sourceId = checkType.ToString();

        for (int i = 0; i < diceCount; i++)
        {
            currentDiceList.Add(CreateDice(minValue, maxValue, sourceId));
        }
    }

    public void CreateDicePool(int diceCount, List<int> faces, DiceCheckType checkType)
    {
        currentDiceList.Clear();
        string sourceId = checkType.ToString();

        for (int i = 0; i < diceCount; i++)
        {
            currentDiceList.Add(CreateDice(faces, sourceId));
        }
    }

    public DiceModel CreateDice(int minValue, int maxValue, string sourceId)
    {
        DiceModel dice = new DiceModel();
        dice.Init(minValue, maxValue, sourceId);
        return dice;
    }

    public DiceModel CreateDice(List<int> faces, string sourceId)
    {
        DiceModel dice = new DiceModel();
        dice.Init(faces, sourceId);
        return dice;
    }

    public void AddDiceToCurrentPool(DiceModel dice)
    {
        if (dice != null)
        {
            currentDiceList.Add(dice);
        }
    }

    public void RemoveDiceFromCurrentPool(DiceModel dice)
    {
        currentDiceList.Remove(dice);
    }

    public DiceModel GetDiceById(string diceId)
    {
        for (int i = 0; i < currentDiceList.Count; i++)
        {
            if (currentDiceList[i].GetDiceId() == diceId)
            {
                return currentDiceList[i];
            }
        }

        return null;
    }

    public void RollAllDices()
    {
        for (int i = 0; i < currentDiceList.Count; i++)
        {
            RollDice(currentDiceList[i]);
        }

        rawTotalValue = GetTotalValue();
        RefreshFinalTotalValue();
        NotifyDiceRolled();
    }

    public void RollDice(DiceModel dice)
    {
        if (dice != null)
        {
            dice.Roll(random);
        }
    }

    public void RollDiceById(string diceId)
    {
        RollDice(GetDiceById(diceId));
    }

    public void RerollDice(DiceModel dice)
    {
        if (CanRerollDice(dice))
        {
            dice.Roll(random);
            RefreshFinalTotalValue();
            NotifyDiceRolled();
        }
    }

    public void RerollDiceById(string diceId)
    {
        RerollDice(GetDiceById(diceId));
    }

    public void RerollAllUnlockedDices()
    {
        for (int i = 0; i < currentDiceList.Count; i++)
        {
            DiceModel dice = currentDiceList[i];
            if (CanRerollDice(dice))
            {
                dice.Roll(random);
            }
        }

        RefreshFinalTotalValue();
        NotifyDiceRolled();
    }

    public bool CanRerollDice(DiceModel dice)
    {
        return dice != null && dice.CanReroll();
    }

    public void SetDiceValue(DiceModel dice, int value)
    {
        if (dice != null)
        {
            dice.SetValue(value);
            RefreshFinalTotalValue();
        }
    }

    public void SetDiceValueById(string diceId, int value)
    {
        SetDiceValue(GetDiceById(diceId), value);
    }

    public void AddDiceValue(DiceModel dice, int value)
    {
        if (dice != null)
        {
            dice.AddValue(value);
            RefreshFinalTotalValue();
        }
    }

    public void AddDiceValueById(string diceId, int value)
    {
        AddDiceValue(GetDiceById(diceId), value);
    }

    public void LockDice(DiceModel dice)
    {
        dice?.Lock();
    }

    public void UnlockDice(DiceModel dice)
    {
        dice?.Unlock();
    }

    public void LockAllDices()
    {
        for (int i = 0; i < currentDiceList.Count; i++)
        {
            currentDiceList[i].Lock();
        }
    }

    public void UnlockAllDices()
    {
        for (int i = 0; i < currentDiceList.Count; i++)
        {
            currentDiceList[i].Unlock();
        }
    }

    public List<DiceModifierModel> GetAllPossibleModifiers()
    {
        return new List<DiceModifierModel>();
    }

    public List<DiceModifierModel> GetAvailableModifiers()
    {
        List<DiceModifierModel> allModifiers = GetAllPossibleModifiers();
        List<DiceModifierModel> availableModifiers = new List<DiceModifierModel>();

        for (int i = 0; i < allModifiers.Count; i++)
        {
            if (CanUseModifier(allModifiers[i]))
            {
                availableModifiers.Add(allModifiers[i]);
            }
        }

        return availableModifiers;
    }

    public bool CanUseModifier(DiceModifierModel modifier)
    {
        return currentAllowModifier && modifier != null && modifier.CanUse() && IsModifierTagAllowed(modifier) && CanPayModifierCost(modifier);
    }

    public bool IsModifierTagAllowed(DiceModifierModel modifier)
    {
        if (modifier == null || allowedModifierTags.Count == 0)
        {
            return true;
        }

        if (modifier.modifierTags == null)
        {
            return false;
        }

        for (int i = 0; i < modifier.modifierTags.Count; i++)
        {
            if (allowedModifierTags.Contains(modifier.modifierTags[i]))
            {
                return true;
            }
        }

        return false;
    }

    public bool CanPayModifierCost(DiceModifierModel modifier)
    {
        return modifier == null || modifier.costType == DiceModifierCostType.None;
    }

    public void PayModifierCost(DiceModifierModel modifier)
    {
    }

    public void ApplyModifier(DiceModifierModel modifier, DiceModel targetDice)
    {
        if (!CanUseModifier(modifier))
        {
            return;
        }

        PayModifierCost(modifier);

        switch (modifier.modifierType)
        {
            case DiceModifierType.RerollDice:
                ApplyRerollModifier(modifier, targetDice);
                break;
            case DiceModifierType.AddValue:
                ApplyAddValueModifier(modifier, targetDice);
                break;
            case DiceModifierType.SetValue:
                ApplySetValueModifier(modifier, targetDice);
                break;
            case DiceModifierType.AddDice:
                ApplyAddDiceModifier(modifier);
                break;
            case DiceModifierType.ReduceDifficulty:
                ApplyReduceDifficultyModifier(modifier);
                break;
        }

        RecordUsedModifier(modifier);
        RefreshFinalTotalValue();
    }

    public void ApplyRerollModifier(DiceModifierModel modifier, DiceModel targetDice)
    {
        RerollDice(targetDice);
    }

    public void ApplyAddValueModifier(DiceModifierModel modifier, DiceModel targetDice)
    {
        int value = modifier.GetIntParam("value", 0);
        AddDiceValue(targetDice, value);
    }

    public void ApplySetValueModifier(DiceModifierModel modifier, DiceModel targetDice)
    {
        int value = modifier.GetIntParam("value", 0);
        SetDiceValue(targetDice, value);
    }

    public void ApplyAddDiceModifier(DiceModifierModel modifier)
    {
        int count = modifier.GetIntParam("count", 1);
        int minValue = modifier.GetIntParam("minValue", 1);
        int maxValue = modifier.GetIntParam("maxValue", 6);
        string sourceId = modifier.GetStringParam("sourceId", modifier.modifierId);

        for (int i = 0; i < count; i++)
        {
            DiceModel dice = CreateDice(minValue, maxValue, sourceId);
            dice.Roll(random);
            AddDiceToCurrentPool(dice);
        }
    }

    public void ApplyReduceDifficultyModifier(DiceModifierModel modifier)
    {
        currentDifficulty -= modifier.GetIntParam("value", 1);
    }

    public void RecordUsedModifier(DiceModifierModel modifier)
    {
        if (modifier == null)
        {
            return;
        }

        modifier.MarkUsed();
        usedModifierList.Add(modifier);
    }

    public int GetUsedModifierCount(string modifierId)
    {
        int count = 0;

        for (int i = 0; i < usedModifierList.Count; i++)
        {
            if (usedModifierList[i].modifierId == modifierId)
            {
                count++;
            }
        }

        return count;
    }

    public int GetTotalValue()
    {
        int total = 0;

        for (int i = 0; i < currentDiceList.Count; i++)
        {
            total += currentDiceList[i].GetCurrentValue();
        }

        return total;
    }

    public int GetRawTotalValue()
    {
        return rawTotalValue;
    }

    public int GetFinalTotalValue()
    {
        return finalTotalValue;
    }

    public void RefreshFinalTotalValue()
    {
        finalTotalValue = GetTotalValue();
    }

    public bool CheckSuccess()
    {
        switch (currentCompareRule)
        {
            case DiceCompareRule.GreaterOrEqual:
                return finalTotalValue >= currentDifficulty;
            case DiceCompareRule.LessThan:
                return finalTotalValue < currentDifficulty;
            case DiceCompareRule.Equal:
                return finalTotalValue == currentDifficulty;
            default:
                return false;
        }
    }

    public int CalculateMargin()
    {
        switch (currentCompareRule)
        {
            case DiceCompareRule.GreaterOrEqual:
                return finalTotalValue - currentDifficulty;
            case DiceCompareRule.LessThan:
                return currentDifficulty - finalTotalValue - 1;
            case DiceCompareRule.Equal:
                return Math.Abs(finalTotalValue - currentDifficulty);
            default:
                return 0;
        }
    }

    public DiceCheckResultModel GenerateCheckResult()
    {
        return new DiceCheckResultModel
        {
            checkType = currentCheckType,
            isSuccess = CheckSuccess(),
            rawTotalValue = rawTotalValue,
            finalTotalValue = finalTotalValue,
            difficulty = currentDifficulty,
            margin = CalculateMargin(),
            finalDiceList = new List<DiceModel>(currentDiceList),
            usedModifierList = new List<DiceModifierModel>(usedModifierList)
        };
    }

    public List<DiceModel> GetCurrentDiceList()
    {
        return currentDiceList;
    }

    public void NotifyCheckStarted()
    {
        onCheckStarted?.Invoke();
    }

    public void NotifyDiceRolled()
    {
        onDiceRolled?.Invoke(currentDiceList, GetTotalValue());
    }

    public void NotifyModifierOptionsGenerated(List<DiceModifierModel> modifiers)
    {
        onModifierOptionsGenerated?.Invoke(modifiers);
    }

    public void NotifyCheckFinished(DiceCheckResultModel result)
    {
        onCheckFinished?.Invoke(result);
    }
}
