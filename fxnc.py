# 
#   Function
#   Copyright © 2024 NatML Inc. All Rights Reserved.
#

from argparse import ArgumentParser
from pathlib import Path
from requests import get
from shutil import unpack_archive

parser = ArgumentParser()
parser.add_argument("--version", type=str, default=None)

def _download_fxnc (url: str, path: Path):
    # Download
    response = get(url)
    response.raise_for_status()
    with open(path, "wb") as f:
        f.write(response.content)
    print(f"Wrote {url} to path: {path}")
    # Unzip
    if path.suffix == ".zip":
        unpack_archive(path, extract_dir=path.parent)
        path.unlink()
        print(f"Extracted {path}")

def _get_latest_version () -> str:
    response = get(f"https://api.github.com/repos/fxnai/fxnc/releases/latest")
    response.raise_for_status()
    release = response.json()
    return release["tag_name"]

def main (): # CHECK # Linux
    args = parser.parse_args()
    version = args.version if args.version else _get_latest_version()
    LIB_PATH_BASE = Path("Packages") / "ai.fxn.fxn3d" / "Plugins"
    LIBS = [
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/libFunction-android-arm64-v8a.so",
            "path": LIB_PATH_BASE / "Android" / "arm64-v8a" / "libFunction.so"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/libFunction-android-armeabi-v7a.so",
            "path": LIB_PATH_BASE / "Android" / "armeabi-v7a" / "libFunction.so"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/libFunction-android-x86.so",
            "path": LIB_PATH_BASE / "Android" / "x86" / "libFunction.so"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/libFunction-android-x86_64.so",
            "path": LIB_PATH_BASE / "Android" / "x86_64" / "libFunction.so"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function-ios-iphoneos.framework.zip",
            "path": LIB_PATH_BASE / "iOS" / "Function.framework.zip"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function-macos.dylib",
            "path": LIB_PATH_BASE / "macOS" / "Function.dylib"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function-x64.dll",
            "path": LIB_PATH_BASE / "Windows" / "Function.dll"
        },
    ]
    for lib in LIBS:
        _download_fxnc(lib["url"], lib["path"])

if __name__ == "__main__":
    main()