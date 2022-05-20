using UnityEngine;
using Bacterio.MapObjects;

namespace Bacterio.Game
{
    public sealed partial class MapController : System.IDisposable
    {
        private sealed partial class TrapController : System.IDisposable
        {
            private sealed partial class TrapTrigger
            {
                private readonly TrapController _trapCtrl = null;

                public TrapTrigger(TrapController trapController)
                {
                    _trapCtrl = trapController;
                }

                public void TriggerExplodingTrap(Trap trap, BlockType blockType, MonoBehaviour target)
                {
                    ref var trapData = ref GlobalContext.trapDb.GetTrapData(trap._dbId);

                    //Play the explosion effect
                    GameContext.effectsController.PlayParticleEffect(Databases.ParticleEffectId.ExplodeTrap, trap.transform.position);

                    //If trap was layed by a cell
                    if (trap._ownerCellIndex != Constants.INVALID_CELL_INDEX)
                    {
                        //Do nothing if we're not on host. Only host gets to damage bacteria
                        if (!Network.NetworkInfo.IsHost)
                            return;

                        //Check for hit bacteria
                        var hits = Physics2D.OverlapCircleAll(trap.transform.position, trapData.radius, Constants.BACTERIA_MASK);
                        if (hits.Length == 0)
                            return;

                        for (int i = 0; i < hits.Length; i++)
                            _trapCtrl._mapCtrl._bacteriaCtrl.DamageBacteria(hits[i].GetComponent<Bacteria>(), trap._attackPower);
                    }
                    else //was layed by a bacteria
                    {
                        //Check for hit bacteria
                        var hits = Physics2D.OverlapCircleAll(trap.transform.position, trapData.radius, Constants.CELLS_MASK);
                        if (hits.Length == 0)
                            return;

                        //Only apply hit to local cell
                        var localCell = _trapCtrl._mapCtrl._cellCtrl.LocalCell;
                        for (int i = 0; i < hits.Length; i++)
                            if (hits[i].gameObject == localCell.gameObject)
                            {
                                _trapCtrl._mapCtrl._cellCtrl.DamageCell(localCell, trap._attackPower);
                                return;
                            }
                    }
                }
            }
        }
    }
}
