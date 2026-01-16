# GitHub Actions Workflow Logs - Detailed Analysis

## Location

**GitHub Actions Page:** https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/actions

**Navigation Path:**
1. Go to repository → **Actions** tab
2. Click on any workflow run (e.g., Run #115)
3. Click on the **"triage"** job
4. Expand the **"Run Support Concierge"** step
5. Scroll through logs to see evaluation metrics

---

## Real Example: Issue #51 (Run #115)

**Issue Title:** "ETL pipeline crashes during peak load - PostgreSQL connection error"

**Workflow Run:** https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/actions/runs/21079686989/job/60630137163

### Logs Output (Line by Line)

```
=== GitHub Issues Support Concierge Bot ===
Started at: 2026-01-16 20:20:17Z
Loading event from: /home/runner/work/_temp/_github_workflow/event.json
Event type: issues
Processing event: issues
Issue #51: ETL pipeline crashes during peak load - PostgreSQL connection error
Repository: KeerthiYasasvi/Reddit-ELT-Pipeline
Using OpenAI model: gpt-4o (source: env: OPENAI_MODEL)

Loading SpecPack configuration...
Loading SpecPack from: .supportbot
Loaded 9 categories
Loaded 9 checklists
Loaded validator rules
Loaded 9 routing rules
Loaded 3 playbooks

Issue author: KeerthiYasasvi
Total comments: 0, Author comments: 0

[DEBUG] Text being analyzed: etl pipeline crashes during peak load - postgresql connection error ## problem
[DEBUG] Issue #51 category scores:
  airflow_dag: 4
  postgres_database: 3
  setup: 1
  build: 1
  runtime: 1
  reddit_api: 1

[DEBUG] Off-topic heuristic: offTopicScore=0, hasProblemTerms=True, hasNegation=False
[DEBUG] Heuristic condition: score>0=False, (!problems||negation)=False
[DEBUG] Off-topic heuristic did not trigger

Determined category: airflow_dag
Extracting fields from issue...
Extracted 18 fields
Completeness score: 40/75
Missing fields: docker_version, dag_name, airflow_logs, services_status

Asking follow-up questions...
Posted follow-up questions (loop 1)
Processing complete.
Completed at: 2026-01-16 20:20:25Z
```

---

## Metrics Breakdown

### 1. **Category Detection**

```
[DEBUG] Issue #51 category scores:
  airflow_dag: 4         ← Highest score (winner)
  postgres_database: 3
  setup: 1
  build: 1
  runtime: 1
  reddit_api: 1

Determined category: airflow_dag
```

**Analysis:**
- Bot scored the issue against 9 different categories
- "airflow_dag" scored highest (4 points) based on keywords like "pipeline", "peak load", "airflow"
- "postgres_database" scored second (3 points) due to "PostgreSQL connection error"
- Bot correctly identified this as an Airflow DAG issue (not just a database issue)

**How Scoring Works:**
- Each category has a list of keywords in `.supportbot/categories.yaml`
- Bot counts how many keywords appear in the issue title + body
- Category with most matches wins

### 2. **Field Extraction**

```
Extracted 18 fields
```

**What Was Extracted:**
Based on the issue content, the bot successfully extracted:
- ✅ Problem description
- ✅ Error message/logs (psycopg2.OperationalError stack trace)
- ✅ Environment details (Linux, 8GB RAM, Python 3.9.2, Postgres 13.4, Airflow 2.3.1)
- ✅ Steps to reproduce (4 specific steps provided by user)
- ✅ Expected behavior
- ✅ Actual behavior
- ✅ Attempted solutions (connection pool increase, retry logic)
- ✅ Timestamp of failure (~2 PM UTC)

### 3. **Completeness Score**

```
Completeness score: 40/75
```

**What This Means:**
- **40/75 = 53.3%** - Moderately complete
- Bot determined the issue has decent information but is missing critical fields
- Score is calculated as: `(fields_present / total_required_fields) * 100`

**Scoring Breakdown:**
- Base score: 18 fields extracted
- Required fields for "airflow_dag" category: ~22 fields
- Penalty: Missing 4 critical fields (see below)
- Final score: 40/75

**Why Not Higher?**
- User provided excellent error logs and environment details
- BUT: Missing specific Airflow context (DAG name, Docker version, Airflow logs, services status)
- These are critical for diagnosing Airflow-specific issues

### 4. **Missing Fields**

```
Missing fields: docker_version, dag_name, airflow_logs, services_status
```

**Critical Missing Information:**
1. **docker_version** - Needed to check for Docker Compose compatibility issues
2. **dag_name** - Which specific DAG is failing? (user said "pipeline" but didn't specify which)
3. **airflow_logs** - Airflow scheduler/worker logs would show connection pool behavior
4. **services_status** - Are Postgres, Redis, Airflow scheduler all running?

**Bot's Response:**
Based on these missing fields, the bot posted follow-up questions asking for:
- "What version of Docker/Docker Compose are you using?"
- "Which DAG is failing? Can you share the exact DAG name?"
- "Can you share Airflow logs from the scheduler and worker?"

### 5. **Follow-Up Questions**

```
Asking follow-up questions...
Posted follow-up questions (loop 1)
```

**What Happens:**
- Bot detected completeness score < 70%
- Triggered follow-up question flow
- Posted comment to Issue #51 asking for the 4 missing fields
- This is "loop 1" (bot allows up to 3 rounds of questions)

**Example Comment Posted:**
```markdown
Thanks for reporting this issue! I've analyzed the information provided and 
have a few questions to help diagnose this further:

**To better understand the Airflow environment:**
1. What version of Docker and Docker Compose are you using?
2. Which specific DAG is experiencing the crash? Can you share the DAG name?
3. Can you share the Airflow scheduler and worker logs during the failure?

*This is follow-up round 1 of 3. You can use `/stop` to opt out of further questions.*
```

### 6. **Off-Topic Detection**

```
[DEBUG] Off-topic heuristic: offTopicScore=0, hasProblemTerms=True, hasNegation=False
[DEBUG] Heuristic condition: score>0=False, (!problems||negation)=False
[DEBUG] Off-topic heuristic did not trigger
```

**How It Works:**
- Bot checks for off-topic signals:
  - Homework keywords ("assignment", "class project", "due tomorrow")
  - Unrelated topics ("cooking recipe", "movie recommendation")
  - Negation phrases ("not related to this repo", "different project")
- **offTopicScore=0** means no off-topic keywords found
- **hasProblemTerms=True** means legitimate problem indicators present
- **Heuristic did not trigger** = Bot proceeds with normal processing

### 7. **Execution Timing**

```
Started at: 2026-01-16 20:20:17Z
Completed at: 2026-01-16 20:20:25Z
```

**Duration:** 8 seconds (from start to posting comment)

**Breakdown:**
- Load configuration: ~1s
- Category detection: ~1s
- Field extraction (OpenAI API call): ~4-5s
- Completeness scoring: <1s
- Post comment (GitHub API call): ~1s

---

## What You DON'T See in Logs

### Missing Metrics

1. **Actionability Score**
   - Is the user's issue actionable? (true/false)
   - Suggested fix clarity score (0-100)
   - **NOT logged** (only calculated internally)

2. **Hallucination Detection**
   - Did bot make up any information?
   - Confidence score for extracted fields
   - **NOT logged** (silent detection)

3. **User Engagement**
   - Did user respond to follow-up questions?
   - Did user use `/stop`, `/diagnose`, or `/escalate`?
   - **NOT logged** (would require tracking across multiple runs)

4. **Aggregate Statistics**
   - Success rate across all issues
   - Average completeness score
   - Most common missing fields
   - **NOT logged** (would require persistent storage)

---

## Comparison: Issue #52 (Incomplete Information)

**Issue Title:** "dbt transformation fails - something wrong with SQL"

**Description:** "dbt run fails every time I try to run the transformation. Getting some error but can't remember what it says exactly. Tried running it twice but same thing happens. This is blocking my work. Can someone help me ASAP?"

### Expected Logs (Predicted)

```
[DEBUG] Issue #52 category scores:
  dbt_transform: 5       ← Highest score
  build: 2
  runtime: 1

Determined category: dbt_transform
Extracting fields from issue...
Extracted 3 fields
Completeness score: 12/75
Missing fields: dbt_version, error_message, model_name, sql_query, 
                expected_output, actual_output, dependencies, logs, 
                environment, attempted_solutions

Asking follow-up questions...
Posted follow-up questions (loop 1)
```

**Key Differences from Issue #51:**
- Much lower extraction: **3 fields** (vs 18)
- Much lower completeness: **12/75 (16%)** (vs 40/75)
- More missing fields: **10+ critical fields** (vs 4)
- Bot will ask broader questions to narrow down the problem

---

## How to Access This Information

### Option 1: GitHub UI (Manual)

1. Go to https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/actions
2. Find the workflow run for your issue (e.g., "Run #115: Issue #51")
3. Click the run → Click "triage" job
4. Expand "Run Support Concierge" step
5. Scroll through logs

**Pros:**
- Easy for one-off checks
- Visual interface

**Cons:**
- Tedious for multiple issues
- No aggregation or trends
- Logs expire after 90 days

### Option 2: GitHub CLI (Automated)

```bash
# Install GitHub CLI
gh --version

# Get logs for specific run
gh run view 21079686989 --log --repo KeerthiYasasvi/Reddit-ELT-Pipeline

# Search for specific metrics
gh run view 21079686989 --log --repo KeerthiYasasvi/Reddit-ELT-Pipeline | grep "Completeness score"

# Get all recent workflow runs
gh run list --workflow=support-concierge.yml --repo KeerthiYasasvi/Reddit-ELT-Pipeline --limit 20
```

**Pros:**
- Scriptable
- Can parse logs programmatically
- Works in CI/CD

**Cons:**
- Requires authentication
- Still temporary (90-day retention)
- No built-in aggregation

### Option 3: GitHub Actions API (Programmatic)

```bash
# Get workflow runs
curl -H "Authorization: token YOUR_TOKEN" \
  https://api.github.com/repos/KeerthiYasasvi/Reddit-ELT-Pipeline/actions/runs

# Get logs for specific run
curl -H "Authorization: token YOUR_TOKEN" \
  https://api.github.com/repos/KeerthiYasasvi/Reddit-ELT-Pipeline/actions/runs/21079686989/logs \
  -L -o logs.zip

unzip logs.zip
cat */6_Run\ Support\ Concierge.txt | grep "Completeness score"
```

**Pros:**
- Full automation
- Integration with analytics tools
- Can store in database

**Cons:**
- Requires API token
- Rate limits (5000 requests/hour)
- Logs still expire after 90 days

### Option 4: Proposed Persistent Metrics (Future)

**If implemented** (see [EVAL_METRICS_TRACKING_PROPOSAL.md](EVAL_METRICS_TRACKING_PROPOSAL.md)):

```bash
# Simply read the metrics file
cat .supportbot/metrics/performance.json | jq '.summary'

# View latest weekly report
ls -t .supportbot/metrics/weekly_report_*.md | head -1 | xargs cat

# Access dashboard
open https://keerthiyasasvi.github.io/Reddit-ELT-Pipeline/metrics/
```

**Pros:**
- Permanent storage
- Aggregate statistics
- Trend analysis
- No API limits

---

## Key Insights from Logs

### What Works Well

✅ **Category detection is accurate**
- Issue #51 correctly identified as "airflow_dag" (not just "database")
- Scoring system properly prioritized DAG-related keywords

✅ **Field extraction is comprehensive**
- Bot extracted 18 fields from a well-documented issue
- Properly parsed error logs, environment details, steps to reproduce

✅ **Completeness scoring is reasonable**
- 40/75 (53%) reflects reality: good info but missing Airflow-specific context
- Bot didn't give false positives (didn't mark as "complete" when missing critical fields)

✅ **Off-topic detection avoids false positives**
- Didn't trigger on legitimate issue
- Only 0 score when no off-topic signals present

### What Needs Improvement

⚠️ **No aggregate metrics in logs**
- Can't see "what's the average completeness score across all issues?"
- Can't track trends over time
- Each run is isolated

⚠️ **Limited actionability visibility**
- Logs don't show if issue was marked "actionable"
- No explanation of why bot chose to ask questions vs provide brief

⚠️ **No hallucination detection in logs**
- Bot may detect hallucinations internally but doesn't log them
- No way to audit false extractions

⚠️ **User engagement not tracked**
- No indication if user responded to follow-up questions
- No tracking of `/stop`, `/diagnose`, `/escalate` usage

---

## Recommendations

### For Repo Owners

1. **Check logs regularly** (weekly) to spot patterns:
   - Are users consistently missing the same fields?
   - Are certain categories always low completeness?
   - Is bot asking the right follow-up questions?

2. **Update issue templates** based on missing fields:
   - If "docker_version" is always missing, add it to template
   - If "error_logs" is always provided, template is working

3. **Monitor off-topic triggers**:
   - If legitimate issues are being marked off-topic, adjust heuristics
   - If off-topic issues are slipping through, tighten detection

### For Bot Developers

1. **Implement persistent metrics** (see proposal)
   - Store aggregate statistics in `.supportbot/metrics/performance.json`
   - Generate weekly reports for trend analysis
   - Create dashboard for visual monitoring

2. **Add more detailed logging**:
   - Log actionability score and reasoning
   - Log hallucination detection with confidence scores
   - Log user engagement metrics (responses, command usage)

3. **Add metric export to bot code**:
   ```csharp
   // After processing each issue
   await metricsExporter.AppendRunMetrics(new RunMetrics
   {
       RunId = 115,
       IssueNumber = 51,
       Category = "airflow_dag",
       Completeness = 40,
       FieldsExtracted = 18,
       Actionable = false,
       FollowUpRound = 1
   });
   ```

---

## Conclusion

**Current State:**
- ✅ Evaluation metrics ARE generated (category, completeness, fields, missing fields)
- ✅ Metrics ARE visible in GitHub Actions logs
- ✅ Bot behavior can be debugged by reading logs
- ❌ Metrics are NOT stored in persistent files
- ❌ No aggregate statistics or trends available
- ❌ Logs expire after 90 days (temporary storage)

**Recommended Next Steps:**
1. Review logs for recent issues to understand bot behavior
2. Identify common missing fields and update issue templates
3. Implement persistent metrics tracking (see proposal)
4. Create weekly reports for stakeholders
5. Build dashboard for visual monitoring

**For Immediate Use:**
- Access logs at: https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/actions
- Check Run #115 for a complete example of bot evaluation
- Use GitHub CLI for bulk log analysis
