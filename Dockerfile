FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["DepartmentLoadApp.csproj", "./"]
RUN dotnet restore "DepartmentLoadApp.csproj"

COPY . .
RUN dotnet publish "DepartmentLoadApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DepartmentLoadApp.dll"]