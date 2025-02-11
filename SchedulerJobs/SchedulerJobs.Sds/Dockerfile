FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src
COPY SchedulerJobs .
RUN dotnet restore --configfile nuget.config "SchedulerJobs.Sds/SchedulerJobs.Sds.csproj"
WORKDIR "/src/SchedulerJobs.Sds"
RUN dotnet publish "SchedulerJobs.Sds.csproj" -c Production -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchedulerJobs.Sds.dll"]