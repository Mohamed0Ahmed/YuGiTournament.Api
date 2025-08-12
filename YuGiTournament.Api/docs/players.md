## اللاعبين (Player)

### مسارات

-   POST `api/Player` (Admin) — إضافة لاعب (Body: PlayerAddedDto)
-   GET `api/Player` — جميع اللاعبين
-   GET `api/Player/ranking` — ترتيب اللاعبين
-   GET `api/Player/players/all` — جميع الدوريات مع الترتيب والمباريات
-   DELETE `api/Player/{playerId}` (Admin) — حذف لاعب
-   GET `api/Player/league/{leagueId}/groups` — مجموعات اللاعبين لدوري محدد
