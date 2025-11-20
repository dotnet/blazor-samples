@echo off
setlocal enabledelayedexpansion

REM Check if there are no arguments
if "%1"=="" (
    echo Please provide an action you would like to perform:
    echo build - builds both projects: Blazor and WASM
    echo clean - removes the artifacts from the previous build
    echo cleanbuild - cleans the previous build and builds both projects: Blazor and WASM
    echo run - runs the Blazor project
    exit /b 1
)


REM Set the action from the first argument
set "action=%1"

REM Switch-case logic based on the action
:switch
    if "%action%"=="build" (
        call :build
        goto :end
    ) else if "%action%"=="clean" (
        call :clean
        goto :end
    ) else if "%action%"=="cleanbuild" (
        call :clean
        call :build
        goto :end
    ) else if "%action%"=="run" (
        dotnet run --project "blazorWasm\blazorWasm.csproj"
    ) else (
        echo Invalid project name: %action%
        exit /b 1
    )


REM Define functions
:build
    dotnet publish -c Debug "blazorWasm\blazorWasm.csproj"
    exit /b 0

:clean
    echo Cleaning the previous build...
    set "blazorBin=blazorWasm\bin"
    set "blazorObj=blazorWasm\obj"
    set "dotnetPublish=blazorWasm\wwwroot\dotnet"
    if exist !blazorBin! rmdir /s /q !blazorBin!
    if exist !blazorObj! rmdir /s /q !blazorObj!
    if exist !dotnetPublish! rmdir /s /q !dotnetPublish!
    exit /b 0

:end
