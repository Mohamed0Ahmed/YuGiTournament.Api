# Changelog - YuGi Tournament API

## [2024-01-29] - إضافة نظام الرسائل الودية

### ✨ المميزات الجديدة

-   **نظام رسائل ودي** يعمل بنفس طريقة النظام الأصلي لكن في جدول منفصل
-   **Player Authorization** - اللاعب يستطيع إرسال رسائل بـ Player Role
-   **Admin Authorization** - الإدارة ترد وتدير الرسائل بـ Admin Role
-   **نفس واجهة النظام الأصلي** تماماً (endpoints, DTOs, behavior)
-   **إدارة حالة القراءة** (مقروء/غير مقروء) للرسائل
-   **حذف الرسائل** مع Soft Delete للأمان
-   **تصنيف الرسائل** (رسالة اللاعب / رد الإدارة)

### 🔧 التحسينات

-   **مرتبط بـ ApplicationUser** (نفس النظام الأصلي)
-   **استخدام نفس DTOs** المستخدمة في النظام الأصلي
-   **UnitOfWork Pattern** للأداء العالي
-   **فهرسة محسنة** للأداء العالي
-   **TypeScript Support** للواجهة الأمامية

### 🛡️ الأمان

-   **Player Role** للاعبين (إرسال رسائل وعرض رسائلهم)
-   **Admin Role** للإدارة (عرض وإدارة جميع الرسائل)
-   **نفس نظام الأمان** المستخدم في النظام الأصلي
-   **التحقق من صحة البيانات** للرسائل
-   **Soft Delete** لحماية البيانات

### 📚 التوثيق

-   **Documentation شامل** لنظام الرسائل
-   **أمثلة API** لجميع العمليات
-   **TypeScript Types** كاملة للتطوير
-   **سيناريوهات الاستخدام** العملية

### 🗂️ ملفات جديدة ومُحدثة

-   `Models/FriendlyMessage.cs` - نموذج الرسائل الودية (مرتبط بـ ApplicationUser)
-   `Data/Configurations/FriendlyMessageConfiguration.cs` - إعدادات قاعدة البيانات
-   `Abstractions/IFriendlyMessageService.cs` - واجهة الخدمة (مطابقة لـ IMessageService)
-   `Services/FriendlyMessageService.cs` - تنفيذ الخدمة (مطابق لـ MessageService)
-   `Controllers/FriendlyMessageController.cs` - واجهة API (مطابقة لـ MessageController)
-   `friendly-message-types.ts` - أنواع TypeScript محدثة
-   `FRIENDLY_MESSAGE_DOCUMENTATION.md` - التوثيق الكامل محدث
-   `Data/ApplicationDbContext.cs` - إضافة DbSet<FriendlyMessage>
-   `Program.cs` - تسجيل FriendlyMessageService في DI

---

## [2024-01-28] - إضافة نظام المباريات الودية

### ✨ المميزات الجديدة

-   **نظام المباريات الودية** منفصل تماماً عن البطولات
-   **إدارة اللاعبين** للمباريات الودية (إضافة، حذف، إلغاء تفعيل)
-   **تسجيل المباريات** مع النتائج التفصيلية
-   **النتيجة الإجمالية** بين أي لاعبين مع إحصائيات شاملة
-   **تاريخ المباريات** مع التفاصيل الكاملة
-   **ترتيب اللاعبين** حسب الأداء والإحصائيات
-   **حذف وتعديل** المباريات مع Soft Delete
-   **فلترة المباريات** بين لاعبين محددين
-   **إحصائيات شاملة** لكل لاعب (انتصارات، هزائم، تعادل، معدل الفوز)
-   **إحصائيات مفصلة** بين لاعبين (مجموع النقاط، الانتصارات، الهزائم، معدل الفوز)
-   **نظام النتائج العريضة** (5-0) مع جدول منفصل وتتبع تلقائي
-   **عرض النتائج العريضة** لجميع المباريات أو للاعب محدد

### 🔧 التحسينات

-   **جداول منفصلة** للاعبين والمباريات الودية
-   **Entity Configurations** محسنة للجداول الجديدة
-   **Service Layer** منفصل للمباريات الودية
-   **Controller** منفصل مع جميع العمليات المطلوبة
-   **DTOs** شاملة للبيانات والاستعلامات
-   **Validation** شامل للبيانات المدخلة
-   **Error Handling** محسن مع رسائل باللغة العربية

### 📝 التوثيق

