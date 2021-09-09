FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["KafkaListener/KafkaListener.csproj", "KafkaListener/"]
RUN dotnet restore "KafkaListener/KafkaListener.csproj"
COPY . .
WORKDIR "/src/KafkaListener"
RUN dotnet build "KafkaListener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KafkaListener.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KafkaListener.dll"]