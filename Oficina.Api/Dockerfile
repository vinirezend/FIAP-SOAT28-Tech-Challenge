# Build context: raiz da solution (docker build -f Oficina.Api/Dockerfile .)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Restore em camada separada para aproveitar cache
COPY Oficina.Api/Oficina.Api.csproj Oficina.Api/
COPY Oficina.Application/Oficina.Application.csproj Oficina.Application/
COPY Oficina.Domain/Oficina.Domain.csproj Oficina.Domain/
COPY Oficina.Infrastructure/Oficina.Infrastructure.csproj Oficina.Infrastructure/
RUN dotnet restore Oficina.Api/Oficina.Api.csproj

COPY Oficina.Api/ Oficina.Api/
COPY Oficina.Application/ Oficina.Application/
COPY Oficina.Domain/ Oficina.Domain/
COPY Oficina.Infrastructure/ Oficina.Infrastructure/
RUN dotnet publish Oficina.Api/Oficina.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Oficina.Api.dll"]
