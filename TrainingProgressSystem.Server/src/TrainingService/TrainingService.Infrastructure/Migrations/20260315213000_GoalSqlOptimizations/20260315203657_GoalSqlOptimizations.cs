using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260315213000_GoalSqlOptimizations
{
    /// <inheritdoc />
    public partial class GoalSqlOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var batch in GoalSqlOptimizationScripts.LoadUpBatches())
            {
                migrationBuilder.Sql(batch);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var batch in GoalSqlOptimizationScripts.LoadDownBatches())
            {
                migrationBuilder.Sql(batch);
            }
        }
    }
}
