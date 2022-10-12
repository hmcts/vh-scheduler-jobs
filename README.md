# vh-scheduler-jobs
Scheduler web jobs

There are 2 versions of the Scheduler Jobs, each in a different project:

**SchedulerJobs** - the legacy version hosted on the Reform infrastructure, an Azure functions app
**SchedulerJobs.Sds** - the new version for the Sds infrastructure, a set of background services with Kubernetes cron jobs

## SchedulerJobs

### Run
To run Sheduler-jobs function:
AzureServiceTokenProvider will use the developer's security context to get a token to authenticate to Key Vault. This removes the need to create a service principal, and share it with the development team. It also prevents credentials from being checked in to source code. AzureServiceTokenProvider will use Azure CLI or Active Directory Integrated Authentication to authenticate to Azure AD to get a token. That token will be used to fetch the secret from Azure Key Vault.

Azure CLI will work if the following conditions are met:
You have Azure CLI 2.0 installed. Version 2.0.12 supports the get-access-token option used by AzureServiceTokenProvider. If you have an earlier version, please upgrade.

You are logged into Azure CLI. You can login using az login command.
Azure Active Directory Authentication will only work if the following conditions are met:
Your on-premise active directory is synced with Azure AD.
You are running this code on a domain joined machine.
Since your developer account has access to the Key Vault, you should see the secret on the web page. Principal Used will show type "User" and your user account.

The user secrets file is required. 

## SchedulerJobs.Sds

### Run
To run the Sds Scheduler Jobs:

1. Populate the user secrets for the SchedulerJobs.Sds project. You can use the same ones as the SchedulerJobs project. If you don't have them you can download a template here and then populate them with the same values as the dev environment: https://tools.hmcts.net/confluence/display/VIH/Project+Setup+-+User+Secrets#ProjectSetupUserSecrets-VH-Scheduler-Jobs
2. Enter the name of the job that you want to run inside the SchedulerJobs/SchedulerJobs.Sds/Properties.launchSettings.json as a `commandLineArg`. eg

```
{
  "profiles": {
    "SchedulerJobs.Sds": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "environmentVariables": {
        "DOTNET_ENVIRONMENT": "Development"
      },
      "commandLineArgs": "ClearHearingsJob"
    }
  }
}
```

the full list of job names avaiable to use is in the SchedulerJobs/SchedulerJobs.Sds/Program.cs, inside the `RegisterJobs` method.

### Changing the cron jobs
