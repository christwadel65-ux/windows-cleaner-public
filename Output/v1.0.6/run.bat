@echo off
REM Script de lancement Windows Cleaner v1.0.6
REM Usage: Execution simple avec double-clic

echo ========================================
echo    Windows Cleaner v1.0.6
echo ========================================
echo.

REM Verification des droits administrateur
net session >nul 2>&1
if %errorLevel% == 0 (
    echo [+] Mode Administrateur : OUI
) else (
    echo [!] Mode Administrateur : NON
    echo.
    echo Certaines fonctionnalites necessitent des droits administrateur.
    echo Pour activer toutes les fonctionnalites :
    echo   - Clic droit sur ce fichier ^> "Executer en tant qu'administrateur"
    echo.
    pause
)

echo.
echo Lancement de Windows Cleaner...
echo.

REM Lancement de l'application
windows-cleaner.exe

echo.
echo ========================================
echo Fermeture...
echo ========================================
pause
