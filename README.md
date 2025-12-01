# Python + C# FastAPI Integration POC

Simple proof-of-concept: Run a Python FastAPI server from C# using pythonnet with virtual environment.

## Prerequisites

- **Python 3.8 - 3.12** (pythonnet doesn't support 3.13 yet)
- **.NET 8.0+**

## Quick Start

1. **Setup** (PowerShell):
```powershell
.\setup.ps1
```

2. **Run**:
```powershell
cd csharp
dotnet run
```

3. **Teardown**:
```powershell
# Press any key in the running app to stop it
# Or just Ctrl+C to terminate
```

## Clean Up

To completely remove the project:
```powershell
# Stop any running processes
c
# Remove virtual environment
Remove-Item -Recurse -Force .venv

# Remove build artifacts
Remove-Item -Recurse -Force csharp\bin, csharp\obj
```

## What It Does

- ✅ Starts a Python FastAPI mock inference API from C#
- ✅ Uses a Python virtual environment via pythonnet
- ✅ Makes HTTP calls to test the API
- ✅ Simple sentiment analysis endpoint

## Project Structure

```
├── python/
│   ├── app.py              # FastAPI app with /health and /predict
│   └── requirements.txt    # fastapi, uvicorn, pydantic
├── csharp/
│   ├── Program.cs          # Everything in one file
│   └── *.csproj
├── .venv/                  # Virtual environment
└── setup.ps1               # Automated setup
```

## API Endpoints

- `GET /health` - Health check
- `POST /predict` - Sentiment prediction
  ```json
  { "text": "I love this!" }
  → { "text": "...", "sentiment": "positive", "confidence": 0.87 }
  ```

## How It Works

The C# code:
1. Finds Python DLL from venv config
2. Initializes pythonnet with venv site-packages
3. Starts FastAPI server in background thread
4. Makes HTTP requests to test the API

Key pythonnet setup:
```csharp
Runtime.PythonDLL = pythonDll;
PythonEngine.Initialize();
PythonEngine.PythonPath = $"{PythonEngine.PythonPath};{venv_site_packages}";
```

## Troubleshooting

**Python DLL not found?**
- Run `setup.ps1` first
- Verify Python 3.8+ is installed

**Module import errors?**
- Check `.venv\Lib\site-packages` exists
- Reinstall: `.\.venv\Scripts\Activate.ps1` then `pip install -r python\requirements.txt`

**Port 8000 in use?**
- Change port in `Program.cs` and `app.py`

## References

- [pythonnet docs](https://pythonnet.github.io/pythonnet/)
- [Virtual env discussion](https://github.com/pythonnet/pythonnet/discussions/2334)
