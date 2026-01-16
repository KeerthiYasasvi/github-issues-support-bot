# Project File Manifest

## Complete Repository Structure

```
github-issues-support/
│
├── .github/
│   └── workflows/
│       └── support-concierge.yml          # GitHub Actions workflow
│
├── src/
│   └── SupportConcierge/
│       ├── SupportConcierge.csproj        # Main project file
│       ├── Program.cs                      # Application entry point
│       │
│       ├── GitHub/
│       │   ├── GitHubApi.cs               # GitHub REST API client
│       │   └── Models.cs                   # GitHub API models
│       │
│       ├── SpecPack/
│       │   ├── SpecPackLoader.cs          # YAML config loader
│       │   └── SpecModels.cs               # Configuration models
│       │
│       ├── Parsing/
│       │   └── IssueFormParser.cs         # Markdown parser
│       │
│       ├── Scoring/
│       │   ├── CompletenessScorer.cs      # Scoring engine
│       │   ├── Validators.cs               # Field validators
│       │   └── SecretRedactor.cs           # Secret pattern detector
│       │
│       ├── Agents/
│       │   ├── OpenAiClient.cs            # OpenAI API wrapper
│       │   ├── Prompts.cs                  # LLM prompts
│       │   └── Schemas.cs                  # JSON schemas for structured outputs
│       │
│       ├── Orchestration/
│       │   ├── Orchestrator.cs            # Main workflow coordinator
│       │   └── StateStore.cs               # State persistence
│       │
│       └── Reporting/
│           └── CommentComposer.cs         # Markdown comment generator
│
├── .supportbot/
│   ├── categories.yaml                     # Issue categories + keywords
│   ├── checklists.yaml                     # Required fields per category
│   ├── validators.yaml                     # Validation rules + secret patterns
│   ├── routing.yaml                        # Labels + assignees per category
│   └── playbooks/
│       ├── build.md                        # Build troubleshooting guide
│       ├── runtime.md                      # Runtime troubleshooting guide
│       └── docs.md                         # Documentation issue guide
│
├── evals/
│   ├── scenarios/
│   │   ├── sample_issue_build_missing_logs.json    # Build issue test
│   │   └── sample_issue_runtime_crash.json         # Runtime issue test
│   └── EvalRunner/
│       ├── EvalRunner.csproj              # Eval project file
│       └── Program.cs                      # Evaluation harness
│
├── .gitignore                              # Git ignore patterns
├── GitHubIssuesSupport.sln                # Visual Studio solution
├── LICENSE                                 # MIT License
├── README.md                               # Main documentation
├── QUICKSTART.md                           # Setup guide
└── ARCHITECTURE.md                         # Design documentation
```

## File Descriptions

### Core Application (src/SupportConcierge/)

**Program.cs** (80 lines)
- Entry point for GitHub Action
- Validates environment variables
- Loads event payload
- Initializes and runs Orchestrator

**Orchestration/Orchestrator.cs** (350+ lines)
- Main workflow coordinator
- Implements issue lifecycle:
  1. Category determination
  2. Field extraction
  3. Completeness scoring
  4. Decision logic (actionable/follow-up/escalate)
- Coordinates all components

**Orchestration/StateStore.cs** (80 lines)
- Extracts/embeds state in HTML comments
- Pattern: `<!-- supportbot_state:{json} -->`
- Tracks loop count, category, asked fields

### GitHub Integration

**GitHub/GitHubApi.cs** (150 lines)
- HTTP client for GitHub REST API
- Methods: Get/Post comments, add labels/assignees, search issues, get files
- Uses bearer token authentication

**GitHub/Models.cs** (80 lines)
- GitHubIssue, GitHubComment, GitHubUser, GitHubLabel, GitHubRepository
- Request/response models for API calls

### Configuration System

**SpecPack/SpecPackLoader.cs** (100 lines)
- Loads YAML files from .supportbot/
- Parses categories, checklists, validators, routing
- Loads Markdown playbooks

**SpecPack/SpecModels.cs** (100 lines)
- Category, CategoryChecklist, RequiredField
- ValidatorRules, RoutingRules, SpecPackConfig
- Type-safe configuration models

### Parsing & Scoring

**Parsing/IssueFormParser.cs** (100 lines)
- Extracts fields from Markdown headings (`### Field Name`)
- Extracts key-value pairs (`Key: Value`)
- Normalizes field names for matching

**Scoring/CompletenessScorer.cs** (120 lines)
- Calculates weighted completeness score
- Identifies missing/invalid fields
- Determines actionability (score >= threshold)

**Scoring/Validators.cs** (120 lines)
- Junk value detection (N/A, idk, empty)
- Format validation (version, email, URL regex)
- Contradiction detection between fields

