# نظام المباريات الودية - Friendly Match System

## 🎯 نظرة عامة

نظام المباريات الودية هو نظام منفصل تماماً عن نظام البطولات، يتيح للاعبين تسجيل مباريات ودية بينهم مع تتبع النتائج الإجمالية والإحصائيات.

### المميزات الرئيسية:

-   ✅ **نظام منفصل تماماً** عن البطولات
-   ✅ **إدارة اللاعبين** للمباريات الودية
-   ✅ **تسجيل المباريات** مع النتائج التفصيلية
-   ✅ **النتيجة الإجمالية** بين أي لاعبين
-   ✅ **تاريخ المباريات** مع التفاصيل
-   ✅ **إحصائيات شاملة** لكل لاعب
-   ✅ **ترتيب اللاعبين** حسب الأداء
-   ✅ **حذف وتعديل** المباريات
-   ✅ **Soft Delete** للبيانات

---

## 🗄️ تصميم قاعدة البيانات

### الجداول الجديدة:

#### 1. FriendlyPlayers Table:

```sql
CREATE TABLE FriendlyPlayers (
    PlayerId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);
```

#### 2. FriendlyMatches Table:

```sql
CREATE TABLE FriendlyMatches (
    MatchId INT PRIMARY KEY IDENTITY(1,1),
    Player1Id INT NOT NULL,
    Player2Id INT NOT NULL,
    Player1Score INT NOT NULL DEFAULT 0,
    Player2Score INT NOT NULL DEFAULT 0,
    PlayedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,

    FOREIGN KEY (Player1Id) REFERENCES FriendlyPlayers(PlayerId),
    FOREIGN KEY (Player2Id) REFERENCES FriendlyPlayers(PlayerId)
);
```

---

## 📡 API Endpoints

### 🔐 Authentication

#### Endpoints متاحة للجميع (Public):

-   عرض ترتيب اللاعبين
-   إحصائيات لاعب محدد
-   النتيجة الإجمالية بين لاعبين
-   إحصائيات جميع اللاعبين
-   عرض جميع المباريات
-   عرض مباريات بين لاعبين محددين
-   نتائج العرائض (Shutouts)
-   تصفية وترتيب النتائج

#### Endpoints تتطلب Admin Role:

-   إدارة اللاعبين (إضافة، حذف، تعديل)
-   إدارة المباريات (تسجيل، حذف، تعديل)

```
Authorization: Bearer <your-jwt-token>
```

---

### 1. إدارة اللاعبين (Admin Only)

#### 1.1 عرض جميع اللاعبين

```
GET /api/FriendlyMatch/players
```

**Authorization:** مطلوب (Admin فقط)

**Response:**

```json
[
    {
        "playerId": 1,
        "fullName": "محمد",
        "createdOn": "2024-01-15T10:30:00Z",
        "isActive": true,
        "totalMatches": 15,
        "totalWins": 8,
        "totalLosses": 5,
        "totalDraws": 2,
        "winRate": 53.33
    }
]
```

#### 1.2 عرض لاعب محدد

```
GET /api/FriendlyMatch/players/{playerId}
```

**Authorization:** مطلوب (Admin فقط)

#### 1.3 إضافة لاعب جديد

```
POST /api/FriendlyMatch/players
```

**Authorization:** مطلوب (Admin فقط)

**Request Body:**

```json
{
    "fullName": "أحمد"
}
```

#### 1.4 حذف لاعب

```
DELETE /api/FriendlyMatch/players/{playerId}
```

**Authorization:** مطلوب (Admin فقط)

#### 1.5 إلغاء تفعيل لاعب

```
PUT /api/FriendlyMatch/players/{playerId}/deactivate
```

**Authorization:** مطلوب (Admin فقط)

### 2. عرض البيانات (Public)

#### 2.1 ترتيب اللاعبين

