#!/bin/bash
git fetch --recurse-submodules \
    && ./.scripts/commands/stage-unv.sh \
    && echo "$(source ./.scripts/commands/goto-unv.sh; git annex sync)" \
    && exit 0
exit 1
