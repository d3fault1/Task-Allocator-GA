namespace FileFormat
{
    public class ProcessorTypeInfo
    {
        public string Name { get; set; }

        public float C2 { get; set; }
        public float C1 { get; set; }
        public float C0 { get; set; }

        public float GetConsumedEnergyPerSecond(float ProcessorFrequency)
        {
            // 10f2 - 25f + 25

            float EngergyPerSecond = C2 * ProcessorFrequency * ProcessorFrequency + C1 * ProcessorFrequency + C0;

            return EngergyPerSecond;
        }
    }



}
