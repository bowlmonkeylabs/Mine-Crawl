#!/bin/bash

unix_path_to_powershell() {
	# - convert to an absolute path
	# - replace any starting /x/ by x:/
	# - replace all / by \
	# - wrap by PowerShell-escaped double quotes: `"
	readlink -f "$1" \
		| sed 's#^/\(\w\)/#\1:/#' \
		| sed 's#/#\\#g' \
		| sed 's#\(.*\)#\`\"\1\`\"#'
}

export unix_path_to_powershell

link=$(unix_path_to_powershell "$1")
target=$(unix_path_to_powershell "$2")

powershell start powershell -ArgumentList \"-Command "New-Item -Path '$link' -ItemType SymbolicLink -Value '$target'"\" -Verb runas

# Exit Script
echo "Press Enter to exit..."
read
exit 0
