FROM gittools/gitversion:5.3.5-linux-alpine.3.10-x64-netcoreapp3.1 AS gitversion
WORKDIR /version 
COPY .git .git
RUN /tools/dotnet-gitversion /ensureassemblyinfo /updateassemblyinfo AssemblyInfo.cs /nofetch /config /GitVersion.yml

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN dotnet publish SchedulerJobs --configfile nuget.config -c Production -o /home/site/wwwroot

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
