using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Dice
{
    public class DiceCheckDebugPanel : MonoBehaviour
    {
        [SerializeField] private DicePanelView dicePanelView;
        [SerializeField] private Button physicalCheckButton;
        [SerializeField] private Button mentalCheckButton;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private int diceCount = 3;
        [SerializeField] private int physicalDifficulty = 4;
        [SerializeField] private int mentalDifficulty = 4;
        [SerializeField] private global::DiceCompareRule compareRule = global::DiceCompareRule.GreaterOrEqual;
        [SerializeField] private bool allowModifier;

        private bool hasListeners;

        private void OnEnable()
        {
            ResolveReferences();
            AddListeners();
        }

        private void Start()
        {
            ResolveReferences();
            AddListeners();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void ResolveReferences()
        {
            if (dicePanelView == null)
            {
                dicePanelView = FindObjectOfType<DicePanelView>(true);
            }
        }

        private void AddListeners()
        {
            if (hasListeners)
                return;

            if (physicalCheckButton != null)
            {
                physicalCheckButton.onClick.AddListener(StartPhysicalCheck);
            }

            if (mentalCheckButton != null)
            {
                mentalCheckButton.onClick.AddListener(StartMentalCheck);
            }

            hasListeners = true;
        }

        private void RemoveListeners()
        {
            if (!hasListeners)
                return;

            if (physicalCheckButton != null)
            {
                physicalCheckButton.onClick.RemoveListener(StartPhysicalCheck);
            }

            if (mentalCheckButton != null)
            {
                mentalCheckButton.onClick.RemoveListener(StartMentalCheck);
            }

            hasListeners = false;
        }

        private void StartPhysicalCheck()
        {
            StartCheck(global::DiceCheckType.Physical, physicalDifficulty);
        }

        private void StartMentalCheck()
        {
            StartCheck(global::DiceCheckType.Mental, mentalDifficulty);
        }

        private void StartCheck(global::DiceCheckType checkType, int difficulty)
        {
            if (dicePanelView == null)
                return;

            SetResultText("检定中...");
            dicePanelView.PlayCheck(checkType, diceCount, difficulty, compareRule, allowModifier, HandleResultConfirmed);
        }

        private void HandleResultConfirmed(global::DiceCheckResultModel result)
        {
            if (result == null)
                return;

            string outcome = result.isSuccess ? "成功" : "失败";
            SetResultText($"{outcome}：{result.finalTotalValue}/{result.difficulty}  差值 {result.margin}");
        }

        private void SetResultText(string text)
        {
            if (resultText != null)
            {
                resultText.text = text;
            }
        }
    }
}
