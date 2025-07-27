# YuGi Tournament API Documentation

## نظرة عامة

هذا التوثيق يغطي جميع endpoints في YuGi Tournament API مع أمثلة للـ requests والـ responses.

---

## Authentication

جميع الـ endpoints تتطلب JWT Bearer Token في الـ Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

---

## 1. Authentication Endpoints

### 1.1 تسجيل لاعب جديد

**Endpoint:** `POST /api/Auth/register`  
**Description:** تسجيل لاعب جديد في النظام  
**Authorization:** مطلوب (Admin فقط)

**Request Body:**

```json
{
    "fullName": "اسم اللاعب"
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم إضافة اللاعب بنجاح",
    "data": {
        "playerId": 1,
        "fullName": "اسم اللاعب"
    }
}
```

### 1.2 تسجيل دخول لاعب

**Endpoint:** `POST /api/Auth/login`  
**Description:** تسجيل دخول لاعب موجود  
**Authorization:** غير مطلوب

**Request Body:**

```json
{
    "fullName": "اسم اللاعب"
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم تسجيل الدخول بنجاح",
    "data": {
        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        "player": {
            "playerId": 1,
            "fullName": "اسم اللاعب"
        }
    }
}
```

---

## 2. Player Endpoints

### 2.1 إضافة لاعب جديد

**Endpoint:** `POST /api/Player`  
**Description:** إضافة لاعب جديد (Admin فقط)  
**Authorization:** مطلوب (Admin)

**Request Body:**

```json
{
    "fullName": "اسم اللاعب الجديد"
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم إضافة اللاعب بنجاح"
}
```

### 2.2 جلب جميع اللاعبين

**Endpoint:** `GET /api/Player`  
**Description:** جلب جميع اللاعبين في البطولة الحالية  
**Authorization:** غير مطلوب

**Response:**

```json
[
    {
        "playerId": 1,
        "fullName": "أحمد محمد",
        "wins": 5,
        "losses": 2,
        "draws": 1,
        "points": 5.5,
        "matchesPlayed": 8,
        "rank": 1,
        "winRate": 62.5
    }
]
```

### 2.3 جلب ترتيب اللاعبين

**Endpoint:** `GET /api/Player/ranking`  
**Description:** جلب ترتيب اللاعبين في البطولة الحالية  
**Authorization:** غير مطلوب

**Response:**

```json
[
    {
        "playerId": 1,
        "fullName": "أحمد محمد",
        "wins": 5,
        "losses": 2,
        "draws": 1,
        "points": 5.5,
        "matchesPlayed": 8,
        "rank": 1,
        "winRate": 62.5
    }
]
```

### 2.4 جلب جميع البطولات مع الترتيب والمباريات

**Endpoint:** `GET /api/Player/players/all`  
**Description:** جلب جميع البطولات مع بيانات اللاعبين والترتيب والمباريات  
**Authorization:** غير مطلوب

**ملاحظة:** هذا الـ endpoint يرجع جميع أنواع البطولات:

-   **الدوريات العادية**: تحتوي على `players` و `matches`
-   **دوريات المجموعات**: تحتوي على `groups` و `knockoutMatches`

**Response:**

