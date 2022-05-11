#!/bin/bash
git pull --recurse-submodules \
    && ./.scripts/commands/stage-unv.sh \
    && ./.scripts/commands/sync-unv.sh \
    && exit 0
exit 1
