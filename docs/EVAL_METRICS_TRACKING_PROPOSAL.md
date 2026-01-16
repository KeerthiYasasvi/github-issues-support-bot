# Bot Evaluation Metrics Tracking Proposal

## Problem Statement

Currently, bot evaluation metrics (field extraction, completeness, category detection, actionability, hallucination) are:
- ✅ Generated during every bot run
- ✅ Visible in GitHub Actions logs
- ✅ Visible in bot comments to users
- ❌ **NOT** stored in any persistent file
- ❌ **NOT** aggregated for trend analysis
- ❌ **NOT** easily accessible for repo owners to evaluate bot performance

## Proposed Solution

Create an **automated evaluation metrics tracking system** that stores aggregate performance data and monthly breakdowns in the repository.

---

## Architecture

### 1. **Metrics Collection File**

Create a JSON file in the repo: `.supportbot/metrics/performance.json`

**Schema:**
```json
{
  "metadata": {
    "last_updated": "2026-01-16T20:45:00Z",
    "total_issues_processed": 116,
    "data_start_date": "2025-12-01T00:00:00Z"
  },
  "overall": {
    "total_issues_processed": 116,
    "success_rate": 0.857,
    "average_completeness_score": 62.4,
    "average_field_extraction_rate": 0.73,
    "average_duration_seconds": 36.2,
    "category_breakdown": {
      "airflow_dag": {
        "count": 15,
        "avg_completeness": 68.2,
        "avg_fields_extracted": 0.78
      },
      "postgres_database": {
        "count": 22,
        "avg_completeness": 71.5,
        "avg_fields_extracted": 0.81
      },
      "runtime": {
        "count": 18,
        "avg_completeness": 55.3,
        "avg_fields_extracted": 0.65
      },
      "build": {
        "count": 12,
        "avg_completeness": 60.1,
        "avg_fields_extracted": 0.70
      },
      "setup": {
        "count": 8,
        "avg_completeness": 45.2,
        "avg_fields_extracted": 0.58
      },
      "reddit_api": {
        "count": 10,
        "avg_completeness": 64.7,
        "avg_fields_extracted": 0.75
      },
      "dbt_transform": {
        "count": 14,
        "avg_completeness": 66.9,
        "avg_fields_extracted": 0.76
      },
      "docker_infra": {
        "count": 9,
        "avg_completeness": 58.4,
        "avg_fields_extracted": 0.68
      },
      "off_topic": {
        "count": 8,
        "avg_completeness": 10.5,
        "avg_fields_extracted": 0.15
      }
    },
    "user_engagement": {
      "total_follow_up_rounds": 45,
      "avg_rounds_per_issue": 0.39,
      "stop_commands_used": 3,
      "diagnose_commands_used": 7,
      "escalate_commands_used": 2,
      "users_responded_to_questions": 12,
      "users_never_responded": 33
    },
    "actionability": {
      "actionable_issues": 67,
      "non_actionable_issues": 41,
      "escalated_to_maintainer": 8,
      "resolved_without_maintainer": 59
    },
    "hallucination_detection": {
      "total_hallucinations_detected": 4,
      "avg_confidence": 0.89,
      "false_positive_rate": 0.02
    }
  },
  "monthly_breakdown": {
    "2025-12": {
      "period": "December 2025",
      "issues_processed": 32,
      "success_rate": 0.84,
      "average_completeness_score": 58.2,
      "average_field_extraction_rate": 0.68,
      "users_responded": 4,
      "escalations": 2
    },
    "2026-01": {
      "period": "January 2026",
      "issues_processed": 84,
      "success_rate": 0.87,
      "average_completeness_score": 65.1,
      "average_field_extraction_rate": 0.76,
      "users_responded": 8,
      "escalations": 6
    }
  },
  "recent_runs": [
    {
      "run_id": 116,
      "issue_number": 52,
      "timestamp": "2026-01-16T20:20:45Z",
      "category": "dbt_transform",
      "completeness": 25,
      "fields_extracted": 3,
      "total_fields": 13,
      "actionable": false,
      "follow_up_round": 1,
      "duration_seconds": 39
    },
    {
      "run_id": 115,
      "issue_number": 51,
      "timestamp": "2026-01-16T20:19:17Z",
      "category": "airflow_dag",
      "completeness": 40,
      "fields_extracted": 18,
      "total_fields": 22,
      "actionable": false,
      "follow_up_round": 1,
      "duration_seconds": 36
    }
  ]
}
```

---

## Implementation Plan

### Phase 1: Add Metrics Export to Bot Code

**File:** `src/SupportConcierge/Reporting/MetricsExporter.cs`

