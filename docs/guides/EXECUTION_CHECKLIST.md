# 📋 Step-by-Step Execution Checklist

Quick reference checklist for implementing the reorganization plan.

---

## PHASE 1: ✅ COMPLETE (Already Done)
- [x] Reorganized github-issues-support documentation
- [x] Created docs/ hierarchy
- [x] Moved analysis files to .archive/

---

## PHASE 2: Repository Preparation & Publishing

### STEP 1: Create Installation/Usage README
**Time**: 1-2 hours  
**Files to Create**: `docs/guides/USE_IN_YOUR_PROJECT.md`

**Tasks:**
- [ ] Document 3 integration methods (Submodule/Copy/Fork)
- [ ] List prerequisites (GitHub account, .NET 8, OpenAI key)
- [ ] Write step-by-step setup for submodule method
- [ ] Include configuration guide
- [ ] Add verification checklist
- [ ] Document troubleshooting scenarios

**Decision**: Recommend Submodule? **YES** ✓

---

### STEP 2: Publish as GitHub Repository
**Time**: 30 minutes  
**Manual Actions on GitHub.com**

**Tasks:**
- [ ] Create new public GitHub repository
  - Name: `github-issues-support-bot`
  - Description: "Intelligent GitHub Action for automated issue triage, routing, and engineer brief generation"
  
- [ ] Add repository settings:
  - [ ] Add topics: `github-actions`, `bot`, `issue-triage`, `ai`, `openai`, `csharp`, `dotnet`
  - [ ] Enable Issues
  - [ ] Enable Discussions
  - [ ] Set main as default branch
  
- [ ] Configure branch protection:
  - [ ] Require PR reviews: ON
  - [ ] Require status checks: ON
  
- [ ] Add files:
  - [ ] CODEOWNERS file
  
- [ ] Push current code:
  ```powershell
  cd D:\Projects\agents\ms-quickstart\github-issues-support
  git remote add origin https://github.com/YOUR_USERNAME/github-issues-support-bot.git
  git branch -M main
  git push -u origin main
  ```

**Get Repository URL for next step**: `https://github.com/YOUR_USERNAME/github-issues-support-bot.git`

---

## PHASE 3: Clean reddit-etl-pipeline

### STEP 3: Remove Bot Files from reddit-etl-pipeline
**Time**: 30 minutes  
**Location**: `D:\Projects\reddit-etl-pipeline`

**Tasks:**
- [ ] Navigate to reddit-etl-pipeline
  ```powershell
  cd D:\Projects\reddit-etl-pipeline
  ```

- [ ] Delete bot-specific directories:
  ```powershell
  Remove-Item ".github-bot" -Recurse -Force
  Remove-Item "src/SupportConcierge" -Recurse -Force
  Remove-Item "evals" -Recurse -Force
  Remove-Item ".supportbot" -Recurse -Force
  ```

- [ ] Verify deletion:
  ```powershell
  Get-ChildItem -Directory | Select-Object Name
  # Should NOT show: .github-bot, SupportConcierge, evals (but SHOULD have .supportbot if project-owned)
  ```

- [ ] Verify project integrity:
  - [ ] src/Reddit-ETL-Pipeline/ still exists
  - [ ] .github/ exists (keep for ETL workflows)
  - [ ] README.md exists
  - [ ] LICENSE exists

- [ ] Git commit:
  ```powershell
  git add -A
  git commit -m "chore: remove bot infrastructure (moving to separate repo)

- Remove .github-bot/ directory
- Remove src/SupportConcierge/
- Remove evals/ directory
- Remove .supportbot/ (will re-add as configured copy)

Bot now integrated via git submodule from standalone repository."
  ```

---

### STEP 4: Delete Test Issues & Workflows
**Time**: 1-2 hours  
**Manual Actions on GitHub.com**

**In reddit-etl-pipeline GitHub repository:**

