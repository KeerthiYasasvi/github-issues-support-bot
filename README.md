# GitHub Issues Support Bot

An intelligent GitHub Action that automatically triages issues, asks targeted follow-up questions, validates completeness, and creates engineer-ready case packets—saving engineering time and improving issue quality.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4-green.svg)](https://openai.com/)

---

## 🤖 What This Bot Does

When someone opens an issue in your repository, this bot:

1. **Categorizes the issue** automatically (setup, build, runtime, bug, feature, docs)
2. **Extracts key information** (error messages, steps to reproduce, environment details)
3. **Scores completeness** against your customizable checklists
4. **Asks follow-up questions** (up to 3 rounds) if information is missing
5. **Generates an engineer brief** with symptoms, repro steps, and suggested next steps
6. **Routes issues** with labels and assigns them to the right team members
7. **Suggests duplicate issues** based on error signature matching

**Result**: Better quality issues, faster triage, less back-and-forth with reporters.

---

## ✨ Key Features

- ✅ **No External Database**: State stored in hidden HTML comments in the issue itself
- ✅ **Deterministic Scoring**: Completeness calculated with rules—not guesswork
- ✅ **AI-Powered Extraction**: Uses OpenAI structured outputs for messy input
- ✅ **Configurable**: Customize categories, checklists, validators, and routing via YAML
- ✅ **Serverless**: Runs entirely on GitHub Actions (no server needed)
- ✅ **Eval Harness**: Test scenarios with dry-run mode before production

---

## 🚀 Quick Start (3 Methods)

### Option A: Git Submodule (Recommended)

Best for version-controlled integration and easy updates.

```bash
# 1. Add bot as submodule
git submodule add https://github.com/YOUR_USERNAME/github-issues-support-bot.git .github-bot
git submodule update --init --recursive

# 2. Copy workflow to your project
cp .github-bot/.github/workflows/support-bot.yml .github/workflows/

# 3. Copy config templates to your project
cp -r .github-bot/.supportbot .supportbot

# 4. Customize configuration for your project
# Edit .supportbot/categories.yaml, validators.yaml, routing.yaml

# 5. Commit changes
git add .gitmodules .github-bot .github/workflows/support-bot.yml .supportbot
git commit -m "Add GitHub Issues Support Bot"
git push
```

**Update bot to latest version:**
```bash
git submodule update --remote .github-bot
git commit -am "Update support bot"
git push
```

### Option B: Direct Copy (Simplest)

Best for quick setup or if you want to heavily customize the bot.

```bash
# 1. Clone this repository
git clone https://github.com/YOUR_USERNAME/github-issues-support-bot.git

# 2. Copy files to your project
cp -r github-issues-support-bot/src/SupportConcierge YOUR_PROJECT/src/
cp -r github-issues-support-bot/.supportbot YOUR_PROJECT/.supportbot
cp github-issues-support-bot/.github/workflows/support-bot.yml YOUR_PROJECT/.github/workflows/

# 3. Customize configuration
cd YOUR_PROJECT
# Edit .supportbot/categories.yaml, validators.yaml, routing.yaml

# 4. Commit to your repository
git add .
git commit -m "Add GitHub Issues Support Bot"
git push
```

### Option C: Fork & Customize

Best if you want to heavily modify the bot and contribute changes back.

1. Fork this repository on GitHub
2. Clone your fork
3. Make customizations
4. Use your fork as a submodule in your projects

---

## ⚙️ Configuration

### Prerequisites

1. **GitHub repository** with Issues enabled
2. **.NET 8 SDK** (for local development/testing)
3. **OpenAI API key** (GPT-4 or GPT-4o recommended)
4. **GitHub Actions** enabled with **Read and write permissions**

### Step 1: Configure GitHub Actions Permissions

Go to **Settings → Actions → General → Workflow permissions**:
- Select **Read and write permissions**
- Save changes

### Step 2: Add OpenAI API Key as Secret

Go to **Settings → Secrets and variables → Actions**:
- Click **New repository secret**
- Name: `OPENAI_API_KEY`
- Value: `sk-...` (your OpenAI API key)
- Click **Add secret**

### Step 3: Customize Bot Behavior

Edit files in `.supportbot/` directory:

#### **categories.yaml** - Define Issue Types
```yaml
categories:
  - name: setup
    keywords: ["install", "setup", "configure", "environment"]
    description: "Installation and setup issues"
  
  - name: runtime
    keywords: ["error", "crash", "exception", "fails"]
    description: "Runtime errors and crashes"
  
  - name: performance
    keywords: ["slow", "memory", "cpu", "performance"]
    description: "Performance and resource issues"
```

#### **checklists.yaml** - Required Information
```yaml
checklists:
  setup:
    - field: "Operating System"
      required: true
      weight: 10
    - field: "Steps Attempted"
      required: true
      weight: 15
    - field: "Error Message"
      required: true
      weight: 20
```

#### **validators.yaml** - Completeness Rules
```yaml
validators:
  - name: "HasErrorMessage"
    pattern: "(error|exception|failed|fatal)"
    points: 20
  - name: "HasReproSteps"
    pattern: "(steps?|reproduce|repro)"
    points: 15
```

#### **routing.yaml** - Auto-Assignment
```yaml
routing:
  setup:
    labels: ["setup", "needs-triage"]
    assignees: ["setup-team"]
  runtime:
    labels: ["bug", "runtime"]
    assignees: ["bug-team"]
```

---

## 📖 How It Works

### Workflow Triggers

The bot activates on these events:
- `issues.opened` - New issue created
- `issues.edited` - Issue body edited
- `issue_comment.created` - Comment added (user responding to questions)

### State Management

Bot state is stored in **hidden HTML comments** at the bottom of each issue:

```html
<!-- SUPPORT_BOT_STATE: {"version":"1.0","round":2,"category":"runtime","completeness":85} -->
```

This allows stateful conversations without external databases.

### Decision Flow

```
Issue Opened
    ↓
Categorize (keywords + AI)
    ↓
Extract Information
    ↓
Score Completeness
    ↓
├─ Completeness ≥ 80? → Generate Brief → Apply Labels → DONE
├─ Round < 3? → Ask Questions → Wait for Response
└─ Round ≥ 3? → Escalate to Maintainers → DONE
```

### Completeness Scoring

**Formula**: `(matched_fields * weights) / total_possible * 100`

- **80-100**: Actionable (generates engineer brief)
- **60-79**: Needs clarification (asks 1-2 questions)
- **<60**: Missing critical info (asks 3+ questions)

---

## 🧪 Testing

### Dry-Run Mode

Test bot behavior without actually commenting on issues:

1. Add environment variable in workflow:
   ```yaml
   env:
     SUPPORTBOT_DRY_RUN: "true"
   ```

2. Check workflow logs to see what bot *would* do:
   ```
   [DRY-RUN] Would comment: "Thanks for the issue! Can you provide..."
   [DRY-RUN] Would apply labels: ["bug", "needs-info"]
   ```

### Evaluation Harness

Run comprehensive test scenarios:

```bash
cd evals/EvalRunner
dotnet run --test-file ../scenarios/sample_issue_runtime_crash.json
```

Example output:
```
✓ Scenario: Runtime Crash Issue
  Category: runtime (correct)
  Completeness: 92/100
  Brief Generated: Yes
  Labels Applied: bug, runtime
  Grade: A
```

---

## 🔧 Advanced Configuration

### Custom OpenAI Model

Add repository variable `OPENAI_MODEL`:
- Go to **Settings → Secrets and variables → Actions → Variables**
- Name: `OPENAI_MODEL`
- Value: `gpt-4o-2024-08-06` (or `gpt-4-turbo-preview`)

### Custom Spec Directory

Add repository variable `SUPPORTBOT_SPEC_DIR`:
- Name: `SUPPORTBOT_SPEC_DIR`
- Value: `.github/supportbot-config` (or your custom path)

### Multiple Categories

You can define as many categories as needed:
```yaml
categories:
  - name: setup
  - name: build
  - name: runtime
  - name: performance
  - name: feature-request
  - name: documentation
  - name: security
```

### Weighted Scoring

Adjust field weights based on importance:
```yaml
checklists:
  runtime:
    - field: "Error Message"
      weight: 30  # Most important
    - field: "Steps to Reproduce"
      weight: 25  # Very important
    - field: "Expected Behavior"
      weight: 15  # Important
    - field: "Actual Behavior"
      weight: 15  # Important
    - field: "Environment"
      weight: 15  # Moderately important
```

---

## 🛠️ Local Development

### Build & Run

```bash
# Build project
cd src/SupportConcierge
dotnet build

# Run locally (requires GitHub webhook payload)
dotnet run --issue-payload path/to/issue.json

# Run evaluation tests
cd ../../evals/EvalRunner
dotnet run --scenario ../scenarios/sample_issue_runtime_crash.json
```

### Environment Variables for Local Testing

Create `.env` file:
```bash
GITHUB_TOKEN=ghp_your_token_here
OPENAI_API_KEY=sk-your_key_here
GITHUB_REPOSITORY=your-username/your-repo
SUPPORTBOT_DRY_RUN=true  # Safe for testing
```

### Debug Mode

Enable verbose logging in workflow:
```yaml
env:
  SUPPORTBOT_DEBUG: "true"
```

---

## 📚 Project Structure

```
.
├── src/SupportConcierge/           # Main bot application
│   ├── Program.cs                  # Entry point
│   ├── Orchestration/
│   │   ├── Orchestrator.cs         # Main workflow logic
│   │   └── StateStore.cs           # State management
│   ├── Agents/
│   │   ├── OpenAiClient.cs         # OpenAI integration
│   │   └── Schemas.cs              # JSON schemas
│   ├── Scoring/
│   │   ├── CompletenessScorer.cs   # Scoring engine
│   │   └── Validators.cs           # Validation rules
│   ├── GitHub/
│   │   └── GitHubApi.cs            # GitHub API client
│   └── Reporting/
│       └── CommentComposer.cs      # Comment generation
│
├── evals/EvalRunner/               # Testing harness
│   ├── Program.cs
│   └── scenarios/                  # Test cases
│
├── .supportbot/                    # Configuration templates
│   ├── categories.yaml
│   ├── checklists.yaml
│   ├── validators.yaml
│   └── routing.yaml
│
├── .github/workflows/
│   └── support-bot.yml             # GitHub Actions workflow
│
└── docs/                           # Documentation
    ├── guides/                     # Setup guides
    ├── technical/                  # Architecture docs
    └── reference/                  # Reference materials
```

---

## 🤝 Areas for Improvement

- **New validators**: Add domain-specific validation rules
- **Language support**: I18n for non-English issues
- **Platform integrations**: Jira, Linear, etc.
- **Enhanced scoring**: ML-based completeness prediction
- **UI improvements**: Better comment formatting

---

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file.
