using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TrainingService.Infrastructure.Data;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260315235500_GoalProgressSnapshots;

[DbContext(typeof(TrainingServiceDbContext))]
[Migration("20260315235500_GoalProgressSnapshots")]
public partial class GoalProgressSnapshots : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in GoalProgressSnapshotScripts.LoadUpBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var batch in GoalProgressSnapshotScripts.LoadDownBatches())
        {
            migrationBuilder.Sql(batch);
        }
    }
}
