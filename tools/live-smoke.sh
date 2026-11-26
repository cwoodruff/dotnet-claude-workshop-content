#!/usr/bin/env bash
# Live smoke run for a BookTracker lab folder (issue #31).
#
#   export ANTHROPIC_API_KEY=sk-ant-...
#   tools/live-smoke.sh /path/to/dotnet-claude-workshop-content/src/c10/BookTracker
#
# Makes real API calls on the model in appsettings.json (a handful of short requests).
# Works on a throwaway copy of the SQLite database, so the lab folder's data is not changed.
# Needs: bash, curl, jq, .NET 10 SDK. Optional: Ollama (for /api/recommend), Docker (container tests).
set -u

APP_DIR="${1:-../dotnet-claude-workshop-content/src/c10/BookTracker}"
API_URL="http://localhost:5255"
MCP_URL="http://localhost:5100"
WORK="$(mktemp -d)"
RESULTS="$WORK/results.txt"
: > "$RESULTS"
API_PID=""
MCP_PID=""

pass() { echo "PASS  $1" | tee -a "$RESULTS"; }
fail() { echo "FAIL  $1" | tee -a "$RESULTS"; }
skip() { echo "SKIP  $1" | tee -a "$RESULTS"; }

cleanup() {
  [ -n "$API_PID" ] && kill "$API_PID" 2>/dev/null
  [ -n "$MCP_PID" ] && kill "$MCP_PID" 2>/dev/null
  wait 2>/dev/null
}
trap cleanup EXIT

for tool in curl jq dotnet; do
  command -v "$tool" >/dev/null 2>&1 || { echo "Missing required tool: $tool"; exit 2; }
done
[ -n "${ANTHROPIC_API_KEY:-}" ] || { echo "Set ANTHROPIC_API_KEY first."; exit 2; }
[ -f "$APP_DIR/BookTracker.sln" ] || { echo "No BookTracker.sln in $APP_DIR"; exit 2; }
APP_DIR="$(cd "$APP_DIR" && pwd)"

MODEL="$(jq -r '.Anthropic.Model' "$APP_DIR/BookTracker.Api/appsettings.json")"
echo "Lab folder: $APP_DIR"
echo "Model:      $MODEL"
echo "Work dir:   $WORK"
echo

export Anthropic__ApiKey="$ANTHROPIC_API_KEY"
export ConnectionStrings__BookTracker="Data Source=$WORK/smoke.db"
export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_NOLOGO=1

wait_for() { # url, seconds
  local i=0
  while [ "$i" -lt "$2" ]; do
    curl -s -o /dev/null "$1" && return 0
    sleep 1; i=$((i + 1))
  done
  return 1
}

# ---- 1. build -----------------------------------------------------------------
if dotnet build "$APP_DIR/BookTracker.sln" -v q > "$WORK/build.log" 2>&1; then
  pass "build"
else
  fail "build (see $WORK/build.log)"; cat "$RESULTS"; exit 1
fi

# ---- 2. start the API ---------------------------------------------------------
dotnet run --no-build --project "$APP_DIR/BookTracker.Api" --urls "$API_URL" > "$WORK/api.log" 2>&1 &
API_PID=$!
if wait_for "$API_URL/" 60; then pass "API starts on $API_URL"; else fail "API did not start (see $WORK/api.log)"; fi

post() { # path, json body, output file -> prints HTTP status
  curl -s -o "$3" -w '%{http_code}' -X POST "$API_URL$1" \
    -H 'Content-Type: application/json' -H 'X-User-Id: smoke' -d "$2"
}

# ---- 3. /api/chat, then a second call to prove the cache read -------------------
S1="$(post /api/chat '{"sessionId":"smoke-a","message":"Which books in the library are science fiction?"}' "$WORK/chat1.json")"
if [ "$S1" = "200" ] && [ -n "$(jq -r '.reply // empty' "$WORK/chat1.json")" ]; then
  pass "/api/chat replies (in $(jq -r .inputTokens "$WORK/chat1.json"), out $(jq -r .outputTokens "$WORK/chat1.json"))"
else
  fail "/api/chat returned HTTP $S1 (see $WORK/chat1.json and $WORK/api.log)"
fi
S2="$(post /api/chat '{"sessionId":"smoke-b","message":"Who wrote Clean Code?"}' "$WORK/chat2.json")"
CACHE_READ="$(jq -r '.cacheReadInputTokens // 0' "$WORK/chat2.json" 2>/dev/null || echo 0)"
if [ "$S2" = "200" ] && [ "${CACHE_READ:-0}" -gt 0 ] 2>/dev/null; then
  pass "prompt cache read on the second call ($CACHE_READ tokens)"
else
  fail "no cache read on the second call (HTTP $S2, cacheReadInputTokens=${CACHE_READ:-?})"
fi

