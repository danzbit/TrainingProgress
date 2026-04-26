using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TrainingService.Infrastructure.Migrations._20260315213000_GoalSqlOptimizations;

internal static class GoalSqlOptimizationScripts
{
    private const string UpResource = "TrainingService.Infrastructure.Migrations.GoalSqlOptimizations.20260315203657_GoalSqlOptimizations.Up.sql";
    private const string DownResource = "TrainingService.Infrastructure.Migrations.GoalSqlOptimizations.20260315203657_GoalSqlOptimizations.Down.sql";

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
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static IReadOnlyList<string> SplitIntoBatches(string script)
    {
        var batches = new List<string>();
        var current = new StringBuilder();

        using var reader = new StringReader(script);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            if (GoBatchSeparatorRegex.IsMatch(line))
            {
                var batch = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    batches.Add(batch);
                }

                current.Clear();
                continue;
            }

            current.AppendLine(line);
        }

        var lastBatch = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(lastBatch))
        {
            batches.Add(lastBatch);
        }

        return batches;
    }
}
