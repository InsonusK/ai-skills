#!/bin/bash
# show.sh <go-path> <python-path> <dotnet-path> [skill] : the three stacks' text of one unit, side by side as diffs to Go
set -u
s=${4:-base-plateau}
G=go/$s-in-go.skill/examples/$1; P=python/$s-in-python.skill/examples/$2; D=dotnet/$s-in-dotnet.skill/examples/$3
echo "######## GO $1"; cat "$G"
echo "######## PY diff"; diff "$G" "$P"
echo "######## DN diff"; diff "$G" "$D"
