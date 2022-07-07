#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["StatefulWebService/StatefulWebService.csproj", "StatefulWebService/"]
RUN dotnet restore "StatefulWebService/StatefulWebService.csproj"
COPY . .
WORKDIR "/src/StatefulWebService"
RUN dotnet build "StatefulWebService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StatefulWebService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StatefulWebService.dll"]