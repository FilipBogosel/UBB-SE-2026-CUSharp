#nullable enable

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Represents a lexicon analysis row for the UI.
/// </summary>
public class LexiconWordItem
{
    public string Word { get; set; } = string.Empty;

    public int Count { get; set; }

    public string Display => $"{Word} ({Count})";
}
