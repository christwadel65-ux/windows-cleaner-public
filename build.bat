@echo off
setlocal
cd /d "d:\GitHub\Windows Cleaner"
echo ====================================
echo  Windows Cleaner v2.0.3 - Build
echo ====================================
echo.
echo Compilation en cours...
echo.
"C:\Program Files\dotnet\dotnet.exe" build src\WindowsCleaner\WindowsCleaner.csproj --configuration Release
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ====================================
    echo ^>^>^> COMPILATION REUSSIE ! ^<^<^<
    echo ====================================
    echo.
    echo Executable: bin\Release\net10.0-windows\windows-cleaner.exe
    echo.
) else (
    echo.
    echo ====================================
    echo ^>^>^> ERREUR lors de la compilation ^<^<^<
    echo ====================================
    echo.
)
endlocal
