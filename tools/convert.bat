@echo off
setlocal enabledelayedexpansion

REM Check if ffmpeg is installed
where ffmpeg >nul 2>&1
if %errorlevel% neq 0 (
    echo ffmpeg is not installed or not in the system PATH.
    pause
    exit /b 1
)

REM Define input and output directories
set "input_dir=videos"
set "output_dir=images"

REM Create output directory if it doesn't exist
if not exist "%output_dir%" (
    mkdir "%output_dir%"
)

REM Loop through all MP4 files in the input directory
for %%f in ("%input_dir%\*.mp4") do (
    set "input_file=%%f"
    set "output_file=%output_dir%\%%~nf.gif"

    REM Convert MP4 to GIF
    ffmpeg -y -i "!input_file!" -vf "fps=10,scale=iw:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" -c:v gif "!output_file!"

    REM Check if the conversion was successful
    if %errorlevel% neq 0 (
        echo Failed to convert "!input_file!" to "!output_file!".
    ) else (
        echo Successfully converted "!input_file!" to "!output_file!".
    )
)

echo Conversion process completed.
pause