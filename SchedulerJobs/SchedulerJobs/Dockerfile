FROM mcr.microsoft.com/dotnet/sdk:6.0 AS installer-env
RUN mkdir -p /home/site/wwwroot
COPY SchedulerJobs .

RUN dotnet publish SchedulerJobs --configfile nuget.config -c Production -o /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet:4.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true 
RUN apt-get update && apt-get -yq install curl
EXPOSE 80
EXPOSE 443

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