```json
[
    {
        "leagueId": 1,
        "leagueName": "بطولة الشتاء 2024",
        "leagueDescription": "بطولة شتوية للاعبين المحترفين",
        "leagueType": 0,
        "systemOfLeague": 0,
        "isFinished": false,
        "createdOn": "2024-01-15T10:30:00Z",
        "players": [
            {
                "playerId": 1,
                "fullName": "أحمد محمد",
                "wins": 5,
                "losses": 2,
                "draws": 1,
                "points": 5.5,
                "matchesPlayed": 8,
                "rank": 1,
                "winRate": 62.5
            }
        ],
        "groups": null,
        "matches": [
            {
                "matchId": 1,
                "score1": 3,
                "score2": 0,
                "isCompleted": true,
                "player1Name": "أحمد محمد",
                "player2Name": "محمد علي",
                "player1Id": 1,
                "player2Id": 2,
                "tournamentStage": 0,
                "winnerId": 1
            }
        ],
        "knockoutMatches": null
    },
    {
        "leagueId": 2,
        "leagueName": "بطولة المجموعات 2024",
        "leagueDescription": "بطولة مجموعات",
        "leagueType": 2,
        "systemOfLeague": 1,
        "isFinished": false,
        "createdOn": "2024-01-20T14:00:00Z",
        "players": null,
        "groups": [
            {
                "groupNumber": 1,
                "players": [
                    {
                        "playerId": 3,
                        "fullName": "محمد علي",
                        "wins": 3,
                        "losses": 1,
                        "draws": 0,
                        "points": 3.0,
                        "matchesPlayed": 4,
                        "rank": 1,
                        "winRate": 75.0
                    }
                ]
            }
        ]
    },
        "matches": null,
        "knockoutMatches": [
          {
            "matchId": 7,
            "score1": 3,
            "score2": 0,
            "isCompleted": true,
            "player1Name": "أحمد محمد",
            "player2Name": "خالد حسن",
            "player1Id": 1,
            "player2Id": 4,
            "tournamentStage": 2,
            "winnerId": 4
          },
          {
            "matchId": 10,
            "score1": 0,
            "score2": 3,
            "isCompleted": true,
            "player1Name": "أحمد محمد",
            "player2Name": "خالد حسن",
            "player1Id": 1,
            "player2Id": 4,
            "tournamentStage": 4,
            "winnerId": 4
          }
        ]
]
```

### مثال لبطولة المجموعات:

```json
{
    "leagueId": 2,
    "leagueName": "بطولة المجموعات 2024",
    "leagueDescription": "بطولة مجموعات للاعبين المحترفين",
    "leagueType": 2,
    "systemOfLeague": 1,
    "isFinished": false,
    "createdOn": "2024-01-20T14:00:00Z",
    "players": null,
    "groups": [
        {
            "groupNumber": 1,
            "players": [
                {
                    "playerId": 1,
                    "fullName": "أحمد محمد",
                    "wins": 3,
                    "losses": 1,
                    "draws": 0,
                    "points": 3.0,
                    "matchesPlayed": 4,
                    "rank": 1,
                    "winRate": 75.0
                }
            ],
            "matches": [
                {
                    "matchId": 1,
                    "score1": 3,
                    "score2": 0,
                    "isCompleted": true,
                    "player1Name": "أحمد محمد",
                    "player2Name": "محمد علي",
                    "player1Id": 1,
                    "player2Id": 2,
                    "tournamentStage": 1,
                    "winnerId": 1
                }
            ]
        }
    ],
    "knockoutMatches": [
        {
            "matchId": 7,
            "score1": 3,
            "score2": 0,
            "isCompleted": true,
            "player1Name": "أحمد محمد",
            "player2Name": "خالد حسن",
            "player1Id": 1,
            "player2Id": 4,
            "tournamentStage": 2,
            "winnerId": 4
        }
    ]
}
```

### 2.6 حذف لاعب

**Endpoint:** `DELETE /api/Player/{playerId}`  
**Description:** حذف لاعب من النظام (Admin فقط)  
**Authorization:** مطلوب (Admin)

**Response:**

```json
{
    "success": true,
    "message": "تم حذف اللاعب بنجاح"
}
```

### 2.7 جلب مجموعات اللاعبين

**Endpoint:** `GET /api/Player/league/{leagueId}/groups`  
**Description:** جلب مجموعات اللاعبين في بطولة معينة  
**Authorization:** غير مطلوب

**Response:**

```json
[
    {
        "groupNumber": 1,
        "players": [
            {
                "playerId": 3,
                "fullName": "محمد علي",
                "wins": 3,
                "losses": 1,
                "draws": 0,
                "points": 3.0,
                "matchesPlayed": 4,
                "rank": 1,
                "winRate": 75.0
            }
        ]
    }
]
```

