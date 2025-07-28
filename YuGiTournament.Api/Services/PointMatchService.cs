using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.DTOs;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Models.ViewModels;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class PointMatchService : IPointMatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PointMatchService> _logger;
        private readonly int _maxRoundsPerMatch;
        private readonly IPlayerRankingService _playerRankingService;

        public PointMatchService(IUnitOfWork unitOfWork, ILogger<PointMatchService> logger, IConfiguration configuration, IPlayerRankingService playerRankingService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _maxRoundsPerMatch = configuration.GetValue<int>("GameRules:MaxRoundsPerMatch");
            _playerRankingService = playerRankingService;
        }

        public async Task<IEnumerable<MatchViewModel>> GetAllMatchesAsync()
        {
            var league = await _unitOfWork.GetRepository<LeagueId>()
                .Find(x => x.IsFinished == false)
                .FirstOrDefaultAsync();

            if (league == null)
                return [];

            return await GetMatchesByLeagueAsync(league.Id);
        }

        public async Task<IEnumerable<LeagueWithMatchesViewModel>> GetAllLeaguesWithMatchesAsync()
        {
            var leagues = await _unitOfWork.GetRepository<LeagueId>()
                .GetAll()
                .Where(l => !l.IsDeleted)
                .ToListAsync();

            if (leagues == null || leagues.Count == 0)
                return [];

            var result = new List<LeagueWithMatchesViewModel>();
            foreach (var league in leagues)
            {
                var matches = await GetMatchesByLeagueAsync(league.Id);
                result.Add(new LeagueWithMatchesViewModel
                {
                    LeagueId = league.Id,
                    LeagueName = league.Name,
                    LeagueDescription = league.Description,
                    LeagueType = league.TypeOfLeague.ToString(),
                    SystemOfLeague = league.SystemOfLeague.ToString(), // Added
                    IsFinished = league.IsFinished,
                    CreatedOn = league.CreatedOn,
                    Matches = matches
                });
            }

            return result;
        }

        public async Task<MatchViewModel?> GetMatchByIdAsync(int matchId)
        {
            return await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => m.MatchId == matchId)
                .Select(m => new MatchViewModel
                {
                    MatchId = m.MatchId,
                    Score1 = m.Score1,
                    Score2 = m.Score2,
                    IsCompleted = m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Player1Id = m.Player1Id,
                    Player2Id = m.Player2Id
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ApiResponse> ResetMatchByIdAsync(int matchId)
        {
            try
            {
                using var transaction = await _unitOfWork.GetDbContext().Database.BeginTransactionAsync();
                var match = await _unitOfWork.GetRepository<Match>()
                    .Find(match => match.MatchId == matchId)
                    .Include(m => m.Rounds.Where(r => !r.IsDeleted))
                    .FirstOrDefaultAsync();

                if (match == null)
                    return new ApiResponse(false, "No Match Here");

                // التحقق من عدم وجود مراحل أحدث بدأت بالفعل
                var hasQuarterFinals = await _unitOfWork.GetRepository<Match>()
                    .Find(m => m.LeagueNumber == match.LeagueNumber && m.Stage == TournamentStage.QuarterFinals && !m.IsDeleted)
                    .AnyAsync();

                var hasSemiFinals = await _unitOfWork.GetRepository<Match>()
                    .Find(m => m.LeagueNumber == match.LeagueNumber && m.Stage == TournamentStage.SemiFinals && !m.IsDeleted)
                    .AnyAsync();

                var hasFinal = await _unitOfWork.GetRepository<Match>()
                    .Find(m => m.LeagueNumber == match.LeagueNumber && m.Stage == TournamentStage.Final && !m.IsDeleted)
                    .AnyAsync();

                // منع إعادة تعيين مباريات مرحلة المجموعات إذا بدأ دور الـ 8
                if (match.Stage == TournamentStage.GroupStage && hasQuarterFinals)
                {
                    return new ApiResponse(false, "لا يمكن إعادة تعيين مباراة في مرحلة المجموعات بعد بدء دور الـ 8");
                }

                // منع إعادة تعيين مباريات دور الـ 8 إذا بدأ نصف النهائي
                if (match.Stage == TournamentStage.QuarterFinals && hasSemiFinals)
                {
                    return new ApiResponse(false, "لا يمكن إعادة تعيين مباراة في دور الـ 8 بعد بدء نصف النهائي");
                }

                // منع إعادة تعيين مباريات نصف النهائي إذا بدأ النهائي
                if (match.Stage == TournamentStage.SemiFinals && hasFinal)
                {
                    return new ApiResponse(false, "لا يمكن إعادة تعيين مباراة في نصف النهائي بعد بدء النهائي");
                }

                // تحقق مما إذا كانت المباراة في دور إقصائي
                var isKnockoutStage = match.Stage == TournamentStage.QuarterFinals ||
                                     match.Stage == TournamentStage.SemiFinals ||
                                     match.Stage == TournamentStage.Final;

                if (isKnockoutStage)
                {
                    // في الأدوار الإقصائية، فقط أزل WinnerId وأعد تعيين النتيجة
                    match.WinnerId = null;
                    match.Score1 = 0;
                    match.Score2 = 0;
                    match.IsCompleted = false;

                    _unitOfWork.GetRepository<Match>().Update(match);
                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse(true, "تم إعادة تعيين المباراة الإقصائية بنجاح.");
                }
                else
                {
                    // في مرحلة المجموعات، استخدم النظام العادي
                    var player1 = await _unitOfWork.GetRepository<Player>()
                        .Find(player => player.PlayerId == match.Player1Id)
                        .FirstOrDefaultAsync();
                    var player2 = await _unitOfWork.GetRepository<Player>()
                        .Find(player => player.PlayerId == match.Player2Id)
                        .FirstOrDefaultAsync();

                    if (player1 == null || player2 == null)
                        return new ApiResponse(false, "All Players Must Be There");

                    // عكس احتساب النقاط بناءً على النتيجة السابقة
                    foreach (var round in match.Rounds)
                    {
                        switch (round.WinnerId)
                        {
                            case var winnerId when winnerId == player1.PlayerId:
                                player1.Wins--;
                                player1.Points--;
                                player2.Losses--;
                                break;
                            case var winnerId when winnerId == player2.PlayerId:
                                player2.Wins--;
                                player2.Points--;
                                player1.Losses--;
                                break;
                            case null when round.IsDraw:
                                player1.Draws--;
                                player2.Draws--;
                                player1.Points -= 0.5;
                                player2.Points -= 0.5;
                                break;
                        }
                    }

                    _unitOfWork.GetRepository<MatchRound>().DeleteRange(match.Rounds);
                    match.Score1 = 0;
                    match.Score2 = 0;
                    match.IsCompleted = false;

                    player1.UpdateStats();
                    player2.UpdateStats();

                    await _unitOfWork.SaveChangesAsync();
                    var leaguePlayers = await _unitOfWork.GetRepository<Player>().GetAll().Where(p => p.LeagueNumber == match.LeagueNumber).ToListAsync();
                    var leagueMatches = await _unitOfWork.GetRepository<Match>().GetAll().Where(m => m.LeagueNumber == match.LeagueNumber).ToListAsync();
                    var rankedPlayers = _playerRankingService.RankPlayers(leaguePlayers, leagueMatches);
                    foreach (var p in rankedPlayers) { var dbPlayer = leaguePlayers.First(x => x.PlayerId == p.PlayerId); dbPlayer.Rank = p.Rank; }
                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new ApiResponse(true, "تم إعادة تعيين الماتش من البداية.");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while resetting match {MatchId}", matchId);
                return new ApiResponse(false, "خطأ في قاعدة البيانات أثناء إعادة تعيين الماتش.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while resetting match {MatchId}", matchId);
                return new ApiResponse(false, $"حصل خطأ أثناء إعادة تعيين الماتش: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateMatchResultAsync(int matchId, MatchResultDto resultDto)
        {
            if (resultDto == null)
                return new ApiResponse(false, "بيانات نتيجة المباراة غير صالحة");

            try
            {
                using var transaction = await _unitOfWork.GetDbContext().Database.BeginTransactionAsync();
                var match = await _unitOfWork.GetRepository<Match>()
                    .Find(match => match.MatchId == matchId)
                    .FirstOrDefaultAsync();

                if (match == null)
                    return new ApiResponse(false, "No Match Here");

                var player1 = await _unitOfWork.GetRepository<Player>()
                    .Find(player => player.PlayerId == match.Player1Id)
                    .FirstOrDefaultAsync();
                var player2 = await _unitOfWork.GetRepository<Player>()
                    .Find(player => player.PlayerId == match.Player2Id)
                    .FirstOrDefaultAsync();

                var league = await _unitOfWork.GetRepository<LeagueId>()
                    .Find(x => x.IsFinished == false)
                    .FirstOrDefaultAsync();

                if (league == null)
                    return new ApiResponse(false, "الدوري لسه مبدأش");

                if (player1 == null || player2 == null)
                    return new ApiResponse(false, "All Players Must Be There");

                // تحقق مما إذا كانت المباراة في دور إقصائي
                var isKnockoutStage = match.Stage == TournamentStage.QuarterFinals ||
                                     match.Stage == TournamentStage.SemiFinals ||
                                     match.Stage == TournamentStage.Final;

                if (isKnockoutStage)
                {
                    // في الأدوار الإقصائية، لا نضيف نقاط للاعبين، فقط نسجل الفائز
                    if (resultDto.WinnerId == null)
                    {
                        return new ApiResponse(false, "لا يمكن أن تكون هناك تعادل في الأدوار الإقصائية");
                    }
                    else if (resultDto.WinnerId == match.Player1Id || resultDto.WinnerId == match.Player2Id)
                    {
                        var winner = resultDto.WinnerId == match.Player1Id ? player1 : player2;
                        var loser = resultDto.WinnerId == match.Player1Id ? player2 : player1;

                        // تسجيل الفائز في المباراة
                        match.WinnerId = winner!.PlayerId;
                        match.IsCompleted = true;

                        // تعيين النتيجة (1 للفائز، 0 للخاسر)
                        if (winner.PlayerId == match.Player1Id)
                        {
                            match.Score1 = 1;
                            match.Score2 = 0;
                        }
                        else
                        {
                            match.Score1 = 0;
                            match.Score2 = 1;
                        }

                        _unitOfWork.GetRepository<Match>().Update(match);
                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();


                        return new ApiResponse(true, $"تم تسجيل فوز {winner.FullName} في {match.Stage}");
                    }
                    else
                    {
                        return new ApiResponse(false, "Invalid winnerId. The winner must be one of the match players");
                    }
                }
                else
                {
                    // في مرحلة المجموعات، نستخدم النظام العادي
                    int matchCount = await _unitOfWork.GetRepository<MatchRound>()
                        .GetAll()
                        .Where(mr => mr.MatchId == matchId && !mr.IsDeleted)
                        .CountAsync();

                    if (matchCount >= _maxRoundsPerMatch)
                        return new ApiResponse(false, $"خلاص بقي هم لعبوا ال {_maxRoundsPerMatch} ماتشات والله");

                    var newRound = new MatchRound
                    {
                        MatchId = matchId,
                        LeagueNumber = league!.Id
                    };
                    string responseMessage;

                    if (resultDto.WinnerId == null)
                    {
                        newRound.IsDraw = true;
                        player1.Draws += 1;
                        player2.Draws += 1;
                        match.Score1 += 0.5;
                        match.Score2 += 0.5;
                        responseMessage = "تم اضافة نصف نقطة لكلا اللاعبين";
                    }
                    else if (resultDto.WinnerId == match.Player1Id || resultDto.WinnerId == match.Player2Id)
                    {
                        var winner = resultDto.WinnerId == match.Player1Id ? player1 : player2;
                        var loser = resultDto.WinnerId == match.Player1Id ? player2 : player1;

                        newRound.WinnerId = winner!.PlayerId;
                        winner.Wins += 1;
                        loser.Losses += 1;

                        if (winner.PlayerId == match.Player1Id)
                            match.Score1 += 1;
                        else
                            match.Score2 += 1;

                        responseMessage = $"تم اضافة نقطة للاعب : {winner.FullName}";
                    }
                    else
                    {
                        return new ApiResponse(false, "Invalid winnerId. The winner must be one of the match players");
                    }

                    await _unitOfWork.GetRepository<MatchRound>().AddAsync(newRound);
                    match.IsCompleted = matchCount + 1 == _maxRoundsPerMatch;
                    player1.UpdateStats();
                    player2.UpdateStats();

                    _unitOfWork.GetRepository<Player>().Update(player1);
                    _unitOfWork.GetRepository<Player>().Update(player2);
                    _unitOfWork.GetRepository<Match>().Update(match);

                    await _unitOfWork.SaveChangesAsync();
                    var leaguePlayers = await _unitOfWork.GetRepository<Player>().GetAll().Where(p => p.LeagueNumber == league.Id).ToListAsync();
                    var leagueMatches = await _unitOfWork.GetRepository<Match>().GetAll().Where(m => m.LeagueNumber == league.Id).ToListAsync();
                    var rankedPlayers = _playerRankingService.RankPlayers(leaguePlayers, leagueMatches);
                    foreach (var p in rankedPlayers) { var dbPlayer = leaguePlayers.First(x => x.PlayerId == p.PlayerId); dbPlayer.Rank = p.Rank; }
                    await _unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();


                    return new ApiResponse(true, responseMessage);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating match {MatchId}", matchId);
                return new ApiResponse(false, "خطأ في قاعدة البيانات أثناء تحديث الماتش.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating match {MatchId}", matchId);
                return new ApiResponse(false, $"Error updating match: {ex.Message}");
            }
        }



        //*****************************s
        private async Task<List<MatchViewModel>> GetMatchesByLeagueAsync(int leagueId)
        {
            return await _unitOfWork.GetRepository<Match>()
                .GetAll()
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => m.LeagueNumber == leagueId)
                .Select(m => new MatchViewModel
                {
                    MatchId = m.MatchId,
                    Score1 = m.Score1,
                    Score2 = m.Score2,
                    IsCompleted = m.IsCompleted,
                    Player1Name = m.Player1.FullName,
                    Player2Name = m.Player2.FullName,
                    Player1Id = m.Player1Id,
                    Player2Id = m.Player2Id,
                    TournamentStage = m.Stage.ToString()
                })
                .ToListAsync();
        }


    }
}