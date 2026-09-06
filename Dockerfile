FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Minioasis.Api/Minioasis.Api.csproj Minioasis.Api/
COPY Minioasis.Application/Minioasis.Application.csproj Minioasis.Application/
COPY Minioasis.Koha/Minioasis.Koha.csproj Minioasis.Koha/
RUN dotnet restore Minioasis.Api/Minioasis.Api.csproj

COPY Minioasis.Api/ Minioasis.Api/
COPY Minioasis.Application/ Minioasis.Application/
COPY Minioasis.Koha/ Minioasis.Koha/
RUN dotnet publish Minioasis.Api/Minioasis.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Minioasis.Api.dll"]
