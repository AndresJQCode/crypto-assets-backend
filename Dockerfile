FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
ENV TZ=America/Bogota
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app/published-app



FROM mcr.microsoft.com/dotnet/aspnet:9.0 as runtime
ENV TZ=America/Bogota
WORKDIR /app
COPY --from=build /app/published-app /app
ENTRYPOINT [ "dotnet", "/app/Api.dll" ]