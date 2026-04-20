FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TreinamentosManager/TreinamentosManager.csproj TreinamentosManager/
RUN dotnet restore TreinamentosManager/TreinamentosManager.csproj
COPY . .
RUN dotnet publish TreinamentosManager/TreinamentosManager.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TreinamentosManager.dll"]
