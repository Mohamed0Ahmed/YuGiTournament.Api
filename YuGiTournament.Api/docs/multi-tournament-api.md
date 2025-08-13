# Multi Tournament API Documentation

## 🎯 نظرة عامة

هذا هو دليل شامل لـ API نظام البطولات المتعددة (Multi Tournament) المحدث بالنظام المبسط الجديد.

## 🔗 Base URL

```
/api/multi
```

---

## 📚 جدول المحتويات

1. [إدارة البطولات](#tournament-management)
2. [إدارة الفرق](#team-management)
3. [إدارة المباريات والنتائج](#match-results)
4. [إدارة اللاعبين](#player-management)
5. [Helper Endpoints](#helper-endpoints)
6. [عرض البيانات العامة](#general-data)
7. [Models و DTOs](#models-dtos)

---

## 🏆 Tournament Management

### 1. إنشاء بطولة جديدة

```http
POST /api/multi/tournaments
```

**Authorization:** `Admin` only

**Request Body:**

```json
{
    "name": "بطولة الربيع 2024",
    "systemOfScoring": "Classic", // أو "Points"
    "teamCount": 4,
    "playersPerTeam": 3
}
```

**Response:**

```json
{
    "success": true,
    "message": "Tournament created successfully"
}
```

**Validation:**

-   `teamCount` >= 2
-   `playersPerTeam` >= 1
-   لا يمكن إنشاء بطولة إذا كان هناك بطولة نشطة أخرى

---

### 2. تحديث حالة البطولة

```http
PUT /api/multi/tournaments/{id}/status
```

**Authorization:** `Admin` only

**Request Body:**

```json
{
    "status": "Started" // أو "Finished"
}
```

**Response:**

```json
{
    "success": true,
    "message": "Tournament status updated successfully"
}
```

**Status Flow:**

-   `Created` → `Started` → `Finished`
-   عند البدء: يتم إنشاء المباريات تلقائياً
-   عند الانتهاء: يتم تحديث إحصائيات اللاعبين

---

### 3. حذف البطولة

```http
DELETE /api/multi/tournaments/{id}
```

**Authorization:** `Admin` only

**Response:**

```json
{
    "success": true,
    "message": "Tournament deleted successfully"
}
```

**Note:** يحذف البطولة وكل البيانات المرتبطة بها (الفرق، المباريات)

---

## 👥 Team Management

### 4. إنشاء فريق جديد

```http
POST /api/multi/tournaments/{tournamentId}/teams
```

**Authorization:** `Admin` only

**Request Body:**

```json
{
    "teamName": "فريق النجوم",
    "playerIds": [1, 2, 3]
}
```

**Response:**

```json
{
    "success": true,
    "message": "Team created successfully"
}
```

**Validation:**

-   عدد اللاعبين = `playersPerTeam` المحدد في البطولة
-   أسماء الفرق فريدة داخل البطولة
-   اللاعبين غير مكررين في نفس البطولة
-   البطولة في حالة `Created` فقط

---

### 5. تحديث الفريق

```http
PUT /api/multi/teams/{teamId}
```

**Authorization:** `Admin` only

**Request Body:**

```json
{
    "teamName": "الفريق الذهبي", // اختياري
    "playerIds": [1, 2, 4] // اختياري
}
```

**Response:**

```json
{
    "success": true,
    "message": "Team updated successfully"
}
```

**Rules:**

-   يمكن التحديث فقط في حالة `Created`
-   للاستبدال أثناء البطولة، استخدم endpoint منفصل

---

## ⚔️ Match & Results

### 6. تسجيل نتيجة مباراة

```http
POST /api/multi/matches/{matchId}/result
```

**Authorization:** `Admin` only

**Request Body للنظام الكلاسيكي:**

```json
{
    "winnerId": 123
}
```

**Request Body لنظام النقاط:**

```json
{
    "score1": 5,
    "score2": 3
}
```

**Response:**

```json
{
    "success": true,
    "message": "Match result recorded successfully"
}
```

**Logic:**

-   **Classic:** `winnerId` يحدد الفائز → 3-0 تلقائياً
-   **Points:** أكبر score يصبح فائز تلقائياً
-   **التعادل:** `winnerId = null` وكل فريق يأخذ نقطة

---

### 7. إلغاء نتيجة مباراة (Undo)

```http
POST /api/multi/matches/{matchId}/undo
```

**Authorization:** `Admin` only

**Response:**

```json
{
    "success": true,
    "message": "Match result undone successfully"
}
```

**What happens:**

-   `score1 = 0, score2 = 0`
-   `winnerId = null`
-   `isCompleted = false`
-   `completedOn = null`
-   إرجاع إحصائيات الفرق للحالة السابقة

---

### 8. جلب مباريات البطولة

```http
GET /api/multi/tournaments/{id}/matches
```

**Response:**

```json
{
    "success": true,
    "message": "Tournament matches retrieved successfully",
    "data": [
        {
            "team1Id": 1,
            "team1Name": "فريق النجوم",
            "team2Id": 2,
            "team2Name": "فريق الأبطال",
            "matches": [
                {
                    "multiMatchId": 101,
                    "player1Id": 1,
                    "player1Name": "أحمد محمد",
                    "player2Id": 4,
                    "player2Name": "سارة أحمد",
                    "score1": 3,
                    "score2": 0,
                    "winnerId": 1,
                    "winnerName": "أحمد محمد",
                    "isCompleted": true,
                    "completedOn": "2024-01-15T10:30:00Z"
                }
            ]
        }
    ]
}
```

---

### 9. جلب مباريات لاعب معين

```http
GET /api/multi/players/{playerId}/matches
```

**Response:**

```json
{
    "success": true,
    "message": "Player matches retrieved successfully",
    "data": {
        "playerId": 123,
        "playerName": "أحمد محمد",
        "tournamentId": 1,
        "tournamentName": "بطولة الربيع 2024",
        "totalMatches": 9,
        "completedMatches": 5,
        "pendingMatches": 4,
        "matches": [
            {
                "multiMatchId": 101,
                "player1Id": 123,
                "player1Name": "أحمد محمد",
                "player2Id": 456,
                "player2Name": "سارة أحمد",
                "team1Id": 1,
                "team1Name": "فريق النجوم",
                "team2Id": 2,
                "team2Name": "فريق الأبطال",
                "score1": 3,
                "score2": 0,
                "winnerId": 123,
                "winnerName": "أحمد محمد",
                "isCompleted": true,
                "completedOn": "2024-01-15T10:30:00Z"
            }
        ]
    }
}
```

**Use Case:** للأدمن عشان يشوف كل مباريات لاعب معين (مكتملة وناقصة) لتسجيل النتائج أو عمل Undo

---

## 👤 Player Management

### 10. جلب كل اللاعبين

```http
GET /api/multi/players
```

**Response:**

```json
{
    "success": true,
    "message": "Players retrieved successfully",
    "data": [
        {
            "playerId": 1,
            "fullName": "أحمد محمد",
            "isActive": true,
            "createdOn": "2024-01-01T00:00:00Z",
            "multiParticipations": 5,
            "multiTitlesWon": 2
        }
    ]
}
```

---

### 11. جلب لاعب محدد

```http
GET /api/multi/players/{playerId}
```

**Response:**

```json
{
    "success": true,
    "message": "Player found",
    "data": {
        "playerId": 1,
        "fullName": "أحمد محمد",
        "isActive": true,
        "createdOn": "2024-01-01T00:00:00Z",
        "multiParticipations": 5,
        "multiTitlesWon": 2
    }
}
```

---

### 12. إضافة لاعب جديد

```http
POST /api/multi/players
```

**Authorization:** `Admin` only

**Request Body:**

```json
{
    "fullName": "محمد أحمد"
}
```

**Response:**

```json
{
    "success": true,
    "message": "Player added successfully",
    "data": {
        "playerId": 10,
        "fullName": "محمد أحمد",
        "isActive": true,
        "createdOn": "2024-01-15T12:00:00Z",
        "multiParticipations": 0,
        "multiTitlesWon": 0
    }
}
```

---

## 🔧 Helper Endpoints

### 13. بدء البطولة

```http
POST /api/multi/tournaments/{id}/start
```

**Authorization:** `Admin` only

**Response:**

```json
{
    "success": true,
    "message": "Tournament status updated successfully"
}
```

**Note:** مجرد shortcut لـ `PUT /tournaments/{id}/status` مع `status: "Started"`

---

### 14. إنهاء البطولة

```http
POST /api/multi/tournaments/{id}/finish
```

**Authorization:** `Admin` only

**Response:**

```json
{
    "success": true,
    "message": "Tournament status updated successfully"
}
```

---

### 15. استبدال لاعب في الفريق

```http
POST /api/multi/teams/{teamId}/replace-player
```

**Authorization:** `Admin` only

**Request Body:**

```json
{
    "replacedPlayerId": 123,
    "newPlayerId": 456
}
```

**Response:**

```json
{
    "success": true,
    "message": "Player replaced successfully"
}
```

**Rules:**

-   يمكن الاستبدال فقط أثناء البطولة (`Started`)
-   يتم تحديث المباريات غير المكتملة تلقائياً

---

## 📊 General Data

### 16. جلب البطولة النشطة

```http
GET /api/multi/tournaments/active
```

**Response:**

```json
{
    "success": true,
    "message": "Active tournament found",
    "data": {
        "multiTournamentId": 1,
        "name": "بطولة الربيع 2024",
        "systemOfScoring": "Classic",
        "teamCount": 4,
        "playersPerTeam": 3,
        "status": "Started",
        "isActive": true,
        "createdOn": "2024-01-01T00:00:00Z",
        "startedOn": "2024-01-02T10:00:00Z",
        "finishedOn": null,
        "championTeamId": null,
        "championTeamName": null,
        "teams": [
            {
                "multiTeamId": 1,
                "teamName": "فريق النجوم",
                "totalPoints": 15,
                "wins": 5,
                "draws": 0,
                "losses": 1,
                "createdOn": "2024-01-01T00:00:00Z",
                "players": [
                    {
                        "playerId": 1,
                        "fullName": "أحمد محمد",
                        "isActive": true
                    }
                ]
            }
        ]
    }
}
```

---

### 17. جلب مباريات البطولة النشطة

```http
GET /api/multi/tournaments/active/matches
```

**Response:** نفس response الـ `GET /tournaments/{id}/matches`

---

### 18. جلب كل البطولات

```http
GET /api/multi/tournaments
```

**Response:**

```json
{
  "success": true,
  "message": "Tournaments retrieved successfully",
  "data": [
    {
      "multiTournamentId": 1,
      "name": "بطولة الربيع 2024",
      "systemOfScoring": "Classic",
      "teamCount": 4,
      "playersPerTeam": 3,
      "status": "Finished",
      "isActive": false,
      "createdOn": "2024-01-01T00:00:00Z",
      "startedOn": "2024-01-02T10:00:00Z",
      "finishedOn": "2024-01-10T18:00:00Z",
      "championTeamId": 1,
      "championTeamName": "فريق النجوم",
      "teams": [...]
    }
  ]
}
```

---

### 19. جلب بطولة محددة

```http
GET /api/multi/tournaments/{id}
```

**Response:** نفس structure البطولة النشطة

---

### 20. جلب ترتيب البطولة

```http
GET /api/multi/tournaments/{id}/standings
```

**Response:**

```json
{
    "success": true,
    "message": "Tournament standings retrieved successfully",
    "data": {
        "tournamentId": 1,
        "tournamentName": "بطولة الربيع 2024",
        "status": "Started",
        "championTeamId": null,
        "standings": [
            {
                "position": 1,
                "multiTeamId": 1,
                "teamName": "فريق النجوم",
                "totalPoints": 15,
                "wins": 5,
                "draws": 0,
                "losses": 1,
                "matchesPlayed": 6
            }
        ]
    }
}
```

---

## 📋 Models & DTOs

### Request DTOs

```typescript
// إنشاء بطولة
interface CreateTournamentDto {
    name: string;
    systemOfScoring: "Classic" | "Points";
    teamCount: number;
    playersPerTeam: number;
}

// تحديث حالة البطولة
interface UpdateTournamentStatusDto {
    status: "Started" | "Finished";
}

// إنشاء فريق
interface TeamCreateDto {
    teamName: string;
    playerIds: number[];
}

// تحديث فريق
interface TeamUpdateDto {
    teamName?: string;
    playerIds?: number[];
}

// تسجيل نتيجة مباراة
interface MultiMatchResultDto {
    winnerId?: number; // للكلاسيك
    score1?: number; // للنقاط
    score2?: number; // للنقاط
}

// استبدال لاعب
interface PlayerReplaceDto {
    replacedPlayerId: number;
    newPlayerId: number;
}

// إضافة لاعب
interface AddPlayerDto {
    fullName: string;
}
```

### Response Types

```typescript
// الاستجابة العامة
interface ApiResponse<T = any> {
    success: boolean;
    message: string;
    data?: T;
}

// تفاصيل المباراة
interface MatchDetail {
    multiMatchId: number;
    player1Id: number;
    player1Name: string;
    player2Id: number;
    player2Name: string;
    team1Id: number;
    team1Name: string;
    team2Id: number;
    team2Name: string;
    score1: number | null;
    score2: number | null;
    winnerId: number | null;
    winnerName: string | null;
    isCompleted: boolean;
    completedOn: string | null;
}

// تفاصيل اللاعب
interface PlayerDetail {
    playerId: number;
    fullName: string;
    isActive: boolean;
    createdOn: string;
    multiParticipations: number;
    multiTitlesWon: number;
}

// تفاصيل الفريق
interface TeamDetail {
    multiTeamId: number;
    teamName: string;
    totalPoints: number;
    wins: number;
    draws: number;
    losses: number;
    createdOn: string;
    players: PlayerDetail[];
}
```

---

## 🔄 أهم التحديثات في النظام الجديد

### 1. **تبسيط تسجيل النتائج:**

-   **Classic:** فقط `winnerId` → 3-0 تلقائياً
-   **Points:** فقط `score1, score2` → تحديد الفائز تلقائياً

### 2. **نظام Undo:**

-   إرجاع المباراة لحالة البداية
-   إرجاع إحصائيات الفرق

### 3. **مباريات اللاعب:**

-   endpoint جديد لجلب كل مباريات لاعب معين
-   مفيد للأدمن لتسجيل النتائج

### 4. **تحسين البيانات المُرجعة:**

-   إضافة `winnerId` و `winnerName` لكل المباريات
-   تفاصيل أكثر في responses

---

## 🚀 استخدام Frontend

### مثال لتسجيل نتيجة كلاسيكية:

```javascript
// Classic System
const recordClassicResult = async (matchId, winnerId) => {
    const response = await fetch(`/api/multi/matches/${matchId}/result`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ winnerId }),
    });

    return await response.json();
};

// Points System
const recordPointsResult = async (matchId, score1, score2) => {
    const response = await fetch(`/api/multi/matches/${matchId}/result`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ score1, score2 }),
    });

    return await response.json();
};
```

### مثال لجلب مباريات لاعب:

```javascript
const getPlayerMatches = async (playerId) => {
    const response = await fetch(`/api/multi/players/${playerId}/matches`);
    const result = await response.json();

    if (result.success) {
        console.log(
            `${result.data.playerName} has ${result.data.totalMatches} matches`
        );
        console.log(
            `Completed: ${result.data.completedMatches}, Pending: ${result.data.pendingMatches}`
        );
    }

    return result;
};
```

---

هذا الدليل شامل لكل الـ endpoints مع أمثلة على الاستخدام. النظام الجديد أبسط وأوضح من السابق! 🎯
