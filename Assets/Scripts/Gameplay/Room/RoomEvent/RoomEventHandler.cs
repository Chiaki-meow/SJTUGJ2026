using System;
using UnityEngine;

namespace Gameplay
{
    public abstract class RoomEventHandler : MonoBehaviour
    {
        public abstract void HandleRoomEvent(RoomCard room, RoomEventData eventData, Action onFinished);
    }
}
