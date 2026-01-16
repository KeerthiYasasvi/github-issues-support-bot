# GitHub Issues Support Bot - Project Structure

## Overview

This directory contains the complete GitHub Issues Support Concierge Bot project with all documentation, code, configuration, and testing materials organized by purpose.

---

## Directory Organization

### 📚 Documentation Files (13 Markdown Files)

All markdown documentation is located in this root directory for easy access and organization:

```
Documentation/
├─ README.md                              Main project overview
├─ QUICKSTART.md                          5-minute setup guide
├─ MANIFEST.md                            Features and capabilities
├─ ANALYSIS_SUMMARY.md                    Executive summary
├─ ARCHITECTURE.md                        System design overview
├─ COMPLETE_SETUP_GUIDE.md                Detailed deployment guide
├─ SETUP_EXECUTION.md                     Historical setup information
├─ IMPLEMENTATION_ANALYSIS.md             Technical deep-dive
├─ OPENAI_INTEGRATION_REFERENCE.md        API integration guide
├─ TROUBLESHOOTING_GUIDE.md               Common issues and fixes
├─ VISUAL_DIAGRAMS.md                     Architecture diagrams
├─ DOCUMENTATION_INDEX.md                 Navigation guide (START HERE)
├─ CHALLENGES_AND_SOLUTIONS.md            Project challenges & lessons
├─ ANALYSIS_REPORT_FINAL.md               Final analysis report
└─ PROJECT_STRUCTURE.md                   This file
```

### 💻 Source Code (src/)

The .NET C# implementation of the bot:

```
src/SupportConcierge/
├─ Program.cs                             Application entry point
├─ SupportConcierge.csproj                Project configuration
│
├─ Agents/                                LLM Integration
│  ├─ OpenAiClient.cs                    OpenAI API client
│  ├─ Prompts.cs                         LLM prompt templates
│  └─ Schemas.cs                         JSON schema definitions
│
├─ GitHub/                                GitHub Integration
│  ├─ GitHubApi.cs                       GitHub REST API client
│  └─ Models.cs                          GitHub data models
│
├─ Orchestration/                         Core Business Logic
│  ├─ Orchestrator.cs                    Main workflow orchestration
│  └─ StateStore.cs                      Conversation state management
│
├─ Parsing/                               Data Extraction
│  └─ IssueFormParser.cs                 Issue form field parsing
│
├─ Scoring/                               Issue Evaluation
│  ├─ CompletenessScorer.cs              Completeness scoring
│  ├─ SecretRedactor.cs                  Secret detection & redaction
│  └─ Validators.cs                      Validation rules engine
│
├─ SpecPack/                              Configuration Loading
│  ├─ SpecPackLoader.cs                  Config file parser
│  └─ SpecModels.cs                      Config data models
│
├─ Reporting/                             Response Composition
│  └─ CommentComposer.cs                 GitHub comment formatter
│
└─ bin/ & obj/                            Build artifacts
```

### ⚙️ Configuration (.supportbot/)

Bot configuration files for categories, validation, and routing:

```
.supportbot/
├─ categories.yaml                        Issue category definitions
├─ checklists.yaml                        Field requirement definitions
├─ validators.yaml                        Validation rule definitions
├─ routing.yaml                           Issue routing rules
└─ playbooks/                             Response templates
   ├─ build.md
   ├─ runtime.md
   └─ docs.md
```

### 🔄 GitHub Integration (.github/)

GitHub Actions workflow configuration:

```
.github/
└─ workflows/
   └─ support-concierge.yml               CI/CD workflow definition
```

### 🧪 Evaluation & Testing (evals/)

Testing and evaluation tools:

```
evals/
├─ EvalRunner/                            Evaluation harness
│  ├─ Program.cs
│  └─ EvalRunner.csproj
│
└─ scenarios/                             Test scenarios
   ├─ sample_issue_build_missing_logs.json
   └─ sample_issue_runtime_crash.json
```

### 📦 Project Files

```
Root Level Files:
├─ github-issues-support.sln              Visual Studio solution
├─ GitHubIssuesSupport.sln                Alternative solution file
├─ LICENSE                                Project license
├─ .gitignore                             Git ignore rules
├─ .env                                   Environment configuration
└─ plan.md                                Project plan notes
```

