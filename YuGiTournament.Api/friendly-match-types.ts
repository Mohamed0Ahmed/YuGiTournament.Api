// YuGi Tournament API - Friendly Match System - TypeScript Interfaces
// يمكن نسخ هذا الملف إلى مشروع الفرونت إند

// ========================================
// DTOs
// ========================================

export interface AddFriendlyPlayerDto {
    fullName: string;
}

export interface RecordFriendlyMatchDto {
    player1Id: number;
    player2Id: number;
    player1Score: number;
    player2Score: number;
}

export interface FriendlyPlayerDto {
    playerId: number;
    fullName: string;
    createdOn: string;
    isActive: boolean;
    totalMatches: number;
    totalWins: number;
    totalLosses: number;
    totalDraws: number;
    winRate: number;
}

export interface FriendlyMatchHistoryDto {
    matchId: number;
    playedOn: string;
    player1Score: number;
    player2Score: number;
    winner: string;
    player1Name: string;
    player2Name: string;
}

export interface OverallScoreDto {
    player1Name: string;
    player2Name: string;
    player1TotalScore: number;
    player2TotalScore: number;
    totalMatches: number;
    leadingPlayer: string;
    scoreDifference: number;

    // Win/Loss Statistics
    player1Wins: number;
    player1Losses: number;
    player1Draws: number;
    player2Wins: number;
    player2Losses: number;
    player2Draws: number;

    // Win Rates
    player1WinRate: number;
    player2WinRate: number;

    // Match History
    matchHistory: FriendlyMatchHistoryDto[];
}

export interface ShutoutResultDto {
    shutoutId: number;
    matchId: number;
    winnerName: string;
    loserName: string;
    achievedOn: string;
    winnerScore: number;
    loserScore: number;
    matchDetails: string;
}

// Advanced Filtering and Pagination Types
export interface MatchFilterDto {
    playerId?: number;
    opponentIds?: number[];
    dateFilter: DateFilter;
    page: number;
    pageSize: number;
}

export enum DateFilter {
    Today = 1,
    Last3Days = 2,
    LastWeek = 3,
    LastMonth = 4,
    AllTime = 5,
}

export interface PaginatedMatchResultDto {
    matches: FriendlyMatchHistoryDto[];
    totalMatches: number;
    currentPage: number;
    pageSize: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
    filterSummary: string;
}

// Shutout Filtering Types
export interface ShutoutFilterDto {
    playerId?: number;
    playerIds?: number[];
    playerRole?: ShutoutPlayerRole;
    dateFilter: DateFilter;
    page: number;
    pageSize: number;
}

export enum ShutoutPlayerRole {
    Any = 1, // اللاعب سواء كسب أو خسر
    Winner = 2, // اللاعب كان الفائز بس
    Loser = 3, // اللاعب كان الخاسر بس
}

export interface PaginatedShutoutResultDto {
    shutouts: ShutoutResultDto[];
    totalShutouts: number;
    currentPage: number;
    pageSize: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
    filterSummary: string;
}

// ========================================
// API Response Types
// ========================================

export interface ApiResponse<T = any> {
    success: boolean;
    message: string;
    data?: T;
}

// ========================================
// Service Interface
// ========================================

export interface IFriendlyMatchApiService {
    // Player Management
    getAllFriendlyPlayers(): Promise<FriendlyPlayerDto[]>;
    getFriendlyPlayerById(playerId: number): Promise<FriendlyPlayerDto | null>;
    addFriendlyPlayer(player: AddFriendlyPlayerDto): Promise<ApiResponse>;
    deleteFriendlyPlayer(playerId: number): Promise<ApiResponse>;
    deactivateFriendlyPlayer(playerId: number): Promise<ApiResponse>;
    getFriendlyPlayersRanking(): Promise<FriendlyPlayerDto[]>;
    getFriendlyPlayerStats(playerId: number): Promise<FriendlyPlayerDto | null>;