---

## 3. League Endpoints

### 3.1 بدء بطولة جديدة

**Endpoint:** `POST /api/League/start`  
**Description:** بدء بطولة جديدة (Admin فقط)  
**Authorization:** مطلوب (Admin)

**Request Body:**

```json
{
    "name": "بطولة الربيع 2024",
    "description": "بطولة ربيعية جديدة",
    "typeOfLeague": 0,
    "systemOfLeague": 0
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم بدء البطولة بنجاح"
}
```

### 3.2 إنهاء بطولة

**Endpoint:** `POST /api/League/{leagueId}/finish`  
**Description:** إنهاء بطولة معينة (Admin فقط)  
**Authorization:** مطلوب (Admin)

**Response:**

```json
{
    "success": true,
    "message": "تم إنهاء البطولة بنجاح"
}
```

---

## 4. Match Endpoints

### 4.1 جلب جميع المباريات

**Endpoint:** `GET /api/Match`  
**Description:** جلب جميع المباريات في البطولة الحالية  
**Authorization:** غير مطلوب

**Response:**

```json
[
    {
        "matchId": 1,
        "score1": 3,
        "score2": 0,
        "isCompleted": true,
        "player1Name": "أحمد محمد",
        "player2Name": "محمد علي",
        "player1Id": 1,
        "player2Id": 2,
        "tournamentStage": "Group Stage"
    }
]
```

### 4.2 تسجيل نتيجة مباراة

**Endpoint:** `POST /api/Match/result`  
**Description:** تسجيل نتيجة مباراة (Admin فقط)  
**Authorization:** مطلوب (Admin)

**Request Body:**

```json
{
    "matchId": 1,
    "score1": 3,
    "score2": 0
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم تسجيل النتيجة بنجاح"
}
```

---

## 5. Message Endpoints

### 5.1 إرسال رسالة

**Endpoint:** `POST /api/Message/send`  
**Description:** إرسال رسالة إلى لاعب آخر  
**Authorization:** مطلوب

**Request Body:**

```json
{
    "receiverId": 2,
    "content": "مرحبا! هل تريد لعب مباراة؟"
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم إرسال الرسالة بنجاح"
}
```

### 5.2 جلب الرسائل

**Endpoint:** `GET /api/Message`  
**Description:** جلب جميع الرسائل للمستخدم الحالي  
**Authorization:** مطلوب

**Response:**

```json
[
    {
        "messageId": 1,
        "content": "مرحبا! هل تريد لعب مباراة؟",
        "senderName": "أحمد محمد",
        "receiverName": "محمد علي",
        "isRead": false,
        "sentAt": "2024-01-15T10:30:00Z"
    }
]
```

### 5.3 تحديد رسالة كمقروءة

**Endpoint:** `POST /api/Message/mark-read`  
**Description:** تحديد رسالة كمقروءة  
**Authorization:** مطلوب

**Request Body:**

```json
{
    "messageId": 1
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم تحديد الرسالة كمقروءة"
}
```

---

## 6. Note Endpoints

### 6.1 إضافة ملاحظة

**Endpoint:** `POST /api/Note`  
**Description:** إضافة ملاحظة جديدة (Admin فقط)  
**Authorization:** مطلوب (Admin)

**Request Body:**

```json
{
    "content": "ملاحظة مهمة للاعبين"
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم إضافة الملاحظة بنجاح"
}
```

### 6.2 جلب الملاحظات

**Endpoint:** `GET /api/Note`  
**Description:** جلب جميع الملاحظات النشطة  
**Authorization:** غير مطلوب

**Response:**

```json
[
    {
        "noteId": 1,
        "content": "ملاحظة مهمة للاعبين",
        "isHidden": false,
        "createdAt": "2024-01-15T10:30:00Z"
    }
]
```

### 6.3 إخفاء ملاحظة

**Endpoint:** `POST /api/Note/hide`  
**Description:** إخفاء ملاحظة (Admin فقط)  
**Authorization:** مطلوب (Admin)

