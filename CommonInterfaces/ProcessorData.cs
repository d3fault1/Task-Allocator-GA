using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ProcessorData : IProcessorData
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public double C2 { get; set; }
        [DataMember]
        public double C1 { get; set; }
        [DataMember]
        public double C0 { get; set; }
        [DataMember]
        public double Frequency { get; set; }
        [DataMember]
        public double Ram { get; set; }
        [DataMember]
        public double DownloadSpeed { get; set; }
        [DataMember]
        public double UploadSpeed { get; set; }

        public double Energy { get { return EnergyPerSec(); } }

        protected double EnergyPerSec()
        {
            return (C2 * Frequency * Frequency) + (C1 * Frequency) + C0;
        }
    }
}
