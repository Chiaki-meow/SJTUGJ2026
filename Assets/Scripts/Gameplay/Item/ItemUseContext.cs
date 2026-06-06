namespace Gameplay
{
    public class ItemUseContext
    {
        public PlayerInventory Inventory;
        public PlayerStateManager PlayerState;
        public PlayerGridMovement Player;
        public BoardManager Board;
        public InGameManager InGame;
        public GameFlowManager GameFlow;
        public RoomCard CurrentRoom;
        public RoomEventData CurrentEvent;
        public RoomEventChoiceData CurrentChoice;
        public CharacterStat CheckStat;
        public int CheckRoll;
        public int CheckTargetNumber;
        public int DiceDelta;
        public string Message { get; private set; }

        public void SetMessage(string message)
        {
            Message = message;
        }

        public void ClearMessage()
        {
            Message = string.Empty;
        }
    }
}
