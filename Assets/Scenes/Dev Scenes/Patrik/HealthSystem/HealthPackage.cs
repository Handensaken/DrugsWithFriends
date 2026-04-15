namespace Scenes.Dev_Scenes.Patrik.Health_system
{
    public struct HealthPackage
    {
        public HealthPackage(int healthAmount, int batchAmount)
        {
            HealthAmount = healthAmount;
            BatchAmount = batchAmount;
        }

        public int HealthAmount { get; }

        public int BatchAmount { get; }
    }
}