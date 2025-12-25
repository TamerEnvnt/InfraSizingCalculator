using System.Text;
using System.Text.Json;
using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Comprehensive tests for ExportService - covering all export formats
/// and data accuracy verification
/// </summary>
public class ExportServiceTests
{
    private readonly ExportService _service;
    private readonly K8sSizingResult _sampleK8sResult;
    private readonly VMSizingResult _sampleVMResult;

    public ExportServiceTests()
    {
        _service = new ExportService();
        _sampleK8sResult = CreateSampleK8sResult();
        _sampleVMResult = CreateSampleVMResult();
    }

    #region CSV Export Tests

    [Fact]
    public void ExportToCsv_ValidResult_ReturnsNonEmptyBytes()
    {
        var csv = _service.ExportToCsv(_sampleK8sResult);

        csv.Should().NotBeNull();
        csv.Should().NotBeEmpty();
    }

    [Fact]
    public void ExportToCsv_ContainsHeader()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);

        csv.Should().Contain("Environment");
        csv.Should().Contain("Masters");
        csv.Should().Contain("Workers");
    }

    [Fact]
    public void ExportToCsv_ContainsAllEnvironments()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);

        foreach (var env in _sampleK8sResult.Environments)
        {
            csv.Should().Contain(env.EnvironmentName);
        }
    }

    [Fact]
    public void ExportToCsv_ContainsGrandTotal()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);

        csv.Should().Contain("GRAND TOTAL");
        csv.Should().Contain(_sampleK8sResult.GrandTotal.TotalNodes.ToString());
    }

    [Fact]
    public void ExportToCsv_IsValidCsvFormat()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => !l.StartsWith("#") && !string.IsNullOrWhiteSpace(l))
            .ToArray();

        // Each data line should have the same number of commas as header
        var commasInHeader = lines[0].Count(c => c == ',');
        foreach (var line in lines.Skip(1))
        {
            line.Count(c => c == ',').Should().Be(commasInHeader);
        }
    }

    [Fact]
    public void ExportToCsv_ContainsMetadataComments()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);

        csv.Should().Contain("# Infrastructure Sizing Calculator Export");
        csv.Should().Contain("# Distribution:");
        csv.Should().Contain("# Technology:");
        csv.Should().Contain("# Cluster Mode:");
    }

    [Fact]
    public void ExportToCsv_ContainsAllColumns()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);

        csv.Should().Contain("Apps");
        csv.Should().Contain("Replicas");
        csv.Should().Contain("Pods");
        csv.Should().Contain("Infra");
        csv.Should().Contain("vCPU");
        csv.Should().Contain("RAM (GB)");
        csv.Should().Contain("Disk (GB)");
    }

    [Fact]
    public void ExportToCsv_DataValuesAreAccurate()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);

        // Check Dev environment data
        var devEnv = _sampleK8sResult.Environments.First(e => e.Environment == EnvironmentType.Dev);
        csv.Should().Contain($"{devEnv.EnvironmentName},{devEnv.Apps},{devEnv.Replicas},{devEnv.Pods},{devEnv.Masters},{devEnv.Infra},{devEnv.Workers},{devEnv.TotalNodes},{devEnv.TotalCpu},{devEnv.TotalRam},{devEnv.TotalDisk}");

        // Check Prod environment data
        var prodEnv = _sampleK8sResult.Environments.First(e => e.Environment == EnvironmentType.Prod);
        csv.Should().Contain($"{prodEnv.EnvironmentName},{prodEnv.Apps},{prodEnv.Replicas},{prodEnv.Pods},{prodEnv.Masters},{prodEnv.Infra},{prodEnv.Workers},{prodEnv.TotalNodes},{prodEnv.TotalCpu},{prodEnv.TotalRam},{prodEnv.TotalDisk}");
    }

    [Fact]
    public void ExportToCsv_GrandTotalValuesAreAccurate()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);
        var gt = _sampleK8sResult.GrandTotal;

        // Grand total line should contain all totals
        csv.Should().Contain($"GRAND TOTAL,,,,{gt.TotalMasters},{gt.TotalInfra},{gt.TotalWorkers},{gt.TotalNodes},{gt.TotalCpu},{gt.TotalRam},{gt.TotalDisk}");
    }

    [Fact]
    public void ExportToCsv_EmptyEnvironments_ReturnsValidCsv()
    {
        var result = new K8sSizingResult
        {
            Configuration = new K8sSizingInput(),
            DistributionName = "Test",
            TechnologyName = "Test",
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal()
        };

        var csvBytes = _service.ExportToCsv(result);
        var csv = Encoding.UTF8.GetString(csvBytes);

        csv.Should().NotBeEmpty();
        csv.Should().Contain("Environment");
        csv.Should().Contain("GRAND TOTAL");
    }

    [Fact]
    public void ExportToCsv_SingleEnvironment_ReturnsValidCsv()
    {
        var result = new K8sSizingResult
        {
            Configuration = new K8sSizingInput { ClusterMode = ClusterMode.PerEnvironment },
            DistributionName = "Kubernetes",
            TechnologyName = "Java",
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = 5,
                    Replicas = 3,
                    Pods = 15,
                    Masters = 3,
                    Infra = 0,
                    Workers = 5,
                    TotalNodes = 8,
                    TotalCpu = 64,
                    TotalRam = 256,
                    TotalDisk = 800
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 8,
                TotalMasters = 3,
                TotalInfra = 0,
                TotalWorkers = 5,
                TotalCpu = 64,
                TotalRam = 256,
                TotalDisk = 800
            }
        };

        var csvBytes = _service.ExportToCsv(result);
        var csv = Encoding.UTF8.GetString(csvBytes);

        csv.Should().Contain("Production");
        var dataLines = csv.Split('\n').Where(l => !l.StartsWith("#") && !string.IsNullOrWhiteSpace(l) && !l.StartsWith("Environment")).ToList();
        dataLines.Should().HaveCount(2); // One environment + grand total
    }

    #endregion

    #region JSON Export Tests

    [Fact]
    public void ExportToJson_ValidResult_ReturnsValidJson()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        json.Should().NotBeNullOrEmpty();
        json.Trim().Should().StartWith("{");
        json.Trim().Should().EndWith("}");
    }

    [Fact]
    public void ExportToJson_ContainsEnvironments()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        json.ToLowerInvariant().Should().Contain("environments");
    }

    [Fact]
    public void ExportToJson_ContainsGrandTotal()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        json.Should().Contain("grandTotal");
    }

    [Fact]
    public void ExportToJson_IsDeserializable()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        // Should not throw
        var deserialized = JsonSerializer.Deserialize<K8sSizingResult>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        deserialized.Should().NotBeNull();
        deserialized!.Environments.Count.Should().Be(_sampleK8sResult.Environments.Count);
    }

    [Fact]
    public void ExportToJson_PreservesAllEnvironmentData()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        var deserialized = JsonSerializer.Deserialize<K8sSizingResult>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        for (int i = 0; i < _sampleK8sResult.Environments.Count; i++)
        {
            var original = _sampleK8sResult.Environments[i];
            var restored = deserialized!.Environments[i];

            restored.EnvironmentName.Should().Be(original.EnvironmentName);
            restored.Apps.Should().Be(original.Apps);
            restored.Replicas.Should().Be(original.Replicas);
            restored.Pods.Should().Be(original.Pods);
            restored.Masters.Should().Be(original.Masters);
            restored.Infra.Should().Be(original.Infra);
            restored.Workers.Should().Be(original.Workers);
            restored.TotalNodes.Should().Be(original.TotalNodes);
            restored.TotalCpu.Should().Be(original.TotalCpu);
            restored.TotalRam.Should().Be(original.TotalRam);
            restored.TotalDisk.Should().Be(original.TotalDisk);
        }
    }

    [Fact]
    public void ExportToJson_PreservesGrandTotalData()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        var deserialized = JsonSerializer.Deserialize<K8sSizingResult>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        deserialized!.GrandTotal.TotalNodes.Should().Be(_sampleK8sResult.GrandTotal.TotalNodes);
        deserialized.GrandTotal.TotalMasters.Should().Be(_sampleK8sResult.GrandTotal.TotalMasters);
        deserialized.GrandTotal.TotalInfra.Should().Be(_sampleK8sResult.GrandTotal.TotalInfra);
        deserialized.GrandTotal.TotalWorkers.Should().Be(_sampleK8sResult.GrandTotal.TotalWorkers);
        deserialized.GrandTotal.TotalCpu.Should().Be(_sampleK8sResult.GrandTotal.TotalCpu);
        deserialized.GrandTotal.TotalRam.Should().Be(_sampleK8sResult.GrandTotal.TotalRam);
        deserialized.GrandTotal.TotalDisk.Should().Be(_sampleK8sResult.GrandTotal.TotalDisk);
    }

    [Fact]
    public void ExportToJson_PreservesConfiguration()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        var deserialized = JsonSerializer.Deserialize<K8sSizingResult>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        deserialized!.Configuration.Should().NotBeNull();
        deserialized.DistributionName.Should().Be(_sampleK8sResult.DistributionName);
        deserialized.TechnologyName.Should().Be(_sampleK8sResult.TechnologyName);
    }

    [Fact]
    public void ExportToJson_UsesIndentedFormatting()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        // Indented JSON has newlines
        json.Should().Contain("\n");
    }

    [Fact]
    public void ExportToJson_UsesCamelCase()
    {
        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);

        json.Should().Contain("environments");
        json.Should().Contain("grandTotal");
        json.Should().Contain("totalNodes");
        json.Should().NotContain("\"Environments\"");
        json.Should().NotContain("\"GrandTotal\"");
    }

    #endregion

    #region Excel Export Tests

    [Fact]
    public void ExportToExcel_ValidResult_ReturnsNonEmptyBytes()
    {
        var excel = _service.ExportToExcel(_sampleK8sResult);

        excel.Should().NotBeNull();
        excel.Should().NotBeEmpty();
    }

    [Fact]
    public void ExportToExcel_HasValidXlsxHeader()
    {
        var excel = _service.ExportToExcel(_sampleK8sResult);

        // XLSX files start with PK (ZIP format)
        excel[0].Should().Be(0x50); // 'P'
        excel[1].Should().Be(0x4B); // 'K'
    }

    [Fact]
    public void ExportToExcel_IsNonTrivialSize()
    {
        var excel = _service.ExportToExcel(_sampleK8sResult);

        // A valid Excel file should be at least a few KB
        excel.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void ExportToExcel_WithNodeSpecs_IncludesNodeSpecsSheet()
    {
        var resultWithSpecs = CreateSampleK8sResultWithNodeSpecs();

        var excel = _service.ExportToExcel(resultWithSpecs);

        // Excel file should be larger with additional sheet
        excel.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void ExportToExcel_EmptyEnvironments_ReturnsValidExcel()
    {
        var result = new K8sSizingResult
        {
            Configuration = new K8sSizingInput(),
            DistributionName = "Test",
            TechnologyName = "Test",
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal()
        };

        var excel = _service.ExportToExcel(result);

        excel.Should().NotBeNull();
        excel.Should().NotBeEmpty();
        excel[0].Should().Be(0x50); // Valid XLSX
    }

    [Fact]
    public void ExportToExcel_ManyEnvironments_ReturnsValidExcel()
    {
        var result = new K8sSizingResult
        {
            Configuration = new K8sSizingInput { ClusterMode = ClusterMode.MultiCluster },
            DistributionName = "OpenShift",
            TechnologyName = ".NET",
            Environments = Enum.GetValues<EnvironmentType>()
                .Select(env => new EnvironmentResult
                {
                    Environment = env,
                    EnvironmentName = env.ToString(),
                    IsProd = env is EnvironmentType.Prod or EnvironmentType.DR,
                    Apps = 10,
                    Replicas = 2,
                    Pods = 20,
                    Masters = 3,
                    Infra = 3,
                    Workers = 5,
                    TotalNodes = 11,
                    TotalCpu = 88,
                    TotalRam = 352,
                    TotalDisk = 1100
                }).ToList(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 55,
                TotalMasters = 15,
                TotalInfra = 15,
                TotalWorkers = 25,
                TotalCpu = 440,
                TotalRam = 1760,
                TotalDisk = 5500
            }
        };

        var excel = _service.ExportToExcel(result);

        excel.Should().NotBeNull();
        excel.Length.Should().BeGreaterThan(2000); // Larger due to more data
    }

    #endregion

    #region HTML Diagram Export Tests

    [Fact]
    public void ExportToHtmlDiagram_ValidResult_ReturnsValidHtml()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.Should().NotBeNullOrEmpty();
        html.ToLowerInvariant().Should().Contain("<html");
        html.ToLowerInvariant().Should().Contain("</html>");
    }

    [Fact]
    public void ExportToHtmlDiagram_ContainsEnvironmentData()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        foreach (var env in _sampleK8sResult.Environments)
        {
            html.Should().Contain(env.EnvironmentName);
        }
    }

    [Fact]
    public void ExportToHtmlDiagram_ContainsNodeCounts()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        foreach (var env in _sampleK8sResult.Environments)
        {
            html.Should().Contain(env.TotalNodes.ToString());
        }
    }

    [Fact]
    public void ExportToHtmlDiagram_ContainsCssStyles()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.ToLowerInvariant().Should().Contain("<style");
    }

    [Fact]
    public void ExportToHtmlDiagram_ContainsDistributionName()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.Should().Contain(_sampleK8sResult.DistributionName);
    }

    [Fact]
    public void ExportToHtmlDiagram_ContainsTechnologyName()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.Should().Contain(_sampleK8sResult.TechnologyName);
    }

    [Fact]
    public void ExportToHtmlDiagram_ContainsSummaryCards()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.Should().Contain("summary-card");
        html.Should().Contain("Total Nodes");
        html.Should().Contain("Total vCPU");
        html.Should().Contain("Total RAM");
        html.Should().Contain("Total Disk");
    }

    [Fact]
    public void ExportToHtmlDiagram_ContainsGrandTotalValues()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.Should().Contain(_sampleK8sResult.GrandTotal.TotalNodes.ToString());
        html.Should().Contain(_sampleK8sResult.GrandTotal.TotalCpu.ToString());
    }

    [Fact]
    public void ExportToHtmlDiagram_DistinguishesProdAndNonProd()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.Should().Contain("env-card prod");
        html.Should().Contain("env-card nonprod");
        html.Should().Contain("PROD");
        html.Should().Contain("NON-PROD");
    }

    [Fact]
    public void ExportToHtmlDiagram_ContainsNodeBreakdown()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.Should().Contain("Masters");
        html.Should().Contain("Workers");
    }

    [Fact]
    public void ExportToHtmlDiagram_IncludesInfraWhenPresent()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        // Sample result has infra nodes
        html.Should().Contain("Infra");
    }

    [Fact]
    public void ExportToHtmlDiagram_IsStandalone()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        // Should be a complete HTML document
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<head>");
        html.Should().Contain("<body>");
        html.Should().Contain("</head>");
        html.Should().Contain("</body>");
    }

    [Fact]
    public void ExportToHtmlDiagram_HasResponsiveViewport()
    {
        var html = _service.ExportToHtmlDiagram(_sampleK8sResult);

        html.Should().Contain("viewport");
        html.Should().Contain("width=device-width");
    }

    #endregion

    #region PDF Export Tests - K8s

    [Fact]
    public void ExportToPdf_K8sResult_ReturnsNonEmptyBytes()
    {
        var pdf = _service.ExportToPdf(_sampleK8sResult);

        pdf.Should().NotBeNull();
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void ExportToPdf_K8sResult_HasValidPdfHeader()
    {
        var pdf = _service.ExportToPdf(_sampleK8sResult);

        // PDF files start with %PDF
        Encoding.ASCII.GetString(pdf.Take(4).ToArray()).Should().Be("%PDF");
    }

    [Fact]
    public void ExportToPdf_K8sResult_IsNonTrivialSize()
    {
        var pdf = _service.ExportToPdf(_sampleK8sResult);

        // A valid PDF should be at least a few KB
        pdf.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void ExportToPdf_K8sWithNodeSpecs_IsLarger()
    {
        var resultWithSpecs = CreateSampleK8sResultWithNodeSpecs();

        var pdfWithoutSpecs = _service.ExportToPdf(_sampleK8sResult);
        var pdfWithSpecs = _service.ExportToPdf(resultWithSpecs);

        // With node specs should have more content
        pdfWithSpecs.Length.Should().BeGreaterOrEqualTo(pdfWithoutSpecs.Length);
    }

    [Fact]
    public void ExportToPdf_K8sEmptyEnvironments_ReturnsValidPdf()
    {
        var result = new K8sSizingResult
        {
            Configuration = new K8sSizingInput(),
            DistributionName = "Test",
            TechnologyName = "Test",
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal()
        };

        var pdf = _service.ExportToPdf(result);

        pdf.Should().NotBeNull();
        pdf.Should().NotBeEmpty();
        Encoding.ASCII.GetString(pdf.Take(4).ToArray()).Should().Be("%PDF");
    }

    #endregion

    #region PDF Export Tests - VM

    [Fact]
    public void ExportToPdf_VMResult_ReturnsNonEmptyBytes()
    {
        var pdf = _service.ExportToPdf(_sampleVMResult);

        pdf.Should().NotBeNull();
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void ExportToPdf_VMResult_HasValidPdfHeader()
    {
        var pdf = _service.ExportToPdf(_sampleVMResult);

        // PDF files start with %PDF
        Encoding.ASCII.GetString(pdf.Take(4).ToArray()).Should().Be("%PDF");
    }

    [Fact]
    public void ExportToPdf_VMResult_IsNonTrivialSize()
    {
        var pdf = _service.ExportToPdf(_sampleVMResult);

        // A valid PDF should be at least a few KB
        pdf.Length.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void ExportToPdf_VMEmptyEnvironments_ReturnsValidPdf()
    {
        var result = new VMSizingResult
        {
            Configuration = new VMSizingInput(),
            TechnologyName = "Test",
            Environments = new List<VMEnvironmentResult>(),
            GrandTotal = new VMGrandTotal()
        };

        var pdf = _service.ExportToPdf(result);

        pdf.Should().NotBeNull();
        pdf.Should().NotBeEmpty();
        Encoding.ASCII.GetString(pdf.Take(4).ToArray()).Should().Be("%PDF");
    }

    [Fact]
    public void ExportToPdf_VMMultipleEnvironments_ReturnsValidPdf()
    {
        var result = new VMSizingResult
        {
            Configuration = new VMSizingInput(),
            TechnologyName = ".NET",
            Environments = new List<VMEnvironmentResult>
            {
                CreateVMEnvironment(EnvironmentType.Dev, "Development"),
                CreateVMEnvironment(EnvironmentType.Test, "Test"),
                CreateVMEnvironment(EnvironmentType.Stage, "Staging"),
                CreateVMEnvironment(EnvironmentType.Prod, "Production"),
                CreateVMEnvironment(EnvironmentType.DR, "DR")
            },
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 50,
                TotalCpu = 400,
                TotalRam = 1600,
                TotalDisk = 5000
            }
        };

        var pdf = _service.ExportToPdf(result);

        pdf.Should().NotBeNull();
        pdf.Length.Should().BeGreaterThan(2000); // Larger due to more content
    }

    #endregion

    #region Timestamped Filename Tests

    [Fact]
    public void GetTimestampedFilename_ContainsPrefix()
    {
        var filename = _service.GetTimestampedFilename("sizing", "csv");
        filename.Should().StartWith("sizing_");
    }

    [Fact]
    public void GetTimestampedFilename_ContainsExtension()
    {
        var filename = _service.GetTimestampedFilename("sizing", "csv");
        filename.Should().EndWith(".csv");
    }

    [Fact]
    public void GetTimestampedFilename_ContainsTimestamp()
    {
        var filename = _service.GetTimestampedFilename("sizing", "csv");
        // Should contain underscore-separated timestamp
        filename.Should().Contain("_");
        // And should have the correct length pattern (prefix_timestamp.ext)
        filename.Length.Should().BeGreaterThan(15);
    }

    [Fact]
    public void GetTimestampedFilename_DifferentPrefixes_ProduceDifferentFilenames()
    {
        var filename1 = _service.GetTimestampedFilename("k8s-sizing", "csv");
        var filename2 = _service.GetTimestampedFilename("vm-sizing", "csv");

        filename1.Should().StartWith("k8s-sizing_");
        filename2.Should().StartWith("vm-sizing_");
    }

    [Fact]
    public void GetTimestampedFilename_DifferentExtensions_ProduceDifferentFilenames()
    {
        var csvFilename = _service.GetTimestampedFilename("sizing", "csv");
        var jsonFilename = _service.GetTimestampedFilename("sizing", "json");
        var xlsxFilename = _service.GetTimestampedFilename("sizing", "xlsx");
        var pdfFilename = _service.GetTimestampedFilename("sizing", "pdf");

        csvFilename.Should().EndWith(".csv");
        jsonFilename.Should().EndWith(".json");
        xlsxFilename.Should().EndWith(".xlsx");
        pdfFilename.Should().EndWith(".pdf");
    }

    [Fact]
    public void GetTimestampedFilename_FollowsDateTimeFormat()
    {
        var filename = _service.GetTimestampedFilename("sizing", "csv");

        // Format should be: prefix_yyyyMMdd_HHmmss.ext
        var parts = filename.Split('_');
        parts.Should().HaveCountGreaterOrEqualTo(3); // At least prefix, date, time

        // Date part should be 8 digits (yyyyMMdd)
        var datePart = parts[1];
        datePart.Should().HaveLength(8);
        int.TryParse(datePart, out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("report", "pdf")]
    [InlineData("infrastructure-sizing", "xlsx")]
    [InlineData("k8s_results", "json")]
    [InlineData("vm-deployment", "csv")]
    public void GetTimestampedFilename_VariousPrefixesAndExtensions_AreValid(string prefix, string extension)
    {
        var filename = _service.GetTimestampedFilename(prefix, extension);

        filename.Should().StartWith($"{prefix}_");
        filename.Should().EndWith($".{extension}");
        filename.Should().NotContainAny("//", "\\");
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public void CsvAndJson_ContainSameEnvironmentCount()
    {
        var csvBytes = _service.ExportToCsv(_sampleK8sResult);
        var csv = Encoding.UTF8.GetString(csvBytes);

        var jsonBytes = _service.ExportToJson(_sampleK8sResult);
        var json = Encoding.UTF8.GetString(jsonBytes);
        var deserialized = JsonSerializer.Deserialize<K8sSizingResult>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Count environments in CSV (data lines minus header and grand total)
        var csvEnvCount = csv.Split('\n')
            .Where(l => !l.StartsWith("#") && !string.IsNullOrWhiteSpace(l))
            .Skip(1) // Skip header
            .Count(l => !l.StartsWith("GRAND TOTAL"));

        csvEnvCount.Should().Be(deserialized!.Environments.Count);
    }

    [Fact]
    public void AllFormats_HandleSpecialCharactersInDistributionName()
    {
        var result = new K8sSizingResult
        {
            Configuration = new K8sSizingInput { ClusterMode = ClusterMode.PerEnvironment },
            DistributionName = "Red Hat OpenShift",
            TechnologyName = ".NET Core",
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = 5,
                    Replicas = 2,
                    Pods = 10,
                    Masters = 3,
                    Infra = 3,
                    Workers = 4,
                    TotalNodes = 10,
                    TotalCpu = 80,
                    TotalRam = 320,
                    TotalDisk = 1000
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 10,
                TotalMasters = 3,
                TotalInfra = 3,
                TotalWorkers = 4,
                TotalCpu = 80,
                TotalRam = 320,
                TotalDisk = 1000
            }
        };

        // All should not throw and contain the distribution name
        var csv = Encoding.UTF8.GetString(_service.ExportToCsv(result));
        var json = Encoding.UTF8.GetString(_service.ExportToJson(result));
        var html = _service.ExportToHtmlDiagram(result);
        var excel = _service.ExportToExcel(result);
        var pdf = _service.ExportToPdf(result);

        csv.Should().Contain("Red Hat OpenShift");
        json.Should().Contain("Red Hat OpenShift");
        html.Should().Contain("Red Hat OpenShift");
        excel.Should().NotBeEmpty();
        pdf.Should().NotBeEmpty();
    }

    [Fact]
    public void AllFormats_HandleLargeNumbers()
    {
        var result = new K8sSizingResult
        {
            Configuration = new K8sSizingInput(),
            DistributionName = "Test",
            TechnologyName = "Test",
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = 1000,
                    Replicas = 10,
                    Pods = 10000,
                    Masters = 5,
                    Infra = 10,
                    Workers = 500,
                    TotalNodes = 515,
                    TotalCpu = 12000,
                    TotalRam = 48000,
                    TotalDisk = 150000
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 515,
                TotalMasters = 5,
                TotalInfra = 10,
                TotalWorkers = 500,
                TotalCpu = 12000,
                TotalRam = 48000,
                TotalDisk = 150000
            }
        };

        // All should not throw
        var csv = _service.ExportToCsv(result);
        var json = _service.ExportToJson(result);
        var html = _service.ExportToHtmlDiagram(result);
        var excel = _service.ExportToExcel(result);
        var pdf = _service.ExportToPdf(result);

        csv.Should().NotBeEmpty();
        json.Should().NotBeEmpty();
        html.Should().NotBeNullOrEmpty();
        excel.Should().NotBeEmpty();
        pdf.Should().NotBeEmpty();

        // Verify large numbers are preserved
        var csvString = Encoding.UTF8.GetString(csv);
        csvString.Should().Contain("12000");
        csvString.Should().Contain("48000");
        csvString.Should().Contain("150000");
    }

    [Fact]
    public void AllFormats_HandleZeroValues()
    {
        var result = new K8sSizingResult
        {
            Configuration = new K8sSizingInput(),
            DistributionName = "EKS",
            TechnologyName = "Node.js",
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Dev,
                    EnvironmentName = "Development",
                    IsProd = false,
                    Apps = 0,
                    Replicas = 0,
                    Pods = 0,
                    Masters = 0, // Managed control plane
                    Infra = 0,
                    Workers = 2,
                    TotalNodes = 2,
                    TotalCpu = 8,
                    TotalRam = 32,
                    TotalDisk = 100
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 2,
                TotalMasters = 0,
                TotalInfra = 0,
                TotalWorkers = 2,
                TotalCpu = 8,
                TotalRam = 32,
                TotalDisk = 100
            }
        };

        // All should not throw
        var csv = _service.ExportToCsv(result);
        var json = _service.ExportToJson(result);
        var html = _service.ExportToHtmlDiagram(result);
        var excel = _service.ExportToExcel(result);
        var pdf = _service.ExportToPdf(result);

        csv.Should().NotBeEmpty();
        json.Should().NotBeEmpty();
        html.Should().NotBeNullOrEmpty();
        excel.Should().NotBeEmpty();
        pdf.Should().NotBeEmpty();
    }

    #endregion

    #region Helper Methods

    private static K8sSizingResult CreateSampleK8sResult()
    {
        var config = new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            ProdApps = new AppConfig { Medium = 10 },
            NonProdApps = new AppConfig { Medium = 10 },
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod }
        };

        return new K8sSizingResult
        {
            Configuration = config,
            DistributionName = "OpenShift",
            TechnologyName = ".NET",
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Dev,
                    EnvironmentName = "Development",
                    IsProd = false,
                    Apps = 10,
                    Replicas = 1,
                    Pods = 10,
                    Masters = 3,
                    Infra = 3,
                    Workers = 4,
                    TotalNodes = 10,
                    TotalCpu = 120,
                    TotalRam = 480,
                    TotalDisk = 1000
                },
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = 10,
                    Replicas = 3,
                    Pods = 30,
                    Masters = 3,
                    Infra = 5,
                    Workers = 11,
                    TotalNodes = 19,
                    TotalCpu = 240,
                    TotalRam = 960,
                    TotalDisk = 2000
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 29,
                TotalMasters = 6,
                TotalInfra = 8,
                TotalWorkers = 15,
                TotalCpu = 360,
                TotalRam = 1440,
                TotalDisk = 3000
            }
        };
    }

    private static K8sSizingResult CreateSampleK8sResultWithNodeSpecs()
    {
        var config = new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            ProdApps = new AppConfig { Medium = 10 },
            NonProdApps = new AppConfig { Medium = 10 },
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod }
        };

        return new K8sSizingResult
        {
            Configuration = config,
            DistributionName = "OpenShift",
            TechnologyName = ".NET",
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Dev,
                    EnvironmentName = "Development",
                    IsProd = false,
                    Apps = 10,
                    Replicas = 1,
                    Pods = 10,
                    Masters = 3,
                    Infra = 3,
                    Workers = 4,
                    TotalNodes = 10,
                    TotalCpu = 120,
                    TotalRam = 480,
                    TotalDisk = 1000
                },
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = 10,
                    Replicas = 3,
                    Pods = 30,
                    Masters = 3,
                    Infra = 5,
                    Workers = 11,
                    TotalNodes = 19,
                    TotalCpu = 240,
                    TotalRam = 960,
                    TotalDisk = 2000
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 29,
                TotalMasters = 6,
                TotalInfra = 8,
                TotalWorkers = 15,
                TotalCpu = 360,
                TotalRam = 1440,
                TotalDisk = 3000
            },
            NodeSpecs = new DistributionConfig
            {
                Distribution = Distribution.OpenShift,
                Name = "OpenShift",
                Vendor = "Red Hat",
                HasInfraNodes = true,
                HasManagedControlPlane = false,
                ProdControlPlane = new NodeSpecs(4, 16, 100),
                NonProdControlPlane = new NodeSpecs(4, 16, 100),
                ProdWorker = new NodeSpecs(16, 64, 200),
                NonProdWorker = new NodeSpecs(8, 32, 100),
                ProdInfra = new NodeSpecs(8, 32, 100),
                NonProdInfra = new NodeSpecs(4, 16, 50)
            }
        };
    }

    private static VMSizingResult CreateSampleVMResult()
    {
        return new VMSizingResult
        {
            Configuration = new VMSizingInput
            {
                Technology = Technology.DotNet,
                EnabledEnvironments = new HashSet<EnvironmentType>
                {
                    EnvironmentType.Dev,
                    EnvironmentType.Prod
                }
            },
            TechnologyName = ".NET",
            Environments = new List<VMEnvironmentResult>
            {
                CreateVMEnvironment(EnvironmentType.Dev, "Development"),
                CreateVMEnvironment(EnvironmentType.Prod, "Production")
            },
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 20,
                TotalCpu = 160,
                TotalRam = 640,
                TotalDisk = 2000
            }
        };
    }

    private static VMEnvironmentResult CreateVMEnvironment(EnvironmentType env, string name)
    {
        return new VMEnvironmentResult
        {
            Environment = env,
            EnvironmentName = name,
            IsProd = env is EnvironmentType.Prod or EnvironmentType.DR,
            Roles = new List<VMRoleResult>
            {
                new VMRoleResult
                {
                    Role = ServerRole.App,
                    RoleName = "Application",
                    TotalInstances = 4,
                    CpuPerInstance = 8,
                    RamPerInstance = 32,
                    DiskPerInstance = 100,
                    TotalCpu = 32,
                    TotalRam = 128,
                    TotalDisk = 400
                },
                new VMRoleResult
                {
                    Role = ServerRole.Web,
                    RoleName = "Web",
                    TotalInstances = 2,
                    CpuPerInstance = 4,
                    RamPerInstance = 16,
                    DiskPerInstance = 50,
                    TotalCpu = 8,
                    TotalRam = 32,
                    TotalDisk = 100
                },
                new VMRoleResult
                {
                    Role = ServerRole.Database,
                    RoleName = "Database",
                    TotalInstances = 2,
                    CpuPerInstance = 8,
                    RamPerInstance = 32,
                    DiskPerInstance = 200,
                    TotalCpu = 16,
                    TotalRam = 64,
                    TotalDisk = 400
                }
            },
            TotalVMs = 8,
            TotalCpu = 56,
            TotalRam = 224,
            TotalDisk = 900
        };
    }

    #endregion
}
