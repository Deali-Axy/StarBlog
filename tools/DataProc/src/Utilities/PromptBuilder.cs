using System.Text;

namespace DataProc.Utilities;

/// <summary>
/// A builder for creating prompts based on a template and provided context variables.
/// </summary>
public class PromptBuilder {
    private readonly string _template;

    private readonly Dictionary<string, string> _context =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of PromptBuilder with the specified template.
    /// </summary>
    /// <param name="template">The template string containing placeholders like {key}.</param>
    public PromptBuilder(string template) {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Template cannot be null or empty.", nameof(template));

        _template = template;
    }

    /// <summary>
    /// Adds a context variable that will replace the corresponding placeholder in the template.
    /// </summary>
    /// <param name="key">The placeholder key without braces.</param>
    /// <param name="value">The value to substitute.</param>
    /// <returns>The same PromptBuilder, enabling fluent chaining.</returns>
    public PromptBuilder AddParameter(string key, string value) {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        _context[key] = value ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Builds and returns the final prompt by replacing all placeholders in the template.
    /// </summary>
    /// <returns>The generated prompt string.</returns>
    public string Build() {
        var result = new StringBuilder(_template);

        foreach (var kvp in _context) {
            // Replace all occurrences of {key}
            result.Replace("{{" + kvp.Key + "}}", kvp.Value);
        }

        return result.ToString();
    }

    /// <summary>
    /// Creates a new PromptBuilder instance from a template.
    /// </summary>
    /// <param name="template">The prompt template string.</param>
    /// <returns>A new PromptBuilder.</returns>
    public static PromptBuilder Create(string template) => new PromptBuilder(template);
}