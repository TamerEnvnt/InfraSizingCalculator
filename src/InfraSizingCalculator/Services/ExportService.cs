using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Export service implementing BR-EX001 through BR-EX003
/// </summary>
public class ExportService : IExportService
{
    /// <summary>
    /// BR-EX001: Timestamp in filename
    /// </summary>
    public string GetTimestampedFilename(string prefix, string extension)
    {
        return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
    }

    /// <summary>
    /// BR-EX002: Complete data export to CSV
    /// </summary>
    public byte[] ExportToCsv(K8sSizingResult result)
    {
        var sb = new StringBuilder();

        // Metadata (BR-EX003)
        sb.AppendLine("# Infrastructure Sizing Calculator Export");
        sb.AppendLine($"# Generated: {result.CalculatedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"# Distribution: {result.DistributionName}");
        sb.AppendLine($"# Technology: {result.TechnologyName}");
        sb.AppendLine($"# Cluster Mode: {result.Configuration.ClusterMode}");
        sb.AppendLine();

        // Headers
        sb.AppendLine("Environment,Apps,Replicas,Pods,Masters,Infra,Workers,Total Nodes,vCPU,RAM (GB),Disk (GB)");

        // Data rows
        foreach (var env in result.Environments)
        {
            sb.AppendLine($"{env.EnvironmentName},{env.Apps},{env.Replicas},{env.Pods},{env.Masters},{env.Infra},{env.Workers},{env.TotalNodes},{env.TotalCpu},{env.TotalRam},{env.TotalDisk}");
        }

        // Grand total
        var gt = result.GrandTotal;
        sb.AppendLine($"GRAND TOTAL,,,,{gt.TotalMasters},{gt.TotalInfra},{gt.TotalWorkers},{gt.TotalNodes},{gt.TotalCpu},{gt.TotalRam},{gt.TotalDisk}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// BR-EX002, BR-EX003: Complete data with metadata export to JSON
    /// </summary>
    public byte[] ExportToJson(K8sSizingResult result)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.SerializeToUtf8Bytes(result, options);
    }

    /// <summary>
    /// BR-EX002, BR-EX003: Multi-sheet Excel export
    /// </summary>
    public byte[] ExportToExcel(K8sSizingResult result)
    {
        using var workbook = new XLWorkbook();

        // Summary worksheet
        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = "Infrastructure Sizing Summary";
        summary.Cell(1, 1).Style.Font.Bold = true;
        summary.Cell(1, 1).Style.Font.FontSize = 16;

        summary.Cell(3, 1).Value = "Generated:";
        summary.Cell(3, 2).Value = result.CalculatedAt.ToString("yyyy-MM-dd HH:mm:ss");

        summary.Cell(4, 1).Value = "Distribution:";
        summary.Cell(4, 2).Value = result.DistributionName;

        summary.Cell(5, 1).Value = "Technology:";
        summary.Cell(5, 2).Value = result.TechnologyName;

        summary.Cell(6, 1).Value = "Cluster Mode:";
        summary.Cell(6, 2).Value = result.Configuration.ClusterMode.ToString();

        summary.Cell(8, 1).Value = "Grand Totals";
        summary.Cell(8, 1).Style.Font.Bold = true;

        summary.Cell(9, 1).Value = "Total Nodes:";
        summary.Cell(9, 2).Value = result.GrandTotal.TotalNodes;

        summary.Cell(10, 1).Value = "Total vCPU:";
        summary.Cell(10, 2).Value = result.GrandTotal.TotalCpu;

        summary.Cell(11, 1).Value = "Total RAM (GB):";
        summary.Cell(11, 2).Value = result.GrandTotal.TotalRam;

        summary.Cell(12, 1).Value = "Total Disk (GB):";
        summary.Cell(12, 2).Value = result.GrandTotal.TotalDisk;

        summary.Columns().AdjustToContents();

        // Environments worksheet
        var envSheet = workbook.Worksheets.Add("Environments");
        var headers = new[] { "Environment", "Type", "Apps", "Replicas", "Pods", "Masters", "Infra", "Workers", "Total Nodes", "vCPU", "RAM (GB)", "Disk (GB)" };

        for (int i = 0; i < headers.Length; i++)
        {
            envSheet.Cell(1, i + 1).Value = headers[i];
            envSheet.Cell(1, i + 1).Style.Font.Bold = true;
            envSheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        int row = 2;
        foreach (var env in result.Environments)
        {
            envSheet.Cell(row, 1).Value = env.EnvironmentName;
            envSheet.Cell(row, 2).Value = env.IsProd ? "Production" : "Non-Production";
            envSheet.Cell(row, 3).Value = env.Apps;
            envSheet.Cell(row, 4).Value = env.Replicas;
            envSheet.Cell(row, 5).Value = env.Pods;
            envSheet.Cell(row, 6).Value = env.Masters;
            envSheet.Cell(row, 7).Value = env.Infra;
            envSheet.Cell(row, 8).Value = env.Workers;
            envSheet.Cell(row, 9).Value = env.TotalNodes;
            envSheet.Cell(row, 10).Value = env.TotalCpu;
            envSheet.Cell(row, 11).Value = env.TotalRam;
            envSheet.Cell(row, 12).Value = env.TotalDisk;
            row++;
        }

        // Grand total row
        envSheet.Cell(row, 1).Value = "GRAND TOTAL";
        envSheet.Cell(row, 1).Style.Font.Bold = true;
        envSheet.Cell(row, 6).Value = result.GrandTotal.TotalMasters;
        envSheet.Cell(row, 7).Value = result.GrandTotal.TotalInfra;
        envSheet.Cell(row, 8).Value = result.GrandTotal.TotalWorkers;
        envSheet.Cell(row, 9).Value = result.GrandTotal.TotalNodes;
        envSheet.Cell(row, 10).Value = result.GrandTotal.TotalCpu;
        envSheet.Cell(row, 11).Value = result.GrandTotal.TotalRam;
        envSheet.Cell(row, 12).Value = result.GrandTotal.TotalDisk;
        envSheet.Row(row).Style.Font.Bold = true;
        envSheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;

        envSheet.Columns().AdjustToContents();

        // Node Specs worksheet
        if (result.NodeSpecs != null)
        {
            var specsSheet = workbook.Worksheets.Add("Node Specs");
            specsSheet.Cell(1, 1).Value = "Node Type";
            specsSheet.Cell(1, 2).Value = "Environment";
            specsSheet.Cell(1, 3).Value = "CPU";
            specsSheet.Cell(1, 4).Value = "RAM (GB)";
            specsSheet.Cell(1, 5).Value = "Disk (GB)";
            specsSheet.Row(1).Style.Font.Bold = true;

            var specs = result.NodeSpecs;
            int specRow = 2;

            specsSheet.Cell(specRow, 1).Value = "Control Plane";
            specsSheet.Cell(specRow, 2).Value = "Production";
            specsSheet.Cell(specRow, 3).Value = specs.ProdControlPlane.Cpu;
            specsSheet.Cell(specRow, 4).Value = specs.ProdControlPlane.Ram;
            specsSheet.Cell(specRow, 5).Value = specs.ProdControlPlane.Disk;
            specRow++;

            specsSheet.Cell(specRow, 1).Value = "Control Plane";
            specsSheet.Cell(specRow, 2).Value = "Non-Production";
            specsSheet.Cell(specRow, 3).Value = specs.NonProdControlPlane.Cpu;
            specsSheet.Cell(specRow, 4).Value = specs.NonProdControlPlane.Ram;
            specsSheet.Cell(specRow, 5).Value = specs.NonProdControlPlane.Disk;
            specRow++;

            specsSheet.Cell(specRow, 1).Value = "Worker";
            specsSheet.Cell(specRow, 2).Value = "Production";
            specsSheet.Cell(specRow, 3).Value = specs.ProdWorker.Cpu;
            specsSheet.Cell(specRow, 4).Value = specs.ProdWorker.Ram;
            specsSheet.Cell(specRow, 5).Value = specs.ProdWorker.Disk;
            specRow++;

            specsSheet.Cell(specRow, 1).Value = "Worker";
            specsSheet.Cell(specRow, 2).Value = "Non-Production";
            specsSheet.Cell(specRow, 3).Value = specs.NonProdWorker.Cpu;
            specsSheet.Cell(specRow, 4).Value = specs.NonProdWorker.Ram;
            specsSheet.Cell(specRow, 5).Value = specs.NonProdWorker.Disk;
            specRow++;

            if (specs.HasInfraNodes)
            {
                specsSheet.Cell(specRow, 1).Value = "Infrastructure";
                specsSheet.Cell(specRow, 2).Value = "Production";
                specsSheet.Cell(specRow, 3).Value = specs.ProdInfra.Cpu;
                specsSheet.Cell(specRow, 4).Value = specs.ProdInfra.Ram;
                specsSheet.Cell(specRow, 5).Value = specs.ProdInfra.Disk;
                specRow++;

                specsSheet.Cell(specRow, 1).Value = "Infrastructure";
                specsSheet.Cell(specRow, 2).Value = "Non-Production";
                specsSheet.Cell(specRow, 3).Value = specs.NonProdInfra.Cpu;
                specsSheet.Cell(specRow, 4).Value = specs.NonProdInfra.Ram;
                specsSheet.Cell(specRow, 5).Value = specs.NonProdInfra.Disk;
            }

            specsSheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Generate HTML diagram for visual representation
    /// </summary>
    public string ExportToHtmlDiagram(K8sSizingResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("    <title>Infrastructure Sizing Diagram</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Segoe UI', Arial, sans-serif; background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%); color: #fff; padding: 2rem; margin: 0; min-height: 100vh; }");
        sb.AppendLine("        .container { max-width: 1200px; margin: 0 auto; }");
        sb.AppendLine("        h1 { text-align: center; color: #4361ee; margin-bottom: 2rem; }");
        sb.AppendLine("        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 2rem; }");
        sb.AppendLine("        .summary-card { background: #0f3460; border-radius: 12px; padding: 1.5rem; text-align: center; }");
        sb.AppendLine("        .summary-card .value { font-size: 2.5rem; font-weight: bold; color: #06d6a0; }");
        sb.AppendLine("        .summary-card .label { color: #a0aec0; font-size: 0.9rem; margin-top: 0.5rem; }");
        sb.AppendLine("        .environments { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1.5rem; }");
        sb.AppendLine("        .env-card { background: #16213e; border-radius: 12px; padding: 1.5rem; border-left: 4px solid; }");
        sb.AppendLine("        .env-card.prod { border-color: #f85149; }");
        sb.AppendLine("        .env-card.nonprod { border-color: #58a6ff; }");
        sb.AppendLine("        .env-card h3 { margin: 0 0 1rem 0; display: flex; align-items: center; gap: 0.5rem; }");
        sb.AppendLine("        .env-card .badge { font-size: 0.7rem; padding: 2px 8px; border-radius: 4px; }");
        sb.AppendLine("        .env-card .badge.prod { background: #f85149; }");
        sb.AppendLine("        .env-card .badge.nonprod { background: #58a6ff; }");
        sb.AppendLine("        .nodes { display: flex; gap: 1rem; margin: 1rem 0; }");
        sb.AppendLine("        .node-group { flex: 1; background: #0d1117; border-radius: 8px; padding: 1rem; text-align: center; }");
        sb.AppendLine("        .node-group .count { font-size: 1.5rem; font-weight: bold; }");
        sb.AppendLine("        .node-group .type { font-size: 0.8rem; color: #8b949e; }");
        sb.AppendLine("        .node-group.master .count { color: #a371f7; }");
        sb.AppendLine("        .node-group.infra .count { color: #f0883e; }");
        sb.AppendLine("        .node-group.worker .count { color: #3fb950; }");
        sb.AppendLine("        .resources { display: flex; gap: 1rem; margin-top: 1rem; }");
        sb.AppendLine("        .resource { flex: 1; text-align: center; }");
        sb.AppendLine("        .resource .value { font-weight: bold; color: #58a6ff; }");
        sb.AppendLine("        .resource .label { font-size: 0.75rem; color: #8b949e; }");
        sb.AppendLine("        .footer { text-align: center; margin-top: 2rem; color: #8b949e; font-size: 0.85rem; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"container\">");
        sb.AppendLine($"        <h1>Infrastructure Sizing: {result.DistributionName}</h1>");

        // Summary cards
        sb.AppendLine("        <div class=\"summary\">");
        sb.AppendLine($"            <div class=\"summary-card\"><div class=\"value\">{result.Environments.Count}</div><div class=\"label\">Clusters</div></div>");
        sb.AppendLine($"            <div class=\"summary-card\"><div class=\"value\">{result.GrandTotal.TotalNodes}</div><div class=\"label\">Total Nodes</div></div>");
        sb.AppendLine($"            <div class=\"summary-card\"><div class=\"value\">{result.GrandTotal.TotalCpu}</div><div class=\"label\">Total vCPU</div></div>");
        sb.AppendLine($"            <div class=\"summary-card\"><div class=\"value\">{result.GrandTotal.TotalRam:N0}</div><div class=\"label\">Total RAM (GB)</div></div>");
        sb.AppendLine($"            <div class=\"summary-card\"><div class=\"value\">{result.GrandTotal.TotalDisk:N0}</div><div class=\"label\">Total Disk (GB)</div></div>");
        sb.AppendLine("        </div>");

        // Environment cards
        sb.AppendLine("        <div class=\"environments\">");
        foreach (var env in result.Environments)
        {
            var envClass = env.IsProd ? "prod" : "nonprod";
            var badgeText = env.IsProd ? "PROD" : "NON-PROD";

            sb.AppendLine($"            <div class=\"env-card {envClass}\">");
            sb.AppendLine($"                <h3>{env.EnvironmentName} <span class=\"badge {envClass}\">{badgeText}</span></h3>");
            sb.AppendLine("                <div class=\"nodes\">");
            sb.AppendLine($"                    <div class=\"node-group master\"><div class=\"count\">{env.Masters}</div><div class=\"type\">Masters</div></div>");
            if (env.Infra > 0)
            {
                sb.AppendLine($"                    <div class=\"node-group infra\"><div class=\"count\">{env.Infra}</div><div class=\"type\">Infra</div></div>");
            }
            sb.AppendLine($"                    <div class=\"node-group worker\"><div class=\"count\">{env.Workers}</div><div class=\"type\">Workers</div></div>");
            sb.AppendLine("                </div>");
            sb.AppendLine("                <div class=\"resources\">");
            sb.AppendLine($"                    <div class=\"resource\"><div class=\"value\">{env.TotalNodes}</div><div class=\"label\">Nodes</div></div>");
            sb.AppendLine($"                    <div class=\"resource\"><div class=\"value\">{env.TotalCpu}</div><div class=\"label\">vCPU</div></div>");
            sb.AppendLine($"                    <div class=\"resource\"><div class=\"value\">{env.TotalRam}</div><div class=\"label\">RAM GB</div></div>");
            sb.AppendLine($"                    <div class=\"resource\"><div class=\"value\">{env.TotalDisk}</div><div class=\"label\">Disk GB</div></div>");
            sb.AppendLine("                </div>");
            sb.AppendLine("            </div>");
        }
        sb.AppendLine("        </div>");

        sb.AppendLine($"        <div class=\"footer\">Generated: {result.CalculatedAt:yyyy-MM-dd HH:mm:ss} | Technology: {result.TechnologyName}</div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Export K8s sizing result to PDF
    /// </summary>
    public byte[] ExportToPdf(K8sSizingResult result)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeK8sHeader(c, result));
                page.Content().Element(c => ComposeK8sContent(c, result));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Export VM sizing result to PDF
    /// </summary>
    public byte[] ExportToPdf(VMSizingResult result)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeVMHeader(c, result));
                page.Content().Element(c => ComposeVMContent(c, result));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeK8sHeader(IContainer container, K8sSizingResult result)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Infrastructure Sizing Report")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                    c.Item().Text($"Kubernetes: {result.DistributionName}")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(100).AlignRight().Column(c =>
                {
                    c.Item().Text(result.CalculatedAt.ToString("yyyy-MM-dd")).FontSize(9).FontColor(Colors.Grey.Medium);
                    c.Item().Text(result.CalculatedAt.ToString("HH:mm:ss")).FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeVMHeader(IContainer container, VMSizingResult result)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Infrastructure Sizing Report")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                    c.Item().Text($"VM Deployment: {result.TechnologyName}")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(100).AlignRight().Column(c =>
                {
                    c.Item().Text(result.CalculatedAt.ToString("yyyy-MM-dd")).FontSize(9).FontColor(Colors.Grey.Medium);
                    c.Item().Text(result.CalculatedAt.ToString("HH:mm:ss")).FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeK8sContent(IContainer container, K8sSizingResult result)
    {
        container.Column(column =>
        {
            // Executive Summary
            column.Item().PaddingTop(15).Text("Executive Summary").FontSize(14).Bold();
            column.Item().PaddingTop(10).Element(c => ComposeSummaryTable(c, result));

            // Configuration
            column.Item().PaddingTop(20).Text("Configuration").FontSize(14).Bold();
            column.Item().PaddingTop(10).Element(c => ComposeConfigSection(c, result));

            // Environment Details
            column.Item().PaddingTop(20).Text("Environment Breakdown").FontSize(14).Bold();
            column.Item().PaddingTop(10).Element(c => ComposeEnvironmentsTable(c, result));

            // Node Specifications
            if (result.NodeSpecs != null)
            {
                column.Item().PaddingTop(20).Text("Node Specifications").FontSize(14).Bold();
                column.Item().PaddingTop(10).Element(c => ComposeNodeSpecsTable(c, result.NodeSpecs));
            }
        });
    }

    private void ComposeVMContent(IContainer container, VMSizingResult result)
    {
        container.Column(column =>
        {
            // Executive Summary
            column.Item().PaddingTop(15).Text("Executive Summary").FontSize(14).Bold();
            column.Item().PaddingTop(10).Element(c => ComposeVMSummaryTable(c, result));

            // Environment Details
            foreach (var env in result.Environments)
            {
                column.Item().PaddingTop(20).Text($"{env.EnvironmentName} Environment").FontSize(14).Bold();
                column.Item().PaddingTop(10).Element(c => ComposeVMEnvironmentTable(c, env));
            }
        });
    }

    private void ComposeSummaryTable(IContainer container, K8sSizingResult result)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
            });

            table.Cell().Element(SummaryLabelStyle).Text("Total Nodes:");
            table.Cell().Element(SummaryValueStyle).Text(result.GrandTotal.TotalNodes.ToString());
            table.Cell().Element(SummaryLabelStyle).Text("Total vCPU:");
            table.Cell().Element(SummaryValueStyle).Text(result.GrandTotal.TotalCpu.ToString());

            table.Cell().Element(SummaryLabelStyle).Text("Total RAM (GB):");
            table.Cell().Element(SummaryValueStyle).Text(result.GrandTotal.TotalRam.ToString("N0"));
            table.Cell().Element(SummaryLabelStyle).Text("Total Disk (GB):");
            table.Cell().Element(SummaryValueStyle).Text(result.GrandTotal.TotalDisk.ToString("N0"));

            table.Cell().Element(SummaryLabelStyle).Text("Environments:");
            table.Cell().Element(SummaryValueStyle).Text(result.Environments.Count.ToString());
            table.Cell().Element(SummaryLabelStyle).Text("Cluster Mode:");
            table.Cell().Element(SummaryValueStyle).Text(result.Configuration.ClusterMode.ToString());
        });
    }

    private void ComposeVMSummaryTable(IContainer container, VMSizingResult result)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
            });

