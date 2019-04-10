FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Graph.Farm.Website/Graph.Farm.Website.csproj", "Graph.Farm.Website/"]
RUN dotnet restore "Graph.Farm.Website/Graph.Farm.Website.csproj"
COPY . .
WORKDIR "/src/Graph.Farm.Website"
RUN dotnet build "Graph.Farm.Website.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Graph.Farm.Website.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Graph.Farm.Website.dll"]