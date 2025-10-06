@echo off
setlocal enabledelayedexpansion

set "LIST_FILE=C:\Users\natha\elements_to_copy.txt"
set /a lineNum=0

REM Base backup location (e.g., your external drive letter)
set "BASE_DEST=D:\"

REM Read the list file line by line
for /f "usebackq delims=" %%a in ("%LIST_FILE%") do (
    set /a lineNum+=1
    if !lineNum! equ 1 (
        REM First line is the destination folder name
        set "DEST_FOLDER=%%a"
        set "FULL_DEST=%BASE_DEST%!DEST_FOLDER!"
        echo Destination folder is: !FULL_DEST!
        REM Ensure destination root exists
        if not exist "!FULL_DEST!" mkdir "!FULL_DEST!"
    ) else (
        REM Check if path is a directory
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