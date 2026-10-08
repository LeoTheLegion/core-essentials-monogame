#!/bin/bash

dotnet test CoreEssentials.Tests/
test_exit=$?

# Always surface a per-file coverage report, even when the 80% threshold gate fails.
pwsh -NoProfile -ExecutionPolicy Bypass "$(dirname "$0")/coverage-report.ps1"

exit $test_exit
