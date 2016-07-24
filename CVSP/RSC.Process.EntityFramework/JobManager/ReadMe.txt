#	How to update JobManager DB

1. Enable-Migrations -ContextTypeName RSC.Process.EntityFramework.JobManagerContext -MigrationsDirectory:JobManagerMigrations
2. Add-Migration -configuration RSC.Process.EntityFramework.JobManagerMigrations.Configuration init
3. Update-Database -configuration RSC.Process.EntityFramework.JobManagerMigrations.Configuration -Verbose
