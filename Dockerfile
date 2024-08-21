FROM a3pdcdsacr01.azurecr.io/serverautomation/dotnet/aspnet:8 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 443
ENV ASPNETCORE_URLS=http://*:8080

COPY . .
USER app
ENTRYPOINT ["dotnet", "TestService.API.dll"]