using System.Collections.Generic;
using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface ICompute
    {
        List<TaskData> Tasks { get; set; }
        List<ProcessorData> Processors { get; set; }
        double Runtime { get; set; }

        [OperationContract]
        List<int[]> Run(List<TaskData> Tasks, List<ProcessorData> Processors, double Runtime, double Timeout);
    }
}