---

## Documentation Strategy

### Purpose of Each Document

| Document | Purpose | Audience | Updates |
|----------|---------|----------|---------|
| **DOCUMENTATION_INDEX.md** | Navigation guide, start here | Everyone | Rarely |
| **CHALLENGES_AND_SOLUTIONS.md** | Technical challenges & lessons | Developers/Interviewees | As issues arise |
| **QUICKSTART.md** | Fast setup (5 min) | Operators | When procedures change |
| **COMPLETE_SETUP_GUIDE.md** | Detailed deployment | DevOps/Admins | When adding features |
| **ARCHITECTURE.md** | System design | Developers | When redesigning |
| **IMPLEMENTATION_ANALYSIS.md** | Technical details | Developers | When major changes occur |
| **OPENAI_INTEGRATION_REFERENCE.md** | API reference | ML Engineers | When SDK changes |
| **TROUBLESHOOTING_GUIDE.md** | Common issues | Support Team | As issues are discovered |
| **VISUAL_DIAGRAMS.md** | Architecture diagrams | Architects | When design changes |
| **ANALYSIS_SUMMARY.md** | Executive overview | Managers | Rarely |
| **README.md** | Project overview | Everyone | When scope changes |

---

## Key Features of Organization

### ✅ Single Location for Docs
All markdown documentation is in the root directory of this project, making it easy to:
- Find and reference documents
- Version control with git
- Maintain consistency
- Share with team members

### ✅ Clear Purpose
Each file has a specific purpose and audience:
- Quick setup vs. detailed setup vs. reference
- Different levels of technical depth
- Interview preparation materials

### ✅ Navigation Made Easy
- **DOCUMENTATION_INDEX.md** provides guided navigation
- Documents link to each other
- Quick reference tables included

### ✅ Organized Code Structure
The `src/` directory organizes code by responsibility:
- Integration layers (GitHub, OpenAI)
- Business logic (Orchestration)
- Data processing (Parsing, Scoring)
- Configuration (SpecPack)

### ✅ Interview Ready
- **CHALLENGES_AND_SOLUTIONS.md** specifically designed for interview Q&A
- Demonstrates problem-solving approach
- Shows debugging methodology
- Documents lessons learned

---

## Project Statistics

### Documentation
- **13 markdown files** covering all aspects
- **~63+ pages** of comprehensive documentation
- **~3 hours** total reading time
- **Q&A format** for technical challenges

### Code
- **~1000+ lines** of C# production code
- **8 modules** with clear responsibilities
- **60+ configuration options** via YAML
- **Comprehensive error handling**

### Testing
- **Sample scenarios** for evaluation
- **Harness tool** for validation
- **Real-world test cases** included

---

## Getting Started

### First Time?
1. Read [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)
2. Choose your role/path
3. Follow the recommended reading order

### Quick Setup?
1. Read [QUICKSTART.md](QUICKSTART.md)
2. Follow the 5-minute steps
3. Test and deploy

### Technical Questions?
1. Check [CHALLENGES_AND_SOLUTIONS.md](CHALLENGES_AND_SOLUTIONS.md) for similar issues
2. Review [TROUBLESHOOTING_GUIDE.md](TROUBLESHOOTING_GUIDE.md)
3. Check [OPENAI_INTEGRATION_REFERENCE.md](OPENAI_INTEGRATION_REFERENCE.md) for API details

### Interview Preparation?
1. Read [CHALLENGES_AND_SOLUTIONS.md](CHALLENGES_AND_SOLUTIONS.md)
2. Study the Q&A format
3. Review the debugging approach demonstrated

---

## Integration Point

**Important Note:** This documentation directory will be maintained here. When the bot code is integrated into other projects (like Reddit-ELT-Pipeline), these markdown files remain the source of truth in this directory.

The bot code (`src/` directory) will continue to exist in both places after integration, but all documentation stays here for easy reference and maintenance.

---

## Maintenance Schedule

- **Documentation:** Updated as needed when issues/features are added
- **CHALLENGES_AND_SOLUTIONS.md:** Updated after each significant debugging session
- **Code:** Updated with each feature/fix
- **Configuration:** Updated when new categories/rules are needed

---

*Last Updated: January 12, 2026*