- [ ] **Issues Tab** - Delete all test issues:
  - [ ] Go to Issues tab
  - [ ] Select all test issues (Issues #1 through #X)
  - [ ] Click "Close with comment" or delete
  - [ ] Confirm no "support-bot" labels remain
  
- [ ] **Actions Tab** - Clean up workflow history:
  - [ ] Go to Actions tab
  - [ ] Delete all support-bot workflow runs
  - [ ] Verify no failed/stuck jobs remain

- [ ] **Workflows File** - Remove bot workflow:
  ```powershell
  # Delete bot workflow file
  Remove-Item "D:\Projects\reddit-etl-pipeline\.github\workflows\support-bot.yml"
  
  # Verify only ETL workflows remain
  Get-ChildItem ".github\workflows" -Filter "*.yml" | Select-Object Name
  ```

- [ ] **Git Commit:**
  ```powershell
  git add -A
  git commit -m "chore: clean up test issues and bot workflow

- Remove all test issues from github.com/reddit-etl-pipeline/issues
- Remove support-bot.yml workflow
- Clean up action history for fresh start"
  
  git push origin main
  ```

---

## PHASE 4: Integration & Setup

### STEP 5: Add as Git Submodule & Update README
**Time**: 1-1.5 hours  
**Location**: `D:\Projects\reddit-etl-pipeline`

**Tasks:**

- [ ] **Add submodule:**
  ```powershell
  cd D:\Projects\reddit-etl-pipeline
  git submodule add https://github.com/YOUR_USERNAME/github-issues-support-bot.git .github-bot
  git submodule update --init --recursive
  ```

- [ ] **Verify submodule added:**
  ```powershell
  Get-Content .gitmodules
  # Should show [submodule ".github-bot"] section
  ```

- [ ] **Check submodule status:**
  ```powershell
  git submodule status
  # Should show: [hash] .github-bot (master)
  ```

- [ ] **Copy bot workflow to project:**
  ```powershell
  Copy-Item ".github-bot\.github\workflows\support-bot.yml" ".github\workflows\support-bot.yml"
  ```

- [ ] **Copy bot config to project:**
  ```powershell
  Copy-Item ".github-bot\.supportbot" ".supportbot" -Recurse
  ```

- [ ] **Customize .supportbot/categories.yaml** for ETL context:
  ```yaml
  # Edit file to include ETL-specific categories like:
  # - setup-etl
  # - data-extraction
  # - transformation-logic
  # - output-loading
  # - performance
  ```

- [ ] **Update reddit-etl-pipeline README.md** - Add section:
  ```markdown
  ## 🤖 Issue Support Bot

  This project uses an automated GitHub Issues Support Bot to help triage and validate issues.

  ### Features
  - Automatic issue categorization (setup, extraction, transformation, loading, performance)
  - Completeness scoring against issue checklist
  - Targeted follow-up questions for missing information
  - Engineer brief generation with context
  - Issue routing with labels and assignees

  ### For Maintainers
  
  The bot is integrated as a Git submodule in `.github-bot/`. To manage it:

  **Update bot to latest version:**
  ```bash
  git submodule update --remote .github-bot
  ```

  **Configure bot behavior:**
  Edit `.supportbot/categories.yaml`, `.supportbot/validators.yaml`, and `.supportbot/routing.yaml`

  **Disable bot (if needed):**
  Remove `.github/workflows/support-bot.yml`

  **Learn more:**
  See [.github-bot/docs/](​.github-bot/docs/) for detailed documentation

  ### For Contributors
  
  When you create an issue, the bot will help validate your report is complete. Follow its suggestions for the fastest resolution.
  ```

- [ ] **Verify bot workflow file has correct paths:**
  ```powershell
  # Open .github/workflows/support-bot.yml
  # Verify it references: .supportbot/ (NOT .github-bot/.supportbot/)
  # This is CRITICAL for the workflow to find config files
  ```

- [ ] **Git commit - Submodule addition:**
  ```powershell
  git add .gitmodules .github-bot
  git commit -m "chore: add github-issues-support-bot as git submodule

- Add github-issues-support-bot as submodule in .github-bot/
- Copy bot workflow to .github/workflows/support-bot.yml
- Copy bot configuration to .supportbot/
- Enables version-controlled bot updates and project-specific customization"
  ```

- [ ] **Git commit - README update:**
  ```powershell
  git add README.md
  git commit -m "docs: document issue support bot integration

- Add bot feature overview
- Document maintainer commands
- Link to bot documentation in submodule"

  git push origin main
  ```

---

## PHASE 5: Testing & Validation

### STEP 6: Create Fresh Issues & Test Scenarios
**Time**: 1.5-2 hours  
**Location**: GitHub.com (reddit-etl-pipeline repo)

**Create Test Issues:**

- [ ] **Issue #1: Scenario 1 - Well-Formed Issue**
  - Title: "Improve ETL pipeline performance on large datasets"
  - Body: [Well-formed with all required fields]
  - Expected: Bot accepts, generates brief, applies labels
  - Result: _____ (PASSED/FAILED)
  - Duration: _____ seconds

- [ ] **Issue #2: Scenario 2 - Incomplete Issue**
  - Title: "ETL job crashes randomly"
  - Body: [Minimal information]
  - Expected: Bot asks follow-up questions
  - Result: _____ (PASSED/FAILED)
  - Bot Questions Asked: _____

- [ ] **Issue #3: Scenario 3 - Memory Enhancement**
  - Title: "Memory consumption increases over time"
  - Body: [Incomplete → user answers follow-ups → complete]
  - Expected: Bot tracks state across comments, finalizes
  - Result: _____ (PASSED/FAILED)
  - State Persistence: _____ (YES/NO)

- [ ] **Issue #4: Scenario 4 - Dry-Run Evaluation**
  - Title: "Data extraction timing issues"
  - Body: [Varying completeness levels]
  - Expected: Dry-run mode evaluates without modifying
  - Result: _____ (PASSED/FAILED)
  - Comments Posted: _____ (0 for dry-run)

- [ ] **Issue #5: Real Use Case**
  - Title: [Realistic ETL issue]
  - Body: [Natural issue report from project context]
  - Expected: Bot handles realistically
  - Result: _____ (PASSED/FAILED)

**Testing Checklist:**
- [ ] All 5 issues created successfully
- [ ] All issues triggered workflow (check Actions tab)
- [ ] No workflow errors or timeout issues
- [ ] Bot comments appearing correctly
- [ ] Labels being applied as expected
- [ ] State being persisted between comments
- [ ] No secrets exposed in logs
- [ ] Performance acceptable (<30 sec per issue)

**Document Results:**
Create file: [TEST_RESULTS_FRESH.md](../../../reddit-etl-pipeline/TEST_RESULTS_FRESH.md)
```markdown
# Fresh Testing Results - reddit-etl-pipeline Integration

Date: [Today's Date]
Tester: [Your Name]
Submodule Commit: [git log --oneline -1 .github-bot]

## Test Configuration
- Categories: ETL-specific (setup, extraction, transformation, loading, performance)
- Workflow: support-bot.yml
- Config: .supportbot/ (customized for ETL)

## Results Summary
| Scenario | Status | Duration | Notes |
|----------|--------|----------|-------|
| #1 - Well-Formed | PASS/FAIL | ___ sec | ___________ |
| #2 - Incomplete | PASS/FAIL | ___ sec | ___________ |
| #3 - Memory | PASS/FAIL | ___ sec | ___________ |
| #4 - Dry-Run | PASS/FAIL | ___ sec | ___________ |
| #5 - Real Case | PASS/FAIL | ___ sec | ___________ |

## Observations
[Document any issues, successes, or interesting behaviors]
```

---

### STEP 7: Confirm Everything Works
**Time**: 30 minutes - 1 hour  
**Final Validation**

**Repository Health Checks:**
- [ ] github-issues-support-bot repository exists and is public
- [ ] Documentation complete in docs/ folder
- [ ] README has clear usage instructions
- [ ] LICENSE file present
- [ ] Topics added to repository
- [ ] No secrets in git history:
  ```powershell
  cd D:\Projects\agents\ms-quickstart\github-issues-support
  git log -p | Select-String "API_KEY|SECRET|PASSWORD" | Measure-Object
  # Result should be 0 matches
  ```

**Integration Health Checks:**
- [ ] reddit-etl-pipeline has .github-bot submodule
- [ ] .gitmodules file correct
- [ ] git submodule status shows correct commit
- [ ] .github/workflows/support-bot.yml present
- [ ] .supportbot/ directory present and customized
- [ ] README.md updated with bot information

**Test Results Validation:**
- [ ] All 5 test issues created and executed
- [ ] All scenarios status documented in TEST_RESULTS_FRESH.md
- [ ] No errors in workflow logs
- [ ] Bot responses appropriate for project context
- [ ] State management working correctly

**Code Quality Checks:**
- [ ] No temporary files in git history
- [ ] .gitignore properly excludes build artifacts
- [ ] .archive/ not pushed to repository
- [ ] No secrets in configuration files

**Final Integration Test:**
- [ ] Fresh clone test:
  ```powershell
  git clone https://github.com/YOUR_USERNAME/reddit-etl-pipeline.git reddit-etl-fresh
  cd reddit-etl-fresh
  git submodule update --init
  Test-Path ".github-bot"  # Should return TRUE
  Test-Path ".github/workflows/support-bot.yml"  # Should return TRUE
  ```

**Success Criteria Met:**
- [ ] github-issues-support-bot published and documented
- [ ] reddit-etl-pipeline cleaned of bot-specific files
- [ ] All test issues and workflows deleted
- [ ] Bot integrated via submodule with fresh configuration
- [ ] All 5 scenarios tested and passing
- [ ] Fresh clone works correctly
- [ ] No secrets or temporary files in history
- [ ] Both READMEs updated with clear documentation

**Interview Ready Checklist:**
- [ ] Can show standalone bot repository
- [ ] Can explain submodule integration approach
- [ ] Can walk through test results (5 scenarios)
- [ ] Can demonstrate fresh clone initialization
- [ ] Can discuss architectural decisions

---

## 🎬 Quick Start Command Sequence

For quick reference, here's the command sequence in order:

```powershell
# STEP 1-2: (Done in docs, published on GitHub.com manually)

# STEP 3: Remove bot files
cd D:\Projects\reddit-etl-pipeline
Remove-Item ".github-bot", "src/SupportConcierge", "evals", ".supportbot" -Recurse -Force
git add -A
git commit -m "chore: remove bot infrastructure"
git push origin main

# STEP 4: (Done on GitHub.com - delete issues/workflows manually)
Remove-Item ".github\workflows\support-bot.yml"
git add -A
git commit -m "chore: clean up test issues and bot workflow"
git push origin main

# STEP 5: Add submodule
git submodule add https://github.com/YOUR_USERNAME/github-issues-support-bot.git .github-bot
Copy-Item ".github-bot\.github\workflows\support-bot.yml" ".github\workflows\"
Copy-Item ".github-bot\.supportbot" ".supportbot" -Recurse
# Edit README.md and .supportbot config
git add .gitmodules .github-bot
git commit -m "chore: add github-issues-support-bot as git submodule"
git add README.md
git commit -m "docs: document issue support bot integration"
git push origin main

# STEP 6-7: (Done on GitHub.com - create issues and test)
```

---

## ❓ Decision Points & Recommendations

**Should we use Submodule?**
→ YES - Version controlled, updateable, clear separation

**Should bot repository be public?**
→ YES - Encourages adoption, demonstrates code quality

**How comprehensive should testing be?**
→ All 5 scenarios - Interview preparation and confidence

**What's the timeline?**
→ 8-10 hours total, doable in 2-3 days

**What could go wrong?**
→ Submodule permissions, workflow trigger issues, state bugs
→ Mitigation: Test fresh clone, verify workflow syntax, multi-cycle state tests

---

**Ready to start?** Which step would you like to begin with?

Current recommendation: **Start with STEP 1** (Create USE_IN_YOUR_PROJECT.md)
