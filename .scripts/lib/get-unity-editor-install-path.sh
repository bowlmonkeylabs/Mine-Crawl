#!/bin/bash
src_dir=$( realpath "${BASH_SOURCE[0]}" | xargs -0 dirname )
function src_realpath() { (cd "$src_dir"; realpath "$1"); }
function src_source() { source "$(cd "$src_dir"; realpath "$1")"; }
function src_exec() { (cd "$src_dir"; $1) }
#-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

# Arguments ---------------------------------------------------------
unity_hub_settings_dir="$APPDATA/UnityHub/"
secondary_editor_install_path_filename="secondaryInstallPath.json"
default_editor_install_path="/c/Program Files/Unity/Hub/Editor/"

# Execute -----------------------------------------------------------
secondary_editor_install_path_file_fullpath=$(cd "$unity_hub_settings_dir"; realpath "$secondary_editor_install_path_filename")
if test "$secondary_editor_install_path_file_fullpath"
then
    secondary_install_path=$(cat "$secondary_editor_install_path_file_fullpath" | tr -d '"')
fi

if [ "$secondary_install_path" = "" ]
then
    editor_install_path="$default_editor_install_path"
else
    editor_install_path="$secondary_install_path"
fi

editor_install_path=$(realpath "$editor_install_path")
echo $editor_install_path
