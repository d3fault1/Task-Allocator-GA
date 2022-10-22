using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileFormat
{
    public class BaseData
    {
        public string FileName { get; set; }

        public Stream Stream { get; set; }

        public string OnlyFileName { get { return Path.GetFileName(FileName); } }

        public List<Error> Errors { get; private set; } = new List<Error>();

        public bool IsValid()
        {
            return Errors.Count == 0;
        }

        public bool HasConformanceErrors
        {
            get
            {
                return Errors.Any(x => x.ErrType == ErrorType.Conformance);
            }
        }

        public bool HasError { get { return Errors.Count > 0; } }

        public void PrintErrors()
        {
            if (!HasError)
                return;

            Console.WriteLine();

            if (Path.GetExtension(FileName).ToLower() == ".taff")
            {
                Console.WriteLine($"Task Allocation File {Path.GetFileName( FileName)} errors");
            }else
            {
                Console.WriteLine($"Config File {Path.GetFileName( FileName)} errors");
            }
            foreach(Error error in Errors)
            {
                Console.WriteLine(error);
            }
        }

        public void AddError(int line, string message, ErrorType ErrType=ErrorType.Logic)
        {
            var err = new Error() { LineNo = line, ErrorMessage = message, ErrType=ErrType };
            this.Errors.Add(err);
        }
    }

    public enum ErrorType
    {
        Logic=0,
        Conformance=1
    }


}
