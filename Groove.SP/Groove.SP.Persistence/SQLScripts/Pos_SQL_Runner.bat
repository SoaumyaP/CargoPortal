echo off

SET /P _server= Server:
SET /P _dbName= DB:
SET /P _username= Username:
SET /P _password= Password:

for %%G in (Views/*.sql) do sqlcmd -S "%_server%" -d"%_dbName%" -U "%_username%" -P "%_password%" -i"Views/%%G"
for %%G in (Triggers/*.sql) do sqlcmd -S "%_server%" -d"%_dbName%" -U "%_username%" -P "%_password%" -i"Triggers/%%G"
for %%G in (Indexes/*.sql) do sqlcmd -S "%_server%" -d"%_dbName%" -U "%_username%" -P "%_password%" -i"Indexes/%%G"
for %%G in (Sequences/*.sql) do sqlcmd -S "%_server%" -d"%_dbName%" -U "%_username%" -P "%_password%" -i"Sequences/%%G"

set /p delExit=Press the ENTER key to exit...: