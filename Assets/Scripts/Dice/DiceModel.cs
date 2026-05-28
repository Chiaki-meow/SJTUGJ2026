using System;
using System.Collections.Generic;

public class DiceModel
{
    private string diceId;
    private string diceTypeId;
    private int minValue;
    private int maxValue;
    private List<int> faces;
    private int currentValue;
    private int previousValue;
    private bool hasRolled;
    private bool isLocked;
    private bool canReroll;
    private string sourceId;
    private string description;

    public string DiceTypeId
    {
        get => diceTypeId;
        set => diceTypeId = value;
    }

    public string Description
    {
        get => description;
        set => description = value;
    }

    public int MinValue => minValue;
    public int MaxValue => maxValue;
    public IReadOnlyList<int> Faces => faces;

    public void Init()
    {
        Init(1, 6, string.Empty);
    }

    public void Init(int minValue, int maxValue, string sourceId)
    {
        diceId = Guid.NewGuid().ToString("N");
        diceTypeId = "basic";
        this.minValue = minValue;
        this.maxValue = maxValue;
        faces = null;
        this.sourceId = sourceId;
        description = string.Empty;
        Reset();
    }

    public void Init(List<int> faces, string sourceId)
    {
        diceId = Guid.NewGuid().ToString("N");
        diceTypeId = "basic";
        this.faces = faces != null ? new List<int>(faces) : new List<int>();
        minValue = 0;
        maxValue = this.faces.Count > 0 ? this.faces.Count - 1 : 0;
        this.sourceId = sourceId;
        description = string.Empty;
        Reset();
    }

    public void Reset()
    {
        currentValue = 0;
        previousValue = 0;
        hasRolled = false;
        isLocked = false;
        canReroll = true;
    }

    public int Roll(Random random)
    {
        if (!CanRoll())
        {
            return currentValue;
        }

        previousValue = currentValue;
        currentValue = faces != null && faces.Count > 0 ? RollByFaces(random) : RollByRange(random);
        MarkRolled();
        return currentValue;
    }

    public int RollByRange(Random random)
    {
        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        return random.Next(minValue, maxValue + 1);
    }

    public int RollByFaces(Random random)
    {
        if (random == null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        if (faces == null || faces.Count == 0)
        {
            return 0;
        }

        return faces[random.Next(0, faces.Count)];
    }

    public void SetValue(int value)
    {
        previousValue = currentValue;
        currentValue = value;
        MarkRolled();
    }

    public void AddValue(int value)
    {
        previousValue = currentValue;
        currentValue += value;
        MarkRolled();
    }

    public void SetPreviousValue(int value)
    {
        previousValue = value;
    }

    public void RestorePreviousValue()
    {
        currentValue = previousValue;
        MarkRolled();
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    public void SetCanReroll(bool value)
    {
        canReroll = value;
    }

    public void MarkRolled()
    {
        hasRolled = true;
    }

    public void MarkUnrolled()
    {
        hasRolled = false;
    }

    public bool CanRoll()
    {
        return !isLocked;
    }

    public bool CanReroll()
    {
        return hasRolled && !isLocked && canReroll;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public bool HasRolled()
    {
        return hasRolled;
    }

    public int GetCurrentValue()
    {
        return currentValue;
    }

    public int GetPreviousValue()
    {
        return previousValue;
    }

    public string GetDiceId()
    {
        return diceId;
    }

    public string GetSourceId()
    {
        return sourceId;
    }
}
