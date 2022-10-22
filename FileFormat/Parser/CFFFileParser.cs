using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace FileFormat
{

    public class CFFFileParser : BaseParser
    {

        static List<string> KeyWords = new List<string>();



        public CFFFileData Data { get { return (CFFFileData)BaseData; } }

        private bool fromStream = false;


        public CFFFileParser(string CFFFilePath)
        {
            var kvlist = Properties.Resources.CFFKeyWords.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            LoadKeyWords(kvlist,KeyWords);

            BaseData = new CFFFileData();
            BaseData.FileName = CFFFilePath;

        }

        public CFFFileParser(Stream stream)
        {
            var kvlist = Properties.Resources.CFFKeyWords.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            LoadKeyWords(kvlist, KeyWords);

            BaseData = new CFFFileData();
            BaseData.Stream = stream;

            fromStream = true;
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
            if (fromStream) LoadFromStream();

            else LoadFile();

            CheckMixedComment();

            CheckKeyWords();

            ParseLogFileSection();

            ParseLimitsSection();

            ParseProgramSection();

            ParseTaskSection();

            ParseProcessorSection();

            ParseProcessorTypeSection();

            ParseLocalCommunicationSection();

            ParseRemoteCommunicationSection();

            AssignProcessorTypeSection();
        }

        private void ParseLogFileSection()
        {
            var sectionStart = GetLine("LOGFILE");
            var sectionEnd = GetLine("END-LOGFILE");


            if (sectionStart == null)
            {
                AddError(-1, "Start of section LOGFILE is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section LOGFILE is missing");
                return;
            }

            var line = Lines.FirstOrDefault(x => x.Data.Contains("DEFAULT") && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, "DEFAULT of section LOGFILE is missing");
                return;
            }


            if (HasEqual(line))
            {
                if (HasQuots(line))
                {
                    var parts = line.Data.Split(EqualSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        string fileName = parts[1].Replace("\"", "");
                        Data.LogFile = fileName;
                    }
                }
            }
        }

        private void ParseLimitsSection()
        {
            var sectionStart = GetLine("LIMITS");
            var sectionEnd = GetLine("END-LIMITS");


            if (sectionStart == null)
            {
                AddError(-1, "Start of section LIMITS is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section LIMITS is missing");
                return;
            }

            /*
            LIMITS
              MINIMUM-TASKS = 1
              MAXIMUM-TASKS = 100
              MINIMUM-PROCESSORS = 1
              MAXIMUM-PROCESSORS = 500
              MINIMUM-PROCESSOR-FREQUENCIES = 1.37
              MAXIMUM-PROCESSOR-FREQUENCIES = 10.0
              MINIMUM-RAM = 1
              MAXIMUM-RAM = 64
              MINIMUM-DOWNLOAD = 1
              MAXIMUM-DOWNLOAD = 1000
              MINIMUM-UPLOAD = 1
              MAXIMUM-UPLOAD = 1000
            END - LIMITS
             */

            var Token = "MINIMUM-TASKS";

            var line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {

                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MINIMUM_TASKS = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }

            Token = "MAXIMUM-TASKS";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MAXIMUM_TASKS = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }

            Token = "MINIMUM-PROCESSORS";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MINIMUM_PROCESSORS = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }

            Token = "MAXIMUM-PROCESSORS";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MAXIMUM_PROCESSORS = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }


            Token = "MINIMUM-PROCESSOR-FREQUENCIES";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetFloatValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MINIMUM_PROCESSOR_FREQUENCIES = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }

            Token = "MAXIMUM-PROCESSOR-FREQUENCIES";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetFloatValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MAXIMUM_PROCESSOR_FREQUENCIES = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }

            /*
              MINIMUM-RAM = 1
              MAXIMUM-RAM = 64
              MINIMUM-DOWNLOAD = 1
              MAXIMUM-DOWNLOAD = 1000
              MINIMUM-UPLOAD = 1
              MAXIMUM-UPLOAD = 1000

            */

            Token = "MINIMUM-RAM";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MINIMUM_RAM = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }

            Token = "MAXIMUM-RAM";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MAXIMUM_RAM = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }


            Token = "MINIMUM-DOWNLOAD";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MINIMUM_DOWNLOAD = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }


            Token = "MAXIMUM-DOWNLOAD";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MAXIMUM_DOWNLOAD = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }


            Token = "MINIMUM-UPLOAD";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MINIMUM_UPLOAD = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }


            Token = "MAXIMUM-UPLOAD";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section LIMITS is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.LIMIT_MAXIMUM_UPLOAD = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }



        }

        private void ParseProgramSection()
        {
            var sectionStart = GetLine("PROGRAM");
            var sectionEnd = GetLine("END-PROGRAM");


            if (sectionStart == null)
            {
                AddError(-1, "Start of section PROGRAM is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section PROGRAM is missing");
                return;
            }

            /*
                PROGRAM
                    DURATION=3.0
                    TASKS=5
                    PROCESSORS=3
                END-PROGRAM
             */

            var Token = "DURATION";

            var line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section PROGRAM is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetFloatValue(line.Data, out var ival))
                    {
                        Data.PROGRAM_DURATION = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }

            Token = "TASKS";

            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section PROGRAM is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.PROGRAM_TASKS = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }

            Token = "PROCESSORS";

            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > sectionStart.LineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"{Token} of section PROGRAM is missing");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        Data.PROGRAM_PROCESSORS = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"{Token} has invalid value at line {line.LineNo}");
                    }
                }
            }




        }

        private void ParseTaskSection()
        {



            char[] lettrs = new char[] { '=', '-' };

            /*
            var lines = Lines.Where(x => x.Data.Contains("TASKS"));
            foreach (var line in lines)
            {
                bool contains = line.Data.Any(x => lettrs.Contains(x));
                if (contains == false)
                {
                    sectionStart = line;
                    break;
                }
            }
            */

            Line sectionStart = GetLine("TASKS");


            var sectionEnd = GetLine("END-TASKS");


            if (sectionStart == null)
            {
                AddError(-1, "Start of section TASKS is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section TASKS is missing");
                return;
            }


            int StartLineNo = sectionStart.LineNo;
            //int EndLineNo = sectionEnd.LineNo;

            while (true)
            {
                var StartLine = Lines.FirstOrDefault(x => x.Data.Contains("TASK") && x.LineNo > StartLineNo && x.LineNo < sectionEnd.LineNo);
                if (StartLine == null)
                {
                    break;
                }

                var EndLine = Lines.FirstOrDefault(x => x.Data.Contains("END-TASK") && x.LineNo > StartLine.LineNo && x.LineNo < sectionEnd.LineNo);
                if (EndLine == null)
                {
                    AddError(-1, $"END-TASK of TASKS at line {StartLine.LineNo} is missing");
                    break;
                }

                ParseTask(StartLine, EndLine);
                StartLineNo = EndLine.LineNo;
            }


        }

        private void ParseTask(Line StartLine, Line EndLine)
        {

            /*

            TASK
                 ID=0
                 RUNTIME=1.0
                 REFERENCE-FREQUENCY=2.2
                 RAM=2
                 DOWNLOAD=100
                 UPLOAD=10
            END-TASK

            */
            TaskInfo task = new TaskInfo();

            Data.Tasks.Add(task);

            string Token = "ID";
            var line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in TASK at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        task.ID = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of TASK at line {line.LineNo}");
                    }
                }
            }


            Token = "RUNTIME";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in TASK at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetFloatValue(line.Data, out var ival))
                    {
                        task.RUNTIME = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of TASK at line {line.LineNo}");
                    }
                }
            }


            Token = "REFERENCE-FREQUENCY";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in TASK at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetFloatValue(line.Data, out var ival))
                    {
                        task.REFERENCE_FREQUENCY = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of TASK at line {line.LineNo}");
                    }
                }
            }

            Token = "RAM";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in TASK at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        task.RAM = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of TASK at line {line.LineNo}");
                    }
                }
            }


            Token = "DOWNLOAD";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in TASK at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        task.DOWNLOAD = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of TASK at line {line.LineNo}");
                    }
                }
            }


            Token = "UPLOAD";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in TASK at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetIntegerValue(line.Data, out var ival))
                    {
                        task.UPLOAD = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of TASK at line {line.LineNo}");
                    }
                }
            }

        }


        private void ParseProcessorSection()
        {

            char[] lettrs = new char[] { '=', '-' };

            /*
           var lines= Lines.Where(x => x.Data.Contains("PROCESSORS"));
            foreach (var line in lines)
            {
                bool contains = line.Data.Any(x => lettrs.Contains(x));
                if (contains == false)
                {
                    sectionStart = line;
                    break;
                }
            }

            */


            Line sectionStart = GetLine("PROCESSORS");




            if (sectionStart == null)
            {
                AddError(-1, "Start of section PROCESSORS is missing");
                return;
            }


            var sectionEnd = Lines.FirstOrDefault(x => x.Data.Contains("END-PROCESSORS") && x.LineNo > sectionStart.LineNo);

            if (sectionEnd == null)
            {
                AddError(-1, "End of section PROCESSORS is missing");
                return;
            }


            int StartLineNo = sectionStart.LineNo;
            //int EndLineNo = sectionEnd.LineNo;

            while (true)
            {
                var StartLine = Lines.FirstOrDefault(x => x.Data.Contains("PROCESSOR") && x.LineNo > StartLineNo && x.LineNo < sectionEnd.LineNo);
                if (StartLine == null)
                {
                    // AddError(-1, $"No of Processor {Data.Processors.Count} < PROGRAM_PROCESSORS {Data.PROGRAM_PROCESSORS}");
                    break;
                }

                var EndLine = Lines.FirstOrDefault(x => x.Data.Contains("END-PROCESSOR") && x.LineNo > StartLine.LineNo && x.LineNo < sectionEnd.LineNo);
                if (EndLine == null)
                {
                    AddError(-1, $"END-PROCESSOR of PROCESSOR at line {StartLine.LineNo} is missing");
                    break;
                }

                ParseProcessor(StartLine, EndLine);
                StartLineNo = EndLine.LineNo;
            }


        }

        private void ParseProcessor(Line StartLine, Line EndLine)
        {

            /*

            
              PROCESSOR
                ID=0
                TYPE="Intel i5"
                FREQUENCY=1.8
                RAM=4
                DOWNLOAD=300
                UPLOAD=50
              END-PROCESSOR

            */
            ProcessorInfo processor = new ProcessorInfo();

            Data.Processors.Add(processor);

            string Token = "ID";
            var line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR at line {StartLine.LineNo}");
            }
            else
            {
                if (GetIntegerValue(line.Data, out var ival))
                {
                    processor.ID = ival;
                }
                else
                {
                    AddError(line.LineNo, $"Invalid {Token} of PROCESSOR at line {line.LineNo}");
                }
            }


            Token = "TYPE";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR at line {StartLine.LineNo}");
            }
            else
            {
                if (GetStringValue(line.Data, out var ival))
                {
                    processor.TYPE = ival;
                }
                else
                {
                    AddError(line.LineNo, $"Invalid {Token} of PROCESSOR at line {line.LineNo}");
                }
            }


            Token = "FREQUENCY";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR at line {StartLine.LineNo}");
            }
            else
            {
                if (GetFloatValue(line.Data, out var ival))
                {
                    processor.FREQUENCY = ival;
                }
                else
                {
                    AddError(line.LineNo, $"Invalid {Token} of PROCESSOR at line {line.LineNo}");
                }
            }

            Token = "RAM";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR at line {StartLine.LineNo}");
            }
            else
            {
                if (GetIntegerValue(line.Data, out var ival))
                {
                    processor.RAM = ival;
                }
                else
                {
                    AddError(line.LineNo, $"Invalid {Token} of PROCESSOR at line {line.LineNo}");
                }
            }


            Token = "DOWNLOAD";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR at line {StartLine.LineNo}");
            }
            else
            {
                if (GetIntegerValue(line.Data, out var ival))
                {
                    processor.DOWNLOAD = ival;
                }
                else
                {
                    AddError(line.LineNo, $"Invalid {Token} of PROCESSOR at line {line.LineNo}");
                }
            }


            Token = "UPLOAD";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR at line {StartLine.LineNo}");
            }
            else
            {
                if (GetIntegerValue(line.Data, out var ival))
                {
                    processor.UPLOAD = ival;
                }
                else
                {
                    AddError(line.LineNo, $"Invalid {Token} of PROCESSOR at line {line.LineNo}");
                }
            }

        }


        private void ParseProcessorTypeSection()
        {

            var sectionStart = GetLine("PROCESSOR-TYPES");
            var sectionEnd = GetLine("END-PROCESSOR-TYPES");

            int ProcessorTypeCount = Data.Processors.Select(x => x.TYPE).Distinct().Count();

            if (sectionStart == null)
            {
                AddError(-1, "Start of section PROCESSOR-TYPES is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section PROCESSOR-TYPES is missing");
                return;
            }


            int StartLineNo = sectionStart.LineNo;
            //int EndLineNo = sectionEnd.LineNo;

            while (true)
            {
                var StartLine = Lines.FirstOrDefault(x => x.Data.Contains("PROCESSOR-TYPE") && x.LineNo > StartLineNo && x.LineNo < sectionEnd.LineNo);
                if (StartLine == null)
                {

                    break;
                }

                var EndLine = Lines.FirstOrDefault(x => x.Data.Contains("END-PROCESSOR-TYPE") && x.LineNo > StartLine.LineNo && x.LineNo < sectionEnd.LineNo);
                if (EndLine == null)
                {
                    AddError(-1, $"END-PROCESSOR-TYPE of PROCESSOR-TYPE at line {StartLine.LineNo} is missing");
                    break;
                }

                ParseProcessorType(StartLine, EndLine);
                StartLineNo = EndLine.LineNo;
            }


        }

        private void ParseProcessorType(Line StartLine, Line EndLine)
        {

            /*

            
              PROCESSOR-TYPE
                    NAME="Intel i5"
                    C2=10
                    C1=-25
                    C0=25
               END-PROCESSOR-TYPE

            */
            ProcessorTypeInfo processorType = new ProcessorTypeInfo();

            Data.ProcessorTypes.Add(processorType);

            string Token = "NAME";
            var line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR-TYPE at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (HasQuots(line))
                    {
                        if (GetStringValue(line.Data, out var strVal))
                        {
                            processorType.Name = strVal;
                        }
                        else
                        {
                            AddError(line.LineNo, $"Invalid {Token} of PROCESSOR-TYPE at line {line.LineNo}");
                        }
                    }
                }
            }


            Token = "C2";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR-TYPE at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetFloatValue(line.Data, out var ival))
                    {
                        processorType.C2 = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of PROCESSOR-TYPE at line {line.LineNo}");
                    }
                }
            }


            Token = "C1";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR-TYPE at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetFloatValue(line.Data, out var ival))
                    {
                        processorType.C1 = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of PROCESSOR-TYPE at line {line.LineNo}");
                    }
                }
            }


            Token = "C0";
            line = Lines.FirstOrDefault(x => x.Data.Contains(Token) && x.LineNo > StartLine.LineNo && x.LineNo < EndLine.LineNo);

            if (line == null)
            {
                AddError(-1, $"Expected {Token} is missing in PROCESSOR-TYPE at line {StartLine.LineNo}");
            }
            else
            {
                if (HasEqual(line))
                {
                    if (GetFloatValue(line.Data, out var ival))
                    {
                        processorType.C0 = ival;
                    }
                    else
                    {
                        AddError(line.LineNo, $"Invalid {Token} of PROCESSOR-TYPE at line {line.LineNo}");
                    }
                }
            }


        }


        private void ParseLocalCommunicationSection()
        {

            var sectionStart = GetLine("LOCAL-COMMUNICATION");
            var sectionEnd = GetLine("END-LOCAL-COMMUNICATION");

            int ProcessorTypeCount = Data.Processors.Select(x => x.TYPE).Distinct().Count();

            if (sectionStart == null)
            {
                AddError(-1, "Start of section LOCAL-COMMUNICATION is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section LOCAL-COMMUNICATION is missing");
                return;
            }


            int StartLineNo = sectionStart.LineNo;
            //int EndLineNo = sectionEnd.LineNo;


            var line = Lines.FirstOrDefault(x => x.Data.Contains("MAP") && x.LineNo > StartLineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"MAP is missing in LOCAL-COMMUNICATION after line {sectionStart.LineNo}");
                return;
            }

            if (HasEqual(line))
            {
                Communication communication = new Communication();
                var parts = line.Data.Split(EqualSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    AddError(-1, $"MAP value is missing in LOCAL-COMMUNICATION after line {sectionStart.LineNo}");
                    return;
                }
                communication.Map = parts[1];
                communication.Init();

                Data.LocalCommunication = communication;
            }
        }

        private void ParseRemoteCommunicationSection()
        {

            var sectionStart = GetLine("REMOTE-COMMUNICATION");
            var sectionEnd = GetLine("END-REMOTE-COMMUNICATION");

            int ProcessorTypeCount = Data.Processors.Select(x => x.TYPE).Distinct().Count();

            if (sectionStart == null)
            {
                AddError(-1, "Start of section REMOTE-COMMUNICATION is missing");
                return;
            }

            if (sectionEnd == null)
            {
                AddError(-1, "End of section REMOTE-COMMUNICATION is missing");
                return;
            }


            int StartLineNo = sectionStart.LineNo;
            //int EndLineNo = sectionEnd.LineNo;


            var line = Lines.FirstOrDefault(x => x.Data.Contains("MAP") && x.LineNo > StartLineNo && x.LineNo < sectionEnd.LineNo);
            if (line == null)
            {
                AddError(-1, $"MAP is missing in REMOTE-COMMUNICATION after line {sectionStart.LineNo}");
                return;
            }

            if (HasEqual(line))
            {
                Communication communication = new Communication();
                var parts = line.Data.Split(EqualSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    AddError(-1, $"MAP value is missing in REMOTE-COMMUNICATION after line {sectionStart.LineNo}");
                    return;
                }
                communication.Map = parts[1];
                communication.Init();

                Data.RemoteCommunication = communication;
            }
        }

        private void AssignProcessorTypeSection()
        {
            foreach(var p in Data.Processors)
            {
                p.ProcessorType = Data.GetProcessorType(p.ID);
            }
        }



    }



}
