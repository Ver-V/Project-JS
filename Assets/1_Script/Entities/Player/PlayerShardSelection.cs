using ProjectJS.Skills;

namespace ProjectJS.PStats
{
    public static class PlayerShardSelection
    {
        public static ShardSpecies SelectedShardSpecies { get; private set; } = ShardSpecies.None;

        public static void SetSelectedShardSpecies(ShardSpecies shardSpecies)
        {
            SelectedShardSpecies = shardSpecies;
        }
    }
}
