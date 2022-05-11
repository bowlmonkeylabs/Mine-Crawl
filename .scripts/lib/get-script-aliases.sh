#!/bin/bash
src_dir=$( realpath ${BASH_SOURCE[0]} | xargs -0 dirname )
function src_realpath() { (cd "$src_dir"; realpath "$1"); }
function src_source() { source "$(cd "$src_dir"; realpath "$1")"; }
#-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

src_source ./ask.sh
src_source ./array-contains.sh

# Arguments ---------------------------------------------------------
if [ $# -eq 0 ]
then
    echo "No root folder provided."
    exit 1
fi
root_dir="$1"; shift
if [ ! -d "$root_dir" ]
then
    echo "Provided root folder does not exist."
    exit 1
fi
root_dir=$(realpath "$root_dir")

if [ $# -eq 1 ]
then
    echo "No scripts folder provided."
    exit 1
fi
scripts_dir="$1"; shift
if [ ! -d "$scripts_dir" ]
then
    echo "Provided scripts folder does not exist."
    exit 1
fi
scripts_dir=$( realpath --relative-to="$root_dir" "$scripts_dir" )

env_prefix="$1"; shift

commands_that_run_without_subshell=($@); shift $#

# Execute -----------------------------------------------------------

new_env_entries=()

# If short name is provided, use it as an alias to access the root folder
if [ "$env_prefix" != "" ]
then
    # Export path as environment variable
    env_prefix_upper=$( echo $env_prefix | tr '[:lower:]' '[:upper:]' )
    path_var_name="$env_prefix_upper""_PATH"
    new_env_entries+=("export $path_var_name=\"$root_dir\"")

    # Alias project short name to cd to the path
    cmd="cd \\\"\${$path_var_name}\\\""
    new_env_entries+=("alias $env_prefix=\"$cmd\"")

    pre_alias="$env_prefix-"
    pre_cmd="$cmd"
else
    pre_alias=""
    pre_cmd="cd \"$root_dir\""
fi

# Print scripts found in the dir
for entry in $(cd "$root_dir"; ls -1 "$scripts_dir")
do
    filename=$(basename -s .sh "$entry")
    path="$scripts_dir/$entry"
    alias="$pre_alias""$filename"

    run_command_without_subshell=$(array_contains2 "commands_that_run_without_subshell" "$filename" && echo true || echo false)
    if [ "$run_command_without_subshell" = true ]
    then
        cmd="$pre_cmd; source \"$path\""
    else
        cmd="($pre_cmd; \"$path\")"
    fi

    new_env_entries+=("alias $alias=\"$cmd\"")
done

# Prints aliases that will be added
for alias in "${new_env_entries[@]}"
do
    echo "$alias"
done