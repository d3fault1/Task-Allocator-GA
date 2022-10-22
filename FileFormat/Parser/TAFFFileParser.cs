using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace FileFormat
{
    public class TAFFFileParser : BaseParser
    {

        static List<string> KeyWords = new List<string>();

        public TAFFFileData Data { get { return (TAFFFileData)BaseData; } }

        public TAFFFileParser(string TAFFFilePath)
        {

            if (KeyWords.Count == 0)
            {
                var kvlist = Properties.Resources.TFFKeyWords.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                LoadKeyWords(kvlist, KeyWords);
            }

            BaseData = new TAFFFileData();
            BaseData.FileName = TAFFFilePath;
        }

        private void CheckKeyWords()
        {
            foreach (var line in Lines)
            {
                var item = line.Data.Trim();
                if (string.IsNullOrEmpty(item))
                    continue;

                if (IsCommentLine(line.Data))
                    continue;

                if (item.Contains('='))
                {
                    item = item.Split('=')[0].Trim();
                }


                if (!KeyWords.Any(x => x == item))
                {
                    AddError(line.LineNo, $"Invalid keyword {line.Data} at line {line.LineNo}", ErrorType.Conformance);
                }
            }
        }



        public void Parse()
        {


            LoadFile();

            CheckMixedComment();
            
            CheckKeyWords();

            ParseConfigSection();

            ParseAllocationSection();

            ValidateFile();
        }



        private void ParseConfigSection()
        {
            var sectionStart = GetLine("CONFIGURATION-DATA");// Lines.FirstOrDefault(x => x.Data.Contains("CONFIGURATION-DATA"));
            var sectionEnd = GetLine("END-CONFIGURATION-DATA"); //Lines.FirstOrDefault(x => x.Data.Contains("END-CONFIGURATION-DATA"));


            if (sectionStart == null)
            {
                AddError(-1, "Start of section CONFIGURATION-DATA is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section CONFIGURATION-DATA is missing");
                return;
            }

            var line = Lines.FirstOrDefault(x => x.Data.Contains("FILENAME") && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, "FILENAME of section CONFIGURATION-DATA is missing");
                return;
            }


            if (HasEqual(line))
            {
                if (HasQuots(line))
                {
                    if (GetStringValue(line.Data, out var strValue))
                    {
                        string fileName = strValue.Replace("\"", "");
                        Data.ConfigurationFileName = fileName;

                        Data.ConfigFilePath = System.IO.Path.GetDirectoryName(Data.FileName) + "\\" + Data.ConfigurationFileName;

                    }
                    else
                    {
                        AddError(-1, "FILENAME of section CONFIGURATION-DATA is missing");
                    }
                }
            }

        }

        private void ParseAllocationSection()
        {
            var sectionStart = GetLine("ALLOCATIONS"); // Lines.FirstOrDefault(x => x.Data.Contains("ALLOCATIONS"));
            var sectionEnd = GetLine("END-ALLOCATIONS"); // Lines.FirstOrDefault(x => x.Data.Contains("END-ALLOCATIONS"));


            if (sectionStart == null)
            {
                AddError(-1, "Start of section ALLOCATIONS is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section ALLOCATIONS is missing");
                return;
            }


            //COUNT=2
            //TASKS = 5
            //PROCESSORS = 3


            var line = Lines.FirstOrDefault(x => x.Data.Contains("COUNT") && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, "COUNT of section ALLOCATIONS is missing");
            }
            else
            {
                SetAllocationCount(line);
            }

            line = Lines.FirstOrDefault(x => x.Data.Contains("TASKS") && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, "TASKS of section ALLOCATIONS is missing");
            }
            else
            {
                SetTaskCount(line);
            }

            line = Lines.FirstOrDefault(x => x.Data.Contains("PROCESSORS") && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, "PROCESSORS of section ALLOCATIONS is missing");
            }
            else
            {
                SetProcessorCount(line);
            }

            int StartLineNo = sectionStart.LineNo;
            //int EndLineNo = sectionEnd.LineNo;

            while (Data.Allocations.Count < Data.AllocationCount)
            {
                var StartLine = Lines.FirstOrDefault(x => x.Data.Contains("ALLOCATION") && x.LineNo > StartLineNo && x.LineNo < sectionEnd.LineNo);
                if (StartLine == null)
                {
                    AddError(-1, $"No of allocations {Data.Allocations.Count} < AllocationCount {Data.AllocationCount}");
                    break;
                }

                var EndLine = Lines.FirstOrDefault(x => x.Data.Contains("END-ALLOCATION") && x.LineNo > StartLine.LineNo && x.LineNo < sectionEnd.LineNo);
                if (EndLine == null)
                {
                    AddError(-1, $"END-ALLOCATION of ALLOCATION at line {StartLine.LineNo} is missing");
                    break;
                }
                EndLine.IsValid = true;
                ParseAllocation(StartLine, EndLine);
                StartLineNo = EndLine.LineNo;
            }

        }


        private void ParseAllocation(Line StartLine, Line EndLine)
        {
            Allocation allocation = new Allocation();
            var Line = Lines.FirstOrDefault(x => x.Data.Contains("ID") && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            BitVector32 flags = new BitVector32(0);

            if (Line == null)
            {
                AddError(-1, $"Expected ID is missing in ALLOCATION at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(Line))
                {
                    if (GetIntegerValue(Line.Data, out var ival))
                    {
                        allocation.ID = ival;
                        flags[0] = true;
                    }
                    else
                    {
                        AddError(Line.LineNo, $"Invalid ID of ALLOCATION at line {Line.LineNo}");
                    }
                }
            }


            Line = Lines.FirstOrDefault(x => x.Data.Contains("MAP") && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);
            if (Line == null)
            {
                AddError(-1, $"Expected MAP is missing in ALLOCATION at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(Line))
                {
                    if (GetStringValue(Line.Data, out var sval))
                    {
                        allocation.Map = sval;
                        flags[1] = true;
                    }
                    else
                    {
                        AddError(Line.LineNo, $"Invalid MAP of ALLOCATION at line {Line.LineNo}");
                    }
                }
            }

            if (flags[0] && flags[1])
            {
                Data.Allocations.Add(allocation);
                allocation.Init(Data.ProcessorCount, Data.TaskCount);
            }


        }



        private void SetAllocationCount(Line line)
        {

            if (HasEqual(line))
            {
                if (GetIntegerValue(line.Data, out var itemp))
                {
                    Data.AllocationCount = itemp;
                }
                else
                {
                    AddError(-1, $"ALLOCATIONS.COUNT does not have valid value at line {line.LineNo}");
                }
            }
        }

        private void SetTaskCount(Line line)
        {

            if (HasEqual(line))
            {
                if (GetIntegerValue(line.Data, out var itemp))
                {
                    Data.TaskCount = itemp;
                }
                else
                {
                    AddError(-1, $"ALLOCATIONS.TASKS does not have valid value at line {line.LineNo}");
                }
            }
        }

        private void SetProcessorCount(Line line)
        {

            if (HasEqual(line))
            {
                if (GetIntegerValue(line.Data, out var itemp))
                {
                    Data.ProcessorCount = itemp;
                }
                else
                {
                    AddError(-1, $"ALLOCATIONS.PROCESSORS does not have valid value at line {line.LineNo}");
                }
            }
        }

        private void ValidateFile()
        {

            foreach(var allocation in Data.Allocations)
            {

            }
        }


    }


    public class ExecutionSummary
    {

        public int ProcessorId { get; set; }
        public string TaskAllocation { get; set; }
        public float ProcessorRuntime { get; set; }

        public int InstalledRAM { get; set; }

        public int RequiredRAM { get; set; }

        public int DownloadCapability { get; set; }
        public int RequiredDownloadSpeed { get; set; }


        public int UploadCapability { get; set; }
        public int RequiredUploadSpeed { get; set; }



        public List<TaskState> TaskStates { get; set; } = new List<TaskState>();

        public string Error { get; set; } = "";

        public bool HasError { get { return Error.Length > 0; } }


    }


    public class TaskState
    {
        public int TaskId { get; set; }
        public int ProcessorId { get; set; }

        public float ProcessorEnergyConsumed { get; set; }

        public float LocalEnergyConsumed { get; set; }
        public float RemoteEnergyConsumed { get; set; }

        public float TaskRuntime { get; set; }


        public float TotalEnergyConsumed { get { return ProcessorEnergyConsumed + LocalEnergyConsumed + RemoteEnergyConsumed; } }
    }



    /// <summary>
    /// It has tasks which are allocated to processors
    /// </summary>
    public class Allocation
    {
        /// <summary>
        /// Allocation ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Allocation Map
        /// </summary>
        public string Map { get; set; }

        private int Rows { get; set; }
        private int Columns { get; set; }

        /// <summary>
        ///Allocation Matrix Rows (processors), Columns (Tasks), allocated tasks to a processor
        ///Matrix[Row,Col]=1 means task (col) is assigned to processor (Row), 0 means task (col) is not assigned to processor (Row)
        /// </summary>
        public int[,] Matrix = null;

        /// <summary>
        /// Program Runtime
        /// </summary>
        public float Runtime
        {
            get
            {
                return ExecutionStats.Max(x => x.ProcessorRuntime);
            }
        }


        public float TotalEnergy
        {
            get
            {

                return ExecutionStats.Sum(x => x.TaskStates.Sum(y => y.TotalEnergyConsumed));

            }
        }

        public List<ExecutionSummary> ExecutionStats { get; private set; } = new List<ExecutionSummary>();



        /// <summary>
        /// Rows x Columns
        /// </summary>
        /// <param name="ProcessorCount"></param>
        /// <param name="TaskCount"></param>
        public void Init(int ProcessorCount, int TaskCount)
        {
            this.Rows = ProcessorCount;
            this.Columns = TaskCount;

            Matrix = new int[ProcessorCount, TaskCount];

            var rows = Map.Split(';');
            int iRow = 0;
            foreach (var strRow in rows)
            {

                var strCols = strRow.Split(',');
                for (int icol = 0; icol < strCols.Length; icol++)
                {
                    if (int.TryParse(strCols[icol].Trim(), out var iVal))
                    {
                        Matrix[iRow, icol] = iVal;
                    }
                }
                iRow++;
            }

        }


        public string GetProcessorTaskVector(int ProcessorId)
        {
            StringBuilder sb = new StringBuilder();
            for (int taskId = 0; taskId < Columns; taskId++)
            {
                sb.Append(Matrix[ProcessorId, taskId].ToString());
                sb.Append(",");

            }

            sb.Length--;
            return sb.ToString();
        }

        /// <summary>
        /// Returns true if task is assigned to Processor else returns false
        /// </summary>
        /// <param name="TaskId"></param>
        /// <param name="ProcessorId"></param>
        /// <returns></returns>
        public bool IsTaskAssignedToProcessor(int TaskId, int ProcessorId)
        {
            return Matrix[ProcessorId, TaskId] == 1;
        }

        /// <summary>
        /// Returns list of TaskId which are assigned to given ProcessorId
        /// </summary>
        /// <param name="ProcessorId"></param>
        /// <returns></returns>
        public List<int> GetProcessorTasks(int ProcessorId)
        {

            List<int> items = new List<int>();

            for (int taskId = 0; taskId < Columns; taskId++)
            {
                if (Matrix[ProcessorId, taskId] == 1)
                {
                    items.Add(taskId);
                }
            }

            return items;
        }

        /// <summary>
        /// Returns ProcessorIds to which given task is assigned
        /// </summary>
        /// <param name="TaskId"></param>
        /// <returns></returns>
        public List<int> GetTaskProcessors(int TaskId)
        {

            List<int> items = new List<int>();

            for (int processorId = 0; processorId < Rows; processorId++)
            {
                if (Matrix[processorId, TaskId] == 1)
                {
                    items.Add(processorId);
                }
            }

            return items;
        }

        /// <summary>
        /// Returns all processors
        /// </summary>
        /// <returns></returns>
        public List<int> GetProcessors()
        {
            List<int> items = new List<int>();
            for (int irow = 0; irow < Rows; irow++)
            {
                items.Add(irow);
            }

            return items;
        }

    }


}
