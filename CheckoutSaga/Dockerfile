FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556 AS build
WORKDIR /src

# 1. Copy project files to restore (leveraging Docker's cache)
COPY ["CheckoutSaga/CheckoutSaga.csproj", "CheckoutSaga/"]
COPY ["Checkout.Contracts/Checkout.Contracts.csproj", "Checkout.Contracts/"]
COPY ["DAL/DAL.csproj", "DAL/"]

# 2. Restore dependencies
RUN dotnet restore "CheckoutSaga/CheckoutSaga.csproj"

# 3. Copy rest of the source code
COPY CheckoutSaga/ CheckoutSaga/
COPY Checkout.Contracts/ Checkout.Contracts/
COPY DAL/ DAL/

# 4. Compile
WORKDIR "/src/CheckoutSaga"
RUN dotnet publish "CheckoutSaga.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 5. Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:b4bea3a52a0a77317fa93c5bbdb076623f81e3e2f201078d89914da71318b5d8 AS runtime
WORKDIR /app
EXPOSE 8080
USER app


COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CheckoutSaga.dll"]