using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainingService.Infrastructure.Data;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260419120000_FixCustomPeriodRecalculation;

[DbContext(typeof(TrainingServiceDbContext))]
[Migration("20260419120000_FixCustomPeriodRecalculation")]
public partial class FixCustomPeriodRecalculation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in FixCustomPeriodRecalculationScripts.LoadUpBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in FixCustomPeriodRecalculationScripts.LoadDownBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }
}
