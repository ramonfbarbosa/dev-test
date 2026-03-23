# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Core/Domain/Domain.csproj Core/Domain/
COPY Core/Common/Common.csproj Core/Common/
COPY Core/Application/Application.csproj Core/Application/
COPY Infra/Persistence/Persistence.csproj Infra/Persistence/
COPY Presentation/WebApi/WebApi.csproj Presentation/WebApi/

RUN dotnet restore Presentation/WebApi/WebApi.csproj

COPY . .
RUN dotnet publish Presentation/WebApi/WebApi.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8081
ENTRYPOINT ["dotnet", "WebApi.dll"]
