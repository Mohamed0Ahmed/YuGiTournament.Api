# دليل التكامل مع الفرونت (Angular Integration Guide)

## 🚀 نظرة عامة سريعة

هذا النظام المبسط يتيح لك إدارة بطولات الفرق بـ **5 APIs أساسية فقط**:

1. **إنشاء بطولة** → إضافة فرق → **بدء البطولة** → تسجيل نتائج → **عرض الترتيب**

---

## 📱 Types للـ Angular/TypeScript

```typescript
// Enums
export type SystemOfScoring = "Classic" | "Points";
export type TournamentStatus = "Created" | "Started" | "Finished";

// Tournament Types
export interface Tournament {
    multiTournamentId: number;
    name: string;
    systemOfScoring: SystemOfScoring;
    teamCount: number;
    playersPerTeam: number;
    status: TournamentStatus;
    isActive: boolean;
    createdOn: string;
    startedOn?: string;
    finishedOn?: string;
    championTeamId?: number;
    championTeamName?: string;
    teams: Team[];
}

export interface Team {
    multiTeamId: number;
    teamName: string;
    totalPoints: number;
    wins: number;
    draws: number;
    losses: number;
    createdOn: string;
    players: Player[];
}

export interface Player {
    playerId: number;
    fullName: string;
    isActive: boolean;
}

export interface Match {
    multiMatchId: number;
    team1Id: number;
    team1Name: string;
    team2Id: number;
    team2Name: string;
    player1Id: number;
    player1Name: string;
    player2Id: number;
    player2Name: string;
    score1?: number;
    score2?: number;
    totalPoints1?: number;
    totalPoints2?: number;
    isCompleted: boolean;
    completedOn?: string;
}

export interface MatchFixture {
    team1Id: number;
    team1Name: string;
    team2Id: number;
    team2Name: string;
    matches: Match[];
}

export interface Standings {
    tournamentId: number;
    tournamentName: string;
    status: string;
    championTeamId?: number;
    standings: TeamStanding[];
}

export interface TeamStanding {
    position: number;
    multiTeamId: number;
    teamName: string;
    totalPoints: number;
    wins: number;
    draws: number;
    losses: number;
    matchesPlayed: number;
}

// Request DTOs
export interface CreateTournamentRequest {
    name: string;
    systemOfScoring: SystemOfScoring;
    teamCount: number;
    playersPerTeam: number;
}

export interface UpdateTournamentStatusRequest {
    status: TournamentStatus;
}

export interface CreateTeamRequest {
    teamName: string;
    playerIds: number[];
}

export interface UpdateTeamRequest {
    teamName?: string;
    playerIds?: number[];
}

export interface MatchResultRequest {
    score1?: number;
    score2?: number;
    totalPoints1?: number;
    totalPoints2?: number;
}

// API Response
export interface ApiResponse<T = any> {
    success: boolean;
    message: string;
    data?: T;
}
```

---

## 🔧 Angular Service (multi-tournament.service.ts)

```typescript
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../environments/environment";

@Injectable({
    providedIn: "root",
})
export class MultiTournamentService {
    private readonly apiUrl = `${environment.apiUrl}/multi`;

    constructor(private http: HttpClient) {}

    // 1. إنشاء بطولة
    createTournament(
        request: CreateTournamentRequest
    ): Observable<ApiResponse<Tournament>> {
        return this.http.post<ApiResponse<Tournament>>(
            `${this.apiUrl}/tournaments`,
            request
        );
    }

    // 2. تحديث حالة البطولة
    updateTournamentStatus(
        id: number,
        request: UpdateTournamentStatusRequest
    ): Observable<ApiResponse> {
        return this.http.put<ApiResponse>(
            `${this.apiUrl}/tournaments/${id}/status`,
            request
        );
    }

    // 3. حذف البطولة
    deleteTournament(id: number): Observable<ApiResponse> {
        return this.http.delete<ApiResponse>(
            `${this.apiUrl}/tournaments/${id}`
        );
    }

    // 4. إنشاء فريق
    createTeam(
        tournamentId: number,
        request: CreateTeamRequest
    ): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(
            `${this.apiUrl}/tournaments/${tournamentId}/teams`,
            request
        );
    }

    // 5. تحديث فريق أو استبدال لاعب
    updateTeam(
        teamId: number,
        request: UpdateTeamRequest
    ): Observable<ApiResponse> {
        return this.http.put<ApiResponse>(
            `${this.apiUrl}/teams/${teamId}`,
            request
        );
    }

    // عرض البيانات
    getTournament(id: number): Observable<ApiResponse<Tournament>> {
        return this.http.get<ApiResponse<Tournament>>(
            `${this.apiUrl}/tournaments/${id}`
        );
    }

    getActiveTournament(): Observable<ApiResponse<Tournament>> {
        return this.http.get<ApiResponse<Tournament>>(
            `${this.apiUrl}/tournaments/active`
        );
    }

    getAllTournaments(): Observable<ApiResponse<Tournament[]>> {
        return this.http.get<ApiResponse<Tournament[]>>(
            `${this.apiUrl}/tournaments`
        );
    }

    getTournamentMatches(id: number): Observable<ApiResponse<MatchFixture[]>> {
        return this.http.get<ApiResponse<MatchFixture[]>>(
            `${this.apiUrl}/tournaments/${id}/matches`
        );
    }

    getTournamentStandings(id: number): Observable<ApiResponse<Standings>> {
        return this.http.get<ApiResponse<Standings>>(
            `${this.apiUrl}/tournaments/${id}/standings`
        );
    }

    // تسجيل نتيجة مباراة
    recordMatchResult(
        matchId: number,
        request: MatchResultRequest
    ): Observable<ApiResponse> {
        return this.http.post<ApiResponse>(
            `${this.apiUrl}/matches/${matchId}/result`,
            request
        );
    }

    // Helper methods
    startTournament(id: number): Observable<ApiResponse> {
        return this.updateTournamentStatus(id, { status: "Started" });
    }

    finishTournament(id: number): Observable<ApiResponse> {
        return this.updateTournamentStatus(id, { status: "Finished" });
    }

    replacePlayer(
        teamId: number,
        replacedPlayerId: number,
        newPlayerId: number
    ): Observable<ApiResponse> {
        return this.updateTeam(teamId, {
            playerIds: [replacedPlayerId, newPlayerId],
        });
    }
}
```

