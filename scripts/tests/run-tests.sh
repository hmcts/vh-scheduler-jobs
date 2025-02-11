#!/bin/sh
set -x

sdsUnitExclusions="[*]SchedulerJobs.Common.*", "[*]SchedulerJobs.Sds.UnitTests.*", "[Testing.Common]*"
sdsServiceExclusions="[*]SchedulerJobs.Common.*", "[*]SchedulerJobs.Services.UnitTests.*", "[Testing.Common]*"
configuration=Release

# Script is for docker compose tests where the script is at the root level
dotnet test SchedulerJobs/SchedulerJobs.Sds.UnitTests/SchedulerJobs.Sds.UnitTests.csproj -c $configuration --results-directory ./TestResults --logger "trx;LogFileName=SchedulerJobs-Sds-Unit-Tests-TestResults.trx" \
    "/p:CollectCoverage=true" \
    "/p:Exclude=\"${sdsUnitExclusions}\"" \
    "/p:CoverletOutput=${PWD}/Coverage/" \
    "/p:MergeWith=${PWD}/Coverage/coverage.json" \
    "/p:CoverletOutputFormat=\"opencover,json,cobertura,lcov\""

dotnet test SchedulerJobs/SchedulerJobs.Services.UnitTests/SchedulerJobs.Services.UnitTests.csproj -c $configuration --results-directory ./TestResults --logger "trx;LogFileName=SchedulerJobs-Services-Unit-Tests-TestResults.trx" \
    "/p:CollectCoverage=true" \
    "/p:Exclude=\"${sdsServiceExclusions}\"" \
    "/p:CoverletOutput=${PWD}/Coverage/" \
    "/p:MergeWith=${PWD}/Coverage/coverage.json" \
    "/p:CoverletOutputFormat=\"opencover,json,cobertura,lcov\""
