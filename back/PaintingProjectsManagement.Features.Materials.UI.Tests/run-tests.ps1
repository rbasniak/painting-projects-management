# PowerShell script to run Playwright tests
# This script is used by Aspire to execute the tests

$ErrorActionPreference = "Stop"

$projectFile = Join-Path $PSScriptRoot "PaintingProjectsManagement.Features.Materials.UI.Tests.csproj"

Write-Host "Running Playwright UI tests..."
Write-Host "Project file: $projectFile"
Write-Host "Working directory: $PSScriptRoot"

# Run the tests
dotnet test $projectFile --verbosity normal

# Exit with the test exit code
exit $LASTEXITCODE
