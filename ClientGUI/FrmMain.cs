using Common;
using FileFormat;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientGUI
{
    public partial class FrmMain : Form
    {

        Logger logger = null;

        //================================================================
        //Change Timeout Value Here (In Minutes)
        //================================================================
        double timeOut = 1.0;
        public FrmMain()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            textBox2.Text = "1";
        }

        public void PrintErrors(BaseData fileData)
        {

            if (!fileData.HasError)
            {
                return;
            }
            WriteBlankLine();

            if (Path.GetExtension(fileData.FileName).ToLower() == ".taff")
            {
                WriteLine($"Task Allocation File {Path.GetFileName(fileData.FileName)} errors");
                WriteBlankLine();
            }
            else
            {
                WriteLine($"Config File {Path.GetFileName(fileData.FileName)} errors");
                WriteBlankLine();
            }

            foreach (Error error in fileData.Errors)
            {
                WriteLine(error.ErrorMessage);
            }
        }

        private void Print(TAFFFileData fileData)
        {
            foreach (var allocation in fileData.Allocations)
            {

                if (allocation.ID > 0)
                {
                    WriteBlankLine();

                }

                var line = $"Allocation ID={allocation.ID},   Time={allocation.Runtime.ToString("00.00")},  Energy={allocation.TotalEnergy}";
                WriteLine(line);

                foreach (var state in allocation.ExecutionStats)
                {
                    //1,1,0,0,0 2/2 GB 200/300 Gbps 40/50 Gbps
                    string lineData = $"{state.TaskAllocation}    {state.RequiredRAM}/{state.InstalledRAM} GB    {state.RequiredDownloadSpeed}/{state.DownloadCapability} Gbps    {state.RequiredUploadSpeed}/{state.UploadCapability} Gbps";
                    if (state.HasError)
                    {
                        // lineData += $" Error: {state.Error}";
                    }

                    WriteLine(lineData);
                }
            }
        }

        private void WriteLine(string line)
        {
            outPutWindow.AppendText(line);
            outPutWindow.AppendText(Environment.NewLine);

            if (logger != null)
                logger.WriteLine(line);
        }

        private void WriteBlankLine()
        {

            outPutWindow.AppendText(Environment.NewLine);
            if (logger != null)
                logger.WriteLine(Environment.NewLine);
        }


        //==============================================================================================================================
        //Problem 2 Code starts from here
        //==============================================================================================================================


        WebResponse response;
        private async void button1_Click(object sender, EventArgs e)
        {
            Stream stream = null;
            Uri result;

            outPutWindow.Clear();

            if (Uri.TryCreate(comboBox1.SelectedItem.ToString(), UriKind.Absolute, out result))
            {
                Task<Stream> taskGet = Task.Run(() => getFileStream(result.AbsoluteUri));
                await taskGet;
                stream = taskGet.Result;
            }

            //if (stream != null && Double.TryParse(textBox2.Text, out timeOut)) Start(stream);

            if (timeOut != 0) Start(stream);

        }

        private Stream getFileStream(string url)
        {
            var webRequest = WebRequest.Create(url);

            try
            {
                response = webRequest.GetResponse();
            }
            catch (Exception resex)
            {
                Invoke((MethodInvoker)delegate { WriteLine(resex.Message); });
                return null;
            }
            return response.GetResponseStream();
        }

        private void CloseWebResponse(WebResponse response)
        {
            response.Close();
        }

        private Allocation CreateAllocation(int id, int procCount, int[] alloc)
        {
            var allocation = new Allocation();
            allocation.ID = id;

            string[] procs = new string[procCount];

            string map = "";
            bool isfirst;

            isfirst = true;
            foreach (var t in alloc)
            {
                for (int i = 0; i < procCount; i++)
                {
                    if (!isfirst) procs[i] += ",";
                    if (i == t) procs[i] += "1";
                    else procs[i] += "0";
                }
                isfirst = false;
            }

            map += procs[0];
            for (int i = 1; i < procCount; i++) map += ";" + procs[i];

            allocation.Map = map;
            return allocation;
        }

        private List<int[]> RunAlgorithm(List<TaskData> tasks, List<ProcessorData> processors, double runtime, string endpoint)
        {
            if (endpoint == "ServerOneEndPoint")
            {
                var client = new AlgorithmOne.ComputeClient();
                var result = client.Run(tasks.ToArray(), processors.ToArray(), runtime, timeOut);
                return result.Select(T => T).ToList();
            }
            if (endpoint == "ServerTwoEndPoint")
            {
                var client = new AlgorithmTwo.ComputeClient();
                var result = client.Run(tasks.ToArray(), processors.ToArray(), runtime, timeOut);
                return result.Select(T => T).ToList();
            }
            else return null;
        }

        private TAFFFileData RunTask(List<TaskData> tasks, List<ProcessorData> processors, CFFFileData configData, double runtime, string endpoint)
        {
            var results = RunAlgorithm(tasks, processors, runtime, endpoint);

            List<Allocation> allocs = new List<Allocation>();

            for (int i = 0; i < results.Count; i++) allocs.Add(CreateAllocation(i, configData.PROGRAM_PROCESSORS, results[i]));

            foreach (var a in allocs) a.Init(configData.PROGRAM_PROCESSORS, configData.PROGRAM_TASKS);

            TAFFFileData tdata = new TAFFFileData { AllocationCount = allocs.Count, Allocations = allocs, ConfigData = configData, ConfigFilePath = "Web", ProcessorCount = configData.PROGRAM_PROCESSORS, TaskCount = configData.PROGRAM_TASKS };
            tdata.ComputeAllocations();

            return tdata;
        }

        private async void Start(Stream stream)
        {
            List<TaskData> tasks = new List<TaskData>();
            List<ProcessorData> processors = new List<ProcessorData>();

            var MyCFFParser = new CFFFileParser(stream);
            MyCFFParser.Parse();
            CFFFileData configData = MyCFFParser.Data;

            var dir = Directory.GetCurrentDirectory();
            var logPath = Path.Combine(dir, configData.LogFile.Replace("\"", ""));
            logger = new Logger(logPath);

            configData.Validate();

            string line;
            if (configData.IsValid())
            {
                line = $"Configuration file is valid";

                WriteLine(line);
                WriteBlankLine();
                PrintErrors(configData);
                double runtime = configData.PROGRAM_DURATION;
                foreach (var p in configData.Processors)
                {
                    processors.Add(new ProcessorData { ID = p.ID, C2 = p.ProcessorType.C2, C1 = p.ProcessorType.C1, C0 = p.ProcessorType.C0, Frequency = p.FREQUENCY, Ram = p.RAM, DownloadSpeed = p.DOWNLOAD, UploadSpeed = p.UPLOAD });
                }
                foreach (var t in configData.Tasks)
                {
                    tasks.Add(new TaskData { ID = t.ID, Time = t.RUNTIME, ReferenceFrequency = t.REFERENCE_FREQUENCY, Ram = t.RAM, DownloadSpeed = t.DOWNLOAD, UploadSpeed = t.UPLOAD, LocalEnergy = configData.GetTaskLocalEnergy(t.ID), RemoteEnergy = configData.GetTaskRemoteEnergy(t.ID) });
                }
                foreach (var t in tasks)
                {
                    Generics.setEligibleProcs(t, processors, runtime);
                }
                CloseWebResponse(response);


                TAFFFileData tdata1, tdata2;
                Task<TAFFFileData> task1 = Task.Run(() => RunTask(tasks, processors, configData, runtime, "ServerOneEndPoint"));
                WriteLine("Task 1 Running.......");
                Task<TAFFFileData> task2 = Task.Run(() => RunTask(tasks, processors, configData, runtime, "ServerTwoEndPoint"));
                WriteLine("Task 2 Running.......");
                WriteBlankLine();
                await task1;
                await task2;
                if (task1.IsCompleted && task2.IsCompleted)
                {
                    tdata1 = task1.Result;
                    tdata2 = task2.Result;
                    if (tdata1.Allocations[0].TotalEnergy < tdata2.Allocations[0].TotalEnergy) Print(tdata1);
                    Print(tdata2);
                }
                else
                {
                    WriteLine("Error: Local Timeout Occured");
                    WriteBlankLine();
                }
            }
            else
            {
                line = $"Configuration file is invalid";

                WriteLine(line);
                WriteBlankLine();
                PrintErrors(configData);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            outPutWindow.Clear();
        }
    }
}
