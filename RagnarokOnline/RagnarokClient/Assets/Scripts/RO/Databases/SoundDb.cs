namespace RO.Databases
{
    public static class SoundDb
    {
        public enum SoundIds
        {
            UIClick = 0,

            //Misc sound effects
            BeginSpell,
            BaseLevelUp,
            WarpIn,
            WarpOut,
            PlayerClothes,

            //Skill sounds
            StormGust,
            JupitelThunder,

            Default,
            Last = Default,

            None
        }

        public static string[] AudioEffects = new string[(int)SoundIds.Last];

        static SoundDb()
        {
            AudioEffects[(int)SoundIds.UIClick] = "ui_click";
            AudioEffects[(int)SoundIds.BaseLevelUp] = "base_level_up";
            AudioEffects[(int)SoundIds.BeginSpell] = "beginspell";
            AudioEffects[(int)SoundIds.WarpIn] = "warp_in";
            AudioEffects[(int)SoundIds.WarpOut] = "warp_out";
            AudioEffects[(int)SoundIds.StormGust] = "stormgust";
            AudioEffects[(int)SoundIds.JupitelThunder] = "jupitel_thunder";
            AudioEffects[(int)SoundIds.PlayerClothes] = "player_clothes";
        }
    }
}
