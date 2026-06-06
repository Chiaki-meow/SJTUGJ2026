using System.Collections.Generic;

public class DiceCheckResultModel
{
    public DiceCheckType checkType;
    public bool isSuccess;
    public int rawTotalValue;
    public int finalTotalValue;
    public int difficulty;
    public int margin;
    public List<DiceModel> finalDiceList;
    public List<DiceModifierModel> usedModifierList;
}
