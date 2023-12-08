#!/bin/sh

download_directory="./releases"

# User settings ----------------------------------------
force_redownload=false

# Detect platform based on shell $OSTYPE
# echo "$OSTYPE"
case $OSTYPE in
  "msys" | "cygwin" | "win32" ) platform="StandaloneWindows64";;
  "darwin" ) platform="StandaloneOSX";;
  "linux-gnu" | "freebsd" ) platform="StandaloneLinux64";;
  * ) \
    log_and_alert "Sorry, I don't know the target platform for $OSTYPE."
    exit 1;;
esac

# Check if zenity is available (to allow user input), otherwise use hard-coded settings
run_headless=false
command -v zenity >/dev/null 2>&1 || { echo "Zenity not found; proceeding with hard-coded options."; run_headless=true; }
if [ "$run_headless" = "true" ]; then

  release_channel="stable"
  # release_channel="latest"
  # release_channel="<tag>"

else

  all_releases_json=$( \
    curl -L -s \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: 2022-11-28" \
    https://api.github.com/repos/bowlmonkeylabs/Mine-Crawl/releases \
  )
  all_release_tags_array=($(echo "$all_releases_json" | jq -r "[\"stable\"] + map(.tag_name) | @sh" | xargs))
  # echo "${all_release_tags_array[*]}"

  release_channel=$(zenity --entry --title "$window_title" --entry-text "${all_release_tags_array[@]}" --text "Select release channel:")

fi
echo "Release: $release_channel, Platform: $platform"

function log_and_alert() {
  echo $1
  if [ "$run_headless" = "false" ]; then
    zenity --info --title "Mine-Crawl Updater" --text $1 --width 300 2>/dev/null
  fi
}

# Program --------------------------------------------------------

if [ $release_channel = "stable" ]; then

  all_releases_json=$( \
    curl -L -s \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: 2022-11-28" \
    https://api.github.com/repos/bowlmonkeylabs/Mine-Crawl/releases \
  )w
  release_info_json=$( \
    echo "$all_releases_json" | \
    # jq "sort_by(.published_at) | map(select(.tag_name | startswith(\"v\"))) | .[0]"
    jq "map(select(.tag_name | startswith(\"v\"))) | .[0]"
  )

elif [ $release_channel = "latest" ]; then

  release_info_json=$( \
    curl -L -s \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: 2022-11-28" \
    https://api.github.com/repos/bowlmonkeylabs/Mine-Crawl/releases/tags/latest \
  )

else

  # TODO handle inputting a generic tag for a sprint version (e.g. v0.10 instead of v0.10.265)

  response=$( \
    curl -L -s \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: 2022-11-28" \
    -w "%{http_code}" \
    "https://api.github.com/repos/bowlmonkeylabs/Mine-Crawl/releases/tags/$release_channel" \
  ); ec=$?
  http_code=$(tail -n1 <<< "$response")             # get the last line
  release_info_json=$(sed '$ d' <<< "$response")    # get all but the last line which contains the status code

  if [ "$http_code" = "404" ]; then
    log_and_alert "The release channel you selected doesn't exist. What are you trying to do?"
    exit 1
  elif [ "$http_code" != "200" ]; then
    log_and_alert "There was a problem fetching the release you requested. HTTP $http_code, $release_info_json"
    exit 1
  fi

fi
# echo "$release_info_json"

target_platform_release_asset_json=$( \
  echo "$release_info_json" | \
  jq ".assets[]? | select(.name | endswith(\"$platform.zip\"))"
)
if [ "$target_platform_release_asset_json" = "" ]; then
  log_and_alert "The release channel you selected is not available for your platform."
  exit 1
fi
# echo "$target_platform_release_asset_json"

filename=$(echo "$target_platform_release_asset_json" | jq -r ".name")
browser_download_url=$(echo "$target_platform_release_asset_json" | jq -r ".browser_download_url")
# echo $filename
# echo $browser_download_url

echo "Downloading $filename"
download_suceeded=false
if [ $force_redownload = "true" ]; then
  http_code=$(curl --clobber --output-dir "$download_directory" --create-dirs -OJL "$browser_download_url" -w "%{http_code}\n"); ec=$?
else
  http_code=$(curl --output-dir "$download_directory" --create-dirs -OJL "$browser_download_url" -w "%{http_code}\n"); ec=$?
fi
# echo "$http_code"
case $ec in
    0) download_suceeeded=true;;
    *) if [ "$http_code" = "200" ]; then 
        download_suceeeded=true; s
      else 
        download_suceeded=false;
        log_and_alert "There was a problem with the download. HTTP $http_code"
        exit 1;
      fi;;
esac

# Unzip downloaded file
cd "$download_directory"
rm -rf $release_channel
unzip -u $filename -d $release_channel

# Mark game as executable
cd "$release_channel"
case $platform in
  "StandaloneWindows64" ) chmod +x "Mine-Crawl.exe";;
  * ) chmod +x "Mine-Crawl";;
esac

log_and_alert "Success."
