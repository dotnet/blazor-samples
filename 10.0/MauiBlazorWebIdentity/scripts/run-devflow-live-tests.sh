#!/bin/bash
set -euo pipefail

# Runs only against the actual Development server and Mac Catalyst app.
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
STATE_DIR="$ROOT/scripts/.devflow-state-$$"
SERVER_PORT="${DEVFLOW_SERVER_PORT:-7157}"
SERVER_URL="${DEVFLOW_SERVER_URL:-https://localhost:$SERVER_PORT}"
AGENT_PORT="${DEVFLOW_AGENT_PORT:-${DEVFLOW_TEST_PORT:-10223}}"
SERVER_PID=""
APP_PID=""
STARTED_BROKER=0

cleanup_pid() {
    pid="$1"
    if [ -n "$pid" ] && kill -0 "$pid" 2>/dev/null; then
        kill "$pid" 2>/dev/null || true
        wait "$pid" 2>/dev/null || true
    fi
}

cleanup() {
    cleanup_pid "$APP_PID"
    cleanup_pid "$SERVER_PID"
    if [ "$STARTED_BROKER" -eq 1 ]; then
        maui devflow broker stop >/dev/null 2>&1 || true
    fi
    rm -rf "$STATE_DIR"
}
trap cleanup EXIT INT TERM

require_free_port() {
    port="$1"
    name="$2"
    if nc -z 127.0.0.1 "$port" >/dev/null 2>&1; then
        echo "$name port $port is already occupied; refusing to stop a process this script did not start." >&2
        exit 1
    fi
}

wait_for_url() {
    url="$1"
    name="$2"
    attempts=0
    until curl --silent --show-error --fail --insecure "$url" >/dev/null 2>&1; do
        attempts=$((attempts + 1))
        if [ "$attempts" -ge 60 ]; then
            echo "$name did not become ready." >&2
            exit 1
        fi
        sleep 1
    done
}

wait_for_cdp() {
    attempts=0
    until maui devflow --agent-port "$AGENT_PORT" --platform maccatalyst webview status 2>&1 |
        grep -q "CDP ready"; do
        attempts=$((attempts + 1))
        if [ "$attempts" -ge 120 ]; then
            echo "DevFlow Blazor CDP bridge did not become ready." >&2
            exit 1
        fi
        sleep 1
    done
}

require_free_port "$SERVER_PORT" "Development server"
require_free_port "$AGENT_PORT" "DevFlow agent"

mkdir -p "$STATE_DIR"
cd "$ROOT"

dotnet test MauiBlazorWeb.IdentityApi.Tests/MauiBlazorWeb.IdentityApi.Tests.csproj
dotnet build MauiBlazorWeb.Web/MauiBlazorWeb.Web.csproj
dotnet build MauiBlazorWeb/MauiBlazorWeb.csproj -f net10.0-maccatalyst
dotnet build MauiBlazorWeb.DevFlow.Tests/MauiBlazorWeb.DevFlow.Tests.csproj
dotnet build MauiBlazorWeb.WebUi.Tests/MauiBlazorWeb.WebUi.Tests.csproj

ASPNETCORE_ENVIRONMENT=Development \
ConnectionStrings__DefaultConnection="Data Source=$STATE_DIR/identity.db" \
dotnet MauiBlazorWeb.Web/bin/Debug/net10.0/MauiBlazorWeb.Web.dll --urls "$SERVER_URL" \
    >"$STATE_DIR/server.log" 2>&1 &
SERVER_PID=$!
wait_for_url "$SERVER_URL/health" "Development server"

WEB_UI_TESTS=1 \
DEVFLOW_SERVER_URL="$SERVER_URL" \
dotnet test MauiBlazorWeb.WebUi.Tests/MauiBlazorWeb.WebUi.Tests.csproj --no-build

if ! maui devflow broker status >/dev/null 2>&1; then
    maui devflow broker start
    STARTED_BROKER=1
fi
maui devflow broker status

DEVFLOW_AGENT_PORT="$AGENT_PORT" \
DEVFLOW_SERVER_URL="$SERVER_URL" \
MauiBlazorWeb/bin/Debug/net10.0-maccatalyst/maccatalyst-arm64/MauiBlazorWeb.app/Contents/MacOS/MauiBlazorWeb \
    >"$STATE_DIR/maui.log" 2>&1 &
APP_PID=$!

maui devflow --agent-port "$AGENT_PORT" --platform maccatalyst agent wait --timeout 60
wait_for_cdp
maui devflow --agent-port "$AGENT_PORT" --platform maccatalyst list
maui devflow --agent-port "$AGENT_PORT" --platform maccatalyst agent status
maui devflow --agent-port "$AGENT_PORT" --platform maccatalyst webview webviews
maui devflow --agent-port "$AGENT_PORT" --platform maccatalyst webview status
maui devflow --agent-port "$AGENT_PORT" --platform maccatalyst webview source >/dev/null

DEVFLOW_LIVE_TESTS=1 \
DEVFLOW_SERVER_URL="$SERVER_URL" \
DEVFLOW_AGENT_PORT="$AGENT_PORT" \
dotnet test MauiBlazorWeb.DevFlow.Tests/MauiBlazorWeb.DevFlow.Tests.csproj --no-build
