#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["SampleWebService/SampleWebService.csproj", "SampleWebService/"]
RUN dotnet restore "SampleWebService/SampleWebService.csproj"
COPY . .
WORKDIR "/src/SampleWebService"
RUN dotnet build "SampleWebService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleWebService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apk --no-cache add curl

ENTRYPOINT ["dotnet", "SampleWebService.dll"]