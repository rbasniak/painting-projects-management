# Code Coverage Setup

This project has been configured with comprehensive code coverage reporting using Coverlet and ReportGenerator.

## What's Been Added

### 1. Coverage Configuration (`back/coverage.config`)
- Configures which assemblies to include/exclude from coverage
- Excludes test projects, third-party libraries, and generated code
- Focuses coverage on your main business logic

### 2. Updated GitHub Workflow (`.github/workflows/dotnet.yml`)
- Added coverage collection to all test runs
- Generates multiple report formats (HTML, Cobertura, LCOV, JSON)
- Creates coverage badges
- Comments on PRs with coverage information
- Uploads coverage reports as artifacts

### 3. Coverage Badge Workflow (`.github/workflows/coverage-badge.yml`)
- Optional workflow for dynamic badge updates
- Requires a GitHub Gist and secret setup

## How to Use

### Running Tests with Coverage Locally

```bash
cd back
dotnet test --coverage --coverage-output-format cobertura --coverage-settings coverage.config
```

### Viewing Coverage Reports

1. Run the GitHub workflow
2. Download the `coverage-reports` artifact
3. Open `index.html` in your browser for detailed coverage analysis

### Updating the README Badge

After your first successful workflow run:

1. Go to the workflow run artifacts
2. Download the `coverage-info` artifact
3. Check the `coverage-info.txt` file for the badge URL
4. Update the README.md badge URL with your actual coverage percentage

Example:
```markdown
[![Code Coverage](https://img.shields.io/badge/coverage-75%25-yellow.svg)](https://github.com/your-username/your-repo/actions)
```

### Setting Up Dynamic Badges (Optional)

For automatic badge updates:

1. Create a GitHub Gist with a JSON file
2. Add `GIST_SECRET` to your repository secrets
3. Update the `gistID` in `.github/workflows/coverage-badge.yml`
4. The badge will update automatically after each successful build

## Coverage Configuration Details

The `coverage.config` file includes:

- **Include**: Your main feature assemblies and rbkApiModules
- **Exclude**: Test projects, Microsoft libraries, and third-party packages
- **ExcludeByFile**: Generated files, migrations, and configuration files
- **ExcludeByAttribute**: Code marked with coverage exclusion attributes

## Troubleshooting

### No Coverage Reports Generated
- Ensure coverlet.collector is installed in test projects
- Check that tests are actually running
- Verify the coverage.config file is in the correct location

### Low Coverage Percentage
- Review the coverage report to identify uncovered code
- Add tests for missing scenarios
- Consider if some code should be excluded from coverage

### Workflow Failures
- Check that all required tools are installed
- Verify file paths in the workflow
- Ensure sufficient permissions for artifact uploads
