#!/bin/bash

# Clean previous binaries

MAC_FILE=../../couch-cli-mac
WINDOWS_FILE=../../couch-cli.exe

if [[ -f "$MAC_FILE" ]]; then
    rm -f "$MAC_FILE"
fi

if [[ -f "$WINDOWS_FILE" ]]; then
    rm -f "$WINDOWS_FILE"
fi

# Building binary for Mac M2 (arm64)
export GOOS=darwin
export GOARCH=arm64

go build -o ../../couch-cli-mac ./main.go
chmod +x "$MAC_FILE"


# Building binary for Windows (amd64)
export GOOS=windows
export GOARCH=amd64

go build -o ../../couch-cli.exe ./main.go