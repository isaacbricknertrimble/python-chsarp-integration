
Write-Host "Setting up Python + C# FastAPI POC..." -ForegroundColor Cyan

if (-not (Get-Command python -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Python not found. Install Python 3.8-3.12" -ForegroundColor Red
    exit 1
}

$pythonVersion = python --version
Write-Host "Python: $pythonVersion" -ForegroundColor Green

if ($pythonVersion -match "Python 3\.13") {
    Write-Host "WARNING: Python 3.13 is not yet supported by pythonnet!" -ForegroundColor Yellow
    Write-Host "Please install Python 3.12 or earlier from https://www.python.org/downloads/" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Continue anyway? (not recommended) [y/N]"
    if ($continue -ne "y") {
        exit 1
    }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: .NET SDK not found. Install .NET 8.0+" -ForegroundColor Red
    exit 1
}

Write-Host ".NET: $(dotnet --version)" -ForegroundColor Green

Write-Host "`nStopping Python processes..."
Get-Process python* -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2


if (Test-Path ".venv") {
    Write-Host "Removing old virtual environment..."
    cmd /c "rmdir /s /q .venv" 2>$null
    Start-Sleep -Seconds 1
}

Write-Host "Creating virtual environment..."
python -m venv .venv --clear --without-pip

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create venv" -ForegroundColor Red
    exit 1
}

Write-Host "Installing pip..."
& .venv\Scripts\python.exe -m ensurepip

Write-Host "Installing Python packages..."
& .venv\Scripts\python.exe -m pip install --no-cache-dir fastapi uvicorn pydantic

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to install packages" -ForegroundColor Red
    exit 1
}

Write-Host "Python packages installed:" -ForegroundColor Green
& .venv\Scripts\python.exe -m pip list

Write-Host "`nRestoring C# packages..."
Set-Location csharp
dotnet restore --verbosity quiet
dotnet build --configuration Release --verbosity quiet
Set-Location ..

Write-Host "`nSetup complete!" -ForegroundColor Green
Write-Host "`nTo run: " -ForegroundColor Cyan -NoNewline
Write-Host "cd csharp && dotnet run"