-   **توثيق شامل** لنظام المباريات الودية (`FRIENDLY_MATCH_DOCUMENTATION.md`)
-   **TypeScript interfaces** للفرونت إند (`friendly-match-types.ts`)
-   **Postman Collection** للاختبار (`Friendly_Match_API.postman_collection.json`)
-   **أمثلة عملية** على الاستخدام والاختبار

### 🗄️ قاعدة البيانات

-   **FriendlyPlayers** table للاعبين الوديين
-   **FriendlyMatches** table للمباريات الودية
-   **ShutoutResults** table للنتائج العريضة (5-0)
-   **Foreign Key Relationships** محسنة
-   **Indexes** للأداء الأمثل
-   **Soft Delete** للبيانات
-   **تسجيل تلقائي** للنتائج العريضة عند تسجيل المباريات

### 🔐 الأمان

-   **JWT Authentication** مطلوب لجميع العمليات
-   **Role-based Authorization** (Admin فقط)
-   **Validation** شامل للبيانات المدخلة
-   **Error Messages** باللغة العربية

---

## [2024-01-27] - إضافة الماتشات للدوريات العادية

### ✨ المميزات الجديدة

-   إضافة حقل `matches` للدوريات العادية في `LeagueResponseDto`
-   فصل الماتشات العادية عن الماتشات الإقصائية في الريسبونس
-   تحسين التوثيق ليشمل التغييرات الجديدة

### 🔧 التحسينات

-   تعديل `PlayerService.GetMatchesForLeague()` لإزالة شرط `IsDeleted` للماتشات
-   تحديث `LeagueResponseDto` ليشمل حقل `matches` منفصل عن `knockoutMatches`
-   تحديث التوثيق والـ TypeScript interfaces

### 📝 التوثيق

-   تحديث `API_DOCUMENTATION.md` ليشمل الحقول الجديدة
-   تحديث `frontend-types.ts` لإضافة حقل `matches`
-   إضافة ملاحظات توضيحية للفرق بين أنواع البطولات

---

## [2024-01-15] - تحسينات الريسبونس والتوثيق

### ✨ المميزات الجديدة

-   إضافة DTOs جديدة لتحسين الريسبونس (`LeagueResponseDto`, `PlayerRankDto`, `GroupDto`, `MatchDto`)
-   تحسين الريسبونس لبطولات المجموعات ليكون موحد مع البطولات العادية
-   إضافة دعم المباريات في الريسبونس (بما فيها الأدوار الإقصائية)
-   تحديث endpoint `/Player/players/all` ليشمل المباريات
-   **تحسين بنية بطولات المجموعات**: كل مجموعة تحتوي على مبارياتها، والمباريات الإقصائية منفصلة
-   إضافة توثيق شامل للـ API
-   إضافة TypeScript interfaces للفرونت إند
-   إضافة Postman Collection للاختبار

### 🔧 التحسينات

-   تحسين `GetAllLeaguesWithRankAsync()` لترجع `LeagueResponseDto[]` بدلاً من `object[]`
-   تحسين `GetGroupedPlayersAsync()` لترجع `GroupDto[]` بدلاً من `object[]`
-   إضافة type safety للـ responses
-   تحسين بنية البيانات المرجعة

### 📝 التوثيق

-   إنشاء `API_DOCUMENTATION.md` مع توثيق شامل لجميع الـ endpoints
-   إنشاء `README.md` للمشروع
-   إنشاء `frontend-types.ts` مع TypeScript interfaces
-   إنشاء `YuGi_Tournament_API.postman_collection.json` للاختبار

### 🏗️ البنية الجديدة

#### LeagueResponseDto

```csharp
public class LeagueResponseDto
{
    public int LeagueId { get; set; }
    public string LeagueName { get; set; }
    public string LeagueDescription { get; set; }
    public LeagueType LeagueType { get; set; }
    public SystemOfLeague SystemOfLeague { get; set; }
    public bool IsFinished { get; set; }
    public DateTime CreatedOn { get; set; }
    public List<PlayerRankDto>? Players { get; set; }
    public List<GroupDto>? Groups { get; set; }
}
```

#### PlayerRankDto

```csharp
public class PlayerRankDto
{
    public int PlayerId { get; set; }
    public string FullName { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double Points { get; set; }
    public int MatchesPlayed { get; set; }
    public int Rank { get; set; }
    public double WinRate { get; set; }
}
```

#### GroupDto

