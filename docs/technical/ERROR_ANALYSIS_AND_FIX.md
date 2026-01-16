# Error Analysis and Fix for GitHub Actions Failures

## Problem Summary

All 5 test workflows (#109-113) failed with the same OpenAI API error:

```
OpenAI API Error: BadRequest
"message": "Invalid parameter: 'response_format' of type 'json_object' is not supported with this model."
```

## Root Cause

The bot is configured to use the `gpt-4` model (as shown in workflow logs line 19: "Using OpenAI model: gpt-4"), but the code in `OpenAiClient.cs` is using `response_format: json_object`, which is **only supported by newer models**:

- ✅ `gpt-4o` (GPT-4 Omni)
- ✅ `gpt-4-turbo` / `gpt-4-turbo-preview`
- ✅ `gpt-3.5-turbo-1106` and later versions
- ❌ `gpt-4` (base model) - **Does NOT support `response_format`**

### Evidence from Logs

From workflow run #113 logs:
```
Line 19: Using OpenAI model: gpt-4 (source: env: OPENAI_MODEL)
Line 39: OpenAI API Error: BadRequest
Line 42: "message": "Invalid parameter: 'response_format' of type 'json_object' is not supported with this model."
```

### Code Location

The error occurs in [OpenAiClient.cs](src/SupportConcierge/Agents/OpenAiClient.cs#L127):
```csharp
response_format = responseFormat  // Line 110
// ...
response.EnsureSuccessStatusCode();  // Line 127 - throws exception
```

The `responseFormat` is set by `GetResponseFormat()` method (lines 67-71):
```csharp
private object GetResponseFormat(JsonElement schemaElement, string schemaName)
{
    return new { type = "json_object" };
}
```

## Solution

Change the OpenAI model from `gpt-4` to a model that supports JSON response formatting.

### Option 1: Use `gpt-4o` (Recommended)

Update the `OPENAI_MODEL` environment variable/secret in the Reddit-ELT-Pipeline repository to `gpt-4o`:

1. Go to: https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/settings/secrets/actions
2. Find the `OPENAI_MODEL` secret (or variable)
3. Change value from `gpt-4` to `gpt-4o`

**Why `gpt-4o`?**
- Most capable model that supports structured outputs
- Faster and cheaper than `gpt-4-turbo`
- Better at JSON formatting and structured responses

### Option 2: Use `gpt-4-turbo`

If `gpt-4o` is not available:
1. Change `OPENAI_MODEL` to `gpt-4-turbo`
2. Still supports `response_format: json_object`

### Option 3: Use `gpt-3.5-turbo` (Budget-Friendly)

For lower cost:
1. Change `OPENAI_MODEL` to `gpt-3.5-turbo`
2. Supports `response_format: json_object`
3. May produce less accurate classifications

## Implementation Steps

### Step 1: Check Current Configuration

Let me check where the model is configured in the Reddit-ELT-Pipeline repository:

```bash
# Check GitHub Actions secrets/variables
# Navigate to: https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/settings/secrets/actions
```

### Step 2: Update Model Configuration

1. **If using GitHub Secrets:**
   - Go to: https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/settings/secrets/actions
   - Edit `OPENAI_MODEL` secret
   - Change from `gpt-4` to `gpt-4o`

2. **If using Environment Variables in Workflow:**
   - Check [.github/workflows/support-concierge.yml](https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/blob/main/.github/workflows/support-concierge.yml)
   - Update the `env:` section to use `gpt-4o`

### Step 3: Re-run Workflows

After updating the model:
1. Go to: https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/actions
2. Click on each failed run (#109-113)
3. Click "Re-run jobs" button
4. Verify they succeed

## Verification

Once the model is updated, the bot should:

1. ✅ Successfully call OpenAI API with `response_format: json_object`
2. ✅ Extract structured data from issue body
3. ✅ Classify issues into correct categories
4. ✅ Score field completeness
5. ✅ Post helpful comments to issues

## Expected Outcomes

After fixing, you should see:
- Green checkmarks ✅ on all workflow runs
- Bot comments posted to issues #45-49
- Eval logs generated successfully

## Alternative: Modify Code to Remove response_format

If you want to keep using `gpt-4`, you would need to modify the code to remove the `response_format` parameter. However, this is **NOT recommended** because:

1. JSON responses may not be properly formatted
2. Parsing reliability will decrease
3. More API calls may be needed to get valid responses

## Next Steps

1. **Update the model configuration** (Option 1 recommended)
2. **Re-run the failed workflows**
3. **Verify bot comments are posted**
4. **Check eval logs are generated**

## Reference

- OpenAI API Documentation: https://platform.openai.com/docs/guides/structured-outputs
- Supported Models: https://platform.openai.com/docs/models
- GitHub Actions Workflow: [support-concierge.yml](https://github.com/KeerthiYasasvi/Reddit-ELT-Pipeline/blob/main/.github/workflows/support-concierge.yml)
