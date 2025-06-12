using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGenreProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Rooms"" 
                ALTER COLUMN ""Genre"" TYPE integer 
                USING CASE 
                    WHEN ""Genre"" = 'Pop' THEN 152
                    WHEN ""Genre"" = 'Alternative' THEN 85
                    WHEN ""Genre"" = 'Rock' THEN 132
                    WHEN ""Genre"" = 'HipHop' THEN 116
                    WHEN ""Genre"" = 'Dance' THEN 113
                    WHEN ""Genre"" = 'Electronic' THEN 153
                    WHEN ""Genre"" = 'Jazz' THEN 144
                    WHEN ""Genre"" = 'Metal' THEN 129
                    ELSE 0
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Rooms"" 
                ALTER COLUMN ""Genre"" TYPE text 
                USING CASE 
                    WHEN ""Genre"" = '152' THEN 'Pop'
                    WHEN ""Genre"" = '85' THEN 'Alternative'
                    WHEN ""Genre"" = '132' THEN 'Rock'
                    WHEN ""Genre"" = '116' THEN 'HipHop'
                    WHEN ""Genre"" = '113' THEN 'Dance'
                    WHEN ""Genre"" = '153' THEN 'Electronic'
                    WHEN ""Genre"" = '144' THEN 'Jazz'
                    WHEN ""Genre"" = '129' THEN 'Metal'
                    ELSE 'Pop'
                END;
            ");
        }
    }
}
