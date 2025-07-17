# استخدم صورة SDK للبناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# انسخ ملفات المشروع فقط
COPY YuGiTournament.Api.csproj ./
COPY . .

# استرجع الحزم
RUN dotnet restore "YuGiTournament.Api.csproj"

# بناء التطبيق
RUN dotnet publish "YuGiTournament.Api.csproj" -c Release -o /app/publish

# استخدم صورة runtime فقط للتشغيل
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "YuGiTournament.Api.dll"] 