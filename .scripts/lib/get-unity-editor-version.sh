#!/bin/bash
src_dir=$( realpath "${BASH_SOURCE[0]}" | xargs -0 dirname )
function src_realpath() { (cd "$src_dir"; realpath "$1"); }
function src_source() { source "$(cd "$src_dir"; realpath "$1")"; }
function src_exec() { (cd "$src_dir"; $1) }
#-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

# Arguments ---------------------------------------------------------
project_root_dir=$(src_realpath "../../")
project_settings_reldir="./ProjectSettings"
project_version_filename="ProjectVersion.txt"
editor_version_key="m_EditorVersion"

project_settings_dir=$(cd "$project_root_dir"; realpath "$project_settings_reldir")

# Execute -----------------------------------------------------------
nl=$'\n'
editor_version_regexp="^$editor_version_key: (.*)$nl"

project_version_file_contents=$(cd "$project_settings_dir"; cat "$project_version_filename")

if [[ $project_version_file_contents =~ $editor_version_regexp ]]
then
    editor_version="${BASH_REMATCH[1]}"
    echo "$editor_version"
    exit 0
else
    exit 1
fi
