using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileFormat
{
    public class TAFFFileData : BaseData
    {
        /// <summary>
        /// Full path of configuration file
        /// </summary>
        public string ConfigFilePath { get; set; }

        /// <summary>
        /// Only file name of configuration file
        /// </summary>
        public string ConfigurationFileName { get; set; }



        /// <summary>
        /// No. of allocations
        /// </summary>
        public int AllocationCount { get; set; }

        /// <summary>
        /// No. of tasks
        /// </summary>
        public int TaskCount { get; set; }

        /// <summary>
        /// No of processors
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// Allocation details, each processor has some tasks assigned
        /// </summary>
        public List<Allocation> Allocations { get; set; } = new List<Allocation>();




        /// <summary>
        /// After parsing of configuration file set this property before calling ComputeAllocations
        /// </summary>
        public CFFFileData ConfigData { get; set; }


        /// <summary>
        /// Validates allocations, task count and processor count etc
        /// </summary>
        public void Validate()
        {

            if(ConfigData==null)
            {
                throw new Exception("Property ConfigData is not initialized");
            }

            foreach (var allocation in Allocations)
            {
                int procCount = allocation.GetProcessors().Count;
                

                if (procCount != ProcessorCount)
                {
                    AddError(-1, $"Allocation {allocation.ID} has processors {procCount} != ALLOCATIONS.PROCESSORS {ProcessorCount}");
                }

                for(int taskId=0; taskId<TaskCount; taskId++)
                {
                   var taskProcessors= allocation.GetTaskProcessors(taskId);

                    if(taskProcessors.Count>1)
                    {
                        string strProc = string.Join(",", taskProcessors);

                        AddError(-1, $"Task {taskId} in Allocation {allocation.ID} is assigned to more than 1 processors {strProc}");
                    }
                }

                if(allocation.Runtime>ConfigData.PROGRAM_DURATION)
                {
                    AddError(-1, $"Allocation ID={allocation.ID} has Runtime {allocation.Runtime} > PROGRAM.DURATION {ConfigData.PROGRAM_DURATION}");
                }


                if (procCount > ConfigData.PROGRAM_PROCESSORS)
                {
                    AddError(-1, $"Allocation ID={allocation.ID} has processors {procCount} > PROGRAM.PROCESSORS {ConfigData.PROGRAM_PROCESSORS}");
                }

            }


            if(TaskCount<ConfigData.LIMIT_MINIMUM_TASKS)
            {
                AddError(-1, $"ALLOCATIONS.TASKS {TaskCount} < LIMIT.MINIMUM_TASKS {ConfigData.LIMIT_MINIMUM_TASKS}");
            }

            if (TaskCount > ConfigData.LIMIT_MAXIMUM_TASKS)
            {
                AddError(-1, $"ALLOCATIONS.TASKS {TaskCount} > LIMIT.MAXIMUM_TASKS {ConfigData.LIMIT_MAXIMUM_TASKS}");
            }

            if (ProcessorCount > ConfigData.LIMIT_MAXIMUM_PROCESSORS)
            {
                AddError(-1, $"ALLOCATIONS.PROCESSORS {ProcessorCount} > LIMIT.MAXIMUM_PROCESSORS {ConfigData.LIMIT_MAXIMUM_PROCESSORS}");
            }

            if (ProcessorCount < ConfigData.LIMIT_MINIMUM_PROCESSORS)
            {
                AddError(-1, $"ALLOCATIONS.PROCESSORS {ProcessorCount} < LIMIT.MINIMUM_PROCESSORS {ConfigData.LIMIT_MINIMUM_PROCESSORS}");
            }

            if (ProcessorCount != ConfigData.PROGRAM_PROCESSORS)
            {
                AddError(-1, $"ALLOCATIONS.PROCESSORS {ProcessorCount} != PROGRAM.PROCESSORS {ConfigData.PROGRAM_PROCESSORS}");
            }

            if (TaskCount != ConfigData.PROGRAM_TASKS)
            {
                AddError(-1, $"ALLOCATIONS.TASKS {TaskCount} != PROGRAM.TASKS {ConfigData.PROGRAM_TASKS}");
            }

        }


        public void ComputeAllocations()
        {

            foreach (var allocation in Allocations)
            {


                var processors = allocation.GetProcessors();

                foreach (var processorId in processors)
                {
                    ExecutionSummary summary = new ExecutionSummary();
                    summary.ProcessorId = processorId;
                    summary.TaskAllocation = allocation.GetProcessorTaskVector(processorId);

                    summary.RequiredRAM = 0;
                    summary.RequiredDownloadSpeed = 0;


                    allocation.ExecutionStats.Add(summary);

                    var processorInfo = ConfigData.Processors.FirstOrDefault(x => x.ID == processorId);

                    if (processorInfo == null)
                    {
                        summary.Error = $"PROCESSOR details are missing in {ConfigData.OnlyFileName} for ID {processorId}";
                        ConfigData.AddError(-1, summary.Error);

                        continue;
                    }





                    summary.InstalledRAM = processorInfo.RAM;
                    summary.DownloadCapability = processorInfo.DOWNLOAD;
                    summary.UploadCapability = processorInfo.UPLOAD;


                    var processorType = ConfigData.ProcessorTypes.FirstOrDefault(x => x.Name == processorInfo.TYPE);
                    if (processorType == null)
                    {
                        summary.Error = $"PROCESSOR-TYPE is missing in {ConfigData.OnlyFileName} for NAME {processorInfo.TYPE}";
                        ConfigData.AddError(-1, summary.Error);
                    }


                    var processorTasks = allocation.GetProcessorTasks(processorId);
                    float processorRuntime = 0.0f;

                    foreach (var taskId in processorTasks)
                    {
                        TaskState taskState = new TaskState();
                        taskState.TaskId = taskId;
                        taskState.ProcessorId = processorId;

                        summary.TaskStates.Add(taskState);

                        var taskInfo = ConfigData.GetTaskInfo(taskId);
                        if (taskInfo != null)
                        {
                            //Corrections in Calculations done here!!!!
                            processorRuntime += taskInfo.RUNTIME * taskInfo.REFERENCE_FREQUENCY / processorInfo.FREQUENCY;
                            taskState.TaskRuntime = taskInfo.RUNTIME * taskInfo.REFERENCE_FREQUENCY / processorInfo.FREQUENCY;

                            if (processorType == null)
                            {
                                taskState.ProcessorEnergyConsumed = 0;
                            }
                            else
                            {
                                taskState.ProcessorEnergyConsumed = processorType.GetConsumedEnergyPerSecond(processorInfo.FREQUENCY) * taskState.TaskRuntime;
                            }


                            taskState.LocalEnergyConsumed = ConfigData.LocalCommunication.GetEnergyConsumption(taskId);
                            taskState.RemoteEnergyConsumed = ConfigData.RemoteCommunication.GetEnergyConsumption(taskId);

                            if (summary.RequiredRAM < taskInfo.RAM)
                            {
                                summary.RequiredRAM = taskInfo.RAM;
                            }

                            if (summary.RequiredDownloadSpeed < taskInfo.DOWNLOAD)
                            {
                                summary.RequiredDownloadSpeed = taskInfo.DOWNLOAD;
                            }

                            if (summary.RequiredUploadSpeed < taskInfo.UPLOAD)
                            {
                                summary.RequiredUploadSpeed = taskInfo.UPLOAD;
                            }


                        }
                        else
                        {
                            AddError(-1, $"TASK.ID={taskId} is missing in file {OnlyFileName}");
                        }
                    }

                    summary.ProcessorRuntime = processorRuntime;
                }
            }
        }

        public void Print()
        {
            foreach (var allocation in Allocations)
            {

                if (allocation.ID > 0)
                {
                    Console.WriteLine();
                }
                Console.WriteLine($"Allocation ID={allocation.ID} Runtime={allocation.Runtime} Energy={allocation.TotalEnergy}");
                foreach (var state in allocation.ExecutionStats)
                {



                    //1,1,0,0,0 2/2 GB 200/300 Gbps 40/50 Gbps
                    string lineData = $"{state.TaskAllocation} {state.RequiredRAM}/{state.InstalledRAM} GB {state.RequiredDownloadSpeed}/{state.DownloadCapability} Gbps {state.RequiredUploadSpeed}/{state.UploadCapability} Gbps";
                    if (state.HasError)
                    {
                        lineData += $" Error: {state.Error}";
                    }

                    Console.WriteLine(lineData);
                }
            }

        }







    }


}
