# Build and publish a release
# dotnet publish -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY out .
ENTRYPOINT ["dotnet", "Staas.dll"]