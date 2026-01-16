# Complete Setup & Deployment Guide

## Pre-Deployment Checklist

### Section 1: OpenAI Setup

**Objective:** Get OpenAI API credentials ready

#### Step 1.1: Create/Access OpenAI Account
- [ ] Go to https://platform.openai.com
- [ ] Sign in or create account
- [ ] Complete email verification
- [ ] Set up billing (credit card required)
- [ ] Note: Free trial credits may be limited

#### Step 1.2: Generate API Key
- [ ] Go to https://platform.openai.com/api-keys
- [ ] Click "Create new secret key"
- [ ] Choose organization (if applicable)
- [ ] Copy the key (you'll need this)
- [ ] Important: Save this securely - you won't be able to see it again!
- [ ] Verify the key starts with `sk-`

#### Step 1.3: Verify API Access
- [ ] Go to https://platform.openai.com/account/usage/overview
- [ ] Check that models are available
- [ ] Verify you have credit/quota available
- [ ] Recommended models: `gpt-4o-2024-08-06` or `gpt-4-turbo`

**Save for later:** Your OpenAI API Key (`sk-...`)

---

### Section 2: Repository Setup

**Objective:** Prepare GitHub repository with Support Concierge code

#### Step 2.1: Create GitHub Repository
- [ ] Go to https://github.com/new
- [ ] Repository name: `github-issues-support`
- [ ] Description: "Support Concierge Bot for issue triage"
- [ ] Choose: Public or Private
- [ ] ✓ Add README
- [ ] Initialize with main branch
- [ ] Create repository

#### Step 2.2: Clone Repository Locally
```bash
git clone https://github.com/YOUR_USERNAME/github-issues-support.git
cd github-issues-support
```

#### Step 2.3: Add Project Files
Copy this project's files to your repository:
```
- src/
- evals/
- .github/workflows/
- .supportbot/
- QUICKSTART.md
- MANIFEST.md
- ARCHITECTURE.md
- README.md
- LICENSE
```

#### Step 2.4: Commit and Push
```bash
git add .
git commit -m "Initial commit: Add Support Concierge Bot"
git push origin main
```

**Verification:** Go to GitHub and verify files appear in repository

---

### Section 3: GitHub Configuration

**Objective:** Configure GitHub Actions environment

#### Step 3.1: Configure Repository Settings

Navigate to: **Repository → Settings → General**

- [ ] Ensure "Issues" checkbox is ✓ checked
- [ ] Branch protection rules (optional): Configure for `main` branch
- [ ] Actions → General:
  - [ ] "Allow all actions and reusable workflows" ✓
  - [ ] Actions permissions: ✓ "Read and write permissions"
  - [ ] Save

#### Step 3.2: Add OpenAI API Key Secret

Navigate to: **Settings → Secrets and variables → Actions**

**Add Secret:**
- [ ] Click "New repository secret"
- [ ] Name: `OPENAI_API_KEY`
- [ ] Secret: (Paste your OpenAI API key from Section 1.2)
- [ ] Click "Add secret"

**Verification:** Secret appears in list (value is hidden with ●●●●●●)

#### Step 3.3: Add Model Configuration (Optional but Recommended)

Navigate to: **Settings → Secrets and variables → Actions → Variables**

**Add Variable:**
- [ ] Click "New repository variable"
- [ ] Name: `OPENAI_MODEL`
- [ ] Value: `gpt-4o-2024-08-06` (or your preferred model)
- [ ] Click "Add variable"

**Alternative models:**
- `gpt-4-turbo` (Latest GPT-4 variant)
- `gpt-4` (Standard GPT-4)
- `gpt-3.5-turbo` (Faster, cheaper)

#### Step 3.4: Configure Spec Directory (Optional)

Navigate to: **Settings → Secrets and variables → Actions → Variables**

**Add Variable:**
- [ ] Click "New repository variable"
- [ ] Name: `SUPPORTBOT_SPEC_DIR`
- [ ] Value: `.supportbot` (default location)
- [ ] Click "Add variable"

---

### Section 4: Workflow Verification

**Objective:** Ensure workflow file is correct

#### Step 4.1: Check Workflow File

Navigate to: **.github/workflows/support-concierge.yml**

Verify it contains:
```yaml
on:
  issues:
    types: [opened, edited]
  issue_comment:
    types: [created]

env:
  GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
  OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4o-2024-08-06' }}
```

- [ ] Triggers are configured correctly
- [ ] Secret references use `${{ secrets.OPENAI_API_KEY }}`
- [ ] Variable references use `${{ vars.OPENAI_MODEL }}`

#### Step 4.2: Check Spec Pack Configuration

Navigate to: **.supportbot/config.yml**

Verify it contains valid YAML with:
- [ ] Categories defined
- [ ] Checklists configured
- [ ] Validators specified
- [ ] No syntax errors

---

## First Run (Post-Deployment)

### Step 1: Trigger Workflow

#### Option A: Create Test Issue (Recommended)
1. Go to your repository
2. Click "Issues" tab
3. Click "New Issue"
4. Title: "Test Issue for Support Concierge"
5. Body: 
   ```
   This is a test issue to verify the Support Concierge Bot is working.
   
   - Does the bot comment?
   - Is the classification correct?
   - Does it ask follow-up questions?
   ```
6. Click "Submit new issue"

#### Option B: Trigger via API
```bash
curl -X POST \
  -H "Authorization: token YOUR_GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  https://api.github.com/repos/YOUR_USERNAME/github-issues-support/issues \
  -d '{"title":"Test Issue","body":"Testing bot"}'
```

### Step 2: Monitor Workflow Execution

1. Go to **Actions** tab
2. Click "Support Concierge Bot" workflow
3. Watch the most recent run
4. Look for:
   - 🟢 **Green checks** on each step (success)
   - Build step completes
   - "Run Support Concierge" step completes
   - No red ✗ marks

### Step 3: Verify Bot Response

1. Go back to the test issue you created
2. Look for a comment from `github-actions[bot]`
3. Comment should contain:
   - Issue classification/category
   - Completeness assessment
   - Follow-up questions (if needed)
   - Checklist of required fields

**Expected comment format:**
```
## Support Concierge Analysis

**Category:** Bug / Feature Request / Documentation

**Completeness:** 60% (6/10 items)

**Status:** ⚠️ Incomplete - Additional information needed

Missing fields:
- Environment details
- Steps to reproduce
- Expected vs actual behavior

**Follow-up Questions:**
Could you please provide:
1. Your operating system and version?
2. Steps to reproduce the issue?
3. Expected vs actual behavior?
```

### Step 4: Verify No Errors

Check the "Run Support Concierge" step output for:
- [ ] ✓ "Processing event: issues"
- [ ] ✓ "Loaded spec pack successfully"
- [ ] ✓ "Determining category..."
- [ ] ✓ "Extracting fields..."
- [ ] ✓ "Creating comment..."
- [ ] ✓ "Processing complete."

**Watch out for:**
- ✗ "ERROR: OPENAI_API_KEY not set" → Re-add secret
- ✗ "ERROR: Model not found" → Change OPENAI_MODEL
- ✗ "ERROR: GITHUB_TOKEN not found" → Check Actions permissions
- ✗ Connection timeouts → Check network, OpenAI status

---

## Production Deployment

### Pre-Production Checklist

- [ ] **Secrets verified**
  - [ ] OPENAI_API_KEY is set and valid
  - [ ] No secrets in code or commits
  - [ ] Secret rotation policy in place

- [ ] **Configuration verified**
  - [ ] .supportbot/ directory committed
  - [ ] Categories match business requirements
  - [ ] Validation rules configured
  - [ ] Spec pack YAML is valid

- [ ] **Workflow tested**
  - [ ] Test issue processes successfully
  - [ ] Bot comments within 2 minutes
  - [ ] No error messages in logs
  - [ ] Multiple test cases pass

- [ ] **Performance baseline**
  - [ ] Average run time noted
  - [ ] API response time acceptable
  - [ ] No rate limiting issues

- [ ] **Monitoring setup**
  - [ ] Alert rules configured (if applicable)
  - [ ] Log aggregation enabled (if applicable)
  - [ ] Backup workflow configured (if applicable)

### Deployment Steps

#### Step 1: Enable for New Issues
Repository is now ready! The workflow will automatically:
- ✓ Trigger on new issues
- ✓ Trigger on issue edits
- ✓ Trigger on new comments
- ✓ Process and classify automatically
- ✓ Post analysis as comments

#### Step 2: Enable for Existing Issues (Optional)
If you want to process existing issues:

```bash
# List all issues
gh issue list --repo YOUR_USERNAME/github-issues-support --limit 100

# For each issue, add a comment to trigger workflow
# (The bot looks at issue updates)
gh issue comment ISSUE_NUMBER --repo YOUR_USERNAME/github-issues-support \
  --body "Triggering analysis update..."
```

#### Step 3: Monitor & Optimize
- [ ] Track bot accuracy for first week
- [ ] Collect feedback from team
- [ ] Adjust categories/rules as needed
- [ ] Fine-tune follow-up questions
- [ ] Optimize model selection if needed

---

## Cost Estimation

### OpenAI API Usage

**Typical consumption per issue:**
- Category classification: ~200 tokens
- Field extraction: ~500 tokens  
- Analysis: ~300 tokens
- **Total per issue:** ~1000 tokens = ~$0.003 USD

**Monthly estimate (100 issues/month):**
- Tokens: ~100,000
- Cost: ~$0.30 USD

**Note:** Pricing varies by model. Check https://openai.com/pricing

### GitHub Actions Usage

**Free tier:** 2,000 minutes/month
**Cost:** $0.24 per 1,000 minutes over limit

**Typical job:**
- Setup .NET: 30 seconds
- Restore/Build: 60 seconds
- Run bot: 30 seconds
- **Total per issue:** ~120 seconds = 0.002 minutes

**Monthly estimate (100 issues):**
- Total: ~0.2 minutes
- Cost: FREE (well under 2,000 minute limit)

---

## Scaling & Optimization

### For Large Volumes

If processing 1000+ issues/month:

1. **Batch Processing**
   ```yaml
   - name: Process Backlog
     if: github.event_name == 'schedule'
     run: |
       # Process multiple issues in single workflow
   ```

2. **Model Selection**
   - Use `gpt-3.5-turbo` for faster processing
   - Use `gpt-4` only for complex cases

3. **Caching**
   - Cache dependencies to speed up builds
   - Cache spec pack configurations

### For High Accuracy

If accuracy is critical:

1. **Use GPT-4o**
   ```
   OPENAI_MODEL=gpt-4o-2024-08-06
   ```

2. **Structured Output**
   - Use JSON schema validation
   - Validate before posting

3. **Review Process**
   - Have humans review automated classifications
   - Collect feedback for model training

---

## Maintenance & Updates

### Weekly Tasks
- [ ] Check workflow success rate (Actions tab)
- [ ] Monitor API usage (OpenAI dashboard)
- [ ] Review bot comments for accuracy
- [ ] Update configuration if needed

### Monthly Tasks
- [ ] Review and adjust categories
- [ ] Update validation rules
- [ ] Check for dependency updates
- [ ] Test with new issues

### Quarterly Tasks
- [ ] Evaluate model performance
- [ ] Consider model upgrade/downgrade
- [ ] Review and update documentation
- [ ] Plan feature enhancements

### Annual Tasks
- [ ] Major version updates
- [ ] API migration if needed
- [ ] Complete audit and review
- [ ] Team training/documentation

---

## Troubleshooting Reference

### Workflow Won't Start
1. Check: Repository → Settings → Actions → Permissions
2. Check: `.github/workflows/support-concierge.yml` has correct triggers
3. Check: Issues are enabled on repository

### Bot Doesn't Comment
1. Check: Workflow "Run Support Concierge" step completes with ✓
2. Check: No errors in step output
3. Check: Bot has permission to comment (Settings → Permissions)
4. Try: Edit the issue to retrigger (should reprocess)

### Workflow Fails Immediately
1. Check: OPENAI_API_KEY secret is set
2. Check: Secret name exactly matches `OPENAI_API_KEY`
3. Check: API key is valid and has credit
4. Try: Re-add secret with exact value

### Slow Processing
1. Check: OpenAI API response time (status.openai.com)
2. Try: Change model to faster option (gpt-3.5-turbo)
3. Check: GitHub Actions concurrency limits
4. Optimize: .supportbot/ configuration complexity

---

## Success Metrics

Track these to measure success:

| Metric | Target | Current |
|--------|--------|---------|
| Workflow success rate | >95% | ___ |
| Average execution time | <1 min | ___ |
| Bot classification accuracy | >80% | ___ |
| Issues needing manual review | <20% | ___ |
| Follow-up response rate | >50% | ___ |
| Time to first response | <2 min | ___ |

---

## Support & Resources

### Documentation
- QUICKSTART.md - 5-minute setup
- MANIFEST.md - Feature reference
- ARCHITECTURE.md - Technical deep-dive
- TROUBLESHOOTING_GUIDE.md - Common issues
- This file - Complete guide

### External Resources
- OpenAI API Docs: https://platform.openai.com/docs
- GitHub Actions: https://docs.github.com/actions
- GitHub REST API: https://docs.github.com/rest

### Getting Help
1. Check Troubleshooting Guide
2. Review workflow logs (Actions tab)
3. Test locally if possible
4. Check OpenAI status page
5. Create GitHub issue with details

