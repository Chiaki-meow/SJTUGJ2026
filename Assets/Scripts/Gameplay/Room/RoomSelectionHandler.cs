using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public abstract class RoomSelectionHandler : MonoBehaviour
    {
        public abstract bool IsSelecting { get; }
        public abstract void ShowRoomSelection(IReadOnlyList<RoomPlacementOption> choices, Action<RoomPlacementOption> selectedCallback);
        public virtual void ShowRoomRotation(RoomPlacementOption selectedOption, Vector2Int requiredDoor, Action<RoomPlacementOption> selectedCallback)
        {
            selectedCallback?.Invoke(selectedOption);
        }

        public virtual void ShowPlacementFailed(string message, Action dismissedCallback)
        {
            dismissedCallback?.Invoke();
        }
    }
}
