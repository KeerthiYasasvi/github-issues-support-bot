# GitHub Issues Support Bot - Implementation Roadmap

## 📋 Executive Summary

**Objective**: Transform `github-issues-support` into a standalone, reusable bot repository, then integrate it into `reddit-etl-pipeline` as a Git submodule with fresh testing.

**Scope**: 7 steps across 2 repositories, ~6-8 hours total effort

**End State**: 
- ✅ `github-issues-support` = Published standalone bot (PyPI/GitHub Releases ready)
- ✅ `reddit-etl-pipeline` = Clean, tested, with bot integrated via submodule
- ✅ Full scenario testing suite validated

---

## 🏗️ Execution Plan

### PHASE 1: Preparation (Current State)
**Status**: Already Complete ✅
- ✅ Reorganized `github-issues-support` documentation (Step 1 prep)
- ✅ Root directory clean with docs/ hierarchy

### PHASE 2: Repository Preparation & Publishing (4-6 hours)

#### STEP 1: Create Installation/Usage README
**Purpose**: Guide users on integrating bot into their projects  
**Effort**: 1-2 hours  
**Deliverable**: [USE_IN_YOUR_PROJECT.md](docs/guides/USE_IN_YOUR_PROJECT.md)

**Content to Include:**
- 3 integration methods (Submodule, Copy, Fork)
- Prerequisites (GitHub account, .NET 8, OpenAI API key)
- Step-by-step setup for each method
- Configuration walkthrough
- Verification checklist
- Troubleshooting common issues

**Decision Point**: Which integration method to recommend for reddit-etl-pipeline?
→ **RECOMMENDATION**: Git Submodule (version-controlled, updateable)

---

#### STEP 2: Publish as GitHub Repository
**Purpose**: Make `github-issues-support` a standalone, reusable project  
**Effort**: 30 mins  
**Actions:**
1. Create new public GitHub repository: `github-issues-support-bot`
2. Set repository description: "Automated issue triage bot for GitHub"
3. Add topics: `github-actions`, `bot`, `issue-triage`, `ai`, `openai`
4. Configure repository settings:
   - Enable Issues (for bot testing)
   - Enable Discussions (for community questions)
   - Add GitHub Pages (optional, for documentation site)
5. Configure branch protection on `main`
6. Add CODEOWNERS file (for review workflow)
7. Push current organized code

**GitHub Repository Setup Details:**
```
Repository Name: github-issues-support-bot
Visibility: Public
Description: "Intelligent GitHub Action for automated issue triage, routing, and engineer brief generation"
Topics: github-actions, bot, issue-triage, ai, openai, csharp, dotnet
Default Branch: main
Branch Protection: Require PR reviews before merge
```

**Critical**: After publishing, get repository URL for submodule step

---

### PHASE 3: Clean reddit-etl-pipeline (2-3 hours)

#### STEP 3: Remove Bot Files from reddit-etl-pipeline
**Purpose**: Clear separation of concerns between ETL project and bot framework  
**Effort**: 30 mins  
**Actions:**

**DELETE these directories (bot-specific, not ETL-specific):**
```
reddit-etl-pipeline/
├── .github-bot/              ← DELETE (moving to separate repo)
├── src/SupportConcierge/     ← DELETE (bot code)
├── evals/                    ← DELETE (bot evaluation tests)
└── .supportbot/              ← DELETE (bot config)
```

**KEEP these (project-specific):**
```
reddit-etl-pipeline/
├── .github/workflows/        ← Keep ETL workflows ONLY
├── src/                      ← Keep ETL code
├── src/Reddit-ETL-Pipeline/  ← Keep project code
├── tests/                    ← Keep project tests
└── README.md                 ← Keep project README
```

**Commands to Execute:**
```powershell
# Navigate to reddit-etl-pipeline
cd D:\Projects\reddit-etl-pipeline

# Remove bot-specific directories
Remove-Item ".github-bot" -Recurse -Force
Remove-Item "src/SupportConcierge" -Recurse -Force
Remove-Item "evals" -Recurse -Force
Remove-Item ".supportbot" -Recurse -Force

# List remaining to verify
Get-ChildItem -Directory | Select-Object Name
```

