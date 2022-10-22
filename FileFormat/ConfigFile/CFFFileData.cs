using System.Collections.Generic;
using System.Linq;

namespace FileFormat
{
    public class CFFFileData : BaseData
    {
        public string LogFile { get; set; }

        public int LIMIT_MINIMUM_TASKS { get; set; }

        /// <summary>
        /// we cannot have a program partitioned into more than LIMIT_MAXIMUM_TASKS tasks,
        /// </summary>
        public int LIMIT_MAXIMUM_TASKS { get; set; }


        public int LIMIT_MINIMUM_PROCESSORS { get; set; }


        /// <summary>
        /// we cannot use more than 500 processors
        /// </summary>
        public int LIMIT_MAXIMUM_PROCESSORS { get; set; } //500




        public float LIMIT_MINIMUM_PROCESSOR_FREQUENCIES { get; set; }

        /// <summary>
        /// we cannot use a processor that has a frequency of more than 10 GHz
        /// </summary>
        public float LIMIT_MAXIMUM_PROCESSOR_FREQUENCIES { get; set; } //10

        public int LIMIT_MINIMUM_RAM { get; set; }

        /// <summary>
        /// we cannot allocate more than 64 GB of RAM to a processor
        /// </summary>
        public int LIMIT_MAXIMUM_RAM { get; set; } //64

        public int LIMIT_MINIMUM_DOWNLOAD { get; set; }
        public int LIMIT_MAXIMUM_DOWNLOAD { get; set; }

        public int LIMIT_MINIMUM_UPLOAD { get; set; }
        public int LIMIT_MAXIMUM_UPLOAD { get; set; }



        /// <summary>
        /// the maximum duration of that program
        /// </summary>
        public float PROGRAM_DURATION { get; set; }

        /// <summary>
        /// the number of tasks that this program must be partitioned
        /// </summary>
        public int PROGRAM_TASKS { get; set; }

        /// <summary>
        /// the number of available processors to run the tasks.
        /// </summary>
        public int PROGRAM_PROCESSORS { get; set; }


        /// <summary>
        /// Task execution details
        /// </summary>
        public List<TaskInfo> Tasks { get; set; } = new List<TaskInfo>();

        /// <summary>
        /// Processor Configuration
        /// </summary>
        public List<ProcessorInfo> Processors { get; set; } = new List<ProcessorInfo>();

        /// <summary>
        /// Processor Coefficients
        /// </summary>
        public List<ProcessorTypeInfo> ProcessorTypes { get; set; } = new List<ProcessorTypeInfo>();

        /// <summary>
        /// Energy consumed when tasks communicate locally
        /// </summary>
        public Communication LocalCommunication { get; set; } = null;

        /// <summary>
        /// Energy consumed when tasks communicate remotely
        /// </summary>
        public Communication RemoteCommunication { get; set; } = null;


        public TaskInfo GetTaskInfo(int TaskId)
        {
            return Tasks.FirstOrDefault(x => x.ID == TaskId);
        }

        public ProcessorInfo GetProcessor(int ProcessorId)
        {
            return Processors.FirstOrDefault(x => x.ID == ProcessorId);
        }

        public ProcessorTypeInfo GetProcessorType(int ProcessorId)
        {
            var processor = Processors.FirstOrDefault(x => x.ID == ProcessorId);
            if (processor == null) return null;

            return ProcessorTypes.FirstOrDefault(x => x.Name == processor.TYPE);
        }

        public float GetTaskLocalEnergy(int TaskId)
        {
            return LocalCommunication.GetEnergyConsumption(TaskId);
        }

        public float GetTaskRemoteEnergy(int TaskId)
        {
            return RemoteCommunication.GetEnergyConsumption(TaskId);
        }


        public void Validate()
        {

            foreach (var processor in Processors)
            {
               
                if (processor.FREQUENCY < LIMIT_MINIMUM_PROCESSOR_FREQUENCIES)
                {
                    AddError(-1, $"PROCESSOR.ID={processor.ID} FREQUENCY {processor.FREQUENCY} < LIMIT.MINIMUM_PROCESSOR_FREQUENCIES {LIMIT_MINIMUM_PROCESSOR_FREQUENCIES}");
                }

                if (processor.FREQUENCY > LIMIT_MAXIMUM_PROCESSOR_FREQUENCIES)
                {
                    AddError(-1, $"PROCESSOR.ID={processor.ID} FREQUENCY {processor.FREQUENCY} > LIMIT.MAXIMUM_PROCESSOR_FREQUENCIES {LIMIT_MAXIMUM_PROCESSOR_FREQUENCIES}");
                }

                if (processor.RAM < LIMIT_MINIMUM_RAM)
                {
                    AddError(-1, $"PROCESSOR.ID={processor.ID} RAM {processor.RAM} GB < LIMIT.MINIMUM_RAM {LIMIT_MINIMUM_RAM} GB");
                }

                if (processor.RAM >LIMIT_MAXIMUM_RAM)
                {
                    AddError(-1, $"PROCESSOR.ID={processor.ID} RAM {processor.RAM} GB > LIMIT.MAXIMUM_RAM {LIMIT_MAXIMUM_RAM} GB");
                }

                if (processor.DOWNLOAD < LIMIT_MINIMUM_DOWNLOAD)
                {
                    AddError(-1, $"PROCESSOR.ID={processor.ID} DOWNLOAD speed {processor.DOWNLOAD} Gbps < LIMIT.MINIMUM_DOWNLOAD {LIMIT_MINIMUM_DOWNLOAD} Gbps");
                }

                if (processor.DOWNLOAD > LIMIT_MAXIMUM_DOWNLOAD)
                {
                    AddError(-1, $"PROCESSOR.ID={processor.ID} DOWNLOAD speed {processor.DOWNLOAD} Gbps > LIMIT.MAXIMUM_DOWNLOAD {LIMIT_MAXIMUM_DOWNLOAD} Gbps");
                }



                if (processor.UPLOAD < LIMIT_MINIMUM_UPLOAD)
                {
                    AddError(-1, $"PROCESSOR.ID={processor.ID} UPLOAD speed {processor.UPLOAD} Gbps < LIMIT.MINIMUM_UPLOAD {LIMIT_MINIMUM_UPLOAD} Gbps");
                }

                if (processor.DOWNLOAD > LIMIT_MAXIMUM_UPLOAD)
                {
                    AddError(-1, $"PROCESSOR.ID={processor.ID} UPLOAD speed {processor.UPLOAD} Gbps > LIMIT.MAXIMUM_UPLOAD {LIMIT_MAXIMUM_UPLOAD} Gbps");
                }

            }



        }



    }



}
