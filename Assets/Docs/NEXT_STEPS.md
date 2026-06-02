# Next Steps

## Current Gameplay Loop

- Start with only the starting room placed.
- Press `WASD` to choose a direction.
- If the target grid already has a room, the player moves there.
- If the target grid is empty, the current room spends 1 `remainingDoors`, draws a `RoomCardData` from `BoardManager.deck`, places it in that direction, then moves the player into it.
- Entering a room triggers its `RoomEventData` if it has not been resolved.
- The temporary `DebugRoomEventHandler` prints the event data to Console.
- Press `Space` to finish the debug event.
- Finishing an event only marks it resolved; it does not auto-expand rooms anymore.

## Next Priority

Build the minimum room event UI before polishing draw-card UI.

Target flow:

1. Show event title, category, and description.
2. Show all choices as buttons.
3. Clicking a choice:
   - If it has no check, show `directOutcome`.
   - If it has a check, temporarily roll `1-6` and show success or failure outcome.
4. Show a `Continue` button.
5. Continue calls the event completion callback and returns to WASD movement.

This gives the game its real core loop:

```text
WASD choose direction
-> draw and place room
-> enter room
-> event UI
-> choose option
-> result
-> continue moving
```

## Useful Inspector Checks

- `PlayerGridMovement.enterStartingRoomOnStart` should usually be unchecked for the current flow.
- `BoardManager.deck` must contain enough `RoomCardData` assets, or empty-grid movement cannot place new rooms.
- `BoardManager.startRoom.doorCount` controls how many new rooms can be placed outward from the starting room.
- Each `RoomCardData.eventData` should point to a `RoomEventData` asset.
- The scene needs a `GameFlowManager` and a `RoomEventHandler` such as `DebugRoomEventHandler`.

## Small Tasks

- Create one new `ItemGain` `RoomEventData` with one `Continue` choice and one `GainItem` effect.
- Assign `Night Resident Event` or `Muttering Patient Omen` to a room card, enter that room, and confirm the Console preview matches the asset.
