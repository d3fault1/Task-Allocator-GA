namespace FileFormat
{
    public class TaskInfo
    {
        /// <summary>
        /// Task ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Time taken by task
        /// </summary>
        public float RUNTIME { get; set; }

        /// <summary>
        /// Frequency of processor on which this task has executed
        /// </summary>
        public float REFERENCE_FREQUENCY { get; set; }

        /// <summary>
        /// RAM used by task
        /// </summary>
        public int RAM { get; set; }

        /// <summary>
        /// Task download speed while running
        /// </summary>
        public int DOWNLOAD { get; set; }

        /// <summary>
        /// Task upload speed while running
        /// </summary>
        public int UPLOAD { get; set; }

        public ProcessorInfo Processor { get; set; }
    }



}
