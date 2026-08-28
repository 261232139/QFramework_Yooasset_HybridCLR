@echo off
setlocal

set "TOOL_ROOT=%~dp0"
set "PROJECT_ROOT=%TOOL_ROOT%..\.."
set "LUBAN=%TOOL_ROOT%Luban\Luban.dll"
set "OUTPUT_ROOT=%PROJECT_ROOT%\PegSolitaireProject\Assets\Game\Config"

if not exist "%LUBAN%" (
    echo [Luban] Generator not found: "%LUBAN%"
    exit /b 1
)

if not exist "%OUTPUT_ROOT%" mkdir "%OUTPUT_ROOT%"

dotnet "%LUBAN%" ^
    -t client ^
    -c cs-bin ^
    -d bin ^
    --conf "%TOOL_ROOT%luban.conf" ^
    -x outputCodeDir="%OUTPUT_ROOT%\Code" ^
    -x outputDataDir="%OUTPUT_ROOT%\Data" ^
    -x pathValidator.rootDir="%PROJECT_ROOT%\PegSolitaireProject"

exit /b %ERRORLEVEL%
