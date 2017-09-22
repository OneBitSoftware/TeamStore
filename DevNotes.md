# Developer Guide

To run/test/debug the project, various dependencies need to be set up:

1. Database - data.db must exist in the root directory
2. The Azure AD Graph Api secret must be set up to be used by the ASP.NET Core Data Protection API
3. You must run the IIS Express web server in a Reply URL configured in Azure's App registrations:
* https://localhost:44366/signin-oidc
* https://localhost:61413/signin-oidc
* https://localhost:44342/signin-oidc
* https://localhost:44325/signin-oidc
* https://localhost:44336/signin-oidc
* https://onebit-teamstore.azurewebsites.net/signin-oidc
4. The ASP.NET Core Data Protection API needs a valid key created (which doesn't expire soon)
5. You need the App to be configured with permissions to an Azure AD Tenant.
6. You need the .NET Core 2 SDK.

## Database Setup

To create/update the local database, run the following from the [ProjectRoot]\TeamStore folder:

1. Build the project
2. dotnet ef database update - creates the DB and applies the last migration. You should be ready to go.
3. dotnet ef migrations add [MigrationName666] - this is used when the Models need updating.

NOTE: if you have issues, delete "data.db" in the project root to start over. EF Migrations will recreate it.


## Graph API Secret

The key "Authentication:AzureAd:ClientSecret" must exist with the appropriate secret key. Keep the secret private.

Manage secrets with these commands:
* dotnet user-secrets list - This will list all secrets for the project
* dotnet user-secrets set Authentication:AzureAd:ClientSecret ValueOfMySecret12345 - you MUST be in the correct project folder (TeamStore for web, IntegrationTests for tests) unless you use the --project parameter

## Data Protection API Key - Database Encryption

ASP.NET Core authentication and cookies use it's normal key DP API. We use a seperate key for DB encryption before writing to the DB and leave ASP.NET to deal with cookies. 
This DB key must be managed securely.

This key is stored in the [ProjectRoot]\Keys folder.

You can generate your own DB encryption key by running the integration tests after 
commentiing out the following:
* options.DisableAutomaticKeyGeneration(); from EncryptionService.cs, line 19
* copy the key from the IntegrationTests\Keys folder to the TeamStore\Keys folder (in the web application)

OR you can copy the one from the UnitTests / IntegrationTests folder for dev purposes.

I haven't figured out a CLI way to generate the keys yet.

## Azure AD Setup

Everything is configured to currently run in my dev Azure AD instance. The application is set up with the return URL's and Key. 

The Azure AD tenant details and client ID are in the appsettings.json file. You can register your own Azure AD Application and change them to yours if you wish. The Graph API secret will also need updating.


## Test projects
Integration Tests create/read data against a real database - testdatabase.db in the IntegrationTests folder. It will be created with .EnsureCreated during test run. 
It will not be deleted.

Unit Tests use the EFCore In-memory database provider.