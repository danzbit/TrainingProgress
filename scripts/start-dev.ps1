#!/usr/bin/env pwsh
# Development Startup Script for Training Progress System
# Usage: ./start-dev.ps1 [-Build] [-Detached] [-NoCache]

param(
    [switch]$Build = $false,
    [switch]$Detached = $false,
    [switch]$NoCache = $false
)

# Get the root directory (parent of scripts folder)
$RootDir = Split-Path -Parent $PSScriptRoot

# Define colors
$ColorSuccess = 'Green'
$ColorError = 'Red'
$ColorInfo = 'Cyan'

# Helper functions
function OutputSuccess {
    param([string]$Message)
    Write-Host $Message -ForegroundColor $ColorSuccess
}

function OutputError {
    param([string]$Message)
    Write-Host $Message -ForegroundColor $ColorError
}

function OutputInfo {
    param([string]$Message)
    Write-Host $Message -ForegroundColor $ColorInfo
}

# Display header
OutputInfo "========================================================="
OutputInfo "  Training Progress System - Development Startup"
OutputInfo "========================================================="
OutputInfo ""

# Construct path to .env.local in root directory
$EnvLocalPath = Join-Path $RootDir ".env.local"

# Check if .env.local exists
if (-not (Test-Path $EnvLocalPath)) {
    OutputError "ERROR: .env.local file not found in root directory!"
    OutputInfo "Please create .env.local with your GROQ_API_KEY:"
    OutputInfo "  1. Copy from .env.example:"
    OutputInfo "     Copy-Item .env.example .env.local"
    OutputInfo "  2. Edit .env.local and add your actual GROQ_API_KEY"
    OutputInfo "  3. Run this script again"
    exit 1
}

# Load environment variables from .env.local
OutputInfo "Loading environment variables from .env.local..."
$envContent = Get-Content $EnvLocalPath
foreach ($line in $envContent) {
    if ($line -and -not $line.StartsWith("#")) {
        $parts = $line.Split("=", 2)
        if ($parts.Count -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim().Trim('"').Trim("'")
            [Environment]::SetEnvironmentVariable($key, $value)
            OutputSuccess "  OK: Set $key"
        }
    }
}

OutputSuccess "Environment variables loaded successfully"
OutputInfo ""

# Change to root directory
Set-Location $RootDir

# Build docker-compose command
$composeCmd = "docker-compose up"

if ($Build) {
    $composeCmd += " --build"
}

if ($Detached) {
    $composeCmd += " -d"
}

if ($NoCache) {
    $composeCmd += " --no-cache"
}

# Display what we're about to run
OutputInfo "Starting Docker Compose..."
OutputInfo "Command: $composeCmd"
OutputInfo ""

# Execute docker-compose with output
try {
    & docker-compose up $(if ($Build) { "--build" }) $(if ($Detached) { "-d" }) $(if ($NoCache) { "--no-cache" }) 2>&1
    $exitCode = $LASTEXITCODE
} catch {
    OutputError "ERROR: Failed to execute docker-compose: $_"
    exit 1
}

# Check result
if ($exitCode -eq 0 -or ($Detached -and $exitCode -eq 0)) {
    OutputSuccess "Docker Compose started successfully!"
    OutputInfo ""
    OutputInfo "Access the application at:"
    OutputInfo "  Frontend:     http://localhost"
    OutputInfo "  BFF API:      http://localhost:5000"
    OutputInfo "  Dashboard:    http://localhost:18888"
    OutputInfo ""
    OutputInfo "Useful commands:"
    OutputInfo "  docker-compose logs -f client"
    OutputInfo "  docker-compose logs -f bff"
    OutputInfo "  docker-compose ps"
    OutputInfo "  docker-compose down"
} else {
    OutputError "ERROR: Docker Compose failed (exit code: $exitCode)"
    OutputInfo ""
    OutputInfo "Trying to show client build logs..."
    & docker-compose logs client 2>&1
    exit 1
}
