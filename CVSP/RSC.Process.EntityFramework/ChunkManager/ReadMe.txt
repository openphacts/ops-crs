#	How to update ChunkManager DB

1. Enable-Migrations -ContextTypeName RSC.Process.EntityFramework.ChunkManagerContext -MigrationsDirectory:ChunkManagerMigrations
2. Add-Migration -configuration RSC.Process.EntityFramework.ChunkManagerMigrations.Configuration init
3. Update-Database -configuration RSC.Process.EntityFramework.ChunkManagerMigrations.Configuration -Verbose
