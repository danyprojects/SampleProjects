using Bacterio.Databases;

namespace Bacterio.MapObjects
{
    public class Trap : Block
    {
        public TrapDbId _dbId;
        //We should not save references in the case the references disappear.
        //But we still need the IDs so we can identify who layed the trap, as well as run any code that might be relevant, such as a specific trap counter in the owner if he's still alive
        public Network.NetworkArrayObject.UniqueId _ownerBacteriaId;
        public int _ownerCellIndex;

        //other fields
        public int _attackPower;

        public Animators.TrapAnimator _animator;

    }
}
