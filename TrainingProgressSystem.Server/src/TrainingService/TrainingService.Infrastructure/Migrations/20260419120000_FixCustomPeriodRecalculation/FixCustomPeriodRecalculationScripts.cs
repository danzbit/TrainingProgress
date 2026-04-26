using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TrainingService.Infrastructure.Migrations._20260419120000_FixCustomPeriodRecalculation;

internal static class FixCustomPeriodRecalculationScripts
{
    private const string UpResource = "TrainingService.Infrastructure.Migrations.FixCustomPeriodRecalculation.20260419120000_FixCustomPeriodRecalculation.Up.sql";
    private const string DownResource = "TrainingService.Infrastructure.Migrations.FixCustomPeriodRecalculation.20260419120000_FixCustomPeriodRecalculation.Down.sql";

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
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static IReadOnlyList<string> SplitIntoBatches(string script)
    {
        var batches = new List<string>();
        var current = new StringBuilder();

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
                current.AppendLine(line);
            }
        }

        var last = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(last))
            batches.Add(last);

        return batches;
    }
}