---

## 📋 أمثلة للاستخدام في الكمبوننت

### 1. إنشاء بطولة جديدة

```typescript
// create-tournament.component.ts
export class CreateTournamentComponent {
    constructor(private tournamentService: MultiTournamentService) {}

    createTournament() {
        const request: CreateTournamentRequest = {
            name: "بطولة الشتاء 2025",
            systemOfScoring: "Classic",
            teamCount: 4,
            playersPerTeam: 3,
        };

        this.tournamentService.createTournament(request).subscribe({
            next: (response) => {
                if (response.success) {
                    console.log("تم إنشاء البطولة بنجاح:", response.data);
                    // التوجه لصفحة إضافة الفرق
                }
            },
            error: (error) => console.error("خطأ في إنشاء البطولة:", error),
        });
    }
}
```

### 2. إضافة فريق مع اللاعبين

```typescript
// add-team.component.ts
export class AddTeamComponent {
    selectedPlayers: number[] = [];

    addTeam(tournamentId: number) {
        const request: CreateTeamRequest = {
            teamName: "فريق النسور",
            playerIds: this.selectedPlayers, // [1, 2, 3]
        };

        this.tournamentService.createTeam(tournamentId, request).subscribe({
            next: (response) => {
                if (response.success) {
                    console.log("تم إضافة الفريق بنجاح");
                    this.selectedPlayers = []; // إعادة تعيين
                }
            },
        });
    }
}
```

### 3. بدء البطولة

```typescript
// tournament-management.component.ts
export class TournamentManagementComponent {
    startTournament(tournamentId: number) {
        this.tournamentService.startTournament(tournamentId).subscribe({
            next: (response) => {
                if (response.success) {
                    console.log("تم بدء البطولة وتوليد المباريات");
                    // التوجه لصفحة المباريات
                }
            },
        });
    }
}
```

### 4. تسجيل نتيجة مباراة

```typescript
// match-result.component.ts
export class MatchResultComponent {
    // نظام Classic
    recordClassicResult(matchId: number, score1: number, score2: number) {
        const request: MatchResultRequest = { score1, score2 };

        this.tournamentService.recordMatchResult(matchId, request).subscribe({
            next: (response) => {
                if (response.success) {
                    console.log("تم تسجيل النتيجة");
                }
            },
        });
    }

    // نظام Points
    recordPointsResult(matchId: number, points1: number, points2: number) {
        const request: MatchResultRequest = {
            totalPoints1: points1,
            totalPoints2: points2,
        };

        this.tournamentService.recordMatchResult(matchId, request).subscribe({
            next: (response) => {
                if (response.success) {
                    console.log("تم تسجيل النقاط");
                }
            },
        });
    }
}
```

### 5. عرض الترتيب

```typescript
// standings.component.ts
export class StandingsComponent implements OnInit {
    standings: Standings | null = null;

    ngOnInit() {
        this.loadStandings();
    }

    loadStandings() {
        const tournamentId = 1; // من الراوت أو الـ state

        this.tournamentService.getTournamentStandings(tournamentId).subscribe({
            next: (response) => {
                if (response.success) {
                    this.standings = response.data;
                }
            },
        });
    }
}
```

