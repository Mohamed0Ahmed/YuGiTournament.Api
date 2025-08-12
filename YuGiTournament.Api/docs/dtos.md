## DTOs

-   LoginDto: `{ email, password }`
-   PlayerLoginDto: `{ phoneNumber, password }`
-   RegisterPlayerDto: `{ phoneNumber, password, firstName, lastName }`
-   ResetPasswordDto: `{ phoneNumber, newPassword }`
-   StartLeagueDto: `{ name, description, typeOfLeague: LeagueType, systemOfLeague: SystemOfLeague }`
-   PlayerAddedDto: `{ fullName }`
-   MatchResultDto: `{ winnerId?: number }`
-   SendMessageRequestDto: `{ content }`
-   MarkMessageDto: `{ marked: boolean }`
-   NoteDto: `{ content }`

-   AddFriendlyPlayerDto: `{ fullName }`
-   RecordFriendlyMatchDto: `{ player1Id, player2Id, player1Score, player2Score }`
-   FriendlyPlayerDto: `{ playerId, fullName, createdOn, isActive, totalMatches, totalWins, totalLosses, totalDraws, winRate }`
-   FriendlyMatchHistoryDto: `{ matchId, playedOn, player1Score, player2Score, winner, player1Name, player2Name }`
-   OverallScoreDto: أسماء + إجماليات + نسب الفوز + `matchHistory: FriendlyMatchHistoryDto[]`

-   MatchFilterDto: `{ playerId?, opponentIds?: number[], dateFilter: DateFilter, page, pageSize }`
-   ShutoutFilterDto: `{ playerId?, playerIds?: number[], playerRole?: ShutoutPlayerRole, dateFilter: DateFilter, page, pageSize }`
-   PaginatedMatchResultDto: `{ matches: FriendlyMatchHistoryDto[], totalMatches, currentPage, pageSize, totalPages, hasPreviousPage, hasNextPage, filterSummary }`
-   PaginatedShutoutResultDto: `{ shutouts: ShutoutResultDto[], totalShutouts, currentPage, pageSize, totalPages, hasPreviousPage, hasNextPage, filterSummary }`
-   PaginatedPlayersResultDto: `{ players: FriendlyPlayerDto[], totalPlayers, currentPage, pageSize, totalPages, hasPreviousPage, hasNextPage, filterSummary }`
-   ShutoutResultDto: `{ shutoutId, matchId, winnerName, loserName, achievedOn, winnerScore=5, loserScore=0, matchDetails }`
