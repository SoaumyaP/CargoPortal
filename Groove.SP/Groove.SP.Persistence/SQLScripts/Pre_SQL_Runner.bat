echo off

SET /P _server= Server:
SET /P _dbName= DB:
SET /P _username= Username:
SET /P _password= Password:

for %%G in (StoreProcs/*.sql) do sqlcmd -S "%_server%" -d"%_dbName%" -U "%_username%" -P "%_password%" -i"StoreProcs/%%G"
for %%G in (Functions/*.sql) do sqlcmd -S "%_server%" -d"%_dbName%" -U "%_username%" -P "%_password%" -i"Functions/%%G"

set /p delExit=Press the ENTER key to exit...: