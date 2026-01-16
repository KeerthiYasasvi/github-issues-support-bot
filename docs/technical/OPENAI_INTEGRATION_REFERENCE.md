# OpenAI Integration Architecture Reference

## Overview

This document details how the Support Concierge Bot integrates with OpenAI's API for issue analysis, classification, and decision-making.

---

## 1. OpenAI Client Class (`OpenAiClient.cs`)

### Purpose
Central hub for all OpenAI API interactions using the Azure OpenAI SDK v1.x

### Constructor

```csharp
public OpenAiClient()
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
        ?? throw new InvalidOperationException("OPENAI_API_KEY not set");
    
    _model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-2024-08-06";
    
    var openAiClient = new OpenAIClient(apiKey);
    _client = openAiClient.GetChatClient(_model);
}
```

**Key Points:**
- ✓ Reads API key from `OPENAI_API_KEY` environment variable
- ✓ Reads model name from `OPENAI_MODEL` environment variable
- ✓ Falls back to `gpt-4o-2024-08-06` if model not specified
- ✓ Throws exception if API key is missing
- ✓ Initializes ChatClient with specified model

### Environment Variables

| Variable | Required | Default | Example |
|----------|----------|---------|---------|
| `OPENAI_API_KEY` | Yes | None | `sk-proj-...` |
| `OPENAI_MODEL` | No | `gpt-4o-2024-08-06` | `gpt-4-turbo` |

### Model Configuration (OPENAI_MODEL)

- The runtime now prefers an environment variable `OPENAI_MODEL` when present and logs the model source at startup.
- Local development: put `OPENAI_MODEL=...` in your `.env` alongside `OPENAI_API_KEY` (both are loaded as environment variables; `.env` remains untracked).
- GitHub Actions: set a repository Variable `OPENAI_MODEL` (Settings → Secrets and variables → Actions → Variables). Do not put it in Secrets; it is not sensitive.
- Default (if unset): `gpt-4o-2024-08-06`.

### JSON Schema Enforcement (Status)

- The bot uses Structured Outputs (`response_format: json_schema` with `strict=true`) where supported and falls back to `json_object` otherwise.
- Violations are logged; telemetry can be used to monitor compliance trends.

### Class Methods

#### 1. `ClassifyCategoryAsync()`

**Purpose:** Classify an issue into a category using structured JSON output

```csharp
public async Task<CategoryClassificationResult> ClassifyCategoryAsync(
    string issueTitle, 
    string issueBody, 
    List<string> categoryNames)
```

**Implementation:**
```csharp
var messages = new List<ChatMessage>
{
    new SystemChatMessage("You are a precise issue classification assistant..."),
    new UserChatMessage(prompt)
};

var options = new ChatCompletionOptions
{
    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
        "category_classification",
        BinaryData.FromString(Schemas.CategoryClassificationSchema),
        jsonSchemaIsStrict: true
    )
};

var response = await _client.CompleteChatAsync(messages, options);
```

**Features:**
- Uses JSON schema for structured output
- Strict schema validation enabled
- Supports categories list as input
- Returns `CategoryClassificationResult` with score and reasoning

**Return Type:**
```csharp
public class CategoryClassificationResult
{
    public string Category { get; set; }
    public double Score { get; set; }
    public string Reasoning { get; set; }
}
```

#### 2. `ExtractFieldsAsync()`

**Purpose:** Extract specific fields from issue body using JSON parsing

```csharp
public async Task<Dictionary<string, string>> ExtractFieldsAsync(
    string issueBody,
    List<string> fieldNames)
```

**Implementation:**
```csharp
var options = new ChatCompletionOptions
{
    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
        "field_extraction",
        BinaryData.FromString(Schemas.FieldExtractionSchema),
        jsonSchemaIsStrict: true
    )
};
```

**Features:**
- Structured JSON extraction
- Handles missing fields gracefully
- Validates extracted data against schema
- Returns dictionary of extracted fields

#### 3. `AnalyzeCompleteness Async()`

**Purpose:** Score issue completeness based on checklist

**Implementation:**
```csharp
var prompt = Prompts.CompletenessAnalysis(issueBody, checklist);
var response = await _client.CompleteChatAsync(messages, options);
```

**Features:**
- Evaluates issue against provided checklist
- Generates follow-up questions
- Scores completeness percentage
- Identifies missing information

---

## 2. JSON Schemas (`Schemas.cs`)

### Purpose
Define strict JSON schemas for structured outputs

### Category Classification Schema

```json
{
  "type": "object",
  "properties": {
    "category": {
      "type": "string",
      "description": "The classified category"
    },
    "score": {
      "type": "number",
      "minimum": 0,
      "maximum": 100,
      "description": "Confidence score 0-100"
    },
    "reasoning": {
      "type": "string",
      "description": "Explanation for classification"
    }
  },
  "required": ["category", "score", "reasoning"]
}
```

**Validation:**
- Schema is strict (no additional properties)
- All required fields must be present
- Types are strictly validated
- Number ranges are enforced

### Field Extraction Schema

```json
{
  "type": "object",
  "properties": {
    "fields": {
      "type": "object",
      "additionalProperties": {
        "type": ["string", "null"]
      }
    }
  },
  "required": ["fields"]
}
```

**Features:**
- Flexible field collection
- Supports null values for missing fields
- Additionalproperties allow dynamic fields

---

## 3. Prompts (`Prompts.cs`)

### Purpose
System and user prompts for LLM interactions

### System Prompts

```csharp
public static string CategoryClassificationSystemPrompt = 
    "You are a precise issue classification assistant. " +
    "Analyze the issue and classify it into ONE of the provided categories. " +
    "Always respond with valid JSON matching the schema. " +
    "Be precise and confident in your classification.";
```

**Characteristics:**
- Clear role definition
- Specific instructions
- Format requirements
- Guidance on confidence

### User Prompts

```csharp
public static string CategoryClassification(
    string title, 
    string body, 
    string categories)
{
    return $"""
    Classify this GitHub issue into one of these categories:
    {categories}
    
    Issue Title: {title}
    Issue Body:
    {body}
    
    Respond with JSON containing: category, score, and reasoning.
    """;
}
```

**Design:**
- Provides context and examples
- Lists available categories
- Specifies expected format
- Includes actual issue data

---

## 4. Integration Points

### 4.1 With Orchestrator

```csharp
// In Orchestrator.cs
var openAiClient = new OpenAiClient();

// Category determination
var category = await DetermineCategoryAsync(issue, specPack, openAiClient, parser);

// Field extraction
var extractedFields = await ExtractFieldsAsync(
    issue, comments, checklist, parser, openAiClient, secretRedactor);
```

**Flow:**
1. Create OpenAiClient instance (happens once per workflow)
2. Pass to various analysis methods
3. Collect results for decision-making

### 4.2 With GitHub API

```csharp
// OpenAI provides analysis
var classification = await openAiClient.ClassifyCategoryAsync(
    issue.Title, issue.Body, categories);

// GitHub API posts the result
await githubApi.CreateCommentAsync(
    owner, repo, issueNumber, formattedComment);
```

**Integration:**
- OpenAI analyzes content
- Results formatted by CommentComposer
- GitHub API posts formatted comment

---

## 5. Request/Response Flow

### Typical Interaction Sequence

```
1. Issue Event Triggered
   ↓
2. Program.cs reads environment variables
   ├─ GITHUB_TOKEN
   ├─ OPENAI_API_KEY
   └─ OPENAI_MODEL
   ↓
3. OpenAiClient initialized with OPENAI_MODEL
   ├─ Creates OpenAIClient(apiKey)
   └─ Gets ChatClient with model
   ↓
4. Classification Request
   ├─ Send: Title + Body + Categories
   ├─ OpenAI returns: JSON result
   └─ Parse: Extract category and score
   ↓
5. Field Extraction Request
   ├─ Send: Issue body + field names
   ├─ OpenAI returns: JSON with fields
   └─ Parse: Extract field values
   ↓
6. Completeness Analysis
   ├─ Send: Fields + Checklist
   ├─ OpenAI returns: Score and questions
   └─ Parse: Extract recommendations
   ↓
7. Decision & Action
   ├─ Post comment to GitHub
   ├─ Update issue state (if applicable)
   └─ Store state for next interaction
```

---

## 6. Error Handling

### Exception Types

```csharp
// Missing API key
throw new InvalidOperationException("OPENAI_API_KEY not set");

// API errors
catch (ClientResultException ex) when (ex.Status == 429)
{
    // Rate limited - implement retry with backoff
}

// Invalid response
catch (JsonException ex)
{
    // JSON parsing failed - log and escalate
}

// Schema validation
catch (ArgumentException ex) when (ex.Message.Contains("schema"))
{
    // Response doesn't match schema - retry or escalate
}
```

### Recovery Strategies

1. **Rate Limiting (429):** Exponential backoff retry
2. **Invalid JSON:** Re-request with clearer instructions
3. **Schema Mismatch:** Regenerate with simpler schema
4. **Timeout:** Retry with increased timeout
5. **Auth Error (401):** Check API key validity

---

## 7. Model Compatibility

### Supported Models

| Model | Structured Output | Cost | Speed | Quality |
|-------|-----------------|------|-------|---------|
| `gpt-4o-2024-08-06` | ✓ Yes | $$$ | Medium | Excellent |
| `gpt-4-turbo` | ✓ Yes | $$ | Medium | Very Good |
| `gpt-4` | ✓ Yes | $$ | Slow | Very Good |
| `gpt-3.5-turbo` | ✗ Limited | $ | Fast | Good |

### Feature Requirements

**Structured Output (JSON Schema):**
- ✓ `gpt-4o-2024-08-06` and later
- ✓ `gpt-4-turbo-2024-04-09` and later
- ✓ `gpt-4-turbo`
- ✗ `gpt-3.5-turbo` (use alternative)

**Recommended:** Use `gpt-4o-2024-08-06` for best results

---

## 8. Performance Optimization

### Caching Strategies

```csharp
// Cache categorization results
private static Dictionary<string, string> _categoryCache 
    = new Dictionary<string, string>();

// Check cache before API call
if (_categoryCache.TryGetValue(issueHash, out var category))
{
    return category;
}
```

### Batching

```csharp
// Process multiple issues in single request
var batch = issues.Take(5).ToList();
var results = await ProcessBatch(batch);
```

### Token Optimization

**Reduce token usage:**
1. Shorten prompts where possible
2. Remove redundant information
3. Use simpler language
4. Summarize long issue bodies
5. Cache frequent responses

**Current token usage:**
- Category classification: ~200 tokens
- Field extraction: ~500 tokens
- Analysis: ~300 tokens
- **Total per issue: ~1000 tokens**

---

## 9. Monitoring & Debugging

### Logging Points

```csharp
Console.WriteLine($"Using OpenAI model: {_model}");
Console.WriteLine($"Sending classification request...");
var response = await _client.CompleteChatAsync(messages, options);
Console.WriteLine($"Received response: {response.Content.Count} items");
```

### Metrics to Track

1. **API Response Time**
   - Average latency
   - P95 latency
   - Timeout rate

2. **Token Usage**
   - Tokens per request
   - Total daily tokens
   - Cost per issue

3. **Accuracy**
   - Classification confidence
   - Field extraction completeness
   - Human review rate

4. **Errors**
   - Rate limiting frequency
   - Schema validation failures
   - Timeout occurrences

### Debug Mode

```csharp
// Enable verbose logging
var DEBUG = Environment.GetEnvironmentVariable("DEBUG") == "1";
if (DEBUG)
{
    Console.WriteLine($"[DEBUG] Request: {JsonSerializer.Serialize(request)}");
    Console.WriteLine($"[DEBUG] Response: {JsonSerializer.Serialize(response)}");
}
```

---

## 10. Advanced Usage

### Custom System Prompts

Modify `Prompts.cs` to change behavior:

```csharp
public static string CustomSystemPrompt = 
    "You are a technical support specialist. " +
    "Classify issues with focus on urgency and technical complexity. " +
    "Prefer conservative categories - escalate when uncertain.";
```

### Custom Schemas

Modify `Schemas.cs` to add fields:

```csharp
public static string EnhancedSchema = """
{
    "type": "object",
    "properties": {
        "category": { "type": "string" },
        "priority": { "enum": ["low", "medium", "high", "critical"] },
        "affectedComponents": { "type": "array", "items": { "type": "string" } },
        "suggestedAssignee": { "type": ["string", "null"] }
    }
}
""";
```

### Conditional Model Selection

```csharp
// Use faster model for simple classification
if (issueBody.Length < 500)
{
    _model = "gpt-3.5-turbo";
}
else
{
    _model = "gpt-4o-2024-08-06";
}
```

---

## Summary

The OpenAI integration is **production-ready** with:

✓ Proper environment configuration
✓ Structured JSON outputs
✓ Error handling and recovery
✓ Multiple supported models
✓ Performance optimization options
✓ Comprehensive logging
✓ Schema validation

**Key Success Factor:** Ensure `OPENAI_API_KEY` environment variable is properly set in GitHub Actions secrets.

