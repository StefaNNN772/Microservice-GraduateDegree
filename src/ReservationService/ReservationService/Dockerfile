FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ReservationService.csproj", "./"]
RUN dotnet restore "ReservationService.csproj"

COPY . .
RUN dotnet build "ReservationService.csproj" -c Release -o /app/build

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

RUN if [ ! -d "Migrations" ]; then dotnet ef migrations add InitialCreate; fi

RUN dotnet publish "ReservationService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8083
EXPOSE 8083

ENTRYPOINT ["dotnet", "ReservationService.dll"]