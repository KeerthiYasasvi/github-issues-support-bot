# Visual Architecture & Data Flow Diagrams

## 1. High-Level System Architecture

```
┌────────────────────────────────────────────────────────────┐
│                    GitHub Repository                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Issues                                              │  │
│  │  ├─ Issue #1: "App crashes on startup"              │  │
│  │  ├─ Issue #2: "Add dark mode"                       │  │
│  │  └─ Issue #3: "Update documentation"               │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  GitHub Actions Workflow (.github/workflows/)         │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │ support-concierge.yml                          │  │  │
│  │  │ Triggers on: issues [opened, edited]           │  │  │
│  │  │             issue_comment [created]            │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────┬─────────────────────────────────────┘
                         │ (runs)
                         ▼
        ┌────────────────────────────────┐
        │ GitHub Actions Runner          │
        │ Environment: ubuntu-latest     │
        │ .NET Version: 8.0              │
        └────────────┬───────────────────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
    ▼                ▼                ▼
┌─────────┐   ┌──────────────┐  ┌──────────────┐
│ Checkout│   │Setup .NET 8  │  │Build Project │
│Repo     │   │              │  │              │
└─────────┘   └──────────────┘  └──────────────┘
    │                │                │
    └────────────────┼────────────────┘
                     ▼
        ┌────────────────────────────────┐
        │ Run SupportConcierge.exe        │
        │ Environment Variables:         │
        │ - GITHUB_TOKEN                 │
        │ - OPENAI_API_KEY               │
        │ - OPENAI_MODEL                 │
        └────────────┬───────────────────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
    ▼                ▼                ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ OpenAI API   │  │ GitHub API   │  │ Local Config │
│ (GPT-4o)     │  │ (REST)       │  │ (.supportbot)│
└──────────────┘  └──────────────┘  └──────────────┘
```

## 2. Issue Processing Workflow

```
Issue Event
(opened/edited/commented)
│
▼
┌─────────────────────────────┐
│ Load Event Payload          │
│ - Issue title & body        │
│ - Issue number              │
│ - Repository info           │
└──────────────┬──────────────┘
               │
               ▼
        ┌──────────────┐
        │ Is Relevant? │
        │ (not bot)    │
        └──────┬───────┘
               │ NO
               ├──────────────────────►SKIP
               │
               │ YES
               ▼
┌─────────────────────────────┐
│ Load Configuration          │
│ - Categories                │
│ - Checklists                │
│ - Validators                │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ Extract Previous State      │
│ - From bot comments         │
│ - Loop count                │
│ - Current category          │
└──────────────┬──────────────┘
               │
         ┌─────┴──────┐
         │            │
         ▼            ▼
    Has State?    New Issue?
     (YES)         (NO)
    REUSE CAT.   CLASSIFY
         │            │
         └──────┬─────┘
                ▼
   ┌────────────────────────────┐
   │ OpenAI: Classify Category  │
   │ Input: Title + Body        │
   │ Output: Category + Score   │
   └──────────────┬─────────────┘
                  │
                  ▼
   ┌────────────────────────────┐
   │ Get Checklist for Category │
   └──────────────┬─────────────┘
                  │
                  ▼
   ┌────────────────────────────┐
   │ OpenAI: Extract Fields     │
   │ Input: Issue body + fields │
   │ Output: Field values       │
   └──────────────┬─────────────┘
                  │
                  ▼
   ┌────────────────────────────┐
   │ OpenAI: Score Completeness │
   │ Input: Fields + Checklist  │
   │ Output: Score + Questions  │
   └──────────────┬─────────────┘
                  │
            ┌─────┴─────────────┐
            │                   │
            ▼                   ▼
      Actionable?          Max Loops?
         (YES)               (YES)
            │                   │
            │                   ▼
            │          ┌──────────────────────┐
            │          │ ESCALATE ISSUE      │
            │          │ - Mark for engineers│
            │          │ - Post comment      │
            │          └─────────┬───────────┘
            │                    │
            ▼                    ▼
   ┌──────────────────────┐   DONE
   │ ASK FOLLOW-UP QUEST. │
   │ - Post comment       │
   │ - Update state       │
   │ - Track for next run │
   └─────────┬────────────┘
             │
             ▼
           DONE
```

## 3. Data Flow: Issue to Classification

