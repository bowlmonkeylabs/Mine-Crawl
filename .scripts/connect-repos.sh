#!/bin/bash
echo "Make sure you are running this script from the folder where you want your new Unity project folder!"
echo
read -r -p "Press Enter to acknowledge this..."
echo

# Get user inputs
echo "Enter the name of the new Unity Project (Same name as its folder)."
echo "Ex. SpikeOutRevenge"
echo
read -r -p "New Project Name: " NewProjName
echo
echo "Enter the Github username of the user that owns the forked clones of base repos."
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
echo "NewProjName = $NewProjName"
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


# Clone new Main repo fork along with Private submodule
read -p "git clone https://github.com/$OwnerGitHubUserName/$NewProjName.git --recurse-submodules"
git clone https://github.com/$OwnerGitHubUserName/$NewProjName.git --recurse-submodules

# Save Project directory
read -p "cd $NewProjName"
cd $NewProjName
read -p "cwd=$(pwd)"
cwd=$(pwd) # Store current dir in variable

# Switch to main on Private
read -p "cd Assets/Private"
cd Assets/Private
read -p "git checkout main"
git checkout main
read -p "cd $cwd"
cd "$cwd"

# Git Config
read -p "git config --local merge.ff only"
git config --local merge.ff only
read -p "git config --local submodule.recurse true"
git config --local submodule.recurse true

#  Clone new Unversioned repo fork in UnversionedTargetDir
read -p "git clone https://github.com/$OwnerGitHubUserName/$NewProjName-Unversioned.git $UnversionedTargetDir/$NewProjName-Unversioned --recurse-submodule"
git clone https://github.com/$OwnerGitHubUserName/$NewProjName-Unversioned.git $UnversionedTargetDir/$NewProjName-Unversioned --recurse-submodule
read -p "cd $UnversionedTargetDir/$NewProjName-Unversioned"
cd $UnversionedTargetDir/$NewProjName-Unversioned

# Run script to init the Annex
read -p "./.scripts/connect-annex.sh $NewProjName"
"./.scripts/connect-annex.sh" $NewProjName

# Make symLink in Assets folder pointing to this forked repo
read -p "cd $cwd"
cd "$cwd"
read -p "./.scripts/setup/lib/create-sym-link.sh "Assets\Unversioned" "$UnversionedTargetDir/$NewProjName-Unversioned""
./.scripts/setup/lib/create-sym-link.sh "Assets\Unversioned" "$UnversionedTargetDir/$NewProjName-Unversioned"

# Exit Script
echo "Press Enter to exit..."
read
exit 0
