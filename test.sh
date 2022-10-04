cwd=$(pwd)
#  Clone new Unversioned repo fork in UnversionedTargetDir
read -p "git clone https://github.com/bowlmonkeylabs/Mine-Crawl-Unversioned.git E:\\/Mine-Crawl-Unversioned --recurse-submodule"
git clone https://github.com/bowlmonkeylabs/Mine-Crawl-Unversioned.git E:\\/Mine-Crawl-Unversioned --recurse-submodule
read -p "cd E:\\/Mine-Crawl-Unversioned"
cd E/Mine-Crawl-Unversioned

# Run script to init the Annex
read -p "./.scripts/connect-annex.sh Mine-Crawl"
"./.scripts/connect-annex.sh" Mine-Crawl

# Make symLink in Assets folder pointing to this forked repo
read -p "cd $cwd"
cd "$cwd"
read -p "./.scripts/setup/lib/create-sym-link.sh "Assets\Unversioned" "E:\\/Mine-Crawl-Unversioned""
./.scripts/setup/lib/create-sym-link.sh "Assets\Unversioned" "E:\\/Mine-Crawl-Unversioned"

# Exit Script
echo "Press Enter to exit..."
read
exit 0