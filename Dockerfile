FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556 AS build
WORKDIR /app

COPY ["CheckoutAPI.csproj", "./"]
RUN dotnet restore "CheckoutAPI.csproj"

COPY . .
RUN dotnet publish "CheckoutAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:b4bea3a52a0a77317fa93c5bbdb076623f81e3e2f201078d89914da71318b5d8 AS runtime
WORKDIR /app
EXPOSE 8080
USER app


COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CheckoutAPI.dll"]