            table.Cell().Element(SummaryLabelStyle).Text("Total VMs:");
            table.Cell().Element(SummaryValueStyle).Text(result.GrandTotal.TotalVMs.ToString());
            table.Cell().Element(SummaryLabelStyle).Text("Total vCPU:");
            table.Cell().Element(SummaryValueStyle).Text(result.GrandTotal.TotalCpu.ToString());

            table.Cell().Element(SummaryLabelStyle).Text("Total RAM (GB):");
            table.Cell().Element(SummaryValueStyle).Text(result.GrandTotal.TotalRam.ToString("N0"));
            table.Cell().Element(SummaryLabelStyle).Text("Total Disk (GB):");
            table.Cell().Element(SummaryValueStyle).Text(result.GrandTotal.TotalDisk.ToString("N0"));

            table.Cell().Element(SummaryLabelStyle).Text("Environments:");
            table.Cell().Element(SummaryValueStyle).Text(result.Environments.Count.ToString());
            table.Cell().Element(SummaryLabelStyle).Text("Technology:");
            table.Cell().Element(SummaryValueStyle).Text(result.TechnologyName);
        });
    }

    private void ComposeConfigSection(IContainer container, K8sSizingResult result)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
            });

            table.Cell().Element(ConfigLabelStyle).Text("Distribution:");
            table.Cell().Element(ConfigValueStyle).Text(result.DistributionName);

            table.Cell().Element(ConfigLabelStyle).Text("Technology:");
            table.Cell().Element(ConfigValueStyle).Text(result.TechnologyName);

            table.Cell().Element(ConfigLabelStyle).Text("Headroom Enabled:");
            table.Cell().Element(ConfigValueStyle).Text(result.Configuration.EnableHeadroom ? "Yes" : "No");
        });
    }

    private void ComposeEnvironmentsTable(IContainer container, K8sSizingResult result)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).Text("Environment");
                header.Cell().Element(HeaderStyle).Text("Apps");
                header.Cell().Element(HeaderStyle).Text("Replicas");
                header.Cell().Element(HeaderStyle).Text("Masters");
                header.Cell().Element(HeaderStyle).Text("Infra");
                header.Cell().Element(HeaderStyle).Text("Workers");
                header.Cell().Element(HeaderStyle).Text("vCPU");
                header.Cell().Element(HeaderStyle).Text("RAM (GB)");
            });

            // Data rows
            foreach (var env in result.Environments)
            {
                var bgColor = env.IsProd ? Colors.Red.Lighten5 : Colors.Blue.Lighten5;

                table.Cell().Element(c => CellStyle(c, bgColor)).Text(env.EnvironmentName);
                table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(env.Apps.ToString());
                table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(env.Replicas.ToString());
                table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(env.Masters.ToString());
                table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(env.Infra.ToString());
                table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(env.Workers.ToString());
                table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(env.TotalCpu.ToString());
                table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(env.TotalRam.ToString("N0"));
            }

            // Grand total row
            table.Cell().Element(TotalRowStyle).Text("GRAND TOTAL");
            table.Cell().Element(TotalRowStyle).Text("");
            table.Cell().Element(TotalRowStyle).Text("");
            table.Cell().Element(TotalRowStyle).AlignCenter().Text(result.GrandTotal.TotalMasters.ToString());
            table.Cell().Element(TotalRowStyle).AlignCenter().Text(result.GrandTotal.TotalInfra.ToString());
            table.Cell().Element(TotalRowStyle).AlignCenter().Text(result.GrandTotal.TotalWorkers.ToString());
            table.Cell().Element(TotalRowStyle).AlignCenter().Text(result.GrandTotal.TotalCpu.ToString());
            table.Cell().Element(TotalRowStyle).AlignCenter().Text(result.GrandTotal.TotalRam.ToString("N0"));
        });
    }

    private void ComposeVMEnvironmentTable(IContainer container, VMEnvironmentResult env)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("Role");
                    header.Cell().Element(HeaderStyle).Text("Instances");
                    header.Cell().Element(HeaderStyle).Text("vCPU");
                    header.Cell().Element(HeaderStyle).Text("RAM (GB)");
                    header.Cell().Element(HeaderStyle).Text("Disk (GB)");
                });

                // Data rows
                foreach (var role in env.Roles)
                {
                    var bgColor = Colors.White;
                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(role.RoleName);
                    table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(role.TotalInstances.ToString());
                    table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(role.TotalCpu.ToString());
                    table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(role.TotalRam.ToString("N0"));
                    table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(role.TotalDisk.ToString("N0"));
                }

                // Environment total
                table.Cell().Element(TotalRowStyle).Text("TOTAL");
                table.Cell().Element(TotalRowStyle).AlignCenter().Text(env.TotalVMs.ToString());
                table.Cell().Element(TotalRowStyle).AlignCenter().Text(env.TotalCpu.ToString());
                table.Cell().Element(TotalRowStyle).AlignCenter().Text(env.TotalRam.ToString("N0"));
                table.Cell().Element(TotalRowStyle).AlignCenter().Text(env.TotalDisk.ToString("N0"));
            });
        });
    }

    private void ComposeNodeSpecsTable(IContainer container, DistributionConfig specs)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).Text("Node Type");
                header.Cell().Element(HeaderStyle).Text("Environment");
                header.Cell().Element(HeaderStyle).Text("CPU");
                header.Cell().Element(HeaderStyle).Text("RAM (GB)");
                header.Cell().Element(HeaderStyle).Text("Disk (GB)");
            });

            // Control Plane
            table.Cell().Element(c => CellStyle(c, Colors.White)).Text("Control Plane");
            table.Cell().Element(c => CellStyle(c, Colors.White)).Text("Production");
            table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdControlPlane.Cpu.ToString());
            table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdControlPlane.Ram.ToString());
            table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdControlPlane.Disk.ToString());

            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).Text("Control Plane");
            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).Text("Non-Production");
            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdControlPlane.Cpu.ToString());
            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdControlPlane.Ram.ToString());
            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdControlPlane.Disk.ToString());

            // Worker
            table.Cell().Element(c => CellStyle(c, Colors.White)).Text("Worker");
            table.Cell().Element(c => CellStyle(c, Colors.White)).Text("Production");
            table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdWorker.Cpu.ToString());
            table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdWorker.Ram.ToString());
            table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdWorker.Disk.ToString());

            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).Text("Worker");
            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).Text("Non-Production");
            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdWorker.Cpu.ToString());
            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdWorker.Ram.ToString());
            table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdWorker.Disk.ToString());

            // Infrastructure (if applicable)
            if (specs.HasInfraNodes)
            {
                table.Cell().Element(c => CellStyle(c, Colors.White)).Text("Infrastructure");
                table.Cell().Element(c => CellStyle(c, Colors.White)).Text("Production");
                table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdInfra.Cpu.ToString());
                table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdInfra.Ram.ToString());
                table.Cell().Element(c => CellStyle(c, Colors.White)).AlignCenter().Text(specs.ProdInfra.Disk.ToString());

                table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).Text("Infrastructure");
                table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).Text("Non-Production");
                table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdInfra.Cpu.ToString());
                table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdInfra.Ram.ToString());
                table.Cell().Element(c => CellStyle(c, Colors.Grey.Lighten4)).AlignCenter().Text(specs.NonProdInfra.Disk.ToString());
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().AlignLeft().Text(text =>
            {
                text.Span("Infrastructure Sizing Calculator").FontSize(8).FontColor(Colors.Grey.Medium);
            });

            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    // Style helpers
    private static IContainer HeaderStyle(IContainer container)
    {
        return container
            .Background(Colors.Blue.Darken2)
            .Padding(5)
            .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(9));
    }

    private static IContainer CellStyle(IContainer container, string backgroundColor)
    {
        return container
            .Background(backgroundColor)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }

    private static IContainer TotalRowStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Padding(5)
            .DefaultTextStyle(x => x.Bold().FontSize(9));
    }

    private static IContainer SummaryLabelStyle(IContainer container)
    {
        return container
            .Padding(3)
            .DefaultTextStyle(x => x.FontColor(Colors.Grey.Darken1).FontSize(10));
    }

    private static IContainer SummaryValueStyle(IContainer container)
    {
        return container
            .Padding(3)
            .DefaultTextStyle(x => x.Bold().FontSize(10));
    }

    private static IContainer ConfigLabelStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten4)
            .Padding(5)
            .DefaultTextStyle(x => x.FontColor(Colors.Grey.Darken1).FontSize(9));
    }

    private static IContainer ConfigValueStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten5)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }
}
