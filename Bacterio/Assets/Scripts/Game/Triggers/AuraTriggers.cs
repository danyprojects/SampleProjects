using UnityEngine;
using Bacterio.MapObjects;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class AuraController : System.IDisposable
        {
            private sealed partial class AuraTriggers
            {
                private AuraController _auraCtrl;

                public AuraTriggers(AuraController auraController)
                {
                    _auraCtrl = auraController;
                }

                public void TriggerCellWoundDetection(Aura aura, Cell cell, Collider2D collider)
                {
                    //Aura only affects wounds and has a trigger delay
                    if (collider.gameObject.layer != Constants.STRUCTURES_LAYER)
                        return;

                    Structure structure = collider.GetComponent<Structure>();

                    //Do nothing if it's not a wound or not enough time has passed yet
                    if (structure._dbId != Databases.StructureDbId.Wound || GlobalContext.localTimeMs < structure._woundData.nextHealByLocalCellMs)
                        return;

                    if (Network.NetworkInfo.IsHost)
                        _auraCtrl._mapCtrl._structureCtrl.HealWound(structure, Constants.DEFAULT_WOUND_HEALING_POWER);
                    else
                        NetworkEvents.StructureEvents.RequestWoundHealEvent.Send(structure, Constants.DEFAULT_WOUND_HEALING_POWER);

                    structure._woundData.nextHealByLocalCellMs = GlobalContext.localTimeMs + Constants.DEFAULT_WOUND_HEAL_INTERVAL;
                }
            }
        }
    }
}

         