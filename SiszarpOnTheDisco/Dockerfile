FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base

RUN apt-get update && apt-get install fontconfig --no-install-recommends --yes

RUN groupadd -r pptruser && \
    useradd -r -g pptruser -G audio,video pptruser && \
    mkdir -p /home/pptruser  && \
    mkdir -p /home/pptruser/Downloads && \
    mkdir -p /app && \
    mkdir -p /pics  && \
    chown -R pptruser:pptruser /home/pptruser && \
    chown -R pptruser:pptruser /app && \
    chown -R pptruser:pptruser /pics

USER pptruser
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SiszarpOnTheDisco/SiszarpOnTheDisco.csproj", "SiszarpOnTheDisco/"]
RUN dotnet restore "SiszarpOnTheDisco/SiszarpOnTheDisco.csproj"
COPY . .
WORKDIR "/src/SiszarpOnTheDisco"
RUN dotnet build "SiszarpOnTheDisco.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SiszarpOnTheDisco.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SiszarpOnTheDisco.dll"]