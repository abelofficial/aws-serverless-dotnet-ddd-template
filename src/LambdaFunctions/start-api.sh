#!/bin/bash

# Check if function names are provided
if [ -z "$1" ]; then
    echo "Usage: ./start-api.sh <function-name> or ./start-api.sh all"
    echo "Example: ./start-api.sh SayHelloFunction"
    echo "Example: ./start-api.sh all"
    exit 1
fi

# Build and publish the project
echo "Building and publishing the project..."
dotnet build
dotnet publish -c Release
dotnet lambda package

if [ "$1" = "all" ]; then
    echo "Starting API Gateway with ALL endpoints"
    sam local start-api
else
    FUNCTION_NAME=$1
    echo "Starting API Gateway for function: $FUNCTION_NAME"
    # Just start the API - it will include all functions defined in template.yaml
    sam local start-api
fi 