// YuGi Tournament API - Multi Tournament System - TypeScript Interfaces
// يمكن نسخ هذا الملف إلى مشروع الفرونت إند

// ========================================
// Enums
// ========================================

export enum SystemOfLeague {
    Classic = 0,
    Points = 1,
}

export enum TournamentStatus {
    Created = 0,
    Started = 1,
    Finished = 2,
}

// ========================================
// Request DTOs
// ========================================

export interface CreateTournamentDto {
    name: string;
    systemOfScoring: SystemOfLeague;
    teamCount: number;
    playersPerTeam: number;
}

export interface UpdateTournamentStatusDto {
    status: TournamentStatus;
}

export interface TeamCreateDto {
    teamName: string;
    playerIds: number[];
}

export interface TeamUpdateDto {
    teamName?: string;
    playerIds?: number[];
}

export interface MultiMatchResultDto {
    winnerId?: number; // للكلاسيك - إجباري للنظام الكلاسيكي
    score1?: number; // للنقاط - إجباري لنظام النقاط (يمكن أن يكون عشري مثل 2.5)
    score2?: number; // للنقاط - إجباري لنظام النقاط (يمكن أن يكون عشري مثل 1.5)
}

export interface PlayerReplaceDto {
    replacedPlayerId: number;
    newPlayerId: number;
}

export interface AddPlayerDto {
    fullName: string;
}

// ========================================
// Response Types
// ========================================

export interface ApiResponse<T = any> {
    success: boolean;
    message: string;
    data?: T;
}

export interface PlayerDetail {
    playerId: number;
    fullName: string;
    isActive: boolean;
    createdOn: string;
    multiParticipations: number;
    multiTitlesWon: number;
}

export interface TeamDetail {
    multiTeamId: number;
    teamName: string;
    totalPoints: number;
    wins: number;
    draws: number;
    losses: number;
    createdOn: string;
    players: PlayerDetail[];
}

export interface MatchDetail {
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

export interface FixtureGroup {
    team1Id: number;
    team1Name: string;
    team2Id: number;
    team2Name: string;
    matches: MatchDetail[];
}

export interface TournamentDetail {
    multiTournamentId: number;
    name: string;
    systemOfScoring: SystemOfLeague;
    teamCount: number;
    playersPerTeam: number;
    status: string; // "Created" | "Started" | "Finished"
    isActive: boolean;
    createdOn: string;
    startedOn: string | null;
    finishedOn: string | null;
    championTeamId: number | null;
    championTeamName: string | null;
    teams: TeamDetail[];
}

export interface PlayerMatchesData {
    playerId: number;
    playerName: string;
    tournamentId: number;
    tournamentName: string;
    totalMatches: number;
    completedMatches: number;
    pendingMatches: number;
    matches: MatchDetail[];
}

export interface StandingEntry {
    position: number;
    multiTeamId: number;
    teamName: string;
    totalPoints: number;
    wins: number;
    draws: number;
    losses: number;
    matchesPlayed: number;
}

export interface TournamentStandings {
    tournamentId: number;
    tournamentName: string;
    status: string;
    championTeamId: number | null;
    standings: StandingEntry[];
}

// ========================================
// API Service Class Example
// ========================================

export class MultiTournamentApiService {
    private baseUrl: string;
    private token?: string;

    constructor(baseUrl: string, token?: string) {
        this.baseUrl = baseUrl;
        this.token = token;
    }

    private async request<T>(
        endpoint: string,
        options: RequestInit = {}
    ): Promise<ApiResponse<T>> {
        const url = `${this.baseUrl}/api/multi${endpoint}`;
        const headers: HeadersInit = {
            "Content-Type": "application/json",
            ...options.headers,
        };

        if (this.token) {
            headers["Authorization"] = `Bearer ${this.token}`;
        }

        const response = await fetch(url, {
            ...options,
            headers,
        });

        return await response.json();
    }

    // ========================================
    // Tournament Management
    // ========================================

