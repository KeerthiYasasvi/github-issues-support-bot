# Quick Start Guide

## Prerequisites

1. **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **OpenAI API Key** - Get from [OpenAI Platform](https://platform.openai.com/api-keys)
3. **GitHub Repository** with Actions enabled

## Installation

### Option 1: In Your Repository

1. **Copy the bot to your repo:**
   ```bash
   # Clone or copy all files to your repository
   git clone <this-repo-url>
   cd github-issues-support
   ```

2. **Verify the structure:**
   ```
   your-repo/
   ├── .github/workflows/support-concierge.yml
   ├── src/SupportConcierge/...
   ├── .supportbot/...
   └── README.md
   ```

3. **Configure GitHub Secrets:**
   - Go to your repo: **Settings → Secrets and variables → Actions**
   - Click **New repository secret**
   - Name: `OPENAI_API_KEY`
   - Value: Your OpenAI API key
   - Click **Add secret**

4. **Update routing.yaml:**
   ```bash
   # Edit .supportbot/routing.yaml
   # Replace @repo-owner with your actual GitHub username
   ```

5. **Commit and push:**
   ```bash
   git add .
   git commit -m "Add support concierge bot"
   git push
   ```

### Option 2: Test Locally First

1. **Build the project:**
   ```bash
   dotnet restore
   dotnet build src/SupportConcierge/SupportConcierge.csproj
   ```

2. **Create a test event file:**
   ```powershell
   @"
   {
     "action": "opened",
     "issue": {
       "number": 1,
       "title": "Build error with npm",
       "body": "When I run npm build, I get: Error: Cannot find module 'webpack'",
       "user": {"login": "testuser", "type": "User"},
       "state": "open",
       "labels": [],
       "assignees": [],
       "html_url": "https://github.com/test/repo/issues/1"
     },
     "repository": {
       "name": "repo",
       "owner": {"login": "test"},
       "full_name": "test/repo",
       "default_branch": "main"
     }
   }
   "@ | Out-File -Encoding utf8 test_event.json
   ```

3. **Set environment variables:**
   ```powershell
   $env:GITHUB_TOKEN = "your_github_token_here"
   $env:OPENAI_API_KEY = "your_openai_key_here"
   $env:GITHUB_EVENT_NAME = "issues"
   $env:SUPPORTBOT_SPEC_DIR = ".supportbot"
   ```

4. **Run the bot:**
   ```bash
   dotnet run --project src/SupportConcierge/SupportConcierge.csproj test_event.json
   ```

5. **Expected output:**
   ```
   === GitHub Issues Support Concierge Bot ===
   Started at: 2026-01-12T...
   Loading event from: test_event.json
   Event type: issues
   Issue #1: Build error with npm
   Repository: test/repo
   Loading SpecPack configuration...
   Determined category: build
   Extracting fields from issue...
   Completeness score: 45/75
   Asking follow-up questions...
   ```

## Run Evaluations

Test the bot with sample scenarios:

```bash
cd evals/EvalRunner
$env:OPENAI_API_KEY = "your_key"
dotnet run
```

Output:
```
=== Support Concierge Evaluation Runner ===

Found 2 test scenarios

--- Running: sample_issue_build_missing_logs ---
✓ Category: build
✓ Score: 85
✓ Actionable: true
✓ Extracted Fields: 7

--- Running: sample_issue_runtime_crash ---
✓ Category: runtime
✓ Score: 55
✓ Actionable: false
✓ Extracted Fields: 4

=== Evaluation Report ===

Scenarios Run: 2
Successful: 2/2 (100.0%)

Metrics:
  Average Completeness Score: 70.0
  Average Fields Extracted: 5.5
  Actionable Rate: 50.0%
```

## First Issue Test

After setup, create a test issue in your repository:

**Title:** "Installation fails on Ubuntu"

**Body:**
```
I'm trying to install the software on Ubuntu but getting an error.

Error: Permission denied when running ./install.sh

I'm using Ubuntu 20.04.
```

The bot should:
1. ✅ Detect category: `setup`
2. ✅ Extract: OS (Ubuntu 20.04), error message
3. ✅ Identify missing: version, installation method, steps
4. ✅ Post follow-up questions asking for the missing info

## Customization Checklist

- [ ] Update `.supportbot/routing.yaml` with real GitHub usernames
- [ ] Customize `.supportbot/categories.yaml` for your project's issue types
- [ ] Adjust completeness thresholds in `.supportbot/checklists.yaml`
- [ ] Add repo-specific playbooks in `.supportbot/playbooks/`
- [ ] Test with sample issues
- [ ] Add eval scenarios for your common issue patterns

## Troubleshooting

### Bot doesn't respond to issues
- Check GitHub Actions logs: **Actions → Latest workflow run**
- Verify `OPENAI_API_KEY` is set correctly
- Ensure workflow has `issues: write` permission

### Bot posts but doesn't extract fields correctly
- Check the issue form structure (use `### Heading` format)
- Review extraction logs in Actions
- Run eval harness to test extraction

### Completeness scores too low/high
- Adjust weights in `.supportbot/checklists.yaml`
- Lower/raise `completeness_threshold`
- Add more field aliases if using different terminology

### Questions are generic
- Improve field descriptions in checklists
- Customize prompts in `src/SupportConcierge/Agents/Prompts.cs`
- Add more context to playbooks

## Next Steps

1. **Monitor first 10 issues** - Watch how the bot performs
2. **Iterate on Spec Packs** - Refine categories, fields, routing based on real usage
3. **Add eval scenarios** - Create test cases for your common issue patterns
4. **Customize playbooks** - Add repo-specific troubleshooting guidance
5. **Train maintainers** - Share the "Engineer Brief" format expectations

## Support

For issues or questions about this bot:
1. Check the [README](README.md) for detailed documentation
2. Review the [Architecture section](README.md#architecture) for system design
3. Run the eval harness to validate behavior
4. File an issue with logs from GitHub Actions

---

Happy triaging! 🚀
