using InfraSizingCalculator.Models;

namespace InfraSizingCalculator.Services.Interfaces;

public interface IExportService
{
    byte[] ExportToCsv(K8sSizingResult result);
    byte[] ExportToJson(K8sSizingResult result);
    byte[] ExportToExcel(K8sSizingResult result);
    byte[] ExportToPdf(K8sSizingResult result);
    byte[] ExportToPdf(VMSizingResult result);
    string ExportToHtmlDiagram(K8sSizingResult result);
    string GetTimestampedFilename(string prefix, string extension);
}