```csharp
public class MetricsExporter
{
    private readonly string _metricsFilePath;
    
    public MetricsExporter(string repoPath)
    {
        _metricsFilePath = Path.Combine(repoPath, ".supportbot", "metrics", "performance.json");
    }
    
    public async Task AppendRunMetrics(RunMetrics metrics)
    {
        // Load existing metrics file
        var allMetrics = await LoadMetrics();
        
        // Append new run to recent_runs
        allMetrics.RecentRuns.Insert(0, metrics);
        
        // Keep only last 100 runs
        if (allMetrics.RecentRuns.Count > 100)
            allMetrics.RecentRuns = allMetrics.RecentRuns.Take(100).ToList();
        
        // Recalculate overall aggregates
        allMetrics.Overall.Summary = CalculateOverallSummary(allMetrics.RecentRuns);
        allMetrics.Overall.CategoryBreakdown = CalculateCategoryBreakdown(allMetrics.RecentRuns);
        allMetrics.Overall.UserEngagement = CalculateUserEngagement(allMetrics.RecentRuns);
        
        // Recalculate monthly breakdown
        allMetrics.MonthlyBreakdown = CalculateMonthlyBreakdown(allMetrics.RecentRuns);
        
        // Update metadata
        allMetrics.Metadata.LastUpdated = DateTime.UtcNow;
        
        // Write back to file
        await SaveMetrics(allMetrics);
    }
    
    private Dictionary<string, object> CalculateMonthlyBreakdown(List<RunMetrics> runs)
    {
        // Group runs by month (YYYY-MM)
        var monthlyGroups = runs
            .GroupBy(r => r.Timestamp.ToString("yyyy-MM"))
            .OrderByDescending(g => g.Key)
            .ToList();
        
        var breakdown = new Dictionary<string, object>();
        
        foreach (var month in monthlyGroups)
        {
            var monthRuns = month.ToList();
            breakdown[month.Key] = new
            {
                period = $"{month.Key}",
                issues_processed = monthRuns.Count,
                success_rate = monthRuns.Count(r => r.IsSuccessful) / (double)monthRuns.Count,
                average_completeness_score = monthRuns.Average(r => r.Completeness),
                average_field_extraction_rate = monthRuns.Average(r => r.FieldsExtracted / (double)r.TotalFields),
                users_responded = monthRuns.Count(r => r.UserResponded),
                escalations = monthRuns.Count(r => r.IsEscalated)
            };
        }
        
        return breakdown;
    }
}
```

**Integration Point:** Add to `Orchestrator.cs` at end of processing:

```csharp
// After posting comment or brief
var metricsExporter = new MetricsExporter(repoPath);
await metricsExporter.AppendRunMetrics(new RunMetrics
{
    RunId = context.RunId,
    IssueNumber = context.IssueNumber,
    Timestamp = DateTime.UtcNow,
    Category = detectedCategory,
    Completeness = completenessScore,
    FieldsExtracted = extractedFields.Count,
    TotalFields = requiredFields.Count,
    Actionable = isActionable,
    FollowUpRound = followUpRound,
    DurationSeconds = (DateTime.UtcNow - startTime).TotalSeconds,
    IsSuccessful = !hasError,
    UserResponded = false, // Will be updated when user comments
    IsEscalated = followUpRound >= 3 && !hasAdequateResponse
});
```

### Phase 2: Add Metrics Dashboard

Create a GitHub Pages dashboard that visualizes the metrics file.

**File:** `.supportbot/metrics/index.html`

