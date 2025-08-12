## إعدادات وتشغيل التطبيق

### الاتصال بقاعدة البيانات

-   `ConnectionStrings:YuGiContext` (PostgreSQL عبر Npgsql)

### الهوية والتوثيق

-   Identity مُفعّل مع مزود EF
-   JWT Bearer:
    -   `Jwt:Issuer`
    -   `Jwt:Audience`
    -   `Jwt:Key`
    -   `Jwt:ExpireDays`

### CORS

-   سياسة `AllowFrontend` تسمح بالمصدرين:
    -   `https://mohamed0ahmed.github.io`
    -   `http://localhost:4200`

### Middlewares

-   UseHttpsRedirection, StaticFiles
-   Logging مبسط لطلبات HTTP
-   Authentication, Authorization

### نقاط فحص سريعة

-   `/`، `/api/health`، `/api/test`

### Seed للمستخدم الأدمن

-   بريد: `a@y.com`
-   كلمة مرور: `123456`
-   أدوار يتم إنشاؤها: `Admin`, `Player`