```
GitHub Issue Data
├─ title: "App crashes on startup"
├─ body: "Using version 2.1.0 on Windows 11, get exception..."
├─ user: "john_doe"
└─ number: 42
     │
     ▼
  ┌─────────────────────────────────────┐
  │ OpenAiClient.ClassifyCategoryAsync()│
  └──────────────────┬──────────────────┘
                     │
         ┌───────────┴───────────┐
         │                       │
    (API KEY)             (MODEL NAME)
         │                       │
         ▼                       ▼
    OPENAI_API_KEY         OPENAI_MODEL
    sk-proj-...            gpt-4o-2024-08-06
         │                       │
         └───────────┬───────────┘
                     │
                     ▼
            ┌──────────────────┐
            │ Create ChatClient│
            └────────┬─────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │ Build Messages         │
        ├─ System: Instructions  │
        └─ User: Issue data      │
                     │
                     ▼
    ┌────────────────────────────────┐
    │ OpenAI API Request             │
    │ POST /chat/completions         │
    │ - Model: gpt-4o-2024-08-06     │
    │ - Messages: [system, user]     │
    │ - Response format: JSON schema │
    │ - Schema: strict validation    │
    └────────────────┬───────────────┘
                     │
                     ▼ (HTTP Response)
    ┌────────────────────────────────┐
    │ Parse Response                 │
    ├─ category: "Bug"               │
    ├─ score: 0.92                   │
    └─ reasoning: "..."              │
                     │
                     ▼
    ┌────────────────────────────────┐
    │ Return CategoryClassificationResult
    │ {                              │
    │   Category = "Bug",            │
    │   Score = 92,                  │
    │   Reasoning = "..."            │
    │ }                              │
    └────────────────────────────────┘
```

## 4. State Tracking Across Interactions

```
First Interaction
│
├─ Bot receives: Issue created
├─ Actions:
│  ├─ Classifies: "Bug"
│  ├─ Scores: 40% complete
│  └─ Posts: Asks 3 follow-up questions
│
└─ Stores State in comment:
   ┌─────────────────────────┐
   │ <!-- SUPPORTBOT_STATE   │
   │ LOOP_COUNT: 1           │
   │ CATEGORY: Bug           │
   │ STATUS: INCOMPLETE      │
   │ SUPPORTBOT_STATE -->    │
   └─────────────────────────┘

        │
        │ (User responds)
        │
        ▼

Second Interaction
│
├─ Bot receives: Comment added
├─ Bot reads: Previous state
│  ├─ Loop count: 1
│  ├─ Category: Bug (reuse)
│  └─ Status: INCOMPLETE
│
├─ Actions:
│  ├─ Extracts: New response data
│  ├─ Scores: 65% complete
│  ├─ Status: STILL INCOMPLETE
│  └─ Posts: 2 more follow-up questions
│
└─ Stores: Updated state
   ┌─────────────────────────┐
   │ <!-- SUPPORTBOT_STATE   │
   │ LOOP_COUNT: 2           │
   │ CATEGORY: Bug           │
   │ STATUS: INCOMPLETE      │
   │ SUPPORTBOT_STATE -->    │
   └─────────────────────────┘

        │
        │ (User responds again)
        │
        ▼

Third Interaction
│
├─ Bot receives: Comment added
├─ Bot reads: Previous state
│  ├─ Loop count: 2
│  ├─ Category: Bug (reuse)
│  └─ Status: INCOMPLETE
│
├─ Decision:
│  ├─ Completeness: 85%
│  ├─ Loop count: 2 (max is 3)
│  └─ ACTIONABLE? YES!
│
├─ Actions:
│  ├─ Generates: Engineer brief
│  ├─ Classifies: READY FOR TRIAGE
│  ├─ Assigns: Component label
│  └─ Posts: Final analysis comment
│
└─ Stores: Final state
   ┌──────────────────────────┐
   │ <!-- SUPPORTBOT_STATE    │
   │ LOOP_COUNT: 3            │
   │ CATEGORY: Bug            │
   │ STATUS: ACTIONABLE       │
   │ ASSIGNED_TO: backend     │
   │ SUPPORTBOT_STATE -->     │
   └──────────────────────────┘
```

## 5. OpenAI Request/Response Flow

