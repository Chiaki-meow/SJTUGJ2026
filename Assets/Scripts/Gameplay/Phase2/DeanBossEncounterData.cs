using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Phase 2/Dean Boss Encounter")]
    public class DeanBossEncounterData : ScriptableObject
    {
        [Header("Bloody Knife Route")]
        public int[] bloodyKnifeCheckTargets = { 8, 9 };
        [TextArea(2, 8)] public string bloodyKnifeIntroText;
        [TextArea(2, 8)] public string bloodyKnifeSingleSuccessText;
        [TextArea(2, 8)] public string bloodyKnifeWinText;
        [TextArea(2, 8)] public string bloodyKnifeFailureText;
        [TextArea(2, 8)] public string bloodyKnifeLoseText;

        [Header("Patient Letter Route")]
        public int[] patientLetterCheckTargets = { 10, 5 };
        [TextArea(2, 8)] public string patientLetterIntroText;
        [TextArea(2, 8)] public string patientLetterSingleSuccessText;
        [TextArea(2, 8)] public string patientLetterWinText;
        [TextArea(2, 8)] public string patientLetterFailureText;
        [TextArea(2, 8)] public string patientLetterLoseText;

        [Header("Shared")]
        public int failureDamage = 1;
        public int requiredSuccessCount = 2;

        public int GetTarget(Phase2Route route, int successIndex)
        {
            int[] targets = route == Phase2Route.PatientLetter ? patientLetterCheckTargets : bloodyKnifeCheckTargets;

            if (targets == null || targets.Length == 0)
                return 0;

            int index = Mathf.Clamp(successIndex, 0, targets.Length - 1);
            return targets[index];
        }
    }
}
