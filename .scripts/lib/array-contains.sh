#!/bin/bash

# https://stackoverflow.com/a/14367368
array_contains () {
    local seeking=$1; shift
    local in=1
    for element; do
        if [[ $element == "$seeking" ]]; then
            in=0
            break
        fi
    done
    return $in
}
arr=(a b c "d e" f g)
# array_contains "a b" "${arr[@]}" && echo yes || echo no    # no
# array_contains "d e" "${arr[@]}" && echo yes || echo no    # yes


# https://stackoverflow.com/a/14367368
array_contains2 () { 
    local array="$1[@]"
    local seeking=$2
    local in=1
    for element in "${!array}"; do
        if [[ $element == "$seeking" ]]; then
            in=0
            break
        fi
    done
    return $in
}
# array_contains2 arr "a b"  && echo yes || echo no    # no
# array_contains2 arr "d e"  && echo yes || echo no    # yes