```
┌─────────────────────┐
│ Build Request       │
├─────────────────────┤
│ Method: POST        │
│ URL: /chat/         │
│      completions    │
│ Auth: Bearer token  │
│       (API key)     │
│ Content-Type: JSON  │
└──────────┬──────────┘
           │
           ▼
┌────────────────────────────────────┐
│ Request Body                       │
├────────────────────────────────────┤
│ {                                  │
│   "model": "gpt-4o-2024-08-06",   │
│   "messages": [                    │
│     {                              │
│       "role": "system",            │
│       "content": "You are a..."    │
│     },                             │
│     {                              │
│       "role": "user",              │
│       "content": "Issue: ..."      │
│     }                              │
│   ],                               │
│   "response_format": {             │
│     "type": "json_schema",         │
│     "json_schema": {               │
│       "name": "classification",    │
│       "schema": {...}              │
│     }                              │
│   }                                │
│ }                                  │
└──────────┬───────────────────────┘
           │
           │ HTTPS POST
           │
          ▼ ▼ ▼
        ┌────────────┐
        │ OpenAI    │
        │ API Server│
        │ (Cloud)  │
        └────────┬───┘
                 │ (Processing)
                 │ - Tokenize
                 │ - Process
                 │ - Generate
                 │ - Validate
                 ▼
┌────────────────────────────────────┐
│ Response Body                      │
├────────────────────────────────────┤
│ {                                  │
│   "id": "chatcmpl-...",           │
│   "choices": [                     │
│     {                              │
│       "message": {                 │
│         "role": "assistant",       │
│         "content": "{              │
│           \"category\": \"Bug\",   │
│           \"score\": 92,           │
│           \"reasoning\": \"...\"   │
│         }"                         │
│       },                           │
│       "finish_reason": "stop"      │
│     }                              │
│   ],                               │
│   "usage": {                       │
│     "prompt_tokens": 145,          │
│     "completion_tokens": 45,       │
│     "total_tokens": 190            │
│   }                                │
│ }                                  │
└──────────┬───────────────────────┘
           │
           ▼
┌─────────────────────┐
│ Parse Response      │
├─────────────────────┤
│ 1. Check status 200 │
│ 2. Extract content  │
│ 3. Parse JSON       │
│ 4. Validate schema  │
│ 5. Return object    │
└──────────┬──────────┘
           │
           ▼
┌────────────────────────────┐
│ CategoryClassificationResult
│ {                          │
│   Category = "Bug",        │
│   Score = 92,              │
│   Reasoning = "..."        │
│ }                          │
└────────────────────────────┘
```

## 6. Environment Variables Flow

```
┌───────────────────────────────────┐
│ GitHub Actions Secrets & Variables│
├───────────────────────────────────┤
│ SECRETS:                          │
│  └─ OPENAI_API_KEY: sk-proj-...   │
│                                   │
│ VARIABLES:                        │
│  ├─ OPENAI_MODEL: gpt-4o-...      │
│  ├─ SUPPORTBOT_SPEC_DIR: .supportbot
│  └─ SUPPORTBOT_BOT_USERNAME: ...  │
└──────────────┬────────────────────┘
               │
               ▼
   ┌──────────────────────────┐
   │ Workflow File (.yml)     │
   │ env:                     │
   │   GITHUB_TOKEN: secrets  │
   │   OPENAI_API_KEY: secret │
   │   OPENAI_MODEL: var      │
   └──────────────┬───────────┘
                  │
                  ▼
   ┌──────────────────────────┐
   │ GitHub Actions Runner    │
   │ Sets environment vars    │
   └──────────────┬───────────┘
                  │
                  ▼
   ┌──────────────────────────┐
   │ Execute Program.cs       │
   │ Access via:              │
   │ Environment.              │
   │ GetEnvironmentVariable()  │
   └──────────────┬───────────┘
                  │
    ┌─────────────┼─────────────┐
    │             │             │
    ▼             ▼             ▼
  GITHUB_TOKEN  OPENAI_API_KEY  OPENAI_MODEL
    │             │             │
    ▼             ▼             ▼
 GitHub API   OpenAI API    Model Selection
 Access       Authentication  gpt-4o-2024-08-06
```

## 7. Configuration File Structure

```
Repository Root
│
├─ .supportbot/
│  │
│  └─ config.yml
│     ├─ Categories
│     │  ├─ name: "Bug"
│     │  │  ├─ description: "..."
│     │  │  └─ keywords: ["crash", "error"]
│     │  ├─ name: "Feature"
│     │  │  ├─ description: "..."
│     │  │  └─ keywords: ["add", "feature"]
│     │  └─ name: "Documentation"
│     │     └─ ...
│     │
│     ├─ Checklists (per category)
│     │  ├─ Bug:
│     │  │  ├─ "Environment details"
│     │  │  ├─ "Steps to reproduce"
│     │  │  ├─ "Expected vs actual"
│     │  │  └─ "Error message"
│     │  └─ Feature:
│     │     ├─ "Use case"
│     │     ├─ "Benefits"
│     │     └─ "Implementation notes"
│     │
│     ├─ Validators
│     │  ├─ minTitleLength: 10
│     │  ├─ minBodyLength: 50
│     │  └─ requiredFields: [...]
│     │
│     └─ Patterns
│        ├─ secretPatterns:
│        │  ├─ "api[_-]?key"
│        │  ├─ "password"
│        │  └─ "token"
│        └─ ...
│
├─ .github/workflows/
│  └─ support-concierge.yml
│     ├─ Triggers
│     ├─ Env vars
│     └─ Steps
│
└─ src/SupportConcierge/
   ├─ Program.cs (reads env vars)
   ├─ Orchestrator.cs (main logic)
   ├─ OpenAiClient.cs (LLM calls)
   ├─ GitHubApi.cs (GitHub API)
   └─ ... other modules
```

---

These diagrams provide visual reference for understanding:
- System architecture and components
- Issue processing workflow
- Data transformations
- State management
- API interactions
- Configuration structure

