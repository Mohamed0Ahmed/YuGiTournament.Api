## YuGi Tournament API - Documentation

### محتوى الوثائق

-   مصادقة: راجع auth.md
-   إعدادات وتشغيل التطبيق: راجع configuration.md
-   المعمارية والبنية: راجع architecture.md
-   نماذج البيانات (Models): راجع models.md
-   كائنات النقل (DTOs): راجع dtos.md
-   الدوري (بدء/إدارة المراحل): راجع league.md
-   المباريات (نظام النقاط/الكلاسيك): راجع matches.md
-   اللاعبين (إدارة/ترتيب/مجموعات): راجع players.md
-   الرسائل (النظام الرئيسي): راجع messages.md
-   الملاحظات: راجع notes.md
-   النظام الودي (لاعبين/مباريات/إحصائيات/نتائج عريضة): راجع friendly-matches.md
-   رسائل النظام الودي: راجع friendly-messages.md
-   نظام الفرق (Multi - Teams): راجع multi.md

### نظرة عامة

-   الطبقات: Controllers → Services → Repositories/UnitOfWork → Data/EF Configurations → Models/DTOs
-   الهوية: ASP.NET Core Identity + JWT (أدوار: Admin, Player)
-   قاعدة البيانات: EF Core + PostgreSQL (Npgsql)
-   CORS: السماح لـ `https://mohamed0ahmed.github.io` و`http://localhost:4200`

### أنواع وأنظمة الدوريات

-   LeagueType:
    -   Single: Round-Robin؛ كل لاعب يواجه كل لاعب مرة واحدة. راجع التفاصيل في `league.md` و`matches.md`.
    -   Groups: دور مجموعات وإقصائيات. راجع `league.md`.
    -   Multi (Teams): فرق تواجه فرق، كل لاعب يواجه كل لاعبي الفرق الأخرى؛ نظام التسجيل للمباراة حسب Points/Classic. راجع `multi.md`.
-   SystemOfLeague:
    -   Points: جولات داخل المباراة؛ فوز الجولة = 1، تعادل الجولة = 0.5؛ الافتراضي 5 جولات (قابل للتعديل). راجع `matches.md`.
    -   Classic: مباراة واحدة؛ فوز = 3 نقاط، تعادل = 1 نقطة لكل لاعب (في المجموعات). راجع `matches.md`.

### روابط سريعة

-   GET `/api/health` — فحص الصحة
-   GET `/` — فحص التشغيل
-   GET `/api/test` — فحص API
