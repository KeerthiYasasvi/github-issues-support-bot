# Step-by-Step Execution Guide - Reddit-ELT-Pipeline

**Corrected Path:** `D:\Projects\reddit\Reddit-ELT-Pipeline`  
**Current Date:** January 12, 2026

---

## STEP 1: Verify Your Environment ✅ (ALREADY DONE)

```
✅ .NET 10.0.101 installed
✅ Git installed
✅ OpenAI API Key in .env
✅ GitHub Token in .env
✅ Optional: `OPENAI_MODEL` in .env (non-secret)
✅ Bot source files ready at D:\Projects\agents\ms-quickstart\github-issues-support
```

---

### Model Selection (OPENAI_MODEL)

The bot now reads an optional `OPENAI_MODEL` environment variable. This is a non-secret toggle and can safely live alongside your existing `.env` secrets.

- Local: add to your `.env` (kept out of git via `.gitignore`)
   ```env
   OPENAI_MODEL=gpt-4o-2024-08-06
   ```
- GitHub Actions: set a repository variable `OPENAI_MODEL` (Settings → Secrets and variables → Actions → Variables). This does not expose your secrets.

If `OPENAI_MODEL` is not set, the bot defaults to `gpt-4o-2024-08-06`. The startup log will show:

```
Using OpenAI model: gpt-4o-2024-08-06 (source: default)
```

If you override via environment:

```
Using OpenAI model: gpt-4-turbo (source: env: OPENAI_MODEL)
```

## STEP 2: Copy Bot Files to Reddit Repo (10 minutes)

**This is the CRITICAL step.** We're copying the bot into your actual Reddit project.

### 2.1: Open PowerShell and Navigate

```powershell
# Open PowerShell and run:
cd D:\Projects\reddit\Reddit-ELT-Pipeline

# Verify you're in the right place
dir
```

**You should see:**
```
Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
d-----         12/8/2025   9:30 AM                dags
d-----         12/8/2025   9:30 AM                dbt_project
d-----         12/8/2025   9:30 AM                etls
-a----        12/8/2025   9:30 AM            1024 .env
-a----        12/8/2025   9:30 AM           54321 docker-compose.yml
-a----        12/8/2025   9:30 AM            5678 README.md
```

**If you don't see this,** you're in the wrong directory. Navigate correctly.

### 2.2: Copy Bot Files

```powershell
# Copy the .github directory (GitHub Actions workflow)
Copy-Item -Path "D:\Projects\agents\ms-quickstart\github-issues-support\.github" `
          -Destination "D:\Projects\reddit\Reddit-ELT-Pipeline\.github" `
          -Recurse -Force

# Copy the src directory (bot application code)
Copy-Item -Path "D:\Projects\agents\ms-quickstart\github-issues-support\src" `
          -Destination "D:\Projects\reddit\Reddit-ELT-Pipeline\src" `
          -Recurse -Force

# Copy the .supportbot directory (configuration + playbooks)
Copy-Item -Path "D:\Projects\agents\ms-quickstart\github-issues-support\.supportbot" `
          -Destination "D:\Projects\reddit\Reddit-ELT-Pipeline\.supportbot" `
          -Recurse -Force

# Copy the evals directory (testing harness)
Copy-Item -Path "D:\Projects\agents\ms-quickstart\github-issues-support\evals" `
          -Destination "D:\Projects\reddit\Reddit-ELT-Pipeline\evals" `
          -Recurse -Force

# Copy .gitignore (prevents secrets from being committed)
Copy-Item -Path "D:\Projects\agents\ms-quickstart\github-issues-support\.gitignore" `
          -Destination "D:\Projects\reddit\Reddit-ELT-Pipeline\.gitignore" `
          -Force
```

### 2.3: Verify All Files Were Copied

