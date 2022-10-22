using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileFormat
{
    public class BaseParser
    {
        internal string[] EqualSeparator = new string[] { "=" };


        Regex REMixedComment = new Regex(@"(?<group1>[A-Za-z0-9]?)(?<slash>//)(?<group2>\S|\s)*");

        public List<Line> Lines { get; private set; } = new List<Line>();



        public BaseData BaseData { get; set; }


        public BaseParser()
        {


        }

        internal void LoadKeyWords(string[] kvlist, List<string> keyWords)
        {
            if (keyWords.Count > 0)
            {
                return;
            }

            foreach (var word in kvlist)
            {
                var item = word.Trim();

                if (string.IsNullOrEmpty(item))
                    continue;

                if (item.Contains('='))
                {
                    item = item.Split('=')[0].Trim();
                }

                keyWords.Add(item);
            }

        }
        

        public Line GetLine(string Data)
        {
            var pattern = $"^\\s*{Data}\\s*$";

            foreach (var line in Lines)
            {
                bool ismatching = Regex.IsMatch(line.Data, pattern);
                if(ismatching)
                {
                    return line;
                }
            }

            return null;
        }

        public Line GetLine(string Data,int AfterLine)
        {
            var query = Lines.Where(x => x.LineNo > AfterLine);

            foreach (var line in query)
            {
                bool ismatching = Regex.IsMatch(line.Data, @"^\s*CONFIGURATION-DATA\s*$");
                if (ismatching)
                {
                    return line;
                }
            }

            return null;
        }

        public void LoadFile()
        {
            using (StreamReader reader = new StreamReader(BaseData.FileName))
            {
                int lineNo = 1;
                while (!reader.EndOfStream)
                {
                    var line = new Line() { LineNo = lineNo++, Data = reader.ReadLine() };
                    Lines.Add(line);
                }
            }
        }

        public void LoadFromStream()
        {
            using (StreamReader reader = new StreamReader(BaseData.Stream))
            {
                int lineNo = 1;
                while (!reader.EndOfStream)
                {
                    var line = new Line() { LineNo = lineNo++, Data = reader.ReadLine() };
                    Lines.Add(line);
                }
            }
        }

        public bool IsValid()
        {
            return BaseData.IsValid();
        }


        /// <summary>
        /// Splits from = sign and returns value from right part of = sign
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="IntValue">Parsed integer value or int.MinValue</param>
        /// <returns>True if value is parsed as integer otherwise false</returns>

        public bool GetIntegerValue(string Line, out int IntValue)
        {
            IntValue = int.MinValue;
            bool bReturn = false;

            
                var parts = Line.Split(EqualSeparator, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out var itemp))
                {
                    IntValue = itemp;
                    bReturn = true;
                }
            
            return bReturn;
        }

        /// <summary>
        /// Splits from = and then returns right part of equal sign
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="StrValue">String value or empty string</param>
        /// <returns>True if there is some value on right side of = sign else false</returns>
        public bool GetStringValue(string Line, out string StrValue)
        {
            StrValue = "";
            bool bReturn = false;
            var parts = Line.Split(EqualSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                StrValue = parts[1].Trim();
                bReturn = true;
            }

            return bReturn;
        }

        public bool HasEqual(Line line)
        {
            bool bOK = true;

            if(!line.Data.Contains('='))
            {
                bOK = false;
                AddError(line.LineNo, $"Line {line.Data} is missing = character at line no {line.LineNo}",ErrorType.Conformance);
            }

            return bOK;
        }

        public bool HasQuots(Line line)
        {
            bool bOK = false;

            var id1 = line.Data.IndexOf("\"");

            if(id1>=0)
            {
                var id2 = line.Data.IndexOf("\"",id1+1);
                if(id2>0)
                {
                    bOK = true;
                }
            }

            if (bOK==false)
            {
                
                AddError(line.LineNo, $"Line {line.Data} is missing \" character at line no {line.LineNo}", ErrorType.Conformance);
            }

            return bOK;
        }

        /// <summary>
        /// Splits from = and then returns right side value
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="IValue">Float value from given string or float.MinValue</param>
        /// <returns>True if rightside value is parsed successfully otherwise returns false</returns>
        public bool GetFloatValue(string Line, out float IValue)
        {
            IValue = float.MinValue;
            bool bReturn = false;
            var parts = Line.Split(EqualSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 && float.TryParse(parts[1].Trim(), out var itemp))
            {
                IValue = itemp;
                bReturn = true;
            }

            return bReturn;
        }



        public void AddError(int line, string message,ErrorType ErrType=ErrorType.Logic)
        {

            BaseData.AddError(line, message,ErrType);
        }

        public bool IsCommentLine(string data)
        {
            int index = data.IndexOf("//");
            if (index < 0)
                return false;

            if (index == 0)
                return true;

            data = data.Trim();
            if (data.IndexOf("//") == 0)
                return true;

            return false;
        }


        public void CheckMixedComment()
        {
            foreach (var line in Lines)
            {
                if (line.Data.Contains(@"//"))
                {
                    var commentIndex = line.Data.IndexOf("//");

                    if (commentIndex > 0)
                    {
                        var left = line.Data.Substring(0, commentIndex);

                        var isletter = left.Any(x => char.IsLetterOrDigit(x));

                        if (isletter)
                        {
                            AddError(line.LineNo, $"Invalid comment {line.Data} at line no {line.LineNo}",ErrorType.Conformance);

                        }
                    }
                }
            }
        }







    }




    public class Line
    {
        public int LineNo { get; set; }
        public string Data { get; set; }

        public bool IsValid { get; set; } = false;

    }


    public class Error
    {
        public ErrorType ErrType { get; set; }
        public int LineNo { get; set; }
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            return ErrorMessage;
        }
    }



}
