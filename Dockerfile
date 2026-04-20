FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TreinamentosManager/TreinamentosManager.csproj TreinamentosManager/
RUN dotnet restore TreinamentosManager/TreinamentosManager.csproj
COPY . .
RUN dotnet publish TreinamentosManager/TreinamentosManager.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT
ENTRYPOINT ["dotnet", "TreinamentosManager.dll"]