```powershell
# Check what we just added
dir

# You should NOW see these NEW directories:
# - .github/
# - src/
# - .supportbot/
# - evals/

# Verify workflow file exists
Test-Path ".github\workflows\support-concierge.yml"
# Should return: True

# Verify configuration exists
Test-Path ".supportbot\checklists.yaml"
# Should return: True

# Verify bot source exists
Test-Path "src\SupportConcierge\Program.cs"
# Should return: True
```

**Expected output:**
```
True
True
True
```

---

## STEP 3: Add GitHub Secret (5 minutes)

Now we tell GitHub about your OpenAI API key.

### 3.1: Go to GitHub Secrets Page

**URL:** https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/settings/secrets/actions

Or manually:
1. Go to: https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline
2. Click **Settings** (top menu)
3. Left sidebar → **Secrets and variables** → **Actions**

### 3.2: Create the Secret

1. Click **"New repository secret"** (green button, top right)
2. **Name:** `OPENAI_API_KEY` (exactly this)
3. **Secret:** Paste your key (do NOT commit it anywhere):
   ```
   sk-<your-key-here>
   ```
4. Click **"Add secret"**

### 3.3: Verify It Was Added

After clicking "Add secret", you should see:

```
OPENAI_API_KEY    ●●●●●● (hidden)    Updated now
```

✅ **Success!** GitHub now knows your OpenAI key.

---

## STEP 4: Commit and Push to GitHub (5 minutes)

Back in PowerShell:

### 4.1: Check What Changed

```powershell
# Still in D:\Projects\reddit\Reddit-ELT-Pipeline
git status
```

**You should see:**
```
Untracked files:
  (use "git add <file>..." to include in what will be committed)
        .github/
        src/
        .supportbot/
        evals/
        .gitignore
```

### 4.2: Stage All Changes

```powershell
git add .
```

### 4.3: Create Commit Message

```powershell
git commit -m "Add GitHub Issues Support Concierge bot with Reddit-ELT-Pipeline categories"
```

**Output should show:**
```
[main abc1234] Add GitHub Issues Support Concierge bot...
 X files changed, Y insertions(+)
```

### 4.4: Push to GitHub

```powershell
git push origin main
```

**Output should show:**
```
Enumerating objects: 45, done.
Counting objects: 100% (45/45), done.
...
remote: Resolving deltas: 100% (25/25), done.
To https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline.git
   abc1234..def5678  main -> main
```

✅ **Success!** Your changes are now on GitHub!

---

## STEP 5: Verify GitHub Actions Workflow (2 minutes)

### 5.1: Check Actions Tab

**URL:** https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/actions

### 5.2: Look for Workflow

You should see:
- A workflow with name something like: **"Add GitHub Issues Support Concierge bot..."`**
- Status: ✅ **green checkmark** (completed successfully)

**OR** if it's still running:
- Status: 🟡 **yellow dot** (in progress)

**OR** if there was an error:
- Status: ❌ **red X** (something went wrong)

### 5.3: Click on the Workflow

Click the workflow name to see logs. If successful, you'll see:
```
✓ Checkout repository
✓ Setup .NET 8
✓ Restore dependencies
✓ Build
✓ Run Support Concierge
```

✅ **If you see all green checkmarks, you're ready for testing!**

---

## STEP 6: Create Test Issue #1 (Actionable Issue) - 5 minutes

This tests the bot with COMPLETE information.

### 6.1: Go to Issues Page

**URL:** https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/issues

Or click: **Issues** tab → **New issue**

### 6.2: Fill Out Issue

**Title:**
```
Airflow DAG failing with "No module named 'etls'" error
```

**Body:**
```
### Category
airflow_dag

### Operating System
Windows 11

### Docker Version
Docker 24.0.6, Docker Compose 2.21.0

### DAG Name
reddit_pipeline

### Error Message
ModuleNotFoundError: No module named 'etls'

### Airflow Logs
```
[2026-01-12 10:30:15,234] {taskinstance.py:1415} ERROR - Task failed with exception
Traceback (most recent call last):
  File "/opt/airflow/dags/reddit_pipeline.py", line 3, in <module>
    from etls.extract import extract_post
ModuleNotFoundError: No module named 'etls'
```

