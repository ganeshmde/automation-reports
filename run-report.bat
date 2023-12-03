@ECHO off
dotnet build Reports.sln
call bin\Debug\net8.0\Reports.exe
pause
