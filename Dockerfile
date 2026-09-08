# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY EmpathicCulturalNetwork.sln ./
COPY src/Empathic.Domain/Empathic.Domain.csproj src/Empathic.Domain/
COPY src/Empathic.Application/Empathic.Application.csproj src/Empathic.Application/
COPY src/Empathic.Infrastructure/Empathic.Infrastructure.csproj src/Empathic.Infrastructure/
COPY src/Empathic.Api/Empathic.Api.csproj src/Empathic.Api/
RUN dotnet restore EmpathicCulturalNetwork.sln

COPY . .
RUN dotnet publish src/Empathic.Api/Empathic.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
RUN mkdir -p /app/App_Data

VOLUME ["/app/App_Data"]
ENTRYPOINT ["dotnet", "Empathic.Api.dll"]