### Services Status
All containers running:
- airflow-webserver: Up
- airflow-scheduler: Up  
- postgres: Up
- redis: Up
```

### 6.3: Click "Create Issue"

---

## STEP 7: Watch the Magic Happen! 🪄 (60 seconds)

### 7.1: Immediate Actions

After creating the issue:

1. **Go to Actions tab:** https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/actions
2. **Look for new workflow run** (should say "support-concierge bot" or similar)
3. **Status:** Should show 🟡 orange/yellow (running)

### 7.2: Monitor Progress

```
⏱️ 10 seconds: Workflow starts
⏱️ 30 seconds: Bot downloading files, loading config
⏱️ 45 seconds: Bot analyzing issue, calling OpenAI
⏱️ 60 seconds: Bot posting comment to GitHub
```

### 7.3: See the Result

1. **Go back to your issue:** https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/issues/XXX
2. **Scroll down** to see comments
3. **You should see a comment from:** `github-actions[bot]`

**The comment should contain:**

```
## 📋 Engineer Brief

**Summary:** Airflow DAG reddit_pipeline fails to import etls module

### 🔍 Symptoms
- ModuleNotFoundError when importing etls.extract
- Error occurs during DAG parsing
- All Docker containers are running

### 💻 Environment
- OS: Windows 11
- Docker: 24.0.6
- Compose: 2.21.0
- DAG: reddit_pipeline

### ✅ Suggested Next Steps
- Verify etls/ directory exists in /opt/airflow/etls
- Check if etls/__init__.py is present
- Confirm volume mount in docker-compose.yml includes etls directory
```

**Also check:**
- ✅ Labels added: `component: airflow`, `type: dag-failure`, `priority: high`
- ✅ Assignee set to: `KeerthiYasasvi`
- ✅ Score shown: `95/75` ✅ Actionable!

---

## STEP 8: Test Incomplete Issue (Follow-up Questions) - 5 minutes

This tests the bot asking for MORE information.

### 8.1: Create Vague Issue

**Title:**
```
Reddit API not working
```

**Body:**
```
I'm trying to run the pipeline but getting an error from Reddit.
Can someone help?
```

### 8.2: Submit Issue

### 8.3: Wait 60 Seconds

### 8.4: Check Response

The bot should post:

```
👋 Hi! I need a bit more information to help route this issue effectively.

**1. What are your Reddit API credentials configured as?**
   _We need to confirm your client_id, secret, and user_agent are set in .env_

**2. Which subreddit are you trying to extract from?**
   _Some subreddits have restrictions that affect extraction_

**3. What is the exact error message from PRAW or the extraction logs?**
   _The error message will tell us if it's authentication, rate limiting, or API access_

---
This is follow-up round 1 of 3. Please provide as much detail as possible.
```

✅ **Success!** Bot asked for more info instead of closing.

---

## STEP 9: Reply to Incomplete Issue (Round 2) - 5 minutes

### 9.1: Reply to the Issue

Click **"Comment"** and add:

```
Thanks for asking!

I'm getting this error:
```
praw.exceptions.InvalidAuth: 401 Unauthorized
```

Subreddit: r/python
Credentials are definitely in .env - I checked.
```

### 9.2: Wait 60 Seconds

### 9.3: Check Response

Bot should now respond with more targeted questions:

```
👋 Thanks for the additional information!

**1. Can you confirm your Reddit user_agent is properly formatted?**
   _Format should be: "YourApp/1.0.0 (by YourUsername)"_

**2. Are you able to access the subreddit r/python directly in a browser?**
   _Some subreddits require certain permissions or have restrictions_

**3. Can you share the exact client_id creation date and app type?**
   _Sometimes Reddit API credentials need to be regenerated_

