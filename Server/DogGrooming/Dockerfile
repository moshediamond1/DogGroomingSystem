# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["DogGrooming/DogGroomingApi.csproj", "DogGrooming/"]
RUN dotnet restore "DogGrooming/DogGroomingApi.csproj"

# Copy everything and publish
COPY . .
WORKDIR "/src/DogGrooming"
RUN dotnet publish "DogGroomingApi.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "DogGroomingApi.dll"]
