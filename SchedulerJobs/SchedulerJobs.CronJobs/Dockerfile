FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SchedulerJobs/SchedulerJobs.CronJobs/SchedulerJobs.CronJobs.csproj", "SchedulerJobs.CronJobs/"]
COPY ["SchedulerJobs/SchedulerJobs.Services/SchedulerJobs.Services.csproj", "SchedulerJobs.Services/"]
COPY ["SchedulerJobs/SchedulerJobs.Common/SchedulerJobs.Common.csproj", "SchedulerJobs.Common/"]
COPY ["SchedulerJobs/nuget.config", "SchedulerJobs/"]
RUN dotnet restore "SchedulerJobs.CronJobs/SchedulerJobs.CronJobs.csproj" --configfile SchedulerJobs/nuget.config
COPY SchedulerJobs .
WORKDIR "/src/SchedulerJobs.CronJobs"
RUN dotnet build "SchedulerJobs.CronJobs.csproj" -c Production -o /app/build --configfile ../SchedulerJobs/nuget.config

FROM build AS publish
RUN dotnet publish "SchedulerJobs.CronJobs.csproj" -c Production -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchedulerJobs.CronJobs.dll"]