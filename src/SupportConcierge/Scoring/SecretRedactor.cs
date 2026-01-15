using System.Text.RegularExpressions;

namespace SupportConcierge.Scoring;

/// <summary>
/// Handles detection and redaction of sensitive information (secrets) from text.
/// Patterns are loaded from the validators configuration.
/// </summary>
public class SecretRedactor
{
    private readonly List<Regex> _secretPatterns;
    private const string RedactionPlaceholder = "[REDACTED]";

    public SecretRedactor(List<string> secretPatternStrings)
    {
        _secretPatterns = secretPatternStrings
            .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Redacts secrets from the given text using configured patterns.
    /// Returns a tuple of (redactedText, secretFindings) where secretFindings 
    /// is a list of found secrets with their descriptions.
    /// </summary>
    public (string RedactedText, List<string> SecretFindings) RedactSecrets(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return (text ?? string.Empty, new List<string>());
        }

        var secretFindings = new List<string>();
        var redactedText = text;

        foreach (var pattern in _secretPatterns)
        {
            var matches = pattern.Matches(redactedText);
            
            foreach (Match match in matches)
            {
                if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
                {
                    // Log the finding (just the type, not the actual secret)
                    var matchValue = match.Value;
                    var secretType = DetermineSecretType(matchValue);
                    
                    secretFindings.Add($"Found {secretType}: {matchValue.Substring(0, Math.Min(20, matchValue.Length))}...");
                    
                    // Replace the matched secret with placeholder
                    redactedText = redactedText.Replace(matchValue, RedactionPlaceholder);
                }
            }
        }

        return (redactedText, secretFindings);
    }

    /// <summary>
    /// Determines the type of secret based on common patterns.
    /// </summary>
    private string DetermineSecretType(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return "unknown";

        secret = secret.ToLower();

        if (secret.Contains("api") || secret.Contains("key") || secret.Contains("token"))
            return "API Key";
        
        if (secret.Contains("password") || secret.Contains("passwd") || secret.Contains("pwd"))
            return "Password";
        
        if (secret.Contains("secret"))
            return "Secret";
        
        if (secret.Contains("credential"))
            return "Credential";
        
        if (secret.Contains("bearer"))
            return "Bearer Token";
        
        if (Regex.IsMatch(secret, @"^[A-Za-z0-9+/]{40,}={0,2}$"))
            return "Base64 Encoded Secret";
        
        if (Regex.IsMatch(secret, @"^[0-9a-f]{32,}$"))
            return "Hash/Token";
        
        return "Sensitive Data";
    }
}