```html
<!DOCTYPE html>
<html>
<head>
    <title>Support Bot Performance Dashboard</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            margin: 0;
            padding: 20px;
            background: #f6f8fa;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
        }
        h1 {
            color: #24292e;
            border-bottom: 3px solid #0366d6;
            padding-bottom: 10px;
        }
        .metrics-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin: 30px 0;
        }
        .metric-card {
            background: white;
            border: 1px solid #e1e4e8;
            border-radius: 6px;
            padding: 20px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        .metric-card h3 {
            margin: 0 0 10px 0;
            color: #586069;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
        }
        .metric-value {
            font-size: 32px;
            font-weight: 700;
            color: #0366d6;
            margin: 10px 0;
        }
        .metric-unit {
            font-size: 14px;
            color: #6a737d;
        }
        .chart-container {
            position: relative;
            height: 400px;
            background: white;
            border: 1px solid #e1e4e8;
            border-radius: 6px;
            padding: 20px;
            margin: 20px 0;
        }
        .monthly-table {
            width: 100%;
            border-collapse: collapse;
            background: white;
            border: 1px solid #e1e4e8;
            border-radius: 6px;
            margin: 20px 0;
            overflow: hidden;
        }
        .monthly-table thead {
            background: #f6f8fa;
            border-bottom: 1px solid #e1e4e8;
        }
        .monthly-table th {
            padding: 12px;
            text-align: left;
            font-weight: 600;
            color: #24292e;
        }
        .monthly-table td {
            padding: 12px;
            border-bottom: 1px solid #e1e4e8;
        }
        .monthly-table tr:hover {
            background: #f6f8fa;
        }
        .status-good { color: #28a745; font-weight: 600; }
        .status-warning { color: #ffc107; font-weight: 600; }
        .status-bad { color: #dc3545; font-weight: 600; }
    </style>
</head>
<body>
    <div class="container">
        <h1>Support Concierge Bot - Performance Dashboard</h1>
        
        <div class="metrics-grid">
            <div class="metric-card">
                <h3>Total Issues Processed</h3>
                <div class="metric-value" id="total-issues">0</div>
            </div>
            <div class="metric-card">
                <h3>Success Rate</h3>
                <div class="metric-value" id="success-rate">0%</div>
            </div>
            <div class="metric-card">
                <h3>Avg Completeness</h3>
                <div class="metric-value" id="avg-completeness">0<span class="metric-unit">/100</span></div>
            </div>
            <div class="metric-card">
                <h3>Avg Field Extraction</h3>
                <div class="metric-value" id="avg-extraction">0%</div>
            </div>
            <div class="metric-card">
                <h3>Avg Duration</h3>
                <div class="metric-value" id="avg-duration">0<span class="metric-unit">s</span></div>
            </div>
            <div class="metric-card">
                <h3>User Response Rate</h3>
                <div class="metric-value" id="response-rate">0%</div>
            </div>
        </div>
        
        <h2>Category Performance</h2>
        <div class="chart-container">
            <canvas id="category-chart"></canvas>
        </div>
        
        <h2>Completeness Trend (Last 30 Days)</h2>
        <div class="chart-container">
            <canvas id="trend-chart"></canvas>
        </div>
        
        <h2>Monthly Breakdown</h2>
        <table class="monthly-table">
            <thead>
                <tr>
                    <th>Month</th>
                    <th>Issues</th>
                    <th>Success Rate</th>
                    <th>Avg Completeness</th>
                    <th>User Responses</th>
                    <th>Escalations</th>
                </tr>
            </thead>
            <tbody id="monthly-tbody">
            </tbody>
        </table>
    </div>
    
    <script>
        fetch('performance.json')
            .then(r => r.json())
            .then(data => renderDashboard(data));
        
        function renderDashboard(data) {
            // Update summary cards
            document.getElementById('total-issues').textContent = data.overall.total_issues_processed;
            document.getElementById('success-rate').textContent = (data.overall.success_rate * 100).toFixed(1) + '%';
            document.getElementById('avg-completeness').innerHTML = data.overall.average_completeness_score.toFixed(1) + '<span class="metric-unit">/100</span>';
            document.getElementById('avg-extraction').textContent = (data.overall.average_field_extraction_rate * 100).toFixed(1) + '%';
            document.getElementById('avg-duration').innerHTML = data.overall.average_duration_seconds.toFixed(1) + '<span class="metric-unit">s</span>';
            
            const responseRate = data.overall.user_engagement.users_responded_to_questions / 
                                (data.overall.user_engagement.users_responded_to_questions + data.overall.user_engagement.users_never_responded);
            document.getElementById('response-rate').textContent = (responseRate * 100).toFixed(1) + '%';
            
            // Category chart
            const categories = Object.keys(data.overall.category_breakdown);
            const completenessValues = categories.map(c => data.overall.category_breakdown[c].avg_completeness);
            
            new Chart(document.getElementById('category-chart'), {
                type: 'bar',
                data: {
                    labels: categories,
                    datasets: [{
                        label: 'Avg Completeness Score',
                        data: completenessValues,
                        backgroundColor: '#0366d6'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false
                }
            });
            
            // Monthly table
            const monthlyTbody = document.getElementById('monthly-tbody');
            Object.entries(data.monthly_breakdown).forEach(([month, stats]) => {
                const row = `
                    <tr>
                        <td>${stats.period}</td>
                        <td>${stats.issues_processed}</td>
                        <td class="${stats.success_rate > 0.8 ? 'status-good' : 'status-warning'}">${(stats.success_rate * 100).toFixed(1)}%</td>
                        <td>${stats.average_completeness_score.toFixed(1)}</td>
                        <td>${stats.users_responded}</td>
                        <td>${stats.escalations}</td>
                    </tr>
                `;
                monthlyTbody.innerHTML += row;
            });
        }
    </script>
</body>
</html>
```

