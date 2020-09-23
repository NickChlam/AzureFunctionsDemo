FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS installer-env

COPY . /src/dotnet-function-app
RUN cd /src/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:3.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet:3.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

ENV AzureWebJobsStorage="UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azureite"
ENV SendGridApiKey="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"
ENV EmailSender="Nick@krevaas.com"
ENV  ConnectionString="Server=sql-server-db,1433; Database=Master;User Id=SA;Password=!passw0rd1985"

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]