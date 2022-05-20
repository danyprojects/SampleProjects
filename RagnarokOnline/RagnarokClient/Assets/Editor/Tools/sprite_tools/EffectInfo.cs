using RO.Databases;

namespace EditorTools
{
    public static class EffectInfo
    {
        public static float[] Offsets = new float[(int)EffectIDs.Last];

        static EffectInfo()
        {
            Offsets[(int)EffectIDs.Devotion] = -11;
            Offsets[(int)EffectIDs.JupitelThunderBall] = -10;
            Offsets[(int)EffectIDs.JupitelThunderExplosion] = -10;
        }
    }
}
