# GitHub Issues Support Concierge - Troubleshooting Guide

## Quick Diagnosis

### Step 1: Check GitHub Actions Execution

Go to your repository → **Actions** tab and look for "Support Concierge Bot" workflow.

**Status Indicators:**
- 🟢 **Green checkmark**: Workflow completed successfully
- 🔴 **Red X**: Workflow failed
- 🟡 **Yellow circle**: Workflow still running
- ⚪ **Gray**: Not yet triggered

### Step 2: View Detailed Logs

Click on the failed/successful run → Expand "Run Support Concierge" step to see full logs.

---

## Common Issues & Fixes

### Issue 1: "ERROR: OPENAI_API_KEY not set"

**Where to find this error:** In the "Run Support Concierge" step output

**Root cause:** The GitHub Actions secret is not configured

**Fix:**
1. Go to Repository → Settings
2. Click "Secrets and variables" → "Actions"
3. Click "New repository secret"
4. Name: `OPENAI_API_KEY`
5. Value: Your OpenAI API key (from https://platform.openai.com/api-keys)
6. Click "Add secret"

**Verification:** Re-run the workflow after adding the secret

---

### Issue 2: "ERROR: GITHUB_EVENT_PATH not found"

**Root cause:** The GitHub event context is not being passed correctly

**This is rare** and usually indicates an environmental/permissions issue

**Fix:**
1. Check that your repository has Issues enabled
2. Check workflow permissions in Settings → Actions → General
3. Ensure "Read and write permissions" is selected

---

### Issue 3: Workflow Doesn't Trigger on New Issue

**Root cause:** The workflow trigger might not be set correctly

**Check:**
1. Go to `.github/workflows/support-concierge.yml`
2. Verify the top section has:
   ```yaml
   on:
     issues:
       types: [opened, edited]
     issue_comment:
       types: [created]
   ```
3. If not, update and commit the file

**Verification:** Create a new issue after committing

---

### Issue 4: "Model not found" or "Invalid model" Error

**Root cause:** OpenAI API key doesn't have access to the specified model

**Fix - Option A: Use correct model name**
1. Go to Repository → Settings → Secrets and variables → Actions
2. Click on variable `OPENAI_MODEL`
3. Ensure value is one of:
   - `gpt-4o-2024-08-06` (recommended)
   - `gpt-4-turbo`
   - `gpt-4`
   - `gpt-3.5-turbo`
4. Save changes

**Fix - Option B: Check OpenAI API Key**
1. Go to https://platform.openai.com/api-keys
2. Verify your API key has access to the model
3. Check that you have sufficient API credits
4. If using Azure OpenAI, update the endpoint configuration

---

### Issue 5: Bot Doesn't Comment on Issues

**Symptoms:** Workflow runs successfully but no comment appears on the issue

**Possible causes:**
1. `.supportbot/` configuration directory missing
2. No categories defined in configuration
3. Issue doesn't match any validation rules
4. Bot comment already exists from previous runs

**Fix:**
1. Verify `.supportbot/` directory exists in repository root
2. Check that `.supportbot/config.yml` has valid YAML syntax
3. Verify categories are properly defined
4. Delete old bot comments if testing
5. Create a new issue to trigger fresh processing

---

### Issue 6: Build Step Fails with ".csproj not found"

**Root cause:** Project file path is incorrect

**Fix:**
1. Verify project exists: `src/SupportConcierge/SupportConcierge.csproj`
2. If file name is different, update workflow:
   ```yaml
   - name: Build
     run: dotnet build src/SupportConcierge/YOUR_PROJECT_NAME.csproj --configuration Release --no-restore
   ```
3. Commit and re-run

---

### Issue 7: Timeout or Hanging Workflow

**Symptoms:** Workflow runs for several minutes without completing

**Possible causes:**
1. OpenAI API is slow or unresponsive
2. Rate limiting is causing delays
3. Bot is stuck waiting for GitHub API response

**Fix:**
1. Check OpenAI status page: https://status.openai.com/
2. Wait a few minutes and re-run workflow
3. If persistent, add timeout to workflow:
   ```yaml
   - name: Run Support Concierge
     timeout-minutes: 10
     env:
       ...
   ```

---

## Configuration Verification Checklist

### 1. Repository Structure

```bash
✓ .github/workflows/support-concierge.yml exists
✓ src/SupportConcierge/Program.cs exists
✓ src/SupportConcierge/SupportConcierge.csproj exists
✓ .supportbot/ directory exists with configuration files
✓ README.md or QUICKSTART.md provides instructions
```

### 2. GitHub Secrets & Variables

```bash
✓ Secret: OPENAI_API_KEY is set
✓ Variable: OPENAI_MODEL is set (or uses default)
✓ Repository has at least "Read and write" permissions for Actions
```

### 3. Workflow File

```bash
✓ Workflow triggers on: issues (opened, edited)
✓ Workflow triggers on: issue_comment (created)
✓ Environment variables properly referenced
✓ .NET version is 8.0.x or compatible
✓ Project path is correct
```

### 4. Issue Configuration

```bash
✓ Repository has Issues enabled
✓ Issue templates are defined (if required)
✓ Categories match between configuration and code
```

---

## Testing Workflow Locally

### Prerequisites

```bash
git clone <repository-url>
cd github-issues-support
export GITHUB_TOKEN=ghp_... # Your GitHub PAT
export OPENAI_API_KEY=sk-proj-... # Your OpenAI API key
```

### Create Test Event File

Save as `test-event.json`:

```json
{
  "action": "opened",
  "issue": {
    "number": 1,
    "title": "Test Issue",
    "body": "This is a test issue for the bot."
  },
  "repository": {
    "name": "github-issues-support",
    "full_name": "YourUsername/github-issues-support",
    "owner": {
      "login": "YourUsername"
    }
  }
}
```

### Run Locally

```bash
export GITHUB_EVENT_PATH=$(pwd)/test-event.json
export GITHUB_EVENT_NAME=issues
dotnet run --project src/SupportConcierge/SupportConcierge.csproj
```

### Expected Output

```
=== GitHub Issues Support Concierge Bot ===
Started at: 2024-01-20 10:30:45Z
Loading event from: /path/to/test-event.json
Event type: issues
Issue #1: Test Issue
Processing event: issues
...
Completed at: 2024-01-20 10:30:50Z
```

---

## Monitoring & Analytics

### Check Workflow Usage

Repository → Settings → Actions → General → Usage

Track:
- Number of workflow runs
- Success/failure rates
- Average execution time

### Monitor API Costs

OpenAI Dashboard → Billing:
- Track daily API costs
- Set spending limits
- Monitor token usage

### GitHub Actions Usage

Repository → Settings → Actions → General:
- Check if on free tier or paid plan
- Monitor minutes used
- Check concurrent job limits

---

## Telemetry & Monitoring

The bot emits lightweight telemetry markers in logs to aid diagnostics:

- `[TELEMETRY]` prefixed lines for notable events (e.g., model used, schema mode selected)
- `[SCHEMA_VIOLATION]` for structured-output validation errors (with brief reason)

Where to see them:
- In GitHub Actions, open a run and search the logs for `TELEMETRY` or `SCHEMA_VIOLATION`.
- Locally, they are printed to standard output/error during `dotnet run`.

Suggested checks:
- Count of `SCHEMA_VIOLATION` across runs trending downwards
- Model source logs show expected `OPENAI_MODEL` in use
- Presence of telemetry around classification/extraction steps in complex issues

---

## Getting Help

### Useful Resources

1. **Support Concierge Documentation**
   - `QUICKSTART.md` - Quick setup guide
   - `MANIFEST.md` - Feature documentation
   - `ARCHITECTURE.md` - Technical architecture

2. **OpenAI Documentation**
   - https://platform.openai.com/docs
   - https://github.com/openai/openai-dotnet

3. **GitHub Actions Documentation**
   - https://docs.github.com/en/actions
   - https://docs.github.com/en/actions/using-jobs/handling-concurrency

4. **GitHub Issues REST API**
   - https://docs.github.com/en/rest/issues/issues

### Enable Debug Mode

Add this to your workflow to get verbose logging:

```yaml
- name: Run Support Concierge
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
    OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
    OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4o-2024-08-06' }}
    DEBUG: "1"
  run: dotnet run --project src/SupportConcierge/SupportConcierge.csproj --configuration Release --no-build
```

### Report Issues

If you encounter problems:

1. Collect logs from GitHub Actions
2. Document the exact error message
3. Include your environment details (OS, .NET version, etc.)
4. Create an issue with reproduction steps

---

## Quick Command Reference

### Test Locally

```bash
# Build project
dotnet build src/SupportConcierge/SupportConcierge.csproj

# Run with test event
export GITHUB_EVENT_PATH=test-event.json
export GITHUB_EVENT_NAME=issues
dotnet run --project src/SupportConcierge/SupportConcierge.csproj
```

### Verify Configuration

```bash
# Check .supportbot directory
ls -la .supportbot/

# Validate YAML syntax
cat .supportbot/config.yml

# Check workflow file
cat .github/workflows/support-concierge.yml
```

### View GitHub Actions Status

```bash
# List recent workflow runs
gh run list --workflow support-concierge.yml

# View specific run
gh run view <run-id> --log

# View logs for failed step
gh run view <run-id> --log --jq '.[]'
```

---

## Success Criteria

Your setup is working correctly when:

✅ Workflow runs trigger on new issues
✅ No red "X" marks in Actions tab
✅ Bot successfully classifies issues
✅ Bot comments with analysis/questions
✅ No error messages in workflow logs
✅ Issues transition through completion states

---

## Rapid Restart Procedure

If workflow is completely broken, follow this to restart from scratch:

1. **Delete repository secrets/variables** (Settings → Secrets and variables)
2. **Re-add OPENAI_API_KEY secret** with fresh API key
3. **Add OPENAI_MODEL variable** (value: `gpt-4o-2024-08-06`)
4. **Test trigger** - Create a new GitHub issue
5. **Monitor Actions tab** for execution and logs

**Time estimate:** 5-10 minutes

