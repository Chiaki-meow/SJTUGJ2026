using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(menuName = "Gameplay/Player Initial State")]
    public class PlayerInitialStateData : ScriptableObject
    {
        [Min(0)] public int physical = 3;
        [Min(0)] public int mental = 3;
        [Min(1)] public int maxHealth = 3;
        [Min(0)] public int health = 3;

        private void OnValidate()
        {
            physical = Mathf.Max(0, physical);
            mental = Mathf.Max(0, mental);
            maxHealth = Mathf.Max(1, maxHealth);
            health = Mathf.Clamp(health, 0, maxHealth);
        }
    }
}
