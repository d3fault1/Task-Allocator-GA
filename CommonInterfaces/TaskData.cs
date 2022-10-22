using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class TaskData : ITaskData
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public double Time { get; set; }
        [DataMember]
        public double ReferenceFrequency { get; set; }
        [DataMember]
        public double Ram { get; set; }
        [DataMember]
        public double DownloadSpeed { get; set; }
        [DataMember]
        public double UploadSpeed { get; set; }
        [DataMember]
        public double LocalEnergy { get; set; }
        [DataMember]
        public double RemoteEnergy { get; set; }
        [DataMember]
        public int[] EligibleProcessors { get; set; }
    }
}
