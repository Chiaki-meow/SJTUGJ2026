using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public abstract class RoomSelectionHandler : MonoBehaviour
    {
        public abstract bool IsSelecting { get; }
        public abstract void ShowRoomSelection(IReadOnlyList<RoomCardData> choices, Action<RoomCardData> selectedCallback);
        public virtual void ShowPlacementFailed(string message, Action dismissedCallback)
        {
            dismissedCallback?.Invoke();
        }
    }
}
