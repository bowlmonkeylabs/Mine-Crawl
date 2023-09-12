#!/bin/bash
REV_START=main
REV_END=HEAD
git log --pretty=tformat:"- %s" --reverse $REV_START...$REV_END