```csharp
public class GroupDto
{
    public int GroupNumber { get; set; }
    public List<PlayerRankDto> Players { get; set; }
}
```

### 🔄 التغييرات في الـ API

#### قبل التحديث:

```json
{
    "LeagueId": 1,
    "LeagueName": "بطولة الشتاء",
    "Players": [
        {
            "playerId": 1,
            "fullName": "أحمد",
            "wins": 5,
            "losses": 2,
            "draws": 1,
            "points": 5.5,
            "matchesPlayed": 8,
            "rank": 1,
            "winRate": 62.5
        }
    ],
    "Groups": null
}
```

#### بعد التحديث:

```json
{
    "leagueId": 1,
    "leagueName": "بطولة الشتاء",
    "leagueDescription": "بطولة شتوية",
    "leagueType": 0,
    "systemOfLeague": 0,
    "isFinished": false,
    "createdOn": "2024-01-15T10:30:00Z",
    "players": [
        {
            "playerId": 1,
            "fullName": "أحمد",
            "wins": 5,
            "losses": 2,
            "draws": 1,
            "points": 5.5,
            "matchesPlayed": 8,
            "rank": 1,
            "winRate": 62.5
        }
    ],
    "groups": null
}
```

### 📋 ملاحظات للفرونت إند

1. الريسبونس الآن موحد لجميع أنواع البطولات
2. بطولات المجموعات تحتوي على `groups` array
3. البطولات العادية تحتوي على `players` array
4. يمكن التحقق من نوع البطولة عبر `leagueType` field

### 🚀 الاستخدام في الفرونت إند

#### Angular Service Example:

```typescript
import {
    LeagueResponseDto,
    PlayerRankDto,
    GroupDto,
    MatchDto,
    TournamentStage,
} from "./frontend-types";

@Injectable()
export class TournamentService {
    getAllLeagues(): Observable<LeagueResponseDto[]> {
        return this.http.get<LeagueResponseDto[]>(
            `${this.baseUrl}/Player/players/all`
        );
    }

    displayLeagueData(league: LeagueResponseDto) {
        if (league.leagueType === LeagueType.Groups) {
            // عرض بيانات المجموعات مع مبارياتها
            league.groups?.forEach((group) => {
                console.log(`المجموعة ${group.groupNumber}:`, group.players);

                // عرض مباريات المجموعة
                group.matches?.forEach((match) => {
                    console.log(
                        `  ${match.player1Name} vs ${match.player2Name} (${match.score1}-${match.score2})`
                    );
                });
            });

            // عرض المباريات الإقصائية
            league.knockoutMatches?.forEach((match) => {
                const stageName = this.getTournamentStageName(
                    match.tournamentStage
                );
                console.log(
                    `${stageName}: ${match.player1Name} vs ${match.player2Name} (${match.score1}-${match.score2})`
                );
            });
        } else {
            // عرض بيانات اللاعبين العاديين
            league.players?.forEach((player) => {
                console.log(`${player.fullName}: ${player.points} points`);
            });

            // عرض المباريات (للبطولات العادية)
            league.knockoutMatches?.forEach((match) => {
                const stageName = this.getTournamentStageName(
                    match.tournamentStage
                );
                console.log(
                    `${stageName}: ${match.player1Name} vs ${match.player2Name} (${match.score1}-${match.score2})`
                );
            });
        }
    }

    private getTournamentStageName(stage: TournamentStage): string {
        switch (stage) {
            case TournamentStage.League:
                return "دوري عادي";
            case TournamentStage.GroupStage:
                return "مرحلة المجموعات";
            case TournamentStage.QuarterFinals:
                return "ربع النهائي";
            case TournamentStage.SemiFinals:
                return "نصف النهائي";
            case TournamentStage.Final:
                return "النهائي";
            default:
                return "غير محدد";
        }
    }
}
```

### 🔧 Breaking Changes

-   تغيير نوع الريسبونس من `object[]` إلى `LeagueResponseDto[]`
-   تغيير أسماء الحقول في الريسبونس (PascalCase إلى camelCase)

### 📦 الملفات المضافة

-   `API_DOCUMENTATION.md` - توثيق شامل للـ API
-   `README.md` - دليل المشروع
-   `frontend-types.ts` - TypeScript interfaces
-   `YuGi_Tournament_API.postman_collection.json` - Postman collection
-   `CHANGELOG.md` - هذا الملف
-   `DTOs/LeagueResponseDto.cs` - DTOs جديدة

---

_آخر تحديث: 15 يناير 2024_
