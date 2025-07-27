// YuGi Tournament API - TypeScript Interfaces
// يمكن نسخ هذا الملف إلى مشروع الفرونت إند

export interface ApiResponse<T = any> {
    success: boolean;
    message: string;
    data?: T;
}

// Authentication
export interface LoginDto {
    fullName: string;
}

export interface RegisterPlayerDto {
    fullName: string;
}

export interface AuthResponse {
    token: string;
    player: PlayerDto;
}

export interface PlayerDto {
    playerId: number;
    fullName: string;
}

// Player Management
export interface PlayerAddedDto {
    fullName: string;
}

export interface PlayerRankDto {
    playerId: number;
    fullName: string;
    wins: number;
    losses: number;
    draws: number;
    points: number;
    matchesPlayed: number;
    rank: number;
    winRate: number;
}

// League Types
export enum LeagueType {
    Single = 0,
    Multi = 1,
    Groups = 2,
}

export enum SystemOfLeague {
    Points = 0,
    Classic = 1,
}

// League Management
export interface StartLeagueDto {
    name: string;
    description: string;
    typeOfLeague: LeagueType;
    systemOfLeague: SystemOfLeague;
}

export interface LeagueResponseDto {
    leagueId: number;
    leagueName: string;
    leagueDescription: string;
    leagueType: LeagueType;
    systemOfLeague: SystemOfLeague;
    isFinished: boolean;
    createdOn: string;
    players?: PlayerRankDto[];
    groups?: GroupDto[];
    matches?: MatchDto[];
    knockoutMatches?: MatchDto[];
}

export interface GroupDto {
    groupNumber: number;
    players: PlayerRankDto[];
    matches: MatchDto[];
}

// Match Management
export interface MatchResultDto {
    matchId: number;
    score1: number;
    score2: number;
}

export interface MatchViewModel {
    matchId: number;
    score1: number;
    score2: number;
    isCompleted: boolean;
    player1Name?: string;
    player2Name?: string;
    player1Id: number;
    player2Id: number;
    tournamentStage: string;
}

export interface MatchDto {
    matchId: number;
    score1: number;
    score2: number;
    isCompleted: boolean;
    player1Name?: string;
    player2Name?: string;
    player1Id: number;
    player2Id: number;
    tournamentStage: TournamentStage;
    winnerId?: number;
}

export enum TournamentStage {
    League = 0,
    GroupStage = 1,
    QuarterFinals = 2,
    SemiFinals = 3,
    Final = 4,
}

// Message System
export interface SendMessageRequestDto {
    receiverId: number;
    content: string;
}

export interface MarkMessageDto {
    messageId: number;
}

export interface MessageDto {
    messageId: number;
    content: string;
    senderName: string;
    receiverName: string;
    isRead: boolean;
    sentAt: string;
}

// Note System
export interface NoteDto {
    content: string;
}

export interface HideNoteDto {
    noteId: number;
}

export interface NoteResponseDto {
    noteId: number;
    content: string;
    isHidden: boolean;
    createdAt: string;
}

// API Service Interfaces
export interface IApiService {
    // Authentication
    login(credentials: LoginDto): Promise<ApiResponse<AuthResponse>>;
    register(player: RegisterPlayerDto): Promise<ApiResponse<PlayerDto>>;

    // Players
    getAllPlayers(): Promise<PlayerRankDto[]>;
    getPlayersRanking(): Promise<PlayerRankDto[]>;
    getAllLeaguesWithRank(): Promise<LeagueResponseDto[]>;
    addPlayer(player: PlayerAddedDto): Promise<ApiResponse>;
    deletePlayer(playerId: number): Promise<ApiResponse>;
    getGroupedPlayers(leagueId: number): Promise<GroupDto[]>;

    // Leagues
    startLeague(league: StartLeagueDto): Promise<ApiResponse>;
    finishLeague(leagueId: number): Promise<ApiResponse>;

    // Matches
    getAllMatches(): Promise<MatchViewModel[]>;
    submitMatchResult(result: MatchResultDto): Promise<ApiResponse>;

    // Messages
    sendMessage(message: SendMessageRequestDto): Promise<ApiResponse>;
    getMessages(): Promise<MessageDto[]>;
    markMessageAsRead(messageId: number): Promise<ApiResponse>;

    // Notes
    addNote(note: NoteDto): Promise<ApiResponse>;
    getNotes(): Promise<NoteResponseDto[]>;
    hideNote(noteId: number): Promise<ApiResponse>;
}

// Utility Types
export type LeagueTypeString = "Single" | "Multi" | "Groups";
export type SystemOfLeagueString = "Points" | "Classic";

// Helper Functions
export const getLeagueTypeString = (type: LeagueType): LeagueTypeString => {
    switch (type) {
        case LeagueType.Single:
            return "Single";
        case LeagueType.Multi:
            return "Multi";
        case LeagueType.Groups:
            return "Groups";
        default:
            return "Single";
    }
};

export const getSystemOfLeagueString = (
    system: SystemOfLeague
): SystemOfLeagueString => {
    switch (system) {
        case SystemOfLeague.Points:
            return "Points";
        case SystemOfLeague.Classic:
            return "Classic";
        default:
            return "Points";
    }
};

// Constants
export const API_BASE_URL = "http://localhost:5000/api";
export const API_ENDPOINTS = {
    // Authentication
    LOGIN: "/Auth/login",
    REGISTER: "/Auth/register",

    // Players
    PLAYERS: "/Player",
    PLAYERS_RANKING: "/Player/ranking",
    ALL_LEAGUES: "/Player/players/all",
    PLAYER_GROUPS: (leagueId: number) => `/Player/league/${leagueId}/groups`,

    // Leagues
    LEAGUE_START: "/League/start",
    LEAGUE_FINISH: (leagueId: number) => `/League/${leagueId}/finish`,

    // Matches
    MATCHES: "/Match",
    MATCH_RESULT: "/Match/result",

    // Messages
    MESSAGES: "/Message",
    SEND_MESSAGE: "/Message/send",
    MARK_MESSAGE_READ: "/Message/mark-read",

    // Notes
    NOTES: "/Note",
    HIDE_NOTE: "/Note/hide",
} as const;
