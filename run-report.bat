@ECHO off
dotnet build Reports.sln
for /l %%x in (1, 1, 5) do (echo:)
call bin\Debug\net8.0\Reports.exe
pause
