@echo off
setlocal ENABLEDELAYEDEXPANSION

if "%DEBUG%" == "true" (
    PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "scripts\build\ci-build.ps1" -Verbose
) else (
    PowerShell -NonInteractive -NoProfile -ExecutionPolicy Unrestricted -Command "scripts\build\ci-build.ps1"
)
echo From Cmd.exe: sonar-scanner-msbuild ci-build.ps1 exited with exit code !errorlevel!
exit !errorlevel!