**Scoring/SecretRedactor.cs** (80 lines)
- Redacts API keys, tokens, passwords
- Uses regex patterns from validators.yaml
- Returns redacted text + list of findings

### OpenAI Integration

**Agents/OpenAiClient.cs** (200 lines)
- Official OpenAI .NET SDK wrapper
- Four structured output methods:
  1. ClassifyCategoryAsync
  2. ExtractCasePacketAsync
  3. GenerateFollowUpQuestionsAsync
  4. GenerateEngineerBriefAsync
- All use JSON Schema for guaranteed valid output

**Agents/Prompts.cs** (120 lines)
- Prompt templates for each LLM task
- Includes grounding instructions
- Prevents hallucinations with explicit constraints

**Agents/Schemas.cs** (180 lines)
- JSON Schema definitions for structured outputs:
  - CategoryClassificationSchema
  - CasePacketExtractionSchema
  - FollowUpQuestionsSchema
  - EngineerBriefSchema

### Reporting

**Reporting/CommentComposer.cs** (150 lines)
- Generates Markdown for:
  1. Follow-up questions
  2. Engineer briefs (with collapsible JSON)
  3. Escalation notices
- Formats symptoms, steps, evidence, warnings

### Configuration Files (.supportbot/)

**categories.yaml** (50 lines)
- 5 categories: setup, build, runtime, bug, docs
- Keywords for auto-detection

**checklists.yaml** (200 lines)
- Required fields per category
- Weights, descriptions, aliases
- Completeness thresholds

**validators.yaml** (60 lines)
- Junk patterns (regex)
- Format validators (version, email, URL)
- Secret patterns (API keys, tokens, passwords)
- Contradiction rules

**routing.yaml** (40 lines)
- Labels per category
- Assignee placeholders
- Escalation mentions

**playbooks/*.md** (3 files, ~50 lines each)
- Category-specific troubleshooting guides
- Used to ground engineer brief next steps
- Customizable per repository

### Evaluation System (evals/)

**scenarios/*.json** (2 files)
- Sample issue payloads
- Expected outcomes (category, actionability, extracted fields)
- Used to validate bot behavior

**EvalRunner/Program.cs** (250 lines)
- Loads test scenarios
- Runs extraction + scoring pipeline
- Detects hallucinations (values not in source)
- Generates metrics report (JSON)

### Documentation

**README.md** (400+ lines)
- What it does (high-level)
- Setup instructions
- Architecture diagram
- Customization guide
- Why deterministic vs. LLM

**QUICKSTART.md** (250 lines)
- Step-by-step setup
- Local testing instructions
- First issue test walkthrough
- Troubleshooting section

**ARCHITECTURE.md** (450+ lines)
- Detailed component design
- Data flow diagrams
- Decision matrix (deterministic vs. LLM)
- Hallucination prevention strategies
- Security considerations

**LICENSE** (MIT)

## Total Lines of Code

| Component | Files | Approx. Lines |
|-----------|-------|---------------|
| Core C# Application | 16 | ~2,000 |
| Configuration (YAML) | 4 | ~350 |
| Playbooks (Markdown) | 3 | ~150 |
| Eval Scenarios (JSON) | 2 | ~100 |
| Eval Runner | 1 | ~250 |
| Documentation | 3 | ~1,100 |
| GitHub Workflow | 1 | ~40 |
| **Total** | **30** | **~4,000** |

## Key Technologies

- **Language**: C# .NET 8
- **Build**: dotnet CLI
- **Dependencies**:
  - `OpenAI` (2.1.0) - Official OpenAI .NET SDK
  - `YamlDotNet` (16.2.0) - YAML parsing
- **Platform**: GitHub Actions (ubuntu-latest)
- **APIs**:
  - GitHub REST API (native HTTP client)
  - OpenAI Chat Completions API (with structured outputs)

## How to Use This Manifest

1. **Building**: `dotnet build GitHubIssuesSupport.sln`
2. **Running**: See QUICKSTART.md for setup
3. **Testing**: `cd evals/EvalRunner && dotnet run`
4. **Customizing**: Edit .supportbot/*.yaml files
5. **Extending**: Add new components following existing patterns

## Development Workflow

```bash
# Clone/navigate to repo
cd github-issues-support

# Restore packages
dotnet restore

# Build
dotnet build

# Run locally (with test event)
dotnet run --project src/SupportConcierge/SupportConcierge.csproj test_event.json

# Run evals
cd evals/EvalRunner
dotnet run
```

## Deployment

1. Copy all files to your GitHub repository
2. Add `OPENAI_API_KEY` secret in repo settings
3. Customize `.supportbot/routing.yaml` with real usernames
4. Push to trigger workflow on next issue

---

**Complete, production-ready codebase** with comprehensive documentation, evaluation harness, and example configurations. No placeholder code - everything is fully implemented and functional.
