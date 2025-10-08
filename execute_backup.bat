@echo off
setlocal enabledelayedexpansion

REM Get the folder of the batch file itself
set "SCRIPT_DIR=%~dp0"
set "LIST_FILE=%SCRIPT_DIR%elements_to_copy.txt"
set /a lineNum=0

for /f "usebackq delims=" %%a in ("%LIST_FILE%") do (
    set /a lineNum+=1
    if !lineNum! equ 1 (
        set "BASE_DEST=%%a:\"
        set "BASE_DEST=!BASE_DEST: =!"
    ) else if !lineNum! equ 2 (
        set "DEST_FOLDER=%%a"
        set "FULL_DEST=!BASE_DEST!!DEST_FOLDER!"
        echo Destination folder is: !FULL_DEST!
        if not exist "!FULL_DEST!" mkdir "!FULL_DEST!"
    ) else (
        if exist "%%a\" (
            echo Copying folder "%%a" to "!FULL_DEST!\%%~nxa"
            robocopy "%%a" "!FULL_DEST!\%%~nxa" /E /COPYALL /XJ
        ) else if exist "%%a" (
            echo Copying file "%%a" to "!FULL_DEST!"
            copy /Y "%%a" "!FULL_DEST!\"
        ) else (
            echo WARNING: %%a does not exist!
        )
    )
)
pause