    // Match Management
    getAllFriendlyMatches(): Promise<FriendlyMatchHistoryDto[]>;
    getFriendlyMatchesBetweenPlayers(
        player1Id: number,
        player2Id: number
    ): Promise<FriendlyMatchHistoryDto[]>;
    recordFriendlyMatch(match: RecordFriendlyMatchDto): Promise<ApiResponse>;
    deleteFriendlyMatch(matchId: number): Promise<ApiResponse>;
    updateFriendlyMatch(
        matchId: number,
        match: RecordFriendlyMatchDto
    ): Promise<ApiResponse>;

    // Statistics and Analysis
    getOverallScoreBetweenPlayers(
        player1Id: number,
        player2Id: number
    ): Promise<OverallScoreDto | null>;

    // Shutout Results (5-0 matches)
    getAllShutoutResults(): Promise<ShutoutResultDto[]>;
    getShutoutResultsByPlayer(playerId: number): Promise<ShutoutResultDto[]>;
    getShutoutResultByMatch(matchId: number): Promise<ShutoutResultDto | null>;
    deleteShutoutResult(shutoutId: number): Promise<ApiResponse>;
}

// ========================================
// Utility Types
// ========================================

export type MatchWinner = "player1" | "player2" | "draw";

export interface MatchResult {
    winner: MatchWinner;
    player1Score: number;
    player2Score: number;
}

export interface PlayerStats {
    playerId: number;
    fullName: string;
    totalMatches: number;
    wins: number;
    losses: number;
    draws: number;
    winRate: number;
    averageScore: number;
}

export interface HeadToHeadStats {
    player1Id: number;
    player2Id: number;
    player1Name: string;
    player2Name: string;
    totalMatches: number;
    player1Wins: number;
    player2Wins: number;
    draws: number;
    player1TotalScore: number;
    player2TotalScore: number;
    leadingPlayer: string;
    scoreDifference: number;
}

// ========================================
// UI Component Types
// ========================================

export interface PlayerSelectionOption {
    value: number;
    label: string;
    isActive: boolean;
}

export interface MatchFormData {
    player1Id: number | null;
    player2Id: number | null;
    player1Score: number;
    player2Score: number;
}

export interface MatchFilterOptions {
    player1Id?: number;
    player2Id?: number;
    dateFrom?: string;
    dateTo?: string;
    winner?: string;
}

export interface PlayerFilterOptions {
    isActive?: boolean;
    hasMatches?: boolean;
    minWinRate?: number;
    maxWinRate?: number;
}

// ========================================
// Constants
// ========================================

