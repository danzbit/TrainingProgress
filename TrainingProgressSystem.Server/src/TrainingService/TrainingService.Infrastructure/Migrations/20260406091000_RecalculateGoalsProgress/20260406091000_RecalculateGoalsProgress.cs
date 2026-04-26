using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainingService.Infrastructure.Data;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260406091000_RecalculateGoalsProgress;

[DbContext(typeof(TrainingServiceDbContext))]
[Migration("20260406091000_RecalculateGoalsProgress")]
public partial class RecalculateGoalsProgress : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in RecalculateGoalsProgressScripts.LoadUpBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in RecalculateGoalsProgressScripts.LoadDownBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }
}
