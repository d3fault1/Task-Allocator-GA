namespace Common
{
    public interface ITaskData
    {
        int ID { get; set; }
        double Time { get; set; }
        double ReferenceFrequency { get; set; }
        double Ram { get; set; }
        double DownloadSpeed { get; set; }
        double UploadSpeed { get; set; }
        double LocalEnergy { get; set; }
        double RemoteEnergy { get; set; }
        int[] EligibleProcessors { get; set; }
    }
}