export const FRIENDLY_MATCH_ENDPOINTS = {
    // Player Management
    GET_ALL_PLAYERS: "/api/FriendlyMatch/players",
    GET_PLAYER_BY_ID: (id: number) => `/api/FriendlyMatch/players/${id}`,
    ADD_PLAYER: "/api/FriendlyMatch/players",
    DELETE_PLAYER: (id: number) => `/api/FriendlyMatch/players/${id}`,
    DEACTIVATE_PLAYER: (id: number) =>
        `/api/FriendlyMatch/players/${id}/deactivate`,
    GET_PLAYERS_RANKING: "/api/FriendlyMatch/players/ranking",
    GET_PLAYER_STATS: (id: number) => `/api/FriendlyMatch/players/${id}/stats`,

    // Match Management
    GET_ALL_MATCHES: "/api/FriendlyMatch/matches",
    GET_MATCHES_BETWEEN_PLAYERS: (player1Id: number, player2Id: number) =>
        `/api/FriendlyMatch/matches/${player1Id}/${player2Id}`,
    RECORD_MATCH: "/api/FriendlyMatch/matches/record",
    DELETE_MATCH: (id: number) => `/api/FriendlyMatch/matches/${id}`,
    UPDATE_MATCH: (id: number) => `/api/FriendlyMatch/matches/${id}`,

    // Statistics
    GET_OVERALL_SCORE: (player1Id: number, player2Id: number) =>
        `/api/FriendlyMatch/overall-score/${player1Id}/${player2Id}`,

    // Advanced Match Filtering and Pagination
    GET_FILTERED_MATCHES: "/api/FriendlyMatch/matches/filtered",
    GET_PLAYER_MATCHES: (playerId: number) =>
        `/api/FriendlyMatch/matches/player/${playerId}`,
    GET_PLAYER_VS_OPPONENTS: (playerId: number) =>
        `/api/FriendlyMatch/matches/player/${playerId}/vs-opponents`,

    // Shutout Results (5-0 matches)
    GET_ALL_SHUTOUTS: "/api/FriendlyMatch/shutouts",
    GET_SHUTOUTS_BY_PLAYER: (playerId: number) =>
        `/api/FriendlyMatch/shutouts/player/${playerId}`,
    GET_SHUTOUT_BY_MATCH: (matchId: number) =>
        `/api/FriendlyMatch/shutouts/match/${matchId}`,
    DELETE_SHUTOUT: (shutoutId: number) =>
        `/api/FriendlyMatch/shutouts/${shutoutId}`,

    // Advanced Shutout Filtering and Pagination
    GET_FILTERED_SHUTOUTS: "/api/FriendlyMatch/shutouts/filtered",
    GET_PLAYER_SHUTOUTS_FILTERED: (playerId: number) =>
        `/api/FriendlyMatch/shutouts/player/${playerId}/filtered`,
    GET_PLAYERS_SHUTOUTS_FILTERED:
        "/api/FriendlyMatch/shutouts/players/filtered",

    // Test
    TEST: "/api/FriendlyMatch/test",
} as const;

// ========================================
// Helper Functions
// ========================================

export const calculateWinRate = (
    wins: number,
    totalMatches: number
): number => {
    return totalMatches > 0 ? (wins / totalMatches) * 100 : 0;
};

export const getMatchWinner = (
    player1Score: number,
    player2Score: number
): MatchWinner => {
    if (player1Score > player2Score) return "player1";
    if (player2Score > player1Score) return "player2";
    return "draw";
};

export const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString("ar-EG", {
        year: "numeric",
        month: "long",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
    });
};

export const validateMatchData = (data: RecordFriendlyMatchDto): string[] => {
    const errors: string[] = [];

    if (data.player1Id === data.player2Id) {
        errors.push("لا يمكن للاعب أن يلعب ضد نفسه");
    }

    if (data.player1Score < 0 || data.player2Score < 0) {
        errors.push("النتيجة يجب أن تكون صفر أو أكثر");
    }

    if (!data.player1Id || !data.player2Id) {
        errors.push("يجب اختيار اللاعبين");
    }

    return errors;
};

export const validatePlayerData = (data: AddFriendlyPlayerDto): string[] => {
    const errors: string[] = [];

    if (!data.fullName || data.fullName.trim().length === 0) {
        errors.push("اسم اللاعب مطلوب");
    }

    if (data.fullName && data.fullName.trim().length < 2) {
        errors.push("اسم اللاعب يجب أن يكون أكثر من حرفين");
    }

    return errors;
};

// ========================================
// Default Values
// ========================================

export const DEFAULT_MATCH_FORM: MatchFormData = {
    player1Id: null,
    player2Id: null,
    player1Score: 0,
    player2Score: 0,
};

export const DEFAULT_PLAYER_FORM: AddFriendlyPlayerDto = {
    fullName: "",
};

export const DEFAULT_MATCH_FILTER: MatchFilterOptions = {
    player1Id: undefined,
    player2Id: undefined,
    dateFrom: undefined,
    dateTo: undefined,
    winner: undefined,
};

export const DEFAULT_PLAYER_FILTER: PlayerFilterOptions = {
    isActive: true,
    hasMatches: undefined,
    minWinRate: undefined,
    maxWinRate: undefined,
};
