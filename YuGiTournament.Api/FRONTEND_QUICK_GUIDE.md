# 🚀 Frontend Quick Guide - Multi Tournament API v2

## 🎯 أهم التغييرات للفرونت اند

### ✅ ما تغير:

1. **تبسيط تسجيل النتائج** - مش محتاج `TotalPoints` تاني
2. **إضافة Undo** - يقدر يلغي أي نتيجة
3. **مباريات اللاعب** - endpoint جديد للاعب المعين
4. **WinnerId جديد** - في كل المباريات دلوقتي

### ❌ ما اتشال:

-   `TotalPoints1` و `TotalPoints2` من كل الـ responses
-   `IsValidClassicScore` validation (بقت تلقائية)

---

## 🔄 تسجيل النتائج - النظام الجديد

### للكلاسيك (Classic):

```typescript
// الطريقة الجديدة - أبسط!
const recordClassicResult = async (matchId: number, winnerId: number) => {
    const response = await fetch(`/api/multi/matches/${matchId}/result`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ winnerId }),
    });
    return await response.json();
};

// هيسجل تلقائياً: الفائز 3 والخاسر 0
```

### للنقاط (Points):

```typescript
// الطريقة الجديدة
const recordPointsResult = async (
    matchId: number,
    score1: number,
    score2: number
) => {
    const response = await fetch(`/api/multi/matches/${matchId}/result`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ score1, score2 }),
    });
    return await response.json();
};

// هيحدد تلقائياً مين الفائز (أو null للتعادل)
```

---

## 🆕 Undo System - جديد!

```typescript
const undoMatchResult = async (matchId: number) => {
    const response = await fetch(`/api/multi/matches/${matchId}/undo`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
    });
    return await response.json();
};

// هيرجع المباراة للحالة الأولية:
// score1 = 0, score2 = 0, winnerId = null, isCompleted = false
```

---

## 👤 مباريات اللاعب - جديد!

```typescript
const getPlayerMatches = async (playerId: number) => {
    const response = await fetch(`/api/multi/players/${playerId}/matches`);
    const result = await response.json();

    if (result.success) {
        console.log(`${result.data.playerName}:`);
        console.log(`- Total: ${result.data.totalMatches}`);
        console.log(`- Completed: ${result.data.completedMatches}`);
        console.log(`- Pending: ${result.data.pendingMatches}`);

        // كل المباريات متاحة في result.data.matches
        return result.data.matches;
    }
};
```

---

## 🔄 تحديث الـ Response Types

### Match Object الجديد:

```typescript
interface MatchDetail {
    multiMatchId: number;
    player1Id: number;
    player1Name: string;
    player2Id: number;
    player2Name: string;
    score1: number | null;
    score2: number | null;
    winnerId: number | null; // 🆕 جديد!
    winnerName: string | null; // 🆕 جديد!
    isCompleted: boolean;
    completedOn: string | null;
    // ❌ مافيش TotalPoints1/2 تاني
}
```

### Request DTO الجديد:

```typescript
interface MultiMatchResultDto {
    winnerId?: number; // للكلاسيك
    score1?: number; // للنقاط
    score2?: number; // للنقاط
    // ❌ مافيش TotalPoints1/2 تاني
}
```

---

## 📱 أمثلة UI Components

### Match Result Form:

```typescript
const MatchResultForm = ({ match, tournamentSystem }) => {
    const [winnerId, setWinnerId] = useState<number>();
    const [score1, setScore1] = useState<number>();
    const [score2, setScore2] = useState<number>();

    const handleSubmit = async () => {
        if (tournamentSystem === "Classic") {
            await recordClassicResult(match.multiMatchId, winnerId!);
        } else {
            await recordPointsResult(match.multiMatchId, score1!, score2!);
        }
    };

    return (
        <div>
            {tournamentSystem === "Classic" ? (
                <div>
                    <label>الفائز:</label>
                    <select
                        onChange={(e) => setWinnerId(Number(e.target.value))}
                    >
                        <option value={match.player1Id}>
                            {match.player1Name}
                        </option>
                        <option value={match.player2Id}>
                            {match.player2Name}
                        </option>
                    </select>
                </div>
            ) : (
                <div>
                    <input
                        type="number"
                        placeholder={`${match.player1Name} النقاط`}
                        onChange={(e) => setScore1(Number(e.target.value))}
                    />
                    <input
                        type="number"
                        placeholder={`${match.player2Name} النقاط`}
                        onChange={(e) => setScore2(Number(e.target.value))}
                    />
                </div>
            )}

            <button onClick={handleSubmit}>تسجيل النتيجة</button>

            {match.isCompleted && (
                <button onClick={() => undoMatchResult(match.multiMatchId)}>
                    إلغاء النتيجة
                </button>
            )}
        </div>
    );
};
```