---

## 🎨 Template Examples

### عرض الترتيب

```html
<!-- standings.component.html -->
<div class="standings-container" *ngIf="standings">
    <h2>{{ standings.tournamentName }}</h2>
    <p>الحالة: {{ standings.status }}</p>

    <table class="standings-table">
        <thead>
            <tr>
                <th>المركز</th>
                <th>اسم الفريق</th>
                <th>النقاط</th>
                <th>فوز</th>
                <th>تعادل</th>
                <th>خسارة</th>
            </tr>
        </thead>
        <tbody>
            <tr
                *ngFor="let team of standings.standings"
                [class.champion]="team.multiTeamId === standings.championTeamId"
            >
                <td>{{ team.position }}</td>
                <td>{{ team.teamName }}</td>
                <td>{{ team.totalPoints }}</td>
                <td>{{ team.wins }}</td>
                <td>{{ team.draws }}</td>
                <td>{{ team.losses }}</td>
            </tr>
        </tbody>
    </table>
</div>
```

### تسجيل نتيجة (نظام Classic)

```html
<!-- match-result.component.html -->
<div class="match-result-form">
    <h3>{{ match.player1Name }} vs {{ match.player2Name }}</h3>

    <div class="score-buttons">
        <button (click)="recordResult(3, 0)" class="win-btn">
            فوز {{ match.player1Name }}
        </button>

        <button (click)="recordResult(1, 1)" class="draw-btn">تعادل</button>

        <button (click)="recordResult(0, 3)" class="win-btn">
            فوز {{ match.player2Name }}
        </button>
    </div>
</div>
```

---

## 🔄 دورة العمل الكاملة

```typescript
// tournament-workflow.service.ts
export class TournamentWorkflowService {
    async createCompleteTournament() {
        try {
            // 1. إنشاء البطولة
            const tournament = await this.createTournament({
                name: "بطولة تجريبية",
                systemOfScoring: "Classic",
                teamCount: 4,
                playersPerTeam: 3,
            });

            // 2. إضافة الفرق
            await this.addAllTeams(tournament.data.multiTournamentId);

            // 3. بدء البطولة
            await this.startTournament(tournament.data.multiTournamentId);

            console.log("تم إعداد البطولة بالكامل!");
        } catch (error) {
            console.error("خطأ في إعداد البطولة:", error);
        }
    }

    private async addAllTeams(tournamentId: number) {
        const teams = [
            { teamName: "الأهلي", playerIds: [1, 2, 3] },
            { teamName: "الزمالك", playerIds: [4, 5, 6] },
            { teamName: "الإسماعيلي", playerIds: [7, 8, 9] },
            { teamName: "المصري", playerIds: [10, 11, 12] },
        ];

        for (const team of teams) {
            await this.tournamentService
                .createTeam(tournamentId, team)
                .toPromise();
        }
    }
}
```

---

## 🚨 نصائح مهمة للتطوير

### 1. **Error Handling**

```typescript
// Global error interceptor
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    intercept(
        req: HttpRequest<any>,
        next: HttpHandler
    ): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError((error: HttpErrorResponse) => {
                if (error.status === 401) {
                    // إعادة توجيه للتسجيل
                }
                return throwError(error);
            })
        );
    }
}
```

### 2. **Loading States**

```typescript
export class TournamentComponent {
    loading = false;

    async loadData() {
        this.loading = true;
        try {
            const data = await this.tournamentService
                .getTournament(1)
                .toPromise();
            // معالجة البيانات
        } finally {
            this.loading = false;
        }
    }
}
```

### 3. **Real-time Updates**

```typescript
// استخدم SignalR أو WebSockets للتحديث المباشر
export class LiveMatchesComponent {
    private connection = new signalR.HubConnectionBuilder()
        .withUrl("/matchHub")
        .build();

    ngOnInit() {
        this.connection.on("MatchResultUpdated", (matchId, result) => {
            this.updateMatchInUI(matchId, result);
        });
    }
}
```

---

## ✅ Checklist للتطبيق

-   [ ] نسخ الـ Types للـ Angular project
-   [ ] إنشاء الـ Service مع جميع الـ methods
-   [ ] إعداد الـ HTTP Interceptors للـ Authentication
-   [ ] إنشاء الكمبوننتس الأساسية
-   [ ] إضافة الـ Error Handling
-   [ ] تطبيق الـ Loading States
-   [ ] اختبار جميع الـ APIs

**🎉 الدوكس دي كاملة وجاهزة للاستخدام في Angular مباشرة!**
