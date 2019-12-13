rmdir /q /s Artifacts

SET exclude=\"[*]SchedulerJobs.Common.*,[SchedulerJobs.Services]SchedulerJobs.Services.BookingApiService,[*]SchedulerJobs.UnitTests.*,[SchedulerJobs]SchedulerJobs.Startup"
dotnet test --no-build SchedulerJobs/SchedulerJobs.UnitTests/SchedulerJobs.UnitTests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat="\"opencover,cobertura,json,lcov\"" /p:CoverletOutput=../Artifacts/Coverage/ /p:MergeWith='../Artifacts/Coverage/coverage.json' /p:Exclude="${exclude}"

reportgenerator -reports:Artifacts/Coverage/coverage.opencover.xml -targetDir:Artifacts/Coverage/Report -reporttypes:HtmlInline_AzurePipelines

"Artifacts/Coverage/Report/index.htm"