# ---- 4. /api/chat/stream --------------------------------------------------------
curl -s -N --max-time 120 -H 'X-User-Id: smoke' \
  "$API_URL/api/chat/stream?q=Describe%20Dune%20in%20two%20sentences" > "$WORK/stream.txt" 2>/dev/null
CHUNKS="$(grep -c '^data: ' "$WORK/stream.txt" || true)"
if [ "${CHUNKS:-0}" -gt 1 ]; then pass "/api/chat/stream sent $CHUNKS chunks"; else fail "/api/chat/stream sent ${CHUNKS:-0} chunks (see $WORK/stream.txt)"; fi

# ---- 5. /api/agent: a prompt that needs two tools ---------------------------------
S3="$(post /api/agent '{"message":"Look up The Name of the Wind, then tell me my current reading progress on it."}' "$WORK/agent.json")"
CALLS="$(jq -r '.toolCalls | length' "$WORK/agent.json" 2>/dev/null || echo 0)"
REPLY="$(jq -r '.reply // empty' "$WORK/agent.json" 2>/dev/null)"
NAMES="$(jq -r '[.toolCalls[].name] | join(", ")' "$WORK/agent.json" 2>/dev/null)"
if [ "$S3" = "200" ] && [ "${CALLS:-0}" -ge 2 ] && [ -n "$REPLY" ] && [ "${REPLY#Stopped:}" = "$REPLY" ]; then
  pass "/api/agent ran $CALLS tool calls ($NAMES)"
else
  fail "/api/agent: HTTP $S3, $CALLS tool calls, reply starts '${REPLY:0:60}' (see $WORK/agent.json)"
fi
ERRS="$(jq -r '[.toolCalls[] | select(.isError)] | length' "$WORK/agent.json" 2>/dev/null || echo 0)"
[ "${ERRS:-0}" -gt 0 ] && echo "      note: $ERRS tool call(s) returned isError=true"

# ---- 6. /api/recommend (needs Ollama for embeddings) ---------------------------
if curl -s -o /dev/null --max-time 3 http://localhost:11434/api/tags; then
  S4="$(post /api/recommend '{"query":"A long fantasy novel to read next"}' "$WORK/recommend.json")"
  SOURCES="$(jq -r '.sources | length' "$WORK/recommend.json" 2>/dev/null || echo 0)"
  if [ "$S4" = "200" ] && [ "${SOURCES:-0}" -gt 0 ]; then
    pass "/api/recommend grounded on $SOURCES source(s)"
  else
    fail "/api/recommend: HTTP $S4, $SOURCES sources (see $WORK/recommend.json)"
  fi
else
  skip "/api/recommend (Ollama is not running on localhost:11434)"
fi

# ---- 7. MCP server: initialize and list tools ------------------------------------
if [ -d "$APP_DIR/BookTracker.Mcp" ]; then
  dotnet run --no-build --project "$APP_DIR/BookTracker.Mcp" --urls "$MCP_URL" > "$WORK/mcp.log" 2>&1 &
  MCP_PID=$!
  wait_for "$MCP_URL/" 60
  mcp() {
    curl -s --max-time 30 -X POST "$MCP_URL/" -H 'Content-Type: application/json' \
      -H 'Accept: application/json, text/event-stream' -d "$1" | sed -n 's/^data: //p;/^{/p'
  }
  INIT="$(mcp '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"live-smoke","version":"1"}}}')"
  PROTO="$(echo "$INIT" | jq -r '.result.protocolVersion // empty' 2>/dev/null | head -1)"
  TOOLS="$(mcp '{"jsonrpc":"2.0","id":2,"method":"tools/list"}' | jq -r '[.result.tools[].name] | join(", ")' 2>/dev/null | head -1)"
  if [ -n "$PROTO" ] && [ -n "$TOOLS" ]; then
    pass "MCP server answers (protocol $PROTO; tools: $TOOLS)"
  else
    fail "MCP server did not answer initialize and tools/list (see $WORK/mcp.log)"
  fi
  echo "      to try it from Claude Code:  claude mcp add --transport http booktracker $MCP_URL/"
else
  skip "MCP server (no BookTracker.Mcp in this lab folder)"
fi

cleanup; API_PID=""; MCP_PID=""

# ---- 8. tests (container tests run only when Docker is up) -----------------------
if dotnet test "$APP_DIR/BookTracker.sln" --no-build > "$WORK/test.log" 2>&1; then
  pass "dotnet test ($(grep -E 'Passed!|Failed!' "$WORK/test.log" | tail -1 | sed 's/^ *//'))"
else
  fail "dotnet test (see $WORK/test.log)"
fi

echo
echo "---- summary ($MODEL) ----"
cat "$RESULTS"
echo "Logs and responses: $WORK"
grep -q '^FAIL' "$RESULTS" && exit 1
exit 0
