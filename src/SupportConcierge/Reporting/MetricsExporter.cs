using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SupportConcierge.Reporting;

public class MetricsExporter
{
    private readonly string _metricsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public MetricsExporter(string repoPath)
    {
        _metricsFilePath = Path.Combine(repoPath, ".supportbot", "metrics", "performance.json");
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public async Task AppendRunMetricsAsync(RunMetrics metrics)
    {
        try
        {
            // Ensure directory exists
            var metricsDir = Path.GetDirectoryName(_metricsFilePath);
            if (!Directory.Exists(metricsDir))
            {
                Directory.CreateDirectory(metricsDir!);
            }

            // Load existing metrics or create new
            var allMetrics = await LoadMetricsAsync();

            // Add new run
            allMetrics.RecentRuns.Insert(0, metrics);

            // Keep only last 100 runs
            if (allMetrics.RecentRuns.Count > 100)
            {
                allMetrics.RecentRuns = allMetrics.RecentRuns.Take(100).ToList();
            }

            // Recalculate aggregates
            allMetrics.Overall.Summary = CalculateOverallSummary(allMetrics.RecentRuns);
            allMetrics.Overall.CategoryBreakdown = CalculateCategoryBreakdown(allMetrics.RecentRuns);
            allMetrics.Overall.UserEngagement = CalculateUserEngagement(allMetrics.RecentRuns);
            allMetrics.Overall.Actionability = CalculateActionability(allMetrics.RecentRuns);
            allMetrics.Overall.HallucinationDetection = CalculateHallucination(allMetrics.RecentRuns);

            // Recalculate monthly breakdown
            allMetrics.MonthlyBreakdown = CalculateMonthlyBreakdown(allMetrics.RecentRuns);

            // Update metadata
            allMetrics.Metadata["last_updated"] = DateTime.UtcNow.ToString("O");
            allMetrics.Metadata["total_issues_processed"] = allMetrics.RecentRuns.Count;

            // Save to file
            await SaveMetricsAsync(allMetrics);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to export metrics: {ex.Message}");
        }
    }

    private Dictionary<string, object> CalculateOverallSummary(List<RunMetrics> runs)
    {
        if (runs.Count == 0)
        {
            return new Dictionary<string, object>();
        }

        return new Dictionary<string, object>
        {
            { "total_issues_processed", runs.Count },
            { "success_rate", runs.Count(r => r.IsSuccessful) / (double)runs.Count },
            { "average_completeness_score", runs.Average(r => r.Completeness) },
            { "average_field_extraction_rate", runs.Average(r => r.FieldsExtracted / (double)r.TotalFields) },
            { "average_duration_seconds", runs.Average(r => r.DurationSeconds) }
        };
    }

    private Dictionary<string, object> CalculateCategoryBreakdown(List<RunMetrics> runs)
    {
        var breakdown = new Dictionary<string, object>();

        var categories = runs.GroupBy(r => r.Category).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var category in categories)
        {
            var runsInCategory = category.Value;
            breakdown[category.Key] = new Dictionary<string, object>
            {
                { "count", runsInCategory.Count },
                { "avg_completeness", runsInCategory.Average(r => r.Completeness) },
                { "avg_fields_extracted", runsInCategory.Average(r => r.FieldsExtracted / (double)r.TotalFields) }
            };
        }

        return breakdown;
    }

    private Dictionary<string, object> CalculateUserEngagement(List<RunMetrics> runs)
    {
        return new Dictionary<string, object>
        {
            { "total_follow_up_rounds", runs.Sum(r => r.FollowUpRound) },
            { "avg_rounds_per_issue", runs.Count > 0 ? runs.Sum(r => r.FollowUpRound) / (double)runs.Count : 0 },
            { "stop_commands_used", runs.Count(r => r.StopCommandUsed) },
            { "diagnose_commands_used", runs.Count(r => r.DiagnoseCommandUsed) },
            { "escalate_commands_used", runs.Count(r => r.EscalateCommandUsed) },
            { "users_responded_to_questions", runs.Count(r => r.UserResponded) },
            { "users_never_responded", runs.Count(r => !r.UserResponded && r.FollowUpRound > 0) }
        };
    }

    private Dictionary<string, object> CalculateActionability(List<RunMetrics> runs)
    {
        return new Dictionary<string, object>
        {
            { "actionable_issues", runs.Count(r => r.Actionable) },
            { "non_actionable_issues", runs.Count(r => !r.Actionable) },
            { "escalated_to_maintainer", runs.Count(r => r.IsEscalated) },
            { "resolved_without_maintainer", runs.Count(r => r.Actionable && !r.IsEscalated) }
        };
    }

    private Dictionary<string, object> CalculateHallucination(List<RunMetrics> runs)
    {
        var hallucinationDetections = runs.Where(r => r.HallucinationCount > 0).ToList();

        return new Dictionary<string, object>
        {
            { "total_hallucinations_detected", runs.Sum(r => r.HallucinationCount) },
            { "avg_confidence", hallucinationDetections.Count > 0 ? hallucinationDetections.Average(r => r.HallucinationConfidence) : 0 },
            { "false_positive_rate", runs.Count > 0 ? hallucinationDetections.Count(r => !r.IsHallucinationConfirmed) / (double)runs.Count : 0 }
        };
    }

    private Dictionary<string, object> CalculateMonthlyBreakdown(List<RunMetrics> runs)
    {
        var breakdown = new Dictionary<string, object>();

        var monthlyGroups = runs
            .GroupBy(r => r.Timestamp.ToString("yyyy-MM"))
            .OrderByDescending(g => g.Key)
            .ToList();

        foreach (var month in monthlyGroups)
        {
            var monthRuns = month.ToList();
            breakdown[month.Key] = new Dictionary<string, object>
            {
                { "period", month.Key },
                { "issues_processed", monthRuns.Count },
                { "success_rate", monthRuns.Count(r => r.IsSuccessful) / (double)monthRuns.Count },
                { "average_completeness_score", monthRuns.Average(r => r.Completeness) },
                { "average_field_extraction_rate", monthRuns.Average(r => r.FieldsExtracted / (double)r.TotalFields) },
                { "users_responded", monthRuns.Count(r => r.UserResponded) },
                { "escalations", monthRuns.Count(r => r.IsEscalated) }
            };
        }

        return breakdown;
    }

    private async Task<PerformanceMetrics> LoadMetricsAsync()
    {
        if (!File.Exists(_metricsFilePath))
        {
            return CreateEmptyMetrics();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_metricsFilePath);
            return JsonSerializer.Deserialize<PerformanceMetrics>(json) ?? CreateEmptyMetrics();
        }
        catch
        {
            return CreateEmptyMetrics();
        }
    }

    private PerformanceMetrics CreateEmptyMetrics()
    {
        return new PerformanceMetrics
        {
            Metadata = new Dictionary<string, object>
            {
                { "last_updated", DateTime.UtcNow.ToString("O") },
                { "total_issues_processed", 0 },
                { "data_start_date", DateTime.UtcNow.ToString("O") }
            },
            Overall = new OverallMetrics
            {
                Summary = new Dictionary<string, object>(),
                CategoryBreakdown = new Dictionary<string, object>(),
                UserEngagement = new Dictionary<string, object>(),
                Actionability = new Dictionary<string, object>(),
                HallucinationDetection = new Dictionary<string, object>()
            },
            MonthlyBreakdown = new Dictionary<string, object>(),
            RecentRuns = new List<RunMetrics>()
        };
    }

    private async Task SaveMetricsAsync(PerformanceMetrics metrics)
    {
        var json = JsonSerializer.Serialize(metrics, _jsonOptions);
        await File.WriteAllTextAsync(_metricsFilePath, json);
    }
}

public class PerformanceMetrics
{
    public Dictionary<string, object> Metadata { get; set; } = new();
    public OverallMetrics Overall { get; set; } = new();
    public Dictionary<string, object> MonthlyBreakdown { get; set; } = new();
    public List<RunMetrics> RecentRuns { get; set; } = new();
}

public class OverallMetrics
{
    public Dictionary<string, object> Summary { get; set; } = new();
    public Dictionary<string, object> CategoryBreakdown { get; set; } = new();
    public Dictionary<string, object> UserEngagement { get; set; } = new();
    public Dictionary<string, object> Actionability { get; set; } = new();
    public Dictionary<string, object> HallucinationDetection { get; set; } = new();
}

public class RunMetrics
{
    public int RunId { get; set; }
    public int IssueNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string Category { get; set; } = string.Empty;
    public double Completeness { get; set; }
    public int FieldsExtracted { get; set; }
    public int TotalFields { get; set; }
    public bool Actionable { get; set; }
    public int FollowUpRound { get; set; }
    public double DurationSeconds { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public bool UserResponded { get; set; }
    public bool IsEscalated { get; set; }
    public bool StopCommandUsed { get; set; }
    public bool DiagnoseCommandUsed { get; set; }
    public bool EscalateCommandUsed { get; set; }
    public int HallucinationCount { get; set; }
    public double HallucinationConfidence { get; set; }
    public bool IsHallucinationConfirmed { get; set; }
}
