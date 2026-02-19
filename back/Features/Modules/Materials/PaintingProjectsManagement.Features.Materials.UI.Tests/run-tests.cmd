@echo off
REM Batch script to run Playwright tests
REM This script is used by Aspire to execute the tests
REM Since dotnet test invokes MSBuild with VSTest target (which is deprecated in .NET 10),
REM we build the project first and then run the executable directly

setlocal

set "PROJECT_FILE=%~dp0PaintingProjectsManagement.Features.Materials.UI.Tests.csproj"
set "PROJECT_DIR=%~dp0"

echo Running Playwright UI tests...
echo Project file: %PROJECT_FILE%
echo Working directory: %PROJECT_DIR%

REM Build the project first with the new test platform enabled
echo Building test project...
dotnet build "%PROJECT_FILE%" /p:EnableTestingPlatformDotnetTestSupport=true /p:TestingPlatformDotnetTestSupport=true --verbosity normal

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    exit /b %ERRORLEVEL%
)

REM Find and run the test executable directly
REM The executable will be in bin\<Configuration>\<TargetFramework>\<ProjectName>.exe
REM We'll use Debug configuration by default, but check for Release if Debug doesn't exist
set "EXE_PATH=%PROJECT_DIR%bin\Debug\net10.0\PaintingProjectsManagement.Features.Materials.UI.Tests.exe"
if not exist "%EXE_PATH%" (
    set "EXE_PATH=%PROJECT_DIR%bin\Release\net10.0\PaintingProjectsManagement.Features.Materials.UI.Tests.exe"
)

if not exist "%EXE_PATH%" (
    echo Error: Test executable not found. Expected at: %EXE_PATH%
    exit /b 1
)

echo Running test executable: %EXE_PATH%
"%EXE_PATH%"

REM Exit with the test exit code
exit /b %ERRORLEVEL%
