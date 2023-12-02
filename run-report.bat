@ECHO off
dotnet build %CD%\Reports.sln
SET PATH=%CD%\bin\Debug\net8.0\Reports.exe
call %PATH%
pause
