#!/usr/bin/env bash

./dependencies.sh

echo "Running web server"
packages/FAKE/tools/FAKE.exe --fsiargs build.local.fsx
 
