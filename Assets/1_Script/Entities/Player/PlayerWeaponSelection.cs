namespace ProjectJS.PStats
{
    public static class PlayerWeaponSelection
    { // setting in lobby ui
        public static int SelectedWeaponIndex { get; set; } = 0;

        //[jh]
        public static Skills.ShardSpecies SelectedShardSpecies { get; set; } = Skills.ShardSpecies.None;
    }
}