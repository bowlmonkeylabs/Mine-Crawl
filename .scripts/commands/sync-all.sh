#!/bin/bash
./.scripts/commands/pull-all.sh \
    && git push --recurse-submodules=on-demand \
    && exit 0
exit 1
