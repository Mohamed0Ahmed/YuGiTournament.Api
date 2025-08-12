## النظام الودي: لاعبين/مباريات/إحصائيات/نتائج 5-0

### إدارة اللاعبين

-   GET `api/FriendlyMatch/players` — كل اللاعبين الوديين
-   GET `api/FriendlyMatch/players/{playerId}` (Admin) — لاعب محدد
-   POST `api/FriendlyMatch/players` (Admin) — إضافة لاعب (Body: AddFriendlyPlayerDto)
-   DELETE `api/FriendlyMatch/players/{playerId}` (Admin) — حذف لاعب
-   PUT `api/FriendlyMatch/players/{playerId}/deactivate` (Admin) — تعطيل لاعب
-   GET `api/FriendlyMatch/players/ranking` — ترتيب الوديين
-   GET `api/FriendlyMatch/players/{playerId}/stats` — إحصائيات لاعب

### إدارة المباريات

-   GET `api/FriendlyMatch/matches` — كل المباريات
-   GET `api/FriendlyMatch/matches/{p1}/{p2}` — مباريات بين لاعبين
-   POST `api/FriendlyMatch/matches/record` (Admin) — تسجيل مباراة (Body: RecordFriendlyMatchDto)
-   DELETE `api/FriendlyMatch/matches/{matchId}` (Admin) — حذف مباراة
-   PUT `api/FriendlyMatch/matches/{matchId}` (Admin) — تعديل مباراة

### إحصائيات وتحليلات

-   GET `api/FriendlyMatch/overall-score/{p1}/{p2}` — إجمالي النتائج بين لاعبين
-   GET `api/FriendlyMatch/players/statistics` — إحصاءات اللاعبين

### نتائج عريضة (5-0)

-   GET `api/FriendlyMatch/shutouts` — كل النتائج العريضة
-   GET `api/FriendlyMatch/shutouts/player/{playerId}` — حسب لاعب
-   GET `api/FriendlyMatch/shutouts/match/{matchId}` — حسب مباراة
-   DELETE `api/FriendlyMatch/shutouts/{shutoutId}` (Admin) — حذف نتيجة عريضة

### تصفية وPagination

-   GET `api/FriendlyMatch/shutouts/filtered` — تصفية النتائج العريضة (Query: ShutoutFilterDto)
-   GET `api/FriendlyMatch/shutouts/player/{playerId}/filtered` — تصفية نتائج لاعب
-   POST `api/FriendlyMatch/shutouts/players/filtered` — تصفية لعدة لاعبين (Body: number[])
-   GET `api/FriendlyMatch/matches/filtered` — تصفية المباريات (Query: MatchFilterDto)
-   GET `api/FriendlyMatch/matches/player/{playerId}` — تصفية مباريات لاعب
-   POST `api/FriendlyMatch/matches/player/{playerId}/vs-opponents` — لاعب ضد منافسين محددين (Body: number[])

### قواعد تحقق مهمة

-   لا يُسمح بلاعب ضد نفسه
-   الدرجات ≥ 0
-   التحقق من وجود وتفعيل اللاعبين قبل التسجيل
-   تسجيل نتيجة 5-0 ينتج `ShutoutResult` تلقائيًا