---
This is follow-up round 2 of 3. Your information is getting more complete!
```

✅ **Success!** Bot is tracking conversation and asking Round 2 questions!

---

## STEP 10: Optional - Escalation Test

### 10.1: Create Very Vague Issue

**Title:**
```
DBT not working
```

**Body:**
```
Help
```

### 10.2: Ignore All Follow-ups (Don't reply 3 times)

The bot will ask 3 rounds of questions and you don't reply.

### 10.3: After Round 3

Bot posts escalation:

```
## ⚠️ Escalation Notice

After 3 rounds of follow-up questions, this issue still doesn't have enough information to be actionable.

**Completeness Score:** 15/75 (needs 75)

Tagging for manual review: @KeerthiYasasvi
```

✅ **Success!** Escalation working!

---

## ✅ SUCCESS CHECKLIST

- [ ] **Step 2:** All bot files copied to `D:\Projects\reddit\Reddit-ELT-Pipeline`
- [ ] **Step 3:** `OPENAI_API_KEY` secret added to GitHub
- [ ] **Step 4:** Files committed and pushed
- [ ] **Step 5:** Workflow shows green checkmark in Actions
- [ ] **Step 6:** Test issue #1 created with complete info
- [ ] **Step 7:** Bot posted engineer brief comment
- [ ] **Step 8:** Bot correctly categorized as `airflow_dag`
- [ ] **Step 9:** Test issue #2 created with incomplete info
- [ ] **Step 10:** Bot asked follow-up questions (Round 1)
- [ ] **Step 11:** Replied with more info
- [ ] **Step 12:** Bot asked Round 2 questions

---

## 📝 WHAT'S HAPPENING BEHIND THE SCENES

When you create an issue:

```
1. GitHub detects the issue.created event
2. Triggers the workflow: .github/workflows/support-concierge.yml
3. Workflow runs on ubuntu-latest
4. Checks out your repo code
5. Downloads and runs our bot (src/SupportConcierge/Program.cs)
6. Bot loads configuration (.supportbot/*.yaml)
7. Bot reads issue title and body
8. CLASSIFIER Agent: Determines category
9. EXTRACTOR Agent: Pulls fields from text
10. SCORER Agent: Rates completeness (0-100)
11. Decision:
    - If score ≥ threshold: SUMMARIZER generates engineer brief
    - If score < threshold: QUESTIONER generates follow-up questions
    - If 3 rounds failed: Posts escalation message
12. Posts comment to GitHub
13. Adds labels and assignees
14. Stores state in hidden HTML comment for next run
```

---

## 🚨 IF SOMETHING GOES WRONG

### Issue 1: Bot doesn't post a comment

```powershell
# Check Actions logs:
1. Go to: https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/actions
2. Click the failed workflow
3. Look for error messages
4. Common issues:
   - OPENAI_API_KEY not set → Go to Step 3
   - Invalid API key → Check .env file
   - No internet → Check connection
```

### Issue 2: Wrong category detected

```
Edit: .supportbot/categories.yaml
Add more specific keywords to your category
Re-commit and push
Create new test issue
```

### Issue 3: Fields not extracted

```
Try using clearer headings in your issue:
Instead of: "Problem: Getting error"
Use: "### Error Message"
```

### Issue 4: Workflow won't start

```
1. Check: Is Actions enabled? 
   Settings → Actions → Allow all actions
2. Check: Does workflow file exist?
   Test-Path ".github\workflows\support-concierge.yml"
3. Retry creating issue
```

---

## 📞 NEXT STEPS AFTER VERIFICATION

1. **Customize for your needs:**
   - Edit `.supportbot/routing.yaml` to auto-assign to different teams
   - Update field weights in `.supportbot/checklists.yaml`
   - Add project-specific playbooks

2. **Monitor real issues:**
   - Create a few real issues on your repo
   - Watch how bot handles them
   - Refine based on what works/doesn't work

3. **Fine-tune prompts:**
   - Edit `src/SupportConcierge/Agents/Prompts.cs` for different tone
   - Edit field descriptions in checklists
   - Add more validation rules

---

**Ready to start? Let's go with Step 2! 🚀**
