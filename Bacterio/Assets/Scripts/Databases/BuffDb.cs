
namespace Bacterio.Databases
{
    public enum BuffDbId 
    {
        MovementSpeedUp = 0,

        Last = MovementSpeedUp,

        Invalid = -1
    }

    public struct BuffData
    {
        public int movementSpeedUpTimestamp;
        public EffectsController.EffectToken movementSpeedUpEffectToken;
    }

    public static class BuffConstants
    {
        public const int MOVEMENT_SPEED_UP_DURATION_MS = 10 * Constants.ONE_SECOND_MS;
        public const int MOVEVEMENT_SPEED_UP_PERCENT = 50;
    }
}
