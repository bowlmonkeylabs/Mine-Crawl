#!/bin/bash
src_dir=$( realpath "${BASH_SOURCE[0]}" | xargs -0 dirname )
function src_realpath() { (cd "$src_dir"; realpath "$1"); }
function src_source() { source "$(cd "$src_dir"; realpath "$1")"; }
function src_exec() { (cd "$src_dir"; eval $1) }
#-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

src_source ./lib/ask.sh

# Arguments ---------------------------------------------------------
root_dir=$(src_realpath "../")
commands_reldir="./.scripts/commands"
commands_dir=$(cd "$root_dir"; realpath "$commands_reldir")
commands_dir_name=$(basename "$commands_dir")

env_prefix="$1"
if [ -z "$1" ]
then
    echo "No environment name provided."
    ask "Would you like to continue installing these aliases with no prefix?" answer
    $answer || exit 0
    env_file_name_prefix=""
else
    
    env_prefix=$( echo $env_prefix | sed 's/ //g' | tr -d '\n' )
    env_file_name_prefix="$env_prefix-"
fi

out_dir=$root_dir
# out_file_name="env-$env_file_name_prefix""$commands_dir_name.sh"
out_file_name=".env.sh"
out_file_path="$root_dir/$out_file_name"

shell="bash"
[ -n ${ZSH_VERSION} ] && shell="zsh"
shell_rc_path="${HOME}/.$shell""rc"

reinstall_out_file_name=".REINSTALL-ENV.sh"
reinstall_out_file_path="$root_dir/$reinstall_out_file_name"

commands_to_run_without_subshell="goto-subm goto-unv"

# Execute -----------------------------------------------------------

# Generate env source file
src_exec "./lib/get-script-aliases.sh \"$root_dir\" \"$commands_dir\" \"$env_prefix\" $commands_to_run_without_subshell" > "$out_file_path"
echo "Generated env: $out_file_path"
cat "$out_file_path"

# Source env file in shell rc
env_entry="source \"$out_file_path\""
already_installed=$(grep "$env_entry" "$shell_rc_path")
if [ -z "$already_installed" ]
then
    echo "" >> "$shell_rc_path"
    echo "$env_entry" >> "$shell_rc_path"
    echo "Installed env: $shell_rc_path"
else
    echo "Env already installed: $shell_rc_path"
fi

# Generate script to easily "reinstall" commands with the same parameters as this run
reinstall_cmd="cd \"$src_dir\"; ./install-commands.sh \"$env_prefix\""
echo "$reinstall_cmd" > "$reinstall_out_file_path"
