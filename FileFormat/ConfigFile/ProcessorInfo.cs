namespace FileFormat
{
    public class ProcessorInfo
    {
        /// <summary>
        /// Processor ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Processor Type
        /// </summary>
        public string TYPE { get; set; }

        /// <summary>
        /// Processor Frequency GHz
        /// </summary>
        public float FREQUENCY { get; set; }

        /// <summary>
        /// RAM Allocated to processor in GB
        /// </summary>
        public int RAM { get; set; }

        /// <summary>
        /// Processor DOWNNLOAD speed capability Gbps
        /// </summary>
        public int DOWNLOAD { get; set; }

        /// <summary>
        /// Processor Upload speed capability Gbps
        /// </summary>
        public int UPLOAD { get; set; }

        public ProcessorTypeInfo ProcessorType { get; set; }
    }



}
