using System;
using Bacterio.Databases;

using Bacterio.MapObjects;

namespace Bacterio.Game
{
    public sealed partial class MapController : IDisposable
    {
        private sealed partial class BuffController
        {
            private MapController _mapCtrl;

            public BuffController(MapController mapController)
            {
                _mapCtrl = mapController;
            }

            public void ApplyBuff(Cell cell, BuffDbId dbId)
            {
                switch(dbId)
                {
                    case BuffDbId.MovementSpeedUp: ApplyMovementSpeedUpBuff(cell); break;

                    default: WDebug.LogWarn("Called apply buff for unhandled buff ID: " + dbId); break;
                }
            }

            private void ApplyMovementSpeedUpBuff(Cell cell)
            {
                //Buff should not stack. If it already exists, then update the timestamp
                if (cell._buffData.movementSpeedUpTimestamp > 0)
                {
                    cell._buffData.movementSpeedUpTimestamp = GlobalContext.localTimeMs + BuffConstants.MOVEMENT_SPEED_UP_DURATION_MS;
                    return;
                }

                //else we apply it, set the time and start a timer to remove it when it fires
                cell.MoveSpeedMultiplier += BuffConstants.MOVEVEMENT_SPEED_UP_PERCENT;
                cell._buffData.movementSpeedUpTimestamp = GlobalContext.localTimeMs + BuffConstants.MOVEMENT_SPEED_UP_DURATION_MS;

                GameContext.timerController.Add(BuffConstants.MOVEMENT_SPEED_UP_DURATION_MS, () => { OnMovementSpeedUpBuffTimerFired(cell); });

                cell._buffData.movementSpeedUpEffectToken = GameContext.effectsController.PlayTrailEffect(TrailEffectId.MoveSpeedUp, cell.transform);
            }

            private void RemoveMovementSpeedUpBuff(Cell cell)
            {
                //If it was already removed by other means, don't do anything
                if (cell._buffData.movementSpeedUpTimestamp == 0)
                    return;

                //We remove the buff from the cell and invalidate the timer flag
                cell._buffData.movementSpeedUpTimestamp = 0;
                cell.MoveSpeedMultiplier -= BuffConstants.MOVEVEMENT_SPEED_UP_PERCENT;

                //Cancel the effect
                GameContext.effectsController.CancelEffect(cell._buffData.movementSpeedUpEffectToken);
            }

            private void OnMovementSpeedUpBuffTimerFired(Cell cell)
            {
                //Could happen if buff was re-applied. Re-schedule timer with the remaining buff time
                if(GlobalContext.localTimeMs < cell._buffData.movementSpeedUpTimestamp)
                    GameContext.timerController.Add(cell._buffData.movementSpeedUpTimestamp - GlobalContext.localTimeMs, () => { OnMovementSpeedUpBuffTimerFired(cell); });
                else
                    RemoveMovementSpeedUpBuff(cell);
            }
        }
    }
}
