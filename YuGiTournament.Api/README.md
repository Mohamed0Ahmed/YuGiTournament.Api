# YuGi Tournament API

## نظرة عامة

API لإدارة بطولات YuGi Oh مع دعم أنواع مختلفة من البطولات (فردية، متعددة، مجموعات) وأنظمة مختلفة (نقاط، كلاسيكي).

## المميزات

-   ✅ إدارة اللاعبين والبطولات
-   ✅ دعم بطولات المجموعات
-   ✅ نظام رسائل بين اللاعبين
-   ✅ نظام ملاحظات
-   ✅ JWT Authentication
-   ✅ Role-based Authorization (Admin/Player)

## التقنيات المستخدمة

-   **.NET 8**
-   **ASP.NET Core Web API**
-   **Entity Framework Core**
-   **SQL Server**
-   **JWT Authentication**

## البداية السريعة

### المتطلبات

-   .NET 8 SDK
-   SQL Server
-   Visual Studio 2022 أو VS Code

### التثبيت

1. Clone المشروع

```bash
git clone <repository-url>
```

2. تحديث connection string في `appsettings.json`

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=...;Database=YuGiTournament;..."
    }
}
```

3. تشغيل الـ migrations

```bash
dotnet ef database update
```

4. تشغيل المشروع

```bash
dotnet run
```

## بنية المشروع

```
YuGiTournament.Api/
├── Controllers/          # API Controllers
├── Services/            # Business Logic
├── Models/              # Entity Models
├── DTOs/                # Data Transfer Objects
├── Repositories/        # Data Access Layer
├── Abstractions/        # Interfaces
└── Data/               # Database Context & Configurations
```

## التوثيق

راجع ملف `API_DOCUMENTATION.md` للحصول على توثيق شامل لجميع الـ endpoints.

## أنواع البطولات

-   **Single (0)**: بطولة فردية
-   **Multi (1)**: بطولة متعددة
-   **Groups (2)**: بطولة مجموعات

## أنظمة البطولات

-   **Points (0)**: نظام النقاط
-   **Classic (1)**: النظام الكلاسيكي

## المساهمة

1. Fork المشروع
2. إنشاء feature branch
3. Commit التغييرات
4. Push للـ branch
5. إنشاء Pull Request

## الترخيص

هذا المشروع مرخص تحت MIT License.

---

_تم تطويره بواسطة فريق YuGi Tournament_
