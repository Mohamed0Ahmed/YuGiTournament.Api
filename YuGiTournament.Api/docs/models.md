## النماذج (Models)

### Player

-   playerId: int
-   fullName: string
-   wins/losses/draws: int
-   points: double
-   matchesPlayed: int
-   rank: int
-   winRate: double
-   groupNumber?: int

### Match

-   matchId, player1Id, player2Id: int
-   score1, score2: double
-   isCompleted: bool
-   stage: TournamentStage
-   winnerId?: int

### MatchRound

-   matchRoundId, matchId: int
-   winnerId?: int
-   isDraw: bool

### LeagueId

-   id: int, name, description: string
-   createdOn: DateTime (UTC)
-   typeOfLeague: LeagueType
-   systemOfLeague: SystemOfLeague
-   isFinished, isDeleted: bool

### Message

-   id: int, senderId: string, senderFullName, senderPhoneNumber: string
-   content: string, isRead, isDeleted: bool
-   sentAt: DateTime, isFromAdmin: bool

### Note

-   id: int, content: string, isDeleted, isHidden: bool

### FriendlyPlayer

-   playerId: int, fullName: string, createdOn: DateTime, isActive: bool

### FriendlyMatch

-   matchId, player1Id, player2Id: int
-   player1Score, player2Score: int
-   playedOn: DateTime, isDeleted: bool

### FriendlyMessage

-   id: int, senderId, senderFullName, senderPhoneNumber: string
-   content: string, isRead, isDeleted: bool, sentAt: DateTime, isFromAdmin: bool

### ShutoutResult (نتيجة 5-0)

-   shutoutId, matchId, winnerId, loserId: int
-   achievedOn: DateTime, isDeleted: bool

### Enums

-   LeagueType: Single=0, Multi=1, Groups=2
-   SystemOfLeague: Points=0, Classic=1
-   TournamentStage: League=0, GroupStage=1, QuarterFinals=2, SemiFinals=3, Final=4
