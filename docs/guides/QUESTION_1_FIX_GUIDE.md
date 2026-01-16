# Question 1 Fix: Centralize OpenAI Model Configuration

## Summary
Two places set OpenAI model:
- **GitHub Actions** (support-concierge.yml): Sets `OPENAI_MODEL: gpt-4` ❌ **NOT USED**
- **OpenAiClient.cs**: Hardcodes `gpt-4o-2024-08-06` ✅ **USED**

## Solution: Read Model from Environment Variable

### Changes Required

#### 1. Update OpenAiClient.cs (src/SupportConcierge/Agents/OpenAiClient.cs)

**Change Line 25 from:**
```csharp
// Use gpt-4o model which supports response_format
_model = "gpt-4o-2024-08-06";
```

**To:**
```csharp
// Read model from environment, default to gpt-4o-2024-08-06 for Structured Outputs support
_model = (Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-2024-08-06").Trim();

// Log source for debugging
if (Environment.GetEnvironmentVariable("OPENAI_MODEL") != null)
    Console.WriteLine($"  Source: OPENAI_MODEL environment variable");
else
    Console.WriteLine($"  Source: Default hardcoded value (Structured Outputs support required)");
```

#### 2. Update GitHub Actions Workflow (.github/workflows/support-concierge.yml)

**Change Line 37 from:**
```yaml
OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4' }}
```

**To:**
```yaml
OPENAI_MODEL: ${{ vars.OPENAI_MODEL || 'gpt-4o-2024-08-06' }}
```

**Explanation:** Updated default to match actual model being used and support Structured Outputs.

### Testing

After changes, run:

```bash
# Test 1: Default model (no env var)
cd src/SupportConcierge
dotnet run

# Expected output:
# Using OpenAI model: gpt-4o-2024-08-06
#   Source: Default hardcoded value (Structured Outputs support required)
```

```bash
# Test 2: Override with environment variable
OPENAI_MODEL=gpt-4-turbo dotnet run

# Expected output:
# Using OpenAI model: gpt-4-turbo
#   Source: OPENAI_MODEL environment variable
```

```bash
# Test 3: Override with specific Structured Outputs model
OPENAI_MODEL=gpt-4o-mini dotnet run

# Expected output:
# Using OpenAI model: gpt-4o-mini
#   Source: OPENAI_MODEL environment variable
```

### Benefits

✅ **Single Source of Truth** - Model set in one place (environment)  
✅ **Flexible** - Can change per environment without code changes  
✅ **Backward Compatible** - Defaults to current production model  
✅ **Clear** - Logs show where model comes from  
✅ **Testable** - Can override in tests  

### Backward Compatibility

- ✅ Existing GitHub Actions workflows work (uses new default)
- ✅ Existing local runs work (uses new default)
- ✅ Can override in any environment
- ✅ No breaking changes

### Estimated Effort

- ⏱️ **Time to implement:** 5 minutes
- ⏱️ **Time to test:** 5 minutes
- ⏱️ **Total:** 10 minutes

---

## Related Considerations

### Model Selection Strategy

When choosing which model to use, consider:

| Model | Structured Outputs | Speed | Cost | Best For |
|-------|-------------------|-------|------|----------|
| `gpt-4o-2024-08-06` | ✅ Yes | ⚡⚡⚡ | 💰💰💰 | Production (strict validation) |
| `gpt-4o-mini` | ✅ Yes | ⚡⚡⚡⚡ | 💰 | Cost-sensitive (fast iteration) |
| `gpt-4-turbo` | ❌ No | ⚡⚡ | 💰💰 | Fallback (lenient parsing only) |
| `gpt-4` | ❌ No | ⚡ | 💰💰💰💰 | Legacy (not recommended) |

**Recommendation:** Keep `gpt-4o-2024-08-06` as default (current production model)

---

**Implementation Status:** Ready to apply  
**Priority:** 🔴 High (removes confusion, fixes inconsistency)
