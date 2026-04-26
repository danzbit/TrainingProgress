using System.Reflection;
using System.Text.RegularExpressions;

namespace TrainingService.Infrastructure.Migrations._20260419100000_RemoveGoalFilter;

internal static class RemoveGoalFilterScripts
{
    private const string UpResource = "TrainingService.Infrastructure.Migrations.RemoveGoalFilter.20260419100000_RemoveGoalFilter.Up.sql";
    private const string DownResource = "TrainingService.Infrastructure.Migrations.RemoveGoalFilter.20260419100000_RemoveGoalFilter.Down.sql";

    private static readonly Regex GoBatchSeparatorRegex = new(
        @"^\s*GO\s*(?:--.*)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<string> LoadUpBatches() => SplitIntoBatches(Load(UpResource));

    public static IReadOnlyList<string> LoadDownBatches() => SplitIntoBatches(Load(DownResource));

    private static string Load(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException($"Embedded SQL resource not found: {resourceName}");
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static IReadOnlyList<string> SplitIntoBatches(string script)
    {
        var batches = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var line in script.Split('\n'))
        {
            if (GoBatchSeparatorRegex.IsMatch(line.TrimEnd('\r')))
            {
                var batch = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(batch))
                    batches.Add(batch);
                current.Clear();
            }
            else
            {
                current.AppendLine(line.TrimEnd('\r'));
            }
        }

        var lastBatch = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(lastBatch))
            batches.Add(lastBatch);

        return batches;
    }
}
