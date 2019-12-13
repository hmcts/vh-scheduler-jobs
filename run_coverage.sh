#bash 
rm -rf Artifacts

exclude=\"[*]SchedulerJobs.Common.*,[SchedulerJobs.Services]SchedulerJobs.Services.BookingApiService,[*]SchedulerJobs.UnitTests.*,[SchedulerJobs]SchedulerJobs.Startup\"
dotnet test --no-build SchedulerJobs/SchedulerJobs.UnitTests/SchedulerJobs.UnitTests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat="\"opencover,cobertura,json,lcov\"" /p:CoverletOutput=../../Artifacts/Coverage/ /p:MergeWith='../Artifacts/Coverage/coverage.json' /p:Exclude="${exclude}"

~/.dotnet/tools/reportgenerator -reports:Artifacts/Coverage/coverage.opencover.xml -targetDir:./Artifacts/Coverage/Report -reporttypes:HtmlInline_AzurePipelines

open ./Artifacts/Coverage/Report/index.htm