    async createTournament(data: CreateTournamentDto): Promise<ApiResponse> {
        return this.request("/tournaments", {
            method: "POST",
            body: JSON.stringify(data),
        });
    }

    async updateTournamentStatus(
        id: number,
        data: UpdateTournamentStatusDto
    ): Promise<ApiResponse> {
        return this.request(`/tournaments/${id}/status`, {
            method: "PUT",
            body: JSON.stringify(data),
        });
    }

    async deleteTournament(id: number): Promise<ApiResponse> {
        return this.request(`/tournaments/${id}`, {
            method: "DELETE",
        });
    }

    async startTournament(id: number): Promise<ApiResponse> {
        return this.request(`/tournaments/${id}/start`, {
            method: "POST",
        });
    }

    async finishTournament(id: number): Promise<ApiResponse> {
        return this.request(`/tournaments/${id}/finish`, {
            method: "POST",
        });
    }

    // ========================================
    // Team Management
    // ========================================

    async createTeam(
        tournamentId: number,
        data: TeamCreateDto
    ): Promise<ApiResponse> {
        return this.request(`/tournaments/${tournamentId}/teams`, {
            method: "POST",
            body: JSON.stringify(data),
        });
    }

    async updateTeam(
        teamId: number,
        data: TeamUpdateDto
    ): Promise<ApiResponse> {
        return this.request(`/teams/${teamId}`, {
            method: "PUT",
            body: JSON.stringify(data),
        });
    }

    async replacePlayer(
        teamId: number,
        data: PlayerReplaceDto
    ): Promise<ApiResponse> {
        return this.request(`/teams/${teamId}/replace-player`, {
            method: "POST",
            body: JSON.stringify(data),
        });
    }

    // ========================================
    // Match Management
    // ========================================

    async recordMatchResult(
        matchId: number,
        data: MultiMatchResultDto
    ): Promise<ApiResponse> {
        return this.request(`/matches/${matchId}/result`, {
            method: "POST",
            body: JSON.stringify(data),
        });
    }

    async undoMatchResult(matchId: number): Promise<ApiResponse> {
        return this.request(`/matches/${matchId}/undo`, {
            method: "POST",
        });
    }

    // Helper methods for different scoring systems
    async recordClassicResult(
        matchId: number,
        winnerId: number
    ): Promise<ApiResponse> {
        return this.recordMatchResult(matchId, { winnerId });
    }

    async recordPointsResult(
        matchId: number,
        score1: number,
        score2: number
    ): Promise<ApiResponse> {
        return this.recordMatchResult(matchId, { score1, score2 });
    }

    // ========================================
    // Data Retrieval
    // ========================================

    async getActiveTournament(): Promise<ApiResponse<TournamentDetail>> {
        return this.request("/tournaments/active");
    }

    async getTournamentById(
        id: number
    ): Promise<ApiResponse<TournamentDetail>> {
        return this.request(`/tournaments/${id}`);
    }

    async getAllTournaments(): Promise<ApiResponse<TournamentDetail[]>> {
        return this.request("/tournaments");
    }

    async getTournamentMatches(
        id: number
    ): Promise<ApiResponse<FixtureGroup[]>> {
        return this.request(`/tournaments/${id}/matches`);
    }

    async getActiveTournamentMatches(): Promise<ApiResponse<FixtureGroup[]>> {
        return this.request("/tournaments/active/matches");
    }

    async getTournamentStandings(
        id: number
    ): Promise<ApiResponse<TournamentStandings>> {
        return this.request(`/tournaments/${id}/standings`);
    }

    async getPlayerMatches(
        playerId: number
    ): Promise<ApiResponse<PlayerMatchesData>> {
        return this.request(`/players/${playerId}/matches`);
    }

    // ========================================
    // Player Management
    // ========================================

    async getAllPlayers(): Promise<ApiResponse<PlayerDetail[]>> {
        return this.request("/players");
    }

    async getPlayerById(playerId: number): Promise<ApiResponse<PlayerDetail>> {
        return this.request(`/players/${playerId}`);
    }

    async addPlayer(data: AddPlayerDto): Promise<ApiResponse<PlayerDetail>> {
        return this.request("/players", {
            method: "POST",
            body: JSON.stringify(data),
        });
    }
}

// ========================================
// Usage Examples
// ========================================

/*
// إنشاء instance من الـ API service
const api = new MultiTournamentApiService('http://localhost:5000', 'your-jwt-token');

// إنشاء بطولة جديدة
const createTournament = async () => {
  const result = await api.createTournament({
    name: 'بطولة الربيع 2024',
    systemOfScoring: SystemOfLeague.Classic,
    teamCount: 4,
    playersPerTeam: 3
  });
  
  if (result.success) {
    console.log('Tournament created successfully!');
  }
};

// تسجيل نتيجة كلاسيكية
const recordClassic = async (matchId: number, winnerId: number) => {
  const result = await api.recordClassicResult(matchId, winnerId);
  return result;
};

// تسجيل نتيجة نقاط
const recordPoints = async (matchId: number, score1: number, score2: number) => {
  const result = await api.recordPointsResult(matchId, score1, score2);
  return result;
};

// جلب مباريات لاعب
const getPlayerMatches = async (playerId: number) => {
  const result = await api.getPlayerMatches(playerId);
  
  if (result.success && result.data) {
    console.log(`${result.data.playerName} has:`);
    console.log(`- Total matches: ${result.data.totalMatches}`);
    console.log(`- Completed: ${result.data.completedMatches}`);
    console.log(`- Pending: ${result.data.pendingMatches}`);
  }
  
  return result;
};

// إلغاء نتيجة مباراة
const undoMatch = async (matchId: number) => {
  const result = await api.undoMatchResult(matchId);
  return result;
};
*/

// ========================================
// Type Guards (مساعدة للفحص)
// ========================================

export const isClassicSystem = (system: SystemOfLeague): boolean => {
    return system === SystemOfLeague.Classic;
};

export const isPointsSystem = (system: SystemOfLeague): boolean => {
    return system === SystemOfLeague.Points;
};

export const isMatchCompleted = (match: MatchDetail): boolean => {
    return match.isCompleted;
};

export const getMatchWinner = (match: MatchDetail): PlayerDetail | null => {
    if (!match.winnerId) return null;

    // يجب جلب بيانات اللاعب من الـ API
    return null; // placeholder
};

export const isMatchDraw = (match: MatchDetail): boolean => {
    return match.isCompleted && match.winnerId === null;
};

// ========================================
// Validation Helpers
// ========================================

export const validateMatchResult = (
    result: MultiMatchResultDto,
    system: SystemOfLeague
): { isValid: boolean; error?: string } => {
    if (isClassicSystem(system)) {
        if (!result.winnerId) {
            return {
                isValid: false,
                error: "Winner ID is required for Classic system",
            };
        }
        return { isValid: true };
    } else {
        if (result.score1 === undefined || result.score2 === undefined) {
            return {
                isValid: false,
                error: "Both scores are required for Points system",
            };
        }
        if (result.score1 < 0 || result.score2 < 0) {
            return { isValid: false, error: "Scores cannot be negative" };
        }
        return { isValid: true };
    }
};

export const validateTeamCreation = (
    team: TeamCreateDto,
    expectedPlayersCount: number
): { isValid: boolean; error?: string } => {
    if (!team.teamName.trim()) {
        return { isValid: false, error: "Team name is required" };
    }

    if (team.playerIds.length !== expectedPlayersCount) {
        return {
            isValid: false,
            error: `Team must have exactly ${expectedPlayersCount} players`,
        };
    }

    // Check for duplicate player IDs
    const uniqueIds = new Set(team.playerIds);
    if (uniqueIds.size !== team.playerIds.length) {
        return {
            isValid: false,
            error: "Duplicate player IDs are not allowed",
        };
    }

    return { isValid: true };
};
