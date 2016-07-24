Approximate order of operations:

To export the latest "legacy" set of OPSIDs run the following:
export-opsids.bat - file for exporting current OPSIDs

Then add proper prefix:
import-prefix.sql - prefix for creating LegacyOpsIds table to be added to the export

Then add GO and new lines:
sql-split.pl - script for inserting GO and new line every 1000 lines

Then run to import all data into SQL Server:
ImportLegacyOpsIds.bat - file for importing OPSIDs into LegacyOpsIds table

Then run in SSMS or isql or osql or sqlcmd:
PopulateOpsIdsInExternalReferences.sql - file for linking old/legacy OPSIDs and assigning new ones

These are predefined OPSIDs from two legacy versions:
LegacyOpsIds.zip - OPS v1.5 OPSIDs
Ops2OpsIds.zip - OPS v2.0 OPSIDs