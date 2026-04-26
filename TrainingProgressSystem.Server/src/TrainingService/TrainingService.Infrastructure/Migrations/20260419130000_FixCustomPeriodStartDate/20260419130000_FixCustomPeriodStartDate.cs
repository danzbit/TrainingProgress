using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainingService.Infrastructure.Data;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260419130000_FixCustomPeriodStartDate;

[DbContext(typeof(TrainingServiceDbContext))]
[Migration("20260419130000_FixCustomPeriodStartDate")]
public partial class FixCustomPeriodStartDate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in FixCustomPeriodStartDateScripts.LoadUpBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in FixCustomPeriodStartDateScripts.LoadDownBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }
}
