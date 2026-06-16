FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["KIDIO.sln", "./"]
COPY ["KIDIO.API/KIDIO.API.csproj", "KIDIO.API/"]
COPY ["KIDIO.Business/KIDIO.Business.csproj", "KIDIO.Business/"]
COPY ["KIDIO.Data/KIDIO.Data.csproj", "KIDIO.Data/"]
COPY ["KIDIO.Common/KIDIO.Common.csproj", "KIDIO.Common/"]

RUN dotnet restore

# Copy everything else and build app
COPY . .
WORKDIR /src/KIDIO.API
RUN dotnet publish -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Tắt HTTPS trong Docker để Render tự động cấu hình HTTPS ở proxy layer
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "KIDIO.API.dll"]
