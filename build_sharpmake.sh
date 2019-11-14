#!/bin/bash

dotnet build "tools/sharpmake/Sharpmake.Application/Sharpmake.Application_Core.csproj" -v q -nologo -c "Release" -r osx-x64 -o "build/tools/sharpmake" /p:PublishSingleFile=true