```
GET /api/FriendlyMatch/players/ranking
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 2.2 إحصائيات لاعب محدد

```
GET /api/FriendlyMatch/players/{playerId}/stats
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 2.3 إحصائيات جميع اللاعبين

```
GET /api/FriendlyMatch/players/statistics
```

**Authorization:** غير مطلوب (متاح للجميع)

**Response:**

```json
[
    {
        "playerId": 1,
        "playerName": "محمد",
        "totalMatches": 15,
        "wins": 8,
        "draws": 2,
        "losses": 5,
        "goalsScored": 45,
        "goalsConceded": 32,
        "winRate": 53.33
    }
]
```

### 3. إدارة المباريات

#### 3.1 عرض جميع المباريات

```
GET /api/FriendlyMatch/matches
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 3.2 عرض مباريات بين لاعبين محددين

```
GET /api/FriendlyMatch/matches/{player1Id}/{player2Id}
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 3.3 تسجيل مباراة جديدة

```
POST /api/FriendlyMatch/matches/record
```

**Authorization:** مطلوب (Admin فقط)

**Request Body:**

```json
{
    "player1Id": 1,
    "player2Id": 2,
    "player1Score": 3,
    "player2Score": 1,
    "playedOn": "2024-01-15T10:30:00Z"
}
```

#### 3.4 حذف مباراة

```
DELETE /api/FriendlyMatch/matches/{matchId}
```

**Authorization:** مطلوب (Admin فقط)

#### 3.5 تعديل مباراة

```
PUT /api/FriendlyMatch/matches/{matchId}
```

**Authorization:** مطلوب (Admin فقط)

### 4. الإحصائيات والتحليل (Public)

#### 4.1 النتيجة الإجمالية بين لاعبين

```
GET /api/FriendlyMatch/overall-score/{player1Id}/{player2Id}
```

**Authorization:** غير مطلوب (متاح للجميع)

**Response:**

```json
{
    "player1Id": 1,
    "player2Id": 2,
    "player1Name": "محمد",
    "player2Name": "أحمد",
    "totalMatches": 5,
    "player1Wins": 3,
    "player2Wins": 1,
    "draws": 1,
    "player1TotalScore": 12,
    "player2TotalScore": 8
}
```

### 5. نتائج العرائض (Public)

#### 5.1 عرض جميع نتائج العرائض

```
GET /api/FriendlyMatch/shutouts
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 5.2 نتائج العرائض للاعب محدد

```
GET /api/FriendlyMatch/shutouts/player/{playerId}
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 5.3 نتيجة عريضة لمباراة محددة

```
GET /api/FriendlyMatch/shutouts/match/{matchId}
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 5.4 حذف نتيجة عريضة

```
DELETE /api/FriendlyMatch/shutouts/{shutoutId}
```

**Authorization:** مطلوب (Admin فقط)

### 6. تصفية وترتيب النتائج (Public)

#### 6.1 تصفية نتائج العرائض

```
GET /api/FriendlyMatch/shutouts/filtered?startDate=2024-01-01&endDate=2024-12-31&minScore=5&maxScore=10
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 6.2 تصفية نتائج العرائض للاعب محدد

```
GET /api/FriendlyMatch/shutouts/player/{playerId}/filtered?startDate=2024-01-01&endDate=2024-12-31
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 6.3 تصفية نتائج العرائض لعدة لاعبين

```
POST /api/FriendlyMatch/shutouts/players/filtered
```

**Authorization:** غير مطلوب (متاح للجميع)

**Request Body:**

```json
[1, 2, 3]
```

### 7. تصفية المباريات (Public)

#### 7.1 تصفية المباريات

```
GET /api/FriendlyMatch/matches/filtered?startDate=2024-01-01&endDate=2024-12-31&playerId=1
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 7.2 مباريات لاعب محدد

```
GET /api/FriendlyMatch/matches/player/{playerId}?startDate=2024-01-01&endDate=2024-12-31
```

**Authorization:** غير مطلوب (متاح للجميع)

#### 7.3 مباريات لاعب ضد منافسين محددين

```
POST /api/FriendlyMatch/matches/player/{playerId}/vs-opponents
```

**Authorization:** غير مطلوب (متاح للجميع)

**Request Body:**

```json
[2, 3, 4]
```

---

## 📊 DTOs

### AddFriendlyPlayerDto

```csharp
public class AddFriendlyPlayerDto
{
    public string FullName { get; set; } = string.Empty;
}
```

### RecordFriendlyMatchDto

```csharp
public class RecordFriendlyMatchDto
{
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public DateTime PlayedOn { get; set; }
}
```

### PlayerStatisticsDto

```csharp
public class PlayerStatisticsDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsScored { get; set; }
    public int GoalsConceded { get; set; }
    public double WinRate { get; set; }
}
```

### OverallScoreDto

```csharp
public class OverallScoreDto
{
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public string Player1Name { get; set; } = string.Empty;
    public string Player2Name { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int Player1Wins { get; set; }
    public int Player2Wins { get; set; }
    public int Draws { get; set; }
    public int Player1TotalScore { get; set; }
    public int Player2TotalScore { get; set; }
}
```

### ShutoutResultDto

```csharp
public class ShutoutResultDto
{
    public int ShutoutId { get; set; }
    public int MatchId { get; set; }
    public int WinnerId { get; set; }
    public string WinnerName { get; set; } = string.Empty;
    public int LoserId { get; set; }
    public string LoserName { get; set; } = string.Empty;
    public int WinnerScore { get; set; }
    public int LoserScore { get; set; }
    public DateTime PlayedOn { get; set; }
}
```

### MatchFilterDto

```csharp
public class MatchFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PlayerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

### ShutoutFilterDto

```csharp
public class ShutoutFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MinScore { get; set; }
    public int? MaxScore { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

---

## 🔧 الخدمات

### IFriendlyMatchService

```csharp
public interface IFriendlyMatchService
{
    // Player Management
    Task<IEnumerable<FriendlyPlayerDto>> GetAllFriendlyPlayersAsync();
    Task<FriendlyPlayerDto?> GetFriendlyPlayerByIdAsync(int playerId);
    Task<ApiResponse> AddFriendlyPlayerAsync(string fullName);
    Task<ApiResponse> DeleteFriendlyPlayerAsync(int playerId);
    Task<ApiResponse> DeactivateFriendlyPlayerAsync(int playerId);

    // Player Statistics
    Task<IEnumerable<FriendlyPlayerDto>> GetFriendlyPlayersRankingAsync();
    Task<FriendlyPlayerDto?> GetFriendlyPlayerStatsAsync(int playerId);
    Task<IEnumerable<PlayerStatisticsDto>> GetPlayersStatisticsAsync();

    // Match Management
    Task<IEnumerable<FriendlyMatchDto>> GetAllFriendlyMatchesAsync();
    Task<IEnumerable<FriendlyMatchDto>> GetFriendlyMatchesBetweenPlayersAsync(int player1Id, int player2Id);
    Task<ApiResponse> RecordFriendlyMatchAsync(RecordFriendlyMatchDto dto);
    Task<ApiResponse> DeleteFriendlyMatchAsync(int matchId);
    Task<ApiResponse> UpdateFriendlyMatchAsync(int matchId, RecordFriendlyMatchDto dto);

    // Overall Score
    Task<OverallScoreDto?> GetOverallScoreBetweenPlayersAsync(int player1Id, int player2Id);

    // Shutout Results
    Task<IEnumerable<ShutoutResultDto>> GetAllShutoutResultsAsync();
    Task<IEnumerable<ShutoutResultDto>> GetShutoutResultsByPlayerAsync(int playerId);
    Task<ShutoutResultDto?> GetShutoutResultByMatchAsync(int matchId);
    Task<ApiResponse> DeleteShutoutResultAsync(int shutoutId);

    // Advanced Filtering
    Task<PaginatedShutoutResultDto> GetFilteredShutoutsAsync(ShutoutFilterDto filter);
    Task<PaginatedShutoutResultDto> GetPlayerShutoutsAsync(int playerId, ShutoutFilterDto filter);
    Task<PaginatedShutoutResultDto> GetPlayersShutoutsAsync(List<int> playerIds, ShutoutFilterDto filter);
    Task<PaginatedMatchResultDto> GetFilteredMatchesAsync(MatchFilterDto filter);
    Task<PaginatedMatchResultDto> GetPlayerMatchesAsync(int playerId, MatchFilterDto filter);
    Task<PaginatedMatchResultDto> GetPlayerVsOpponentsMatchesAsync(int playerId, List<int> opponentIds, MatchFilterDto filter);
}
```

---

## 🎯 ملخص الصلاحيات

### Endpoints متاحة للجميع (Public):

-   `GET /api/FriendlyMatch/players/ranking` - ترتيب اللاعبين
-   `GET /api/FriendlyMatch/players/{playerId}/stats` - إحصائيات لاعب محدد
-   `GET /api/FriendlyMatch/players/statistics` - إحصائيات جميع اللاعبين
-   `GET /api/FriendlyMatch/overall-score/{player1Id}/{player2Id}` - النتيجة الإجمالية
-   `GET /api/FriendlyMatch/matches` - عرض جميع المباريات
-   `GET /api/FriendlyMatch/matches/{player1Id}/{player2Id}` - مباريات بين لاعبين
-   `GET /api/FriendlyMatch/matches/filtered` - تصفية المباريات
-   `GET /api/FriendlyMatch/matches/player/{playerId}` - مباريات لاعب محدد
-   `POST /api/FriendlyMatch/matches/player/{playerId}/vs-opponents` - مباريات ضد منافسين
-   `GET /api/FriendlyMatch/shutouts` - نتائج العرائض
-   `GET /api/FriendlyMatch/shutouts/player/{playerId}` - نتائج العرائض للاعب
-   `GET /api/FriendlyMatch/shutouts/match/{matchId}` - نتيجة عريضة لمباراة
-   `GET /api/FriendlyMatch/shutouts/filtered` - تصفية نتائج العرائض
-   `GET /api/FriendlyMatch/shutouts/player/{playerId}/filtered` - تصفية نتائج العرائض للاعب
-   `POST /api/FriendlyMatch/shutouts/players/filtered` - تصفية نتائج العرائض لعدة لاعبين

### Endpoints تتطلب Admin Role:

-   `GET /api/FriendlyMatch/players` - عرض جميع اللاعبين
-   `GET /api/FriendlyMatch/players/{playerId}` - عرض لاعب محدد
-   `POST /api/FriendlyMatch/players` - إضافة لاعب جديد
-   `DELETE /api/FriendlyMatch/players/{playerId}` - حذف لاعب
-   `PUT /api/FriendlyMatch/players/{playerId}/deactivate` - إلغاء تفعيل لاعب
-   `POST /api/FriendlyMatch/matches/record` - تسجيل مباراة جديدة
-   `DELETE /api/FriendlyMatch/matches/{matchId}` - حذف مباراة
-   `PUT /api/FriendlyMatch/matches/{matchId}` - تعديل مباراة
-   `DELETE /api/FriendlyMatch/shutouts/{shutoutId}` - حذف نتيجة عريضة

---

## 🚀 المميزات المتقدمة

-   ✅ **Soft Delete** - حذف آمن للبيانات
-   ✅ **Pagination** - ترقيم الصفحات للنتائج الكبيرة
-   ✅ **Advanced Filtering** - تصفية متقدمة حسب التاريخ واللاعبين
-   ✅ **Role-based Authorization** - صلاحيات مختلفة للمدير واللاعبين
-   ✅ **Comprehensive Statistics** - إحصائيات شاملة ومفصلة
-   ✅ **Flexible Query System** - نظام استعلامات مرن وقوي
