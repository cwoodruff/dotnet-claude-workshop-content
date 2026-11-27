#!/usr/bin/env bash
# Rebuild the reference-card PDFs from their HTML sources.
# Needs WeasyPrint plus the Inter and Source Serif 4 fonts installed.
# Usage: tools/cards/build-cards.sh  (run from the repo root)
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
root="$(cd "$here/../.." && pwd)"
build() { weasyprint "$here/$1.html" "$root/decks/$2/$1.pdf"; echo "built decks/$2/$1.pdf"; }
build steering-reference-card "Day 1"
build models-cost-reference-card "Day 2"
build day2-faq-reference-card "Day 2"