**Request Body:**

```json
{
    "noteId": 1
}
```

**Response:**

```json
{
    "success": true,
    "message": "تم إخفاء الملاحظة بنجاح"
}
```

---

## Data Types

### LeagueType Enum

-   `0`: Single (بطولة فردية)
-   `1`: Multi (بطولة متعددة)
-   `2`: Groups (بطولة مجموعات)

### SystemOfLeague Enum

-   `0`: Points (نظام النقاط)
-   `1`: Classic (النظام الكلاسيكي)

### TournamentStage Enum

-   `0`: League (دوري عادي)
-   `1`: GroupStage (مرحلة المجموعات)
-   `2`: QuarterFinals (ربع النهائي)
-   `3`: SemiFinals (نصف النهائي)
-   `4`: Final (النهائي)

### PlayerRankDto

```json
{
    "playerId": 1,
    "fullName": "اسم اللاعب",
    "wins": 5,
    "losses": 2,
    "draws": 1,
    "points": 5.5,
    "matchesPlayed": 8,
    "rank": 1,
    "winRate": 62.5
}
```

### GroupDto

```json
{
    "groupNumber": 1,
    "players": [
        {
            "playerId": 1,
            "fullName": "اسم اللاعب",
            "wins": 3,
            "losses": 1,
            "draws": 0,
            "points": 3.0,
            "matchesPlayed": 4,
            "rank": 1,
            "winRate": 75.0
        }
    ],
    "matches": [
        {
            "matchId": 1,
            "score1": 3,
            "score2": 0,
            "isCompleted": true,
            "player1Name": "أحمد محمد",
            "player2Name": "محمد علي",
            "player1Id": 1,
            "player2Id": 2,
            "tournamentStage": 1,
            "winnerId": 1
        }
    ]
}
```

### MatchDto

```json
{
    "matchId": 1,
    "score1": 3,
    "score2": 0,
    "isCompleted": true,
    "player1Name": "أحمد محمد",
    "player2Name": "محمد علي",
    "player1Id": 1,
    "player2Id": 2,
    "tournamentStage": 4,
    "winnerId": 1
}
```

### LeagueResponseDto

```json
{
  "leagueId": 1,
  "leagueName": "اسم البطولة",
  "leagueDescription": "وصف البطولة",
  "leagueType": 0,
  "systemOfLeague": 0,
  "isFinished": false,
  "createdOn": "2024-01-15T10:30:00Z",
  "players": [...],
  "groups": [...],
  "matches": [...],
  "knockoutMatches": [...]
}
```

**ملاحظات مهمة:**

-   `players`: موجودة للدوريات العادية (غير المجموعات)
-   `groups`: موجودة لدوريات المجموعات فقط
-   `matches`: موجودة للدوريات العادية (systemOfLeague = 0 أو 1)
-   `knockoutMatches`: موجودة لدوريات المجموعات فقط (systemOfLeague = 2)

---

## Error Responses

جميع الـ endpoints ترجع نفس شكل الـ error response:

```json
{
    "success": false,
    "message": "رسالة الخطأ باللغة العربية"
}
```

### Common Error Codes

-   `400`: Bad Request - بيانات غير صحيحة
-   `401`: Unauthorized - غير مصرح
-   `403`: Forbidden - محظور (Admin فقط)
-   `404`: Not Found - غير موجود
-   `500`: Internal Server Error - خطأ في الخادم

---

## Notes for Frontend Development

1. **Authentication**: استخدم JWT token في جميع الـ requests المطلوبة
2. **Error Handling**: تعامل مع جميع الـ error responses
3. **Loading States**: أضف loading states للـ async operations
4. **Real-time Updates**: يمكن استخدام polling للتحديثات المباشرة
5. **Responsive Design**: تأكد من أن التطبيق يعمل على جميع الأجهزة

---

## Base URL

```
http://localhost:5000/api
```

---

_آخر تحديث: يناير 2024_
