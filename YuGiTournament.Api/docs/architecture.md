## المعمارية والبنية

-   طبقات منطقية:

    -   Controllers: تعريف Endpoints
    -   Services: منطق الأعمال (حقن عبر DI)
    -   Repositories + UnitOfWork: الوصول للبيانات EF Core
    -   Data: `ApplicationDbContext` + Configurations
    -   Models/DTOs: نماذج الكيان وكائنات النقل

-   أنظمة فرعية:

    -   الدوري: بدء/إنهاء، مجموعات، أدوار إقصائية
    -   المباريات: نظام نقاط/كلاسيك عبر `IMatchServiceSelector`
    -   النظام الودي: لاعبين/مباريات/إحصائيات/نتائج 5-0
    -   الرسائل والملاحظات: إدارة تواصل وإعلانات

-   اختيار خدمة المباريات:

    -   `MatchServiceSelector.GetMatchService(SystemOfLeague)`
        -   Points → `PointMatchService`
        -   Classic → `ClassicMatchService`

-   الأمان: JWT + Roles (Admin/Player)
