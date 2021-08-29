@echo off
set configuration=Debug

call "%~dp0clean.bat"
dotnet test "%~dp0Aqua.sln" -c %configuration%

set exitcode=%errorlevel%
if %exitcode% neq 0 goto :failure

goto :end

:failure
echo build failed!
pause

:end
exit /b %exitcode%