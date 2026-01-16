# Telemetry & Monitoring Guide

## Overview

The JSON Schema Enforcement Option B implementation emits telemetry events and validation warnings to enable monitoring and debugging of schema compliance. All events are written to stderr using `Console.Error.WriteLine()`.

## Event Categories

### 1. Schema Enforcement Events

**Prefix:** `[SCHEMA_ENFORCEMENT]`  
**Severity:** Warning  
**Frequency:** Once per schema parsing failure

#### Examples
```
[SCHEMA_ENFORCEMENT] Failed to use json_schema format for 'engineer_brief': {error_message}. Falling back to json_object.
[SCHEMA_ENFORCEMENT] Failed to use json_schema format for 'category_classification': {error_message}. Falling back to json_object.
```

**Meaning:** Schema could not be parsed or API call with json_schema format failed. System falling back to basic json_object type.

**Action:** Review schema JSON validity if this occurs frequently.

---

### 2. Telemetry Events

**Prefix:** `[TELEMETRY]`  
**Severity:** Info  
**Frequency:** Only when strict deserialization fails and lenient fallback is used

#### Schema: `json_deserialization_fallback`
```
[TELEMETRY] json_deserialization_fallback schema={schema_name} error={error_message}
```

#### Examples
```
[TELEMETRY] json_deserialization_fallback schema=category_classification error=The JSON value could not be converted to SupportConcierge.Agents.CategoryClassificationResult. Path: $.confidence | LineNumber: 0 | BytePositionInLine: 0.

[TELEMETRY] json_deserialization_fallback schema=engineer_brief error=The JSON value '' was expected to be of type 'System.Collections.Generic.List`1[System.String]' but it represents a null value.

[TELEMETRY] json_deserialization_fallback schema=follow_up_questions error=A JSON object could not be deserialized into type 'SupportConcierge.Agents.FollowUpQuestionsResponse'. The JSON contains unexpected property name 'question_items' instead of expected 'questions'.

[TELEMETRY] json_deserialization_fallback schema=case_packet error=The JSON value could not be converted to System.Collections.Generic.Dictionary`2[System.String,System.String]. Expected JSON object, but got array.
```

**Meaning:** LLM response was valid JSON but didn't match C# type signature. System fell back to lenient JsonElement parsing to extract available fields.

**Action:** Track frequency of telemetry events. High frequency indicates:
- LLM consistently returning unexpected field names/types
- Schema definitions need refinement
- Prompts need more explicit instructions

---

### 3. Schema Violation Events

**Prefix:** `[SCHEMA_VIOLATION]`  
**Severity:** Warning  
**Frequency:** Emitted when critical fields are missing or malformed

#### CategoryClassification Violations
```
[SCHEMA_VIOLATION] CategoryClassification response missing 'category' field. Response: {json_content}
[SCHEMA_VIOLATION] CategoryClassification fallback parsing failed to extract 'category'
[SCHEMA_VIOLATION] CategoryClassification complete parsing failure: {error_message}
```

#### FollowUpQuestions Violations
```
[SCHEMA_VIOLATION] FollowUpQuestions response has no questions. Response: {json_content}
[SCHEMA_VIOLATION] FollowUpQuestions fallback parsing extracted no questions
[SCHEMA_VIOLATION] FollowUpQuestions complete parsing failure: {error_message}
```

#### CasePacket Violations
```
[SCHEMA_VIOLATION] CasePacket extraction found {N} fields, expected {M} fields
[SCHEMA_VIOLATION] CasePacket complete parsing failure: {error_message}
```

#### EngineerBrief Violations
```
[SCHEMA_VIOLATION] EngineerBrief missing required 'summary' field. Response: {json_content}
[SCHEMA_VIOLATION] EngineerBrief missing required 'symptoms' array
[SCHEMA_VIOLATION] EngineerBrief missing required 'environment' object
[SCHEMA_VIOLATION] EngineerBrief missing required 'key_evidence' array
[SCHEMA_VIOLATION] EngineerBrief missing required 'next_steps' array
[SCHEMA_VIOLATION] EngineerBrief has fewer than 2 'validation_confirmations' (required minimum 2)
[SCHEMA_VIOLATION] Error in engineer brief lenient parsing: {error_message}
[SCHEMA_VIOLATION] EngineerBrief complete parsing failure: {error_message}
```

**Meaning:** Required fields are missing from LLM response. System attempted fallback but encountered issues.

**Action:** Review if fields are mandatory or should be optional. Consider adjusting prompts to emphasize required fields.

---

### 4. Schema Parse Errors

**Prefix:** `[WARNING]`  
**Severity:** Warning  
**Frequency:** Only if Schemas.cs JSON is malformed

#### Examples
```
[WARNING] Failed to parse schema 'engineer_brief' for Structured Outputs: Unexpected end of JSON input
[WARNING] Failed to parse schema 'category_classification' for Structured Outputs: 'C' is an invalid start of a value
```

**Meaning:** JSON schema definition itself is invalid. This should never occur in production.

**Action:** Verify Schemas.cs JSON syntax if this appears.

---

## Monitoring Strategy

### 1. Event Aggregation

Sample queries for log aggregation systems:

```bash
# Count telemetry fallback events by schema
grep "[TELEMETRY]" logs/* | grep "json_deserialization_fallback" | 
  awk -F'schema=' '{print $2}' | awk -F' ' '{print $1}' | sort | uniq -c

# Total fallback count
grep -c "[TELEMETRY]" logs/*

# Schema violations
grep -c "[SCHEMA_VIOLATION]" logs/*

# Enforcement failures
grep -c "[SCHEMA_ENFORCEMENT]" logs/*
```

### 2. Alert Thresholds

Recommended thresholds for alerting:

| Event Type | Good | Warning | Critical |
|------------|------|---------|----------|
| **[TELEMETRY] per hour** | < 5 | 5-20 | > 20 |
| **[SCHEMA_VIOLATION] per hour** | < 3 | 3-10 | > 10 |
| **[SCHEMA_ENFORCEMENT] per day** | 0 | 1-5 | > 5 |

### 3. Dashboards

Suggested dashboard metrics:

```
┌─────────────────────────────────────────┐
│ Schema Enforcement Health Dashboard     │
├─────────────────────────────────────────┤
│ Telemetry Events (1h):        12         │
│ Schema Violations (1h):        3         │
│ Strict Success Rate:           97.4%    │
│ Fallback Usage Rate:           2.6%     │
│                                         │
│ By Schema Type:                         │
│ - engineer_brief:              8        │
│ - category_classification:     3        │
│ - follow_up_questions:         1        │
│ - case_packet:                 0        │
│                                         │
│ Trend (24h): ▁▂▂▃▂▂▃▄▃▂▂▁    Stable    │
└─────────────────────────────────────────┘
```

---

## Log Parsing Examples

### Extract schema names from telemetry
```bash
grep "\[TELEMETRY\].*json_deserialization_fallback" logs/* |
  sed 's/.*schema=//' | sed 's/ error.*//' | sort | uniq -c | sort -rn
```

### Find issues with specific schema
```bash
grep "\[TELEMETRY\].*schema=engineer_brief" logs/*
```

### Timeline of violations
```bash
grep "\[SCHEMA_VIOLATION\]" logs/* | 
  sed 's/^.*\([0-9]\{2\}:[0-9]\{2\}:[0-9]\{2\}\).*/\1/' | 
  sort | uniq -c
```

### Extract error messages
```bash
grep "\[TELEMETRY\]" logs/* | 
  sed 's/.*error=//' | 
  sort | uniq -c | sort -rn | head -10
```

---

## Debugging with Telemetry

### Scenario 1: High Engineer Brief Telemetry

**Symptom:** Frequent `[TELEMETRY] json_deserialization_fallback schema=engineer_brief` events

**Investigation:**
1. Check error messages - are they about `possible_duplicates` field?
2. Verify Schemas.EngineerBriefSchema JSON is valid
3. Review Prompts.GenerateEngineerBrief - are instructions clear?
4. Check if LLM model changed or behavior shifted

**Solution:**
- If only missing optional fields → OK, expected behavior
- If missing required fields → Adjust prompt to emphasize importance
- If completely malformed → Escalate to prompt engineering

### Scenario 2: Repeated Schema Enforcement Failures

**Symptom:** Multiple `[SCHEMA_ENFORCEMENT] Failed to use json_schema format` events

**Investigation:**
1. Check if model changed from gpt-4o to older version
2. Verify schema size doesn't exceed 5000 object properties
3. Check if recursive schemas are valid

**Solution:**
- Update model to gpt-4o-2024-08-06 or later
- Simplify schema if too complex
- Enable more verbose logging to see full error

### Scenario 3: Missing Required Fields

**Symptom:** `[SCHEMA_VIOLATION] EngineerBrief missing required 'summary' field`

**Investigation:**
1. Check if fallback parsing even ran
2. Review raw JSON from `Response: {...}` in log
3. Verify schema defines field as required

**Solution:**
- Adjust prompt to force LLM to always include field
- Consider if field should be optional
- Test with different LLM model

---

## Integration with Monitoring Systems

### Application Insights (Azure)
```csharp
// Custom logging via Application Insights
_telemetryClient.TrackEvent("SchemaViolation", 
    new { schema = "engineer_brief", field = "summary", issueId = "123" });
```

### CloudWatch (AWS)
```json
{
  "timestamp": "2026-01-16T10:30:45Z",
  "level": "WARNING",
  "message": "[TELEMETRY] json_deserialization_fallback schema=engineer_brief error=...",
  "schema": "engineer_brief",
  "event_type": "telemetry_fallback"
}
```

### Splunk
```
source="/var/log/app" 
| search "\[TELEMETRY\]" 
| stats count by schema
| chart count by schema
```

### Datadog
```
service:supportbot schema:engineer_brief 
| stats count by error_type
```

---

## Best Practices

1. **Monitor Continuously**
   - Set up alerts for telemetry event spikes
   - Track trends over days/weeks
   - Watch for model behavior changes

2. **Act on Patterns**
   - If one schema consistently fails → Refactor prompt
   - If all schemas spike → Check LLM model/version
   - If intermittent → Could be quota limits

3. **Optimize Iteratively**
   - Use telemetry to identify problem areas
   - Test prompt improvements
   - Measure impact with before/after telemetry counts

4. **Document Findings**
   - Keep runbook of known issues and solutions
   - Track what telemetry patterns mean
   - Share insights with team

---

**Last Updated:** January 16, 2026  
**Version:** 1.0 (Implementation Complete)
