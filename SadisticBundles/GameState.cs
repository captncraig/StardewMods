namespace SadisticBundles
{
    public class GameState
    {
        public static GameState Current;

        public bool Activated { get; set; }
        public bool Declined { get; set; }

        public bool UpgradeTomorrow { get; set; }
    }
}
