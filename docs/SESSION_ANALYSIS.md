This content has been consolidated into existing documentation. Please refer to:

- `docs/technical/OPENAI_INTEGRATION_REFERENCE.md` (Model configuration, schema enforcement)
- `docs/technical/ARCHITECTURE.md` (Evals & Retrospective Metrics)
- `docs/guides/SETUP_EXECUTION.md` (OPENAI_MODEL configuration in local and CI)

---

## Question 1 Detailed Analysis

### The Problem

```
GitHub Actions Workflow (.github/workflows/support-concierge.yml)
├─ Line 37: OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4' }}
└─ Status: SET ✓ but UNUSED ✗

Application Code (src/SupportConcierge/Agents/OpenAiClient.cs)
├─ Line 25: _model = "gpt-4o-2024-08-06"
└─ Status: USED ✓ (hardcoded)

Evidence:
✓ grep search confirmed 0 matches for "OPENAI_MODEL" in C# code
✓ Environment variable never referenced by application
✓ Workflow setting is dead code
```

### Why This Matters

| Issue | Impact | Severity |
|-------|--------|----------|
| Configuration Inconsistency | Developers confused about which setting is used | 🔴 High |
| Dead Code | Workflow env var is never read | 🟡 Medium |
| Inflexibility | Model change requires code modification | 🟡 Medium |
| Future-Proofing | If model deprecated, needs manual code update | 🟡 Medium |

### The Fix

**File:** `src/SupportConcierge/Agents/OpenAiClient.cs`

Current (line 25):
```csharp
private readonly string _model = "gpt-4o-2024-08-06";
```

Replace with:
```csharp
private readonly string _model;

public OpenAiClient()
{
    _model = Environment.GetEnvironmentVariable("OPENAI_MODEL") 
        ?? "gpt-4o-2024-08-06";
    
    var source = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_MODEL"))
        ? "environment variable OPENAI_MODEL"
        : "default hardcoded value";
    
    Console.WriteLine($"[OPENAI] Using model: {_model} (from: {source})");
}
```

**File:** `.github/workflows/support-concierge.yml`

Current (line 37):
```yaml
OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4' }}
```

Update to:
```yaml
OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4o-2024-08-06' }}
```

### Testing the Fix

**Test 1: Default behavior (no override)**
```bash
cd src/SupportConcierge
dotnet run
# Expected output: [OPENAI] Using model: gpt-4o-2024-08-06 (from: default hardcoded value)
```

**Test 2: Override via environment variable**
```bash
$env:OPENAI_MODEL = "gpt-4-turbo"
cd src/SupportConcierge
dotnet run
# Expected output: [OPENAI] Using model: gpt-4-turbo (from: environment variable OPENAI_MODEL)
```

**Test 3: Workflow override**
```yaml
# In GitHub Actions: set repo variable OPENAI_MODEL
# Expected: Workflow uses environment variable value
```

---

## Question 2 Detailed Analysis

### The Problem

```
Current Evals System
├─ Mode: Offline only (dry-run)
├─ Scenarios: Hardcoded local files (2 samples)
├─ Testing: No real GitHub API interaction
├─ Distribution: Not deployable to consumers
└─ Customization: Requires code changes

Consumer Needs
├─ Online testing against actual repos
├─ Per-repository customization
├─ Result comparison and metrics
├─ Automated periodic runs
└─ Template-based setup
```

### Why This Matters

| Need | Current | With Solution | Value |
|------|---------|--------------|-------|
| Test real behavior | ❌ Local only | ✅ Real issues | Know actual bot quality |
| Customization | ❌ Code changes | ✅ Config file | Easy per-repo setup |
| Distribution | ❌ Manual | ✅ Template | Consumers can self-serve |
| Automation | ❌ Manual runs | ✅ GitHub Actions | Scheduled periodic evals |
| Results tracking | ❌ No baseline | ✅ Comparison | Measure improvements |

### The Solution Architecture

#### Phases

**Phase 1: Configuration System (1-2 hours)**
- Create `evals/templates/evals.config.json` template
- Define config schema
- Support offline and online modes

**Phase 2: Enhance EvalRunner (2-3 hours)**
- Add config file loading
- Implement online mode (real GitHub interactions)
- Preserve offline mode (existing tests)
- Add result validation and comparison