**Validation**:
- [ ] .github-bot/ removed
- [ ] src/SupportConcierge/ removed
- [ ] evals/ removed
- [ ] .supportbot/ removed
- [ ] Project still has src/, .github/, README.md

---

#### STEP 4: Delete Test Issues & Workflows
**Purpose**: Clean state for fresh testing round  
**Effort**: 1-2 hours  
**Manual Actions:**

**In reddit-etl-pipeline GitHub repo:**
1. Delete all test issues (Issues #1-#23 or however many)
2. Delete bot-specific workflows:
   - ❌ `.github/workflows/support-bot.yml`
   - ✅ Keep: `.github/workflows/etl-*.yml` (project workflows)
3. Clear issue history:
   - Run any cleanup scripts
   - Reset state comments if bot had written to issues

**Decision Point**: Keep or delete GitHub Actions history?
→ **RECOMMENDATION**: Delete (clean slate for fresh testing)

**Manual Cleanup Checklist:**
```
GitHub Issues Tab:
☐ Delete all test issues (Issues #1 onwards)
☐ Verify no "support-bot" labels remain
☐ Clear any pinned issues

GitHub Actions Tab:
☐ Delete support-bot workflow runs
☐ Verify no failed bot runs

GitHub Workflows Files:
☐ Remove .github/workflows/support-bot.yml
☐ Verify remaining workflows are ETL-specific
```

---

### PHASE 4: Integration & Setup (2-3 hours)

#### STEP 5: Add as Git Submodule & Update README
**Purpose**: Integrate bot framework into project with version control  
**Effort**: 1-1.5 hours  
**Actions:**

**Add Submodule:**
```powershell
cd D:\Projects\reddit-etl-pipeline

# Add bot as submodule
git submodule add https://github.com/YOUR_USERNAME/github-issues-support-bot.git .github-bot

# Initialize and update
git submodule update --init --recursive
```

**Verify Submodule:**
```powershell
# Check .gitmodules file
Get-Content .gitmodules

# Should show:
# [submodule ".github-bot"]
#     path = .github-bot
#     url = https://github.com/YOUR_USERNAME/github-issues-support-bot.git
```

**Setup Bot in reddit-etl-pipeline:**
1. Copy bot workflow to project workflows:
   ```powershell
   Copy-Item ".github-bot/.github/workflows/support-bot.yml" ".github/workflows/"
   ```

2. Copy bot config to project:
   ```powershell
   Copy-Item ".github-bot/.supportbot" ".supportbot" -Recurse
   ```

3. Configure `.supportbot/categories.yaml` for ETL project context:
   ```yaml
   categories:
     - name: setup
       keywords: ["install", "setup", "build", "environment"]
       description: "Environment setup and installation issues"
   ```

**Update README.md:**
Add section like:
```markdown
## 🤖 Issue Support Bot

This project uses an automated GitHub Issues Support Bot to help triage and route issues efficiently.

### Features
- Automatic issue categorization
- Completeness scoring
- Targeted follow-up questions
- Engineer brief generation
- Issue routing with labels and assignees

### For Maintainers
The bot is integrated as a Git submodule in `.github-bot/`. To:
- **Update bot**: `git submodule update --remote .github-bot`
- **Configure behavior**: Edit `.supportbot/*.yaml` files
- **Disable bot**: Remove workflow from `.github/workflows/support-bot.yml`

### For Contributors
The bot will automatically help validate your issue is complete before a maintainer reviews it.

See [.github-bot/README.md](.github-bot/README.md) for bot documentation.
```

**Commit Structure:**
```powershell
git add .gitmodules .github-bot
git commit -m "chore: add github-issues-support-bot as submodule

- Integrate bot as git submodule (.github-bot/)
- Copy bot workflow to project workflows
- Add bot configuration files (.supportbot/)
- Update README with bot documentation links"

git add README.md
git commit -m "docs: document issue support bot integration"

git push origin main
```

---

### PHASE 5: Testing & Validation (2-3 hours)

#### STEP 6: Create Fresh Issues & Test Scenarios
**Purpose**: Validate bot works correctly in reddit-etl-pipeline context  
**Effort**: 1.5-2 hours

**Create Test Issues (One per Scenario):**

**Issue #1: Scenario 1 - Well-Formed Issue** (Expected: Accepted immediately)
```
Title: "Add caching layer to pipeline"
Body: [Complete issue with all required fields]
Expected: Bot accepts, routes to team, generates engineer brief
```

**Issue #2: Scenario 2 - Incomplete Issue** (Expected: Asks questions)
```
Title: "Pipeline crashes"
Body: [Minimal information]
Expected: Bot asks for error message, steps to reproduce, environment
```

**Issue #3: Scenario 3 - Memory Enhancement** (Expected: Tracks state)
```
Title: "Out of memory on large datasets"
Body: [Incomplete → answers follow-ups → complete]
Expected: Bot tracks responses in state, finalizes after ~3 exchanges
```

**Issue #4: Scenario 4 - Dry-Run Evaluation**
```
Title: "Performance optimization needed"
Body: [Various completeness levels]
Expected: Dry-run mode returns evaluation without modifying issue
```

**Issue #5: Real Use Case** (For authenticity)
```
Title: [Realistic issue for ETL pipeline]
Body: [Natural issue report]
Expected: Bot handles realistically
```

**Testing Checklist:**
```
☐ Issue #1: Bot accepts well-formed issue
☐ Issue #1: Engineer brief generated and commented
☐ Issue #1: Labels applied correctly
☐ Issue #1: Issue assigned to team member (if configured)

☐ Issue #2: Bot asks follow-up questions
☐ Issue #2: Awaits response in thread

☐ Issue #3: Bot tracks memory state between comments
☐ Issue #3: State persists across comments
☐ Issue #3: Finalizes when complete

☐ Issue #4: Dry-run mode works
☐ Issue #4: No actual comments/labels applied

☐ Issue #5: Real scenario works naturally

☐ All workflows executed without errors
☐ No secrets exposed in logs
☐ Performance acceptable (<30 sec per issue)
```

**Documentation:**
Create file: [TEST_RESULTS_FRESH.md](../reddit-etl-pipeline/TEST_RESULTS_FRESH.md)
```markdown
# Fresh Testing Results - reddit-etl-pipeline Integration

## Test Environment
- Submodule Version: [commit hash]
- Configuration: Standard ETL categorization
- Date: [date]

## Test Cases
### Scenario 1: Well-Formed Issue
- ✅ PASSED
- Duration: 8 seconds
- Brief Generated: Yes

### Scenario 2: Incomplete Issue
- ✅ PASSED
- Follow-up Questions Asked: 3
- User Response: Pending

[... continue for all scenarios ...]
```

---

#### STEP 7: Confirm Everything Works
**Purpose**: Final validation before considering complete  
**Effort**: 30 mins - 1 hour

**Comprehensive Validation Checklist:**

**GitHub Repository Health:**
```
☐ github-issues-support-bot repo exists and is public
☐ Documentation complete (docs/ folder)
☐ README has clear usage instructions
☐ LICENSE file present
☐ INSTALLATION.md has 3 methods documented
```

**reddit-etl-pipeline Integration:**
```
☐ Submodule added correctly (.github-bot/)
☐ Submodule at correct commit version
☐ .gitmodules file correct
☐ Bot workflow in .github/workflows/support-bot.yml
☐ Bot config in .supportbot/
☐ README updated with bot information
```

**Testing Results:**
```
☐ All 5 test issues created and executed
☐ All scenarios passed (1, 2, 3, 4, +1 realistic)
☐ No errors in workflow logs
☐ Bot responses appropriate for ETL context
☐ State management working correctly
```

**Code Quality:**
```
☐ No secrets in git history
☐ .gitignore properly excludes build artifacts
☐ .archive/ excluded from distribution
☐ No temporary files committed
```

**Documentation:**
```
☐ Submodule documented in README
☐ Test results documented
☐ Configuration options explained
☐ Troubleshooting guide available
```

**Final Verification Commands:**
```powershell
# 1. Verify submodule structure
cd D:\Projects\reddit-etl-pipeline
git config --file=.gitmodules --list

# 2. Check submodule status
git submodule status

# 3. Verify bot files accessible
Test-Path ".github-bot/.github/workflows/support-bot.yml"
Test-Path ".supportbot/"

# 4. Verify git history clean
git log --oneline -10  # Should show submodule addition

# 5. Verify no secrets
git log -p | Select-String "API_KEY|SECRET|PASSWORD" -ErrorAction SilentlyContinue

# 6. Test fresh clone
git clone https://github.com/YOUR_USERNAME/reddit-etl-pipeline.git reddit-etl-test
cd reddit-etl-test
git submodule update --init
Test-Path ".github-bot"
```

---

## 🚦 Critical Decisions & Recommendations

| Decision | Options | Recommendation | Rationale |
|----------|---------|-----------------|-----------|
| **Integration Method** | Submodule / Copy / Fork | **Submodule** | Version-controlled, easy updates, clear separation |
| **Repository Visibility** | Public / Private | **Public** | Encourages adoption, demonstrates code quality |
| **Test Scope** | Minimal / Comprehensive | **Comprehensive** | Interview preparation, confidence building |
| **Cleanup Strategy** | Keep history / Clean slate | **Clean slate** | Fresh testing, removes noise from logs |
| **Configuration** | Shared / Project-specific | **Project-specific** | Each project owns its category definitions |

---

## ⚠️ Risk Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| **Secrets in git history** | Medium | Critical | Pre-commit scan, git filter-branch if needed |
| **Submodule permissions issue** | Low | High | Test clone from fresh machine |
| **Bot workflow not triggering** | Medium | High | Verify workflow syntax, check GitHub Actions logs |
| **State persistence bug** | Medium | Medium | Test across multiple comment cycles |
| **Integration breaks existing ETL** | Low | Medium | Keep project code separate from bot dirs |

---

## 📅 Timeline Estimate

| Phase | Steps | Effort | Total |
|-------|-------|--------|-------|
| **Preparation** | Current state | — | ✅ Done |
| **Repository** | 1-2 | 3.5 hrs | 3.5 hrs |
| **Cleanup** | 3-4 | 1.5 hrs | 5 hrs |
| **Integration** | 5 | 1.5 hrs | 6.5 hrs |
| **Testing** | 6-7 | 2.5 hrs | **9 hours total** |

**Realistic Timeline**: 8-10 hours over 2-3 days (accounting for GitHub UI latency, workflow execution time)

---

## ✅ Success Criteria

**Project is successfully reorganized when:**
- ✅ `github-issues-support-bot` is published GitHub repository with clean documentation
- ✅ `reddit-etl-pipeline` has bot integrated as submodule with zero breaking changes
- ✅ All 5 test scenarios pass with documented results
- ✅ README in both repos clearly explains purpose and usage
- ✅ No secrets or temporary files in git history
- ✅ Fresh clone of reddit-etl-pipeline can initialize submodule successfully
- ✅ User can demonstrate during interview: "Here's the bot, here's how I integrated it, here are the test results"

---

## 🎯 Interview Talking Points (After Completion)

When discussing this project in interviews:

1. **Architecture**: "The bot uses a modular architecture that separates concerns—deterministic scoring, LLM for complex tasks, and stateful orchestration without external databases."

2. **Reusability**: "I designed it as a standalone framework so it can be integrated into any GitHub project via Git submodule, with project-specific configuration."

3. **Integration**: "We demonstrate the pattern by integrating it into the reddit-etl-pipeline project—showing how it works with real-world ETL use cases."

4. **Testing**: "I created a comprehensive evaluation harness that tests all scenarios—from well-formed issues to incomplete ones requiring follow-up—with dry-run capabilities."

5. **Organization**: "I reorganized both repositories professionally—separated bot infrastructure from project code, created clear documentation hierarchy, and documented the integration pattern."

---

## 📝 Next Steps

1. **Review this roadmap** - Are there any changes to approach?
2. **Proceed with STEP 1** - Create USE_IN_YOUR_PROJECT.md
3. **Publish repository** - STEP 2
4. **Execute cleanup** - STEPS 3-4 (can parallelize)
5. **Integrate submodule** - STEP 5
6. **Execute testing** - STEPS 6-7

**Ready to proceed?** Confirm which step to start with.

---

*Last updated: January 14, 2026*
