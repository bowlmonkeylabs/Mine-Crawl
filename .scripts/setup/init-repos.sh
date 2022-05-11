#!/bin/bash
echo "Make sure you are running this script from the folder that contains your new Unity project folder!"
echo
read -r -p "Press Enter to acknowledge this..."
echo

# Get user inputs
echo "Enter the name of the new Unity Project (Same name as its folder)."
echo "Ex. SpikeOutRevenge"
echo
read -r -p "New Project Name: " NewProjectName
echo
echo "Enter the Github username of the user that will own the forked clones of base repos. (Probably your Github username)"
echo "Ex. githubUser22"
echo
read -r -p "Github Username: " OwnerGitHubUserName
echo
echo "Enter the directory where you would like to clone the Unversioned repo."
echo "NOTE: You probably want this at the root of some drive to avoid windows path character limit!"
echo "Ex. E:\\"
echo
read -r -p "Target Dir for Unversioned: " UnversionedTargetDir
echo

# Accept Inputs
echo "Your selections:"
echo "NewProjectName = $NewProjectName"
echo "OwnerGitHubUserName = $OwnerGitHubUserName"
echo "UnversionedTargetDir = $UnversionedTargetDir"
echo 
read -r -p "Does this look correct? (Y/N) " IsCorrect

# Exit if user does not accept
if [ "$IsCorrect" != "y" ] && [ "$IsCorrect" != "Y" ]; then
  echo "Terminating...Press Enter to close"
  read
  exit 1
fi

echo "You accepted the selections"


#Create new Main repo fork and combine with existing Unity project folder
read -p "mkdir temp"
mkdir temp
read -p "cd temp"
cd temp
read -p "git init -b main"
git init -b main
read -p "hub create $OwnerGitHubUserName/$NewProjectName"
hub create $OwnerGitHubUserName/$NewProjectName
read -p "git remote rm origin"
git remote rm origin
read -p "git remote add origin https://github.com/$OwnerGitHubUserName/$NewProjectName.git"
git remote add origin https://github.com/$OwnerGitHubUserName/$NewProjectName.git
read -p "git config --local merge.ff only"
git config --local merge.ff only
read -p "git config --local submodule.recurse true"
git config --local submodule.recurse true
read -p "cd .."
cd ..
read -p "mv temp/.git $NewProjectName/.git"
mv temp/.git $NewProjectName/.git
read -p "rm -rf temp"
rm -rf temp
read -p "cd $NewProjectName"
cd $NewProjectName


# Pull from Main template into fork and push to new repo
read -p "cwd=$(pwd)"
cwd=$(pwd) # Store current dir in variable
read -p "git remote add upstream https://github.com/bowlmonkeylabs/UnityTemplate.git"
git remote add upstream https://github.com/bowlmonkeylabs/UnityTemplate.git
read -p "git config --local submodule.recurse false"
git config --local submodule.recurse false
read -p "git pull --rebase upstream main"
git pull --rebase upstream main
read -p "git config --local submodule.recurse true"
git config --local submodule.recurse true
read -p "git remote rm upstream"
git remote rm upstream
read -p "git add ."
git add .
#Should have the .gitignore at this point
read -p "git commit -m "Initial Commit""
git commit -m "Initial Commit"
read -p "git push --set-upstream origin main"
git push --set-upstream origin main

# Create new Private repo fork inside Assets folder
read -p "cd Assets"
cd Assets
read -p "mkdir $NewProjectName-Private"
mkdir $NewProjectName-Private
read -p "cd $NewProjectName-Private"
cd $NewProjectName-Private
read -p "git init -b main"
git init -b main
read -p "hub create -p $OwnerGitHubUserName/$NewProjectName-Private"
hub create -p $OwnerGitHubUserName/$NewProjectName-Private

