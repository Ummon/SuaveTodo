#!/usr/bin/env bash

if [ ! -d .paket ]; then
    echo "Installing Paket"
    mkdir .paket
    curl https://github.com/fsprojects/Paket/releases/download/1.4.0/paket.bootstrapper.exe -L --insecure -o .paket/paket.bootstrapper.exe
    chmod u+x .paket/paket.bootstrapper.exe
    .paket/paket.bootstrapper.exe
    chmod u+x .paket/paket.exe
    chmod u+x packages/FAKE/tools/FAKE.exe
fi

if [ ! -f paket.lock ]; then
    echo "Installing dependencies"
    .paket/paket.exe install
else
    echo "Restoring dependencies"
    .paket/paket.exe restore
fi
