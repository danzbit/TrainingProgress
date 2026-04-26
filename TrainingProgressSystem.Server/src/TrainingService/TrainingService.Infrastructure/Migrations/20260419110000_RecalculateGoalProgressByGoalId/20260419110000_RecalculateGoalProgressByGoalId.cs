using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainingService.Infrastructure.Data;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260419110000_RecalculateGoalProgressByGoalId;

[DbContext(typeof(TrainingServiceDbContext))]
[Migration("20260419110000_RecalculateGoalProgressByGoalId")]
public partial class RecalculateGoalProgressByGoalId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in RecalculateGoalProgressByGoalIdScripts.LoadUpBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in RecalculateGoalProgressByGoalIdScripts.LoadDownBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }
}
