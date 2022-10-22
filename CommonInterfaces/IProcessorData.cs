namespace Common
{
    public interface IProcessorData
    {
        int ID { get; set; }
        double C2 { get; set; }
        double C1 { get; set; }
        double C0 { get; set; }
        double Frequency { get; set; }
        double Ram { get; set; }
        double DownloadSpeed { get; set; }
        double UploadSpeed { get; set; }
        double Energy { get; }
    }
}
