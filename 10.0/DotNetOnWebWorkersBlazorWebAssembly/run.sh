#!/bin/bash

# Check if there are no arguments
if [ -z "$1" ]; then
    echo "Please provide an action you would like to perform:"
    echo "build - builds both projects: Blazor and WASM"
    echo "clean - removes the artifacts from the previous build"
    echo "cleanbuild - cleans the previous build and builds both projects: Blazor and WASM"
    echo "run - runs the Blazor project"
    exit 1
fi

# Set the action from the first argument
action="$1"

# Define functions
build() {
    dotnet publish -c Debug "blazorWasm/blazorWasm.csproj"
    exit 0
}

clean() {
    echo "Cleaning the previous build..."
    blazorBin="blazorWasm/bin"
    blazorObj="blazorWasm/obj"
    dotnetPublish="blazorWasm/wwwroot/dotnet"

    for dir in "$blazorBin" "$blazorObj" "$dotnetPublish"; do
        [ -d "$dir" ] && rm -rf "$dir"
    done

    exit 0
}

# Switch-case logic based on the action
case "$action" in
    build)
        build
        ;;
    clean)
        clean
        ;;
    cleanbuild)
        clean
        build
        ;;
    run)
        dotnet run --project "blazorWasm/blazorWasm.csproj"
        ;;
    *)
        echo "Invalid action: $action"
        exit 1
        ;;
esac