### Phase 3: Enable GitHub Pages

Add to `.github/workflows/pages.yml`:

```yaml
name: Deploy Dashboard

on:
  push:
    branches: [main]
    paths:
      - '.supportbot/metrics/performance.json'
      - '.supportbot/metrics/index.html'
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - name: Checkout
        uses: actions/checkout@v4
      
      - name: Upload artifact
        uses: actions/upload-pages-artifact@v2
        with:
          path: '.supportbot/metrics'
      
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v2
```

---

## Benefits

### For Repo Owners

1. **Performance Overview:** Single dashboard showing bot effectiveness
2. **Monthly Trends:** See if bot improves or degrades month-to-month
3. **Category Insights:** Identify which issue types bot handles best/worst
4. **User Behavior:** Understand response rates and engagement
5. **Data-Driven Decisions:** Make configuration changes based on real data

### For Bot Developers

1. **Regression Detection:** Catch performance drops after code changes
2. **Category Tuning:** Identify which categories need better prompts
3. **A/B Testing:** Compare different extraction strategies using monthly data
4. **Audit Trail:** Track all issues processed (last 100 runs in memory)

---

## Privacy Considerations

**What to Store:**
- ✅ Aggregate metrics (counts, averages, percentages)
- ✅ Issue numbers (already public)
- ✅ Categories detected
- ✅ Completeness scores
- ✅ Field extraction rates
- ✅ Command usage (stop, diagnose, escalate)

**What NOT to Store:**
- ❌ Issue content (may contain sensitive data)
- ❌ User email addresses
- ❌ API keys or credentials
- ❌ Personal identifiable information (PII)
- ❌ Error messages with stack traces

---

## Rollout Plan

### Week 1: Foundation
- [ ] Create `MetricsExporter.cs` class
- [ ] Add JSON schema for `performance.json`
- [ ] Integrate into `Orchestrator.cs`
- [ ] Test with 10 issues

### Week 2: Dashboard
- [ ] Create HTML dashboard at `.supportbot/metrics/index.html`
- [ ] Add Chart.js visualizations
- [ ] Test dashboard with sample data

### Week 3: GitHub Pages
- [ ] Create GitHub Pages deployment workflow
- [ ] Enable Pages for repository
- [ ] Deploy dashboard (accessible at `https://username.github.io/repo/metrics/`)

### Week 4: Validation
- [ ] Monitor first week of automatic collection
- [ ] Verify calculations are accurate
- [ ] Check that monthly breakdown updates correctly

---

## Example Usage

### Viewing Current Metrics

```bash
# View raw overall metrics
cat .supportbot/metrics/performance.json | jq '.overall.summary'

# View monthly breakdown
cat .supportbot/metrics/performance.json | jq '.monthly_breakdown'

# View recent runs (last 10)
cat .supportbot/metrics/performance.json | jq '.recent_runs[:10]'
```

### Dashboard Access

Visit: `https://keerthiyasasvi.github.io/Reddit-ELT-Pipeline/metrics/`

**On dashboard you'll see:**
- Overall success rate, completeness score, extraction rate
- Category performance chart (which issue types bot handles best)
- Monthly breakdown table showing trends
- User engagement metrics

### API Access (for external monitoring)

```bash
# Get overall performance
curl https://raw.githubusercontent.com/KeerthiYasasvi/Reddit-ELT-Pipeline/main/.supportbot/metrics/performance.json \
  | jq '.overall'

# Get last month's performance
curl https://raw.githubusercontent.com/KeerthiYasasvi/Reddit-ELT-Pipeline/main/.supportbot/metrics/performance.json \
  | jq '.monthly_breakdown["2026-01"]'
```

---

## Success Metrics

After implementing this tracking system, we should see:

1. **Dashboard adoption:** Repo owner checks it weekly
2. **Data-driven improvements:** At least 2 bot configuration changes based on metrics
3. **Performance trend:** Success rate improves or stays above 85%
4. **Transparency:** Stakeholders can reference metrics in discussions

---

## Conclusion

This approach provides:

✅ **Persistent storage** - `.supportbot/metrics/performance.json` is committed to repo  
✅ **Overall aggregates** - Completeness, extraction, success rate across all issues  
✅ **Monthly breakdown** - Track performance trends month-by-month  
✅ **Visual dashboard** - GitHub Pages with charts and tables  
✅ **API access** - Programmatic access via raw GitHub URLs  
✅ **Simple implementation** - No complex infrastructure needed  

**Recommended:** Implement this full approach for complete visibility into bot performance over time.
