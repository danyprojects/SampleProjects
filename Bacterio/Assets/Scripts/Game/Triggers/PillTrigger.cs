using UnityEngine;
using Bacterio.MapObjects;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class PillController : System.IDisposable
        {
            private sealed partial class PillTriggers
            {
                private PillController _pillCtrl;

                public PillTriggers(PillController pillController)
                {
                    _pillCtrl = pillController;
                }

                public void TriggerMovementSpeedPill(Cell cell, Pill pill)
                {
                    _pillCtrl._mapCtrl._buffCtrl.ApplyBuff(cell, Databases.BuffDbId.MovementSpeedUp);
                    //Run consume pill effect
                }
            }
        }
    }
}

