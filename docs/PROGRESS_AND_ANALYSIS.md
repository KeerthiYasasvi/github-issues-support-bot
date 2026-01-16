This content has been consolidated into existing documentation. Please refer to:

- `docs/reference/INDEX.md` (Recent Updates)
- `docs/technical/OPENAI_INTEGRATION_REFERENCE.md` (Model configuration, schema enforcement)
- `docs/technical/ARCHITECTURE.md` (Evals & Retrospective Metrics)
  OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
  OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4' }}  # ← Set but ignored
  SUPPORTBOT_SPEC_DIR: ${{ vars.SUPPORTBOT_SPEC_DIR || '.supportbot' }}
```

**OpenAiClient.cs (Line 25):**
```csharp
public OpenAiClient()
{
    _apiKey = (Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "").Trim();
    
    // Use gpt-4o model which supports response_format
    _model = "gpt-4o-2024-08-06";  // ← Hardcoded, ignores OPENAI_MODEL env var
    
    _httpClient = new HttpClient();
    // ...
}
```

### Why This Matters

1. **Inconsistency:** If someone changes `OPENAI_MODEL` variable in GitHub Actions, it has no effect
2. **Inflexible:** Can't switch models without code changes
3. **Confusing:** Developers don't realize the workflow setting is ignored
4. **Risk:** If gpt-4o-2024-08-06 is deprecated, code needs manual update

### ✅ SOLUTION: Centralize Model Configuration

**Recommended Fix:**

**1. Update OpenAiClient.cs Constructor:**
```csharp
public OpenAiClient()
{
    _apiKey = (Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "").Trim();
    
    if (string.IsNullOrEmpty(_apiKey))
        throw new InvalidOperationException("OPENAI_API_KEY not set or empty");
    
    // Read model from environment, default to gpt-4o-2024-08-06
    _model = (Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-2024-08-06").Trim();
    
    _httpClient = new HttpClient();
    _httpClient.DefaultRequestHeaders.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

    Console.WriteLine($"Using OpenAI model: {_model}");
    Console.WriteLine($"  Source: {(Environment.GetEnvironmentVariable("OPENAI_MODEL") != null ? "OPENAI_MODEL env var" : "default hardcoded value")}");
}
```

**2. Update support-concierge.yml (Line 37):**
```yaml
env:
  GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
  OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4o-2024-08-06' }}  # Updated default
  SUPPORTBOT_SPEC_DIR: ${{ vars.SUPPORTBOT_SPEC_DIR || '.supportbot' }}
```

**3. Update Program.cs to pass model:**
```csharp
// Set environment variable for OpenAiClient to read
if (!string.IsNullOrEmpty(model))
{
    Environment.SetEnvironmentVariable("OPENAI_MODEL", model);
}
```

### Benefits

| Aspect | Before | After |
|--------|--------|-------|
| **Consistency** | 2 sources (1 used) | 1 source (env var) |
| **Flexibility** | Hardcoded in code | Configurable per environment |
| **Testability** | Can't change in tests | Can override for testing |
| **Clarity** | Confusing | Clear where model comes from |
| **Maintainability** | Must edit code | Change env var / GitHub variable |

---

## Part 3: Question 2 - Evals for Online Repositories

### 📊 Current Evals System

**Architecture:**
```
evals/
├── EvalRunner/
│   └── Program.cs (449 lines)
│       ├── Hardcoded: Local scenario files
│       ├── Dry-run mode: Pre-generated results
│       ├── Live mode: Uses OPENAI_API_KEY
│       └── Output: eval_report.json (local file)
└── scenarios/
    ├── sample_issue_build_missing_logs.json
    └── sample_issue_runtime_crash.json
```

**Current Limitations:**
1. ❌ **Offline Only:** Scenarios embedded in repository
2. ❌ **Not Distributed:** No way for submodule consumers to generate their own evals
3. ❌ **Repo-Agnostic:** Can't test against actual GitHub repository
4. ❌ **Manual:** New scenarios require code changes
5. ❌ **No Repository Context:** Doesn't use actual repo docs, playbookss, structure

### ✅ SOLUTION: Distributed Evals System for Submodules

**Architecture Overview:**
```
github-issues-support-bot/
├── evals/
│   ├── EvalRunner/
│   │   └── Program.cs (enhanced)
│   ├── scenarios/          # Local test scenarios
│   ├── templates/          # NEW: Templates for consumers
│   │   ├── evals.config.json
│   │   └── local-eval.ps1
│   └── docs/
│       └── EVALS_SETUP_GUIDE.md
│
Reddit-ELT-Pipeline/
├── .github-bot/ (submodule)
└── .github-bot-evals/      # NEW: Repo-specific evals
    ├── evals.config.json
    ├── scenarios/          # Repo-specific test issues
    ├── expected-results/   # Expected outputs
    └── eval_report.json    # Results
```

**Implementation Steps:**

#### Step 1: Create Evals Configuration Template
**evals/templates/evals.config.json**
```json
{
  "enabled": true,
  "mode": "online",
  "repository": {
    "owner": "KeerthiYasasvi",
    "name": "Reddit-ELT-Pipeline",
    "docs_path": ".supportbot",
    "playbook_path": ".supportbot/playbooks"
  },
  "test_scenarios": [
    {
      "name": "build-failure",
      "issue_title": "Build failed after upgrade",
      "issue_body": "...",
      "expected_category": "bug",
      "expected_fields": ["error_message", "build_log"]
    }
  ],
  "openai_config": {
    "model": "gpt-4o-2024-08-06",
    "timeout_seconds": 30,
    "max_retries": 3
  },
  "output": {
    "report_file": "eval_report.json",
    "verbose": false
  }
}
```

#### Step 2: Create Distributed Eval Runner
**Enhanced Program.cs:**
```csharp
public class EvalRunner
{
    private readonly string _configPath;
    private readonly EvalConfig _config;
    
    public async Task<int> RunAsync()
    {
        // 1. Load config from repo-specific location
        _config = LoadConfig(_configPath);
        
        // 2. If online mode:
        if (_config.Mode == "online")
        {
            // - Load repo docs
            // - Load repo playbooks
            // - Create test scenarios in actual GitHub repo
            // - Run bot against real repo
            // - Compare results to expected
            // - Cleanup test issues
            
            var results = await RunOnlineEvals(_config);
        }
        
        // 3. If offline mode:
        else
        {
            var results = RunOfflineEvals(_config);
        }
        
        // 4. Generate report
        SaveReport(results);
    }
}
```

#### Step 3: Setup Guide for Consumers
**evals/docs/EVALS_SETUP_GUIDE.md**
```markdown
# Running Evals in Your Repository

## Quick Start

1. Copy evaluation template to your repo:
   ```bash
   cp -r .github-bot/evals/templates/ ./.github-bot-evals/
   ```

2. Customize evals.config.json:
   ```json
   {
     "repository": {
       "owner": "YOUR_GITHUB_USERNAME",
       "name": "YOUR_REPO_NAME"
     }
   }
   ```

3. Create test scenarios in `.github-bot-evals/scenarios/`:
   ```json
   {
     "name": "my-test-issue",
     "issue_title": "Issue title",
     "issue_body": "Issue description",
     "expected_category": "bug"
   }
   ```

4. Run evals:
   ```bash
   cd .github-bot/evals/EvalRunner
   dotnet run -- --config ../../.github-bot-evals/evals.config.json
   ```

## Online Mode (Against Real Repository)

Requires:
- GitHub token with repo write access
- OPENAI_API_KEY
- .supportbot directory in your repo

```bash
GITHUB_TOKEN=xxx OPENAI_API_KEY=yyy dotnet run -- \
  --config .github-bot-evals/evals.config.json \
  --mode online
```

Results:
- Creates test issues in your repo
- Runs bot against them
- Compares to expected results
- Cleans up test issues
- Generates eval_report.json
```

#### Step 4: GitHub Action for Automated Evals
**Optional: .github/workflows/evals.yml**
```yaml
name: Evals (Weekly)
on:
  schedule:
    - cron: '0 2 * * 0'  # Weekly at 2 AM Sunday
  workflow_dispatch:

jobs:
  evals:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          submodules: recursive
      
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Run Evals
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          cd .github-bot/evals/EvalRunner
          dotnet run -- --config ../../../.github-bot-evals/evals.config.json --mode online
      
      - name: Upload Results
        uses: actions/upload-artifact@v3
        with:
          name: evals-report
          path: .github-bot-evals/eval_report.json
```

### Benefits

| Aspect | Current | Proposed |
|--------|---------|----------|
| **Scope** | Local scenarios only | Online + repo-specific |
| **Testing** | Dry-run samples | Real issues in real repo |
| **Distribution** | N/A | Each repo runs own evals |
| **Repo Context** | None | Uses actual docs/playbooks |
| **Automation** | Manual | Scheduled workflows |
| **Customization** | Must edit code | Config file |
| **Results** | Generic | Repo-specific metrics |

### Implementation Priority

| Priority | Task | Effort | Impact |
|----------|------|--------|--------|
| 🔴 **High** | Update OpenAiClient to read OPENAI_MODEL env var | 30 min | Fixes dual config issue |
| 🟡 **Medium** | Create evals.config.json template | 1 hour | Enables distributed evals |
| 🟡 **Medium** | Create EVALS_SETUP_GUIDE.md | 45 min | Enables consumer adoption |
| 🟢 **Low** | Add GitHub Action workflow | 1.5 hours | Enables automation |

---

## Recommended Next Steps

### Phase 2 (Short Term - 1-2 days)
1. ✅ Fix dual model configuration (Question 1)
2. ✅ Create evals configuration template
3. ✅ Create setup guide for consumers

### Phase 3 (Medium Term - 1 week)
4. ✅ Implement online evals support
5. ✅ Create GitHub Action for automated evals
6. ✅ Test with Reddit-ELT-Pipeline submodule

### Phase 4 (Long Term)
7. ✅ Add metrics dashboard
8. ✅ Add historical trending
9. ✅ Add AI-powered suggestions for improvements

---

## Files Affected

### For Question 1 Fix
- `src/SupportConcierge/Agents/OpenAiClient.cs` (Line 25)
- `src/SupportConcierge/Program.cs` (if applicable)
- `.github/workflows/support-concierge.yml` (Line 37)

### For Question 2 Implementation
- `evals/EvalRunner/Program.cs` (extend)
- **New:** `evals/templates/evals.config.json`
- **New:** `evals/docs/EVALS_SETUP_GUIDE.md`
- **Optional:** `.github/workflows/evals.yml`

---

**Status:** Ready for implementation in Phase 2  
**Recommendation:** Start with Question 1 fix (quick win), then tackle Question 2 (strategic long-term value)
