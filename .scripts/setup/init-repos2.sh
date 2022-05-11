#!/bin/bash
src_dir=$( realpath "${BASH_SOURCE[0]}" | xargs -0 dirname )
function src_realpath() { (cd "$src_dir"; realpath "$1"); }
function src_source() { source "$(cd "$src_dir"; realpath "$1")"; }
function src_exec() { (cd "$src_dir"; $1) }
#-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

# Arguments ---------------------------------------------------------
if [ $# -eq 0 ]
then
    echo "Enter the name of the new Unity Project (Same name as its folder)."
    echo "Ex. SpikeOutRevenge"
    echo
    read -r -p "New Project Name: " new_project_name
    echo
else
    new_project_name="$1"; shift
fi
if [ ! -d "$new_project_name" ]
then
    echo "Provided project folder does not exist."
    echo "Make sure you are running this script from the folder that contains your new Unity project folder!"
    exit 1
fi

new_project_repo_name_main="$new_project_name"
new_project_repo_name_private="$new_project_repo_name_main-Private"
new_project_repo_name_unversioned="$new_project_repo_name_main-Unversioned"

assets_folder_private="Private"
assets_folder_unversioned="Unversioned"

template_repo_main="https://github.com/bowlmonkeylabs/UnityTemplate"
template_repo_private="https://github.com/bowlmonkeylabs/UnityTemplate-Private"
template_repo_unversioned="https://github.com/bowlmonkeylabs/UnityTemplate-Unversioned"

# if [ $# -eq 0 ]
# then
#     echo "No scripts folder provided."
#     exit 1
# fi
# scripts_dir="$1"; shift
# if [ ! -d "$scripts_dir" ]
# then
#     echo "Provided scripts folder does not exist."
#     exit 1
# fi
# scripts_dir=$( realpath --relative-to="$root_dir" "$scripts_dir" )

# env_prefix="$1"; shift

# commands_that_run_without_subshell=($@); shift $#


# Execute -----------------------------------------------------------

# ----- Main repo

# Initialize GitHub repository in the unity project folder
# gh repo create "$new_project_repo_name_main" \
#     --template "$template_repo_main" \
#     --private --clone
## TODO Make this repo --public
cd "$new_project_repo_name_main"

# Set local git preferences
# git config --local merge.ff only
# git config --local submodule.recurse true

# Commit Unity project files
# git add .
# git commit -m "Add Unity project files."

read

# ----- Private repo

# Initialize a private git repository as a submodule
cd Assets
gh repo create "$new_project_repo_name_private" \
    --template "$template_repo_private" \
    --private --clone
mv "$new_project_repo_name_private" "$assets_folder_private"
cd "$assets_folder_private"
# git pull

# git submodule sync
# git submodule update --init --recursive

cd ..

# read



# ----- Unversioned (git-annex) repo

# cd Assets
# gh repo create "$new_project_repo_name_unversioned" \
#     --template "$template_repo_unversioned" \
#     --private --clone
# read
# mv "$new_project_repo_name_unversioned" "$assets_folder_unversioned"
# read
# cd "$assets_folder_private"
# git submodule sync
# git submodule update --init --recursive

read
