# المرحلة الأولى: بناء المشروع
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RequestLifeCycle.csproj", "./"]
RUN dotnet restore "RequestLifeCycle.csproj"
COPY . .
RUN dotnet publish -c Release -o /app/publish

# المرحلة الثانية: تشغيل التطبيق
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RequestLifeCycle.dll"]