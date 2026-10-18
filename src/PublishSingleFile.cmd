@echo off
setlocal
cd /d "%~dp0"
dotnet publish "%~dp0PrototypeUltimateTool.csproj" -c Release -p:PublishProfile=WinX64SingleFile
if errorlevel 1 (
    echo.
    echo Single-file publish failed. Review the errors above.
    exit /b %errorlevel%
)
echo.
echo Publish completed. Output: "%~dp0publish\win-x64\"
echo Keep settings.ini beside the EXE. Languages and Shaders folders remain external runtime assets.
exit /b 0
