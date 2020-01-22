# vh-scheduler-jobs
Scheduler web jobs

# Run
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