# Pull from Private template into fork and push to new repo
read -p "git remote add upstream https://github.com/bowlmonkeylabs/UnityTemplate-Private"
git remote add upstream https://github.com/bowlmonkeylabs/UnityTemplate-Private
read -p "git config --local submodule.recurse false"
git config --local submodule.recurse false
read -p "git lfs install --skip-smudge --local"
git lfs install --skip-smudge --local
read -p "git pull --rebase --recurse-submodules upstream main"
git pull --rebase --recurse-submodules upstream main
read -p "git lfs pull upstream main"
git lfs pull upstream main
read -p "git submodule update --init --recursive"
git submodule update --init --recursive
read -p "git config --local submodule.recurse true"
git config --local submodule.recurse true
read -p "git lfs fetch --all upstream"
git lfs fetch --all upstream
read -p "git remote rm upstream"
git remote rm upstream
read -p "git add ."
git add .
read -p "git commit -m "Initial Commit""
git commit -m "Initial Commit"
read -p "git push --set-upstream origin main"
git push --set-upstream origin main

# Delete this fork and re-clone as a submodule (Prevents cloning issues)
read -p "cd $cwd"
cd "$cwd"
read -p "rm -r -f Assets/$NewProjectName-Private"
rm -r -f Assets/$NewProjectName-Private
read -p "git submodule add https://github.com/$OwnerGitHubUserName/$NewProjectName-Private.git Assets/Private"
git submodule add https://github.com/$OwnerGitHubUserName/$NewProjectName-Private.git Assets/Private
read -p "cd Assets/$NewProjectName-Private"
cd Assets/Private

# Submodule config
read -p "git config --local merge.ff only"
git config --local merge.ff only

# Pull submodules for Private
read -p "git submodule update --init --recursive"
git submodule update --init --recursive

# Switch to main on Private
read -p "git checkout main"
git checkout main

# Push addition of submodule to main repo
read -p "cd $cwd"
cd "$cwd"
read -p "git add ."
git add .
read -p "git commit -m "Add Private submodule""
git commit -m "Add Private submodule"
read -p "git push"
git push

#  Create new Unversioned repo fork in UnversionedTargetDir
read -p "mkdir $UnversionedTargetDir/$NewProjectName-Unversioned"
mkdir $UnversionedTargetDir/$NewProjectName-Unversioned
read -p "cd $UnversionedTargetDir/$NewProjectName-Unversioned"
cd $UnversionedTargetDir/$NewProjectName-Unversioned
read -p "git init -b main"
git init -b main
read -p "hub create -p $OwnerGitHubUserName/$NewProjectName-Unversioned"
hub create -p $OwnerGitHubUserName/$NewProjectName-Unversioned
read -p "git remote rm origin"
git remote rm origin
read -p "git remote add origin https://github.com/$OwnerGitHubUserName/$NewProjectName-Unversioned.git"
git remote add origin https://github.com/$OwnerGitHubUserName/$NewProjectName-Unversioned.git

# Pull from Unversioned template into fork and push to new repo
read -p "git remote add upstream https://github.com/bowlmonkeylabs/UnityTemplate-Unversioned"
git remote add upstream https://github.com/bowlmonkeylabs/UnityTemplate-Unversioned
read -p "git config --local submodule.recurse false"
git config --local submodule.recurse false
read -p "git pull --rebase upstream main"
git pull --rebase upstream main
read -p "git config --local submodule.recurse true"
git config --local submodule.recurse true
read -p "git remote rm upstream"
git remote rm upstream
read -p "git push --set-upstream origin main"
git push --set-upstream origin main

# Run script to init the Annex
read -p "./.scripts/setup/create-annex.sh $NewProjectName"
"./.scripts/setup/create-annex.sh" $NewProjectName

# Init the annex
# read -p "git annex init "$AnnexRemoteName" "
# git annex init "$AnnexRemoteName" 
# read -p "git annex enableremote cloud"
# git annex enableremote cloud

# Make symLink in Assets folder pointing to this forked repo
read -p "cd $cwd"
cd "$cwd"
read -p "./.scripts/setup/lib/create-sym-link.sh "Assets\Unversioned" "$UnversionedTargetDir/$NewProjectName-Unversioned""
./.scripts/setup/lib/create-sym-link.sh "Assets\Unversioned" "$UnversionedTargetDir/$NewProjectName-Unversioned"

# Init Package Manager Assets
read -p "python .scripts/setup/lib/init-package-manager.py"
python .scripts/setup/lib/init-package-manager.py

# Exit Script
echo "Press Enter to exit..."
read
exit 0
