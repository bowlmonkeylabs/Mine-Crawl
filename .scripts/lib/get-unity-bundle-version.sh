#!/bin/bash
src_dir=$( realpath "${BASH_SOURCE[0]}" | xargs -0 dirname )
function src_realpath() { (cd "$src_dir"; realpath "$1"); }
function src_source() { source "$(cd "$src_dir"; realpath "$1")"; }
function src_exec() { (cd "$src_dir"; $1) }
#-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

# Arguments ---------------------------------------------------------
project_root_dir=$(src_realpath "../../")
project_settings_reldir="./ProjectSettings"
project_settings_filename="ProjectSettings.asset"
bundle_version_key="bundleVersion"

project_settings_dir=$(cd "$project_root_dir"; realpath "$project_settings_reldir")

# Execute -----------------------------------------------------------
nl=$'\n'
# bundle_version_regexp="$bundle_version_key:[:space:](.+?)[:space:]"
bundle_version_regexp="$bundle_version_key: ([^ ]*)"

project_settings_file_contents=$(cd "$project_settings_dir"; cat "$project_settings_filename")
# echo $bundle_version_regexp

if [[ $project_settings_file_contents =~ $bundle_version_regexp ]]
then
    bundle_version="${BASH_REMATCH[1]}"
    echo "$bundle_version"
    exit 0
else
    echo "fail"
    exit 1
fi
