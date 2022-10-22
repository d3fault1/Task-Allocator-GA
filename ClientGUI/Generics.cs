using Common;
using System.Collections.Generic;
using System.Linq;

namespace ClientGUI
{
    static class Generics
    {
        public static void setEligibleProcs(ITaskData task, IEnumerable<IProcessorData> processors, double runtime)
        {
            List<int> eligibleProcsList = new List<int>();
            double reqMem = task.Ram;
            double reqDL = task.DownloadSpeed;
            double reqUL = task.UploadSpeed;
            double mul = task.Time * task.ReferenceFrequency;
            foreach (var p in processors)
            {
                double reqTime = mul / p.Frequency;
                if (reqTime > runtime || reqMem > p.Ram || reqDL > p.DownloadSpeed || reqUL > p.UploadSpeed) continue;
                else eligibleProcsList.Add(processors.ToList().IndexOf(p));
            }
            task.EligibleProcessors = eligibleProcsList.ToArray();
        }
    }
}
