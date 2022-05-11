#!/bin/bash
src_dir=$( realpath "${BASH_SOURCE[0]}" | xargs -0 dirname )
function src_realpath() { (cd "$src_dir"; realpath "$1"); }
function src_source() { source "$(cd "$src_dir"; realpath "$1")"; }
function src_exec() { (cd "$src_dir"; $1) }
#-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

# Arguments ---------------------------------------------------------
project_path=$(cd "$src_dir"; realpath "../..")
project_unity_version=$(cd "$project_path"; "./.scripts/lib/get-unity-editor-version.sh")

unity_editor_install_path=$(src_exec ../lib/get-unity-editor-install-path.sh)
if test -d "$unity_editor_install_path"
then
    echo ""
else
    echo "Unity editor install path not found."
    exit 1
fi
unity_editor_executable_relpath="./$project_unity_version/Editor/Unity.exe"
if (cd "$unity_editor_install_path"; test "$unity_editor_executable_relpath")
then
    unity_editor_executable_path=$(cd "$unity_editor_install_path"; realpath "$unity_editor_executable_relpath")
else
    echo "Unity Editor matching the project version not found. Aborting..."
    exit 1
fi

# Execute -----------------------------------------------------------
"$unity_editor_executable_path" \
    -projectPath "$project_path" \
    -quit -batchmode \
    -logfile - \
    -executeMethod BML.Build.Builder.BuildProjectCommandLine \
    -buildTarget StandaloneWindows64 \
    -overwriteMode Overwrite \
    -buildOptions Development \
    -buildOptions AutoRunPlayer
echo ""
echo "Unity Editor process exited with code $?"
