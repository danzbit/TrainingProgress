using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainingService.Infrastructure.Data;
using TrainingService.Infrastructure.Migrations._20260419100000_RemoveGoalFilter;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260419100000_RemoveGoalFilter;

[DbContext(typeof(TrainingServiceDbContext))]
[Migration("20260419100000_RemoveGoalFilter")]
public partial class RemoveGoalFilter : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in RemoveGoalFilterScripts.LoadUpBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in RemoveGoalFilterScripts.LoadDownBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }
}
