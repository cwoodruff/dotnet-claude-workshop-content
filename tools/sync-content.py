#!/usr/bin/env python3
"""Mirror the attendee-facing folders from this (private) repo into dotnet-claude-workshop-content.

    python3 tools/sync-content.py ../dotnet-claude-workshop-content           # copy
    python3 tools/sync-content.py ../dotnet-claude-workshop-content --check   # report only, exit 1 on drift

The private repo wins. Mirrored folders are copied file for file, and files the private
repo does not have are deleted from the public copy. Decks (.pptx) are copied with their
speaker notes removed. The lab folders (src/c0..src/c10) are not touched: the public repo
is their source of truth.

Needs python-pptx (pip install python-pptx).
"""
import filecmp
import io
import os
import shutil
import sys
import zipfile

from pptx import Presentation
from pptx.opc.constants import RELATIONSHIP_TYPE as RT

MIRRORED = ["decks", "demos", "dotnet-claude-code-kit", "tools"]
SKIP_DIRS = {"bin", "obj", ".vs", ".idea", "node_modules"}
SKIP_FILES = {".DS_Store"}
FIXED_TIME = (2026, 1, 1, 0, 0, 0)


def without_notes(path):
    """Return the deck's bytes with every slide's speaker notes removed, written deterministically."""
    prs = Presentation(path)
    for slide in prs.slides:
        for rid, rel in list(slide.part.rels.items()):
            if rel.reltype == RT.NOTES_SLIDE:
                slide.part.rels.pop(rid)
    raw = io.BytesIO()
    prs.save(raw)
    # Rewrite the zip with fixed timestamps and order so the same deck always gives the same bytes.
    out = io.BytesIO()
    with zipfile.ZipFile(raw) as src, zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as dst:
        for name in src.namelist():
            info = zipfile.ZipInfo(name, FIXED_TIME)
            info.compress_type = zipfile.ZIP_DEFLATED
            dst.writestr(info, src.read(name))
    return out.getvalue()


def files_under(root):
    found = set()
    if not os.path.isdir(root):
        return found
    for here, dirs, files in os.walk(root):
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
        for f in files:
            if f not in SKIP_FILES:
                found.add(os.path.relpath(os.path.join(here, f), root))
    return found


def read_bytes(path):
    with open(path, "rb") as fh:
        return fh.read()


def copy_one(src, dst, check):
    """Copy one file (decks without notes). Return True when the public copy changed or would change."""
    if src.endswith(".pptx"):
        data = without_notes(src)
        if os.path.exists(dst) and read_bytes(dst) == data:
            return False
        if not check:
            os.makedirs(os.path.dirname(dst), exist_ok=True)
            with open(dst, "wb") as fh:
                fh.write(data)
        return True
    if os.path.exists(dst) and filecmp.cmp(src, dst, shallow=False):
        return False
    if not check:
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        shutil.copy2(src, dst)
    return True


def sync_folder(src_root, dst_root, folder, check):
    changes = []
    src_files, dst_files = files_under(src_root), files_under(dst_root)
    for rel in sorted(src_files):
        dst = os.path.join(dst_root, rel)
        existed = os.path.exists(dst)
        if copy_one(os.path.join(src_root, rel), dst, check):
            changes.append(("update" if existed else "add", f"{folder}/{rel}"))
    for rel in sorted(dst_files - src_files):
        changes.append(("delete", f"{folder}/{rel}"))
        if not check:
            os.remove(os.path.join(dst_root, rel))
    return changes


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    check = "--check" in sys.argv
    if len(args) != 1:
        sys.exit(__doc__)
    private = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    public = os.path.abspath(args[0])
    if not os.path.isdir(os.path.join(public, "src", "c0")):
        sys.exit(f"{public} does not look like dotnet-claude-workshop-content (no src/c0).")

    changes = []
    for folder in MIRRORED:
        changes += sync_folder(os.path.join(private, folder), os.path.join(public, folder), folder, check)

    for kind, path in changes:
        print(f"{kind:7} {path}")
    if not changes:
        print("In sync.")
    elif check:
        print(f"\n{len(changes)} file(s) out of sync. Run without --check to copy.")
        sys.exit(1)
    else:
        print(f"\n{len(changes)} file(s) changed in {public}.")


if __name__ == "__main__":
    main()
