FROM gittools/gitversion:5.3.5-linux-alpine.3.10-x64-netcoreapp3.1 AS gitversion
WORKDIR /version 
COPY .git .git
RUN /tools/dotnet-gitversion /ensureassemblyinfo /updateassemblyinfo AssemblyInfo.cs /nofetch /config /GitVersion.yml

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS installer-env
RUN mkdir -p /home/site/wwwroot
COPY SchedulerJobs .
COPY --from=gitversion /version/AssemblyInfo.cs .

RUN dotnet publish SchedulerJobs --configfile nuget.config -c Debug -o /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet:3.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true 
#RUN apt-get update && apt-get -yq install curl
EXPOSE 80

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