### Player Matches View:

```typescript
const PlayerMatchesView = ({ playerId }) => {
    const [playerMatches, setPlayerMatches] = useState(null);

    useEffect(() => {
        getPlayerMatches(playerId).then(setPlayerMatches);
    }, [playerId]);

    if (!playerMatches) return <div>Loading...</div>;

    return (
        <div>
            <h2>{playerMatches.playerName}</h2>
            <p>المباريات: {playerMatches.totalMatches}</p>
            <p>مكتملة: {playerMatches.completedMatches}</p>
            <p>ناقصة: {playerMatches.pendingMatches}</p>

            <div>
                {playerMatches.matches.map((match) => (
                    <MatchCard
                        key={match.multiMatchId}
                        match={match}
                        onUndo={() => undoMatchResult(match.multiMatchId)}
                    />
                ))}
            </div>
        </div>
    );
};
```

### Match Card Component:

```typescript
const MatchCard = ({ match, onUndo }) => {
    const getMatchStatus = () => {
        if (!match.isCompleted) return "لم تبدأ";
        if (match.winnerId === null) return "تعادل";
        return `الفائز: ${match.winnerName}`;
    };

    return (
        <div className="match-card">
            <div>
                <span>{match.player1Name}</span>
                <span>vs</span>
                <span>{match.player2Name}</span>
            </div>

            {match.isCompleted && (
                <div>
                    <span>
                        {match.score1} - {match.score2}
                    </span>
                    <span>{getMatchStatus()}</span>
                    <button onClick={onUndo}>إلغاء</button>
                </div>
            )}

            <div>
                <small>
                    {match.team1Name} vs {match.team2Name}
                </small>
            </div>
        </div>
    );
};
```

---

## 🔧 Helper Functions

### Validation:

```typescript
const validateMatchResult = (result: MultiMatchResultDto, system: string) => {
    if (system === "Classic") {
        if (!result.winnerId) {
            return { valid: false, error: "يجب اختيار الفائز" };
        }
    } else {
        if (result.score1 === undefined || result.score2 === undefined) {
            return { valid: false, error: "يجب إدخال النقاط للاعبين" };
        }
        if (result.score1 < 0 || result.score2 < 0) {
            return { valid: false, error: "النقاط لا يمكن أن تكون سالبة" };
        }
    }
    return { valid: true };
};
```

### Match Status:

```typescript
const getMatchWinner = (match: MatchDetail) => {
    if (!match.isCompleted) return null;
    if (match.winnerId === null) return "تعادل";
    return match.winnerName;
};

const isMatchDraw = (match: MatchDetail) => {
    return match.isCompleted && match.winnerId === null;
};

const getMatchScore = (match: MatchDetail) => {
    if (!match.isCompleted) return "لم تبدأ";
    return `${match.score1} - ${match.score2}`;
};
```

---

## ⚠️ تحديثات مطلوبة في الكود الحالي

### 1. تحديث Match Result Form:

```diff
// الطريقة القديمة
- { totalPoints1, totalPoints2, score1, score2 }

// الطريقة الجديدة
+ { winnerId, score1, score2 }
```

### 2. تحديث Match Display:

```diff
// إضافة عرض الفائز
+ {match.winnerName && <span>الفائز: {match.winnerName}</span>}
+ {match.winnerId === null && match.isCompleted && <span>تعادل</span>}
```

### 3. إضافة Undo Button:

```diff
+ {match.isCompleted && (
+   <button onClick={() => undoMatch(match.multiMatchId)}>
+     إلغاء النتيجة
+   </button>
+ )}
```

### 4. إضافة Player Matches View:

```typescript
// إضافة صفحة/مكون جديد لعرض مباريات اللاعب
const PlayerMatchesPage = () => {
    // استخدم getPlayerMatches(playerId)
};
```

---

## 🎯 خطة التحديث المقترحة

### المرحلة 1: تحديث تسجيل النتائج

1. ✅ تحديث `MultiMatchResultDto`
2. ✅ تحديث منطق التسجيل (Classic/Points)
3. ✅ إزالة `TotalPoints` من UI

### المرحلة 2: إضافة Undo

1. ✅ إضافة Undo button للمباريات المكتملة
2. ✅ تحديث UI بعد Undo

### المرحلة 3: مباريات اللاعب

1. ✅ إضافة صفحة مباريات اللاعب
2. ✅ ربطها بالـ admin panel

### المرحلة 4: تحسينات

1. ✅ إضافة عرض الفائز في كل مكان
2. ✅ تحسين UX للنظام الجديد

---

النظام الجديد أبسط وأوضح! 🚀
لو محتاج تفاصيل أكتر لأي جزء، قولي! 😊
