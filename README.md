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

To run the Sds Scheduler Jobs locally:

1. Populate the user secrets for the SchedulerJobs.Sds project. You can use the same ones as the SchedulerJobs project. If you don't have them you can download a template here and then populate them with the same values as the dev environment: https://tools.hmcts.net/confluence/display/VIH/Project+Setup+-+User+Secrets#ProjectSetupUserSecrets-VH-Scheduler-Jobs
2. Enter the name of the job that you want to run inside the SchedulerJobs/SchedulerJobs.Sds/Properties.launchSettings.json as a `commandLineArg`.

#### Example - running the ClearHearingsJob

`SchedulerJobs.Sds/Properties.launchSettings.json`

``` json
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

3. Run the SchedulerJobs.Sds project

### Run the cron jobs

In the hosted environment the jobs run as cron jobs in Kubernetes.

These jobs run on a pre-defined schedule. The schedules can be configured by changing the `schedule` property inside the helm chart within `charts/vh-scheduler-jobs.values.yaml`.

#### Example - changing the ClearHearingsJob to run at 10am UTC

`charts/vh-scheduler-jobs.values.yaml`

``` yaml
- cronJobName: vh-clear-hearings-job
  schedule: "0 10 * * *"
  concurrencyPolicy: Forbid
  args:
  - ClearHearingsJob
```

Additional details:

**cronJobName**: the name of the cron job resource in Kubernetes  
**schedule**: a cron expression denoting the timing of the job  
**concurrencyPolicy**: currently set to Forbid to prevent multiple instances of the job from running at the same time  
**args**: the value in here should match the name of the associated job's class in .NET (inside `SchedulerJobs/SchedulerJobs.Sds/Jobs`)

**Important note**: any changes made to the cron jobs in the `values.yaml` must also be applied to the `charts/vh-scheduler-jobs/values.dev.template.yaml`.

More information on cron jobs here: https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/

To run one of the cron jobs on demand in Kubernetes (eg for testing) you can edit the schedule inside the yml file for the `CronJob` resource under `Workloads` using a tool like Lens. Full setup guide for using Lens on the SDS cluster is here: https://tools.hmcts.net/confluence/display/PUBH/Troubleshooting

This is an alternative to the Trigger command which cannot currently be used due to permission restrictions on the cluster.

#### Example - changing the ClearHearingsJob to run at 10am UTC alternative

```` yaml
spec:
  schedule: 0 10 * * *
  concurrencyPolicy: Forbid
  suspend: false
````

and then saving the file.

When the job runs, a temporary pod will be created in Kubernetes, with a Success/Failed status denoting the result.

#### Example - running clear hearings job

| Name                        | Status    |
| --------------------------- | --------- |
| vh-clear-hearings-job-pr-93 | Succeeded |

### Logging

Short-lived logs for the cron jobs can be viewed under the logs for the created Kubernetes pods. Long-lived logs are available in application insights as Traces or Exceptions.

#### Example: traces for clear hearings job

``` ksl
traces 
| where cloud_RoleInstance like 'vh-clear-hearings-job'
```

#### Example: exceptions for clear hearings job

``` ksl
exceptions 
| where cloud_RoleInstance like 'vh-clear-hearings-job'
```

Job failures can also be viewed by using the customDimensions.FormattedMessage:

#### Example: viewing all job failures

``` ksl
exceptions 
| where customDimensions.FormattedMessage like 'Job failed'
```

## Running the app as a container

Visit the VH-Setup repository for
[Instructions to run as a container locally.](https://github.com/hmcts/vh-setup/tree/main/docs/local-container-setup).