**Phase 3: Setup & Documentation (1 hour)**
- Create setup script (`setup-evals.ps1`)
- Create setup guide
- Document usage examples

**Phase 4: Automation (Optional, 1 hour)**
- Create GitHub Action workflow template
- Support scheduled runs
- Generate reports and artifacts

#### Consumer Usage

For each repository that uses github-issues-support-bot as submodule:

**1. Setup**
```powershell
# Copy template
cp .github-bot/evals/templates/evals.config.json .github-bot-evals/

# Customize
edit .github-bot-evals/evals.config.json  # Set owner, repo, docs path
```

**2. Add test scenarios**
```json
{
  "scenarios": [
    {
      "id": "our-build-failure",
      "name": "Build Fails After Upgrade",
      "issue_title": "Build fails after upgrading framework",
      "expected_fields": ["build_log", "solution"]
    }
  ]
}
```

**3. Run locally (offline)**
```bash
dotnet run -- --config ../../.github-bot-evals/evals.config.json
```

**4. Run against real repo (online)**
```bash
GITHUB_TOKEN=xxx OPENAI_API_KEY=yyy dotnet run -- \
  --config ../../.github-bot-evals/evals.config.json \
  --mode online
```

### Data Flow (Online Mode)

```
1. Read config → 2. Authenticate GitHub
                    ↓
3. For each scenario:
   ├─ Create test issue in repo (with label: eval-test)
   ├─ Wait for bot response (2 sec)
   ├─ Fetch bot's comment
   ├─ Validate response against expected fields
   ├─ Close test issue (cleanup)
   └─ Record result
   
4. Generate report with results
5. Save to eval_report.json
```

---

## Recommended Implementation Order

### Immediate (This Week)
1. **✅ Fix Question 1** - 10 minute implementation
   - Centralize model configuration
   - Remove dead code from workflow
   - Add logging to show source
   - Test with override scenarios

### Short Term (Next Week)
2. **Question 2 - Phase 1** - Create configuration system
   - evals.config.json template
   - Configuration schema documentation
   
3. **Question 2 - Phase 2** - Enhance EvalRunner
   - Add config loading
   - Implement online mode
   - Add validation logic

### Medium Term (Following Week)
4. **Question 2 - Phase 3** - Setup and documentation
   - Setup script
   - Comprehensive guide
   - Usage examples

### Long Term (Optional)
5. **Question 2 - Phase 4** - Automation
   - GitHub Action workflow
   - Scheduled evaluations
   - Report generation

---

## Document Reference Map

| Document | Purpose | Link |
|----------|---------|------|
| **QUESTION_1_FIX_GUIDE.md** | Step-by-step fix for model configuration | [Open](QUESTION_1_FIX_GUIDE.md) |
| **QUESTION_2_EVALS_ENHANCEMENT.md** | Distributed evals architecture & implementation | [Open](QUESTION_2_EVALS_ENHANCEMENT.md) |
| **JSON_SCHEMA_IMPLEMENTATION.md** | Implementation details for Option B | [Open](JSON_SCHEMA_IMPLEMENTATION.md) |
| **TELEMETRY_MONITORING_GUIDE.md** | How to monitor telemetry events | [Open](TELEMETRY_MONITORING_GUIDE.md) |
| **ARCHITECTURE_ANALYSIS.md** | Original architecture investigation | [Open](ARCHITECTURE_ANALYSIS.md) |

---

## Next Actions

### For Question 1 (Ready to Implement)
- [ ] Update OpenAiClient.cs constructor
- [ ] Update support-concierge.yml defaults
- [ ] Run tests with environment override
- [ ] Commit changes
- [ ] Update documentation

### For Question 2 (Ready for Phase 1)
- [ ] Create evals/templates/evals.config.json
- [ ] Define EvalConfig classes
- [ ] Create setup-evals.ps1 script
- [ ] Write evals setup guide

---

## Key Takeaways

✅ **Both questions answered with evidence-based findings**
✅ **Solutions documented with implementation guides**
✅ **No blockers identified for implementation**
✅ **Infrastructure ready for distributed evals**
✅ **Model configuration inconsistency identified and fixable**

**Recommendation:** Proceed with Question 1 fix immediately (10 min), then plan Question 2 implementation phases.
