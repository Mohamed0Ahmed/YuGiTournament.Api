using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.ApiResponses;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.DTOs;

namespace YuGiTournament.Api.Services
{
    public class LeagueResetService : ILeagueResetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeagueResetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LeagueResponse> GetCurrentLeague()
        {
            var league = await _unitOfWork.GetRepository<LeagueId>().Find(x => x.IsFinished == false).FirstOrDefaultAsync();

            if (league == null)
                return new LeagueResponse { Response = new ApiResponse(false, "لا يوجد دوري حاليا"), League = null };

            var response = new ApiResponse(true, "بيانات الدوري الحالي");
            return new LeagueResponse { Response = response, League = league };
        }


        public async Task<ApiResponse> ResetLeagueAsync(int leagueId)
        {
            var dbContext = _unitOfWork.GetDbContext();
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var league = _unitOfWork.GetRepository<LeagueId>().Find(x => x.Id == leagueId).FirstOrDefault();
                if (league == null)
                    return new ApiResponse(false, "الدوري ده مش موجود");

                if (league.IsFinished == true)
                    return new ApiResponse(false, "هذا الدوري منتهي بالفعل");
                else
                    league.IsFinished = true;

                _unitOfWork.GetRepository<LeagueId>().Update(league);

                // await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("DELETE FROM \"Messages\"");
                // await _unitOfWork.GetDbContext().Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"Messages_Id_seq\" RESTART WITH 1");

                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE \"Matches\" SET \"IsDeleted\" = TRUE WHERE \"LeagueNumber\" = {0}", leagueId);
                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE \"MatchRounds\" SET \"IsDeleted\" = TRUE WHERE \"LeagueNumber\" = {0}", leagueId);
                await dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE \"Players\" SET \"IsDeleted\" = TRUE WHERE \"LeagueNumber\" = {0}", leagueId);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new ApiResponse(false, "حدث خطأ اثناء انهاء الدوري");
            }

            return new ApiResponse(true, "تم انهاء الدوري");
        }

        public async Task<ApiResponse> DeleteLeague(int leagueId)
        {
            var dbContext = _unitOfWork.GetDbContext();
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var league = _unitOfWork.GetRepository<LeagueId>().Find(x => x.Id == leagueId).FirstOrDefault();
                if (league == null)
                    return new ApiResponse(false, "الدوري ده مش موجود");

                league.IsDeleted = true;

                _unitOfWork.GetRepository<LeagueId>().Update(league);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new ApiResponse(false, "حدث خطأ اثناء حذف الدوري");
            }

            return new ApiResponse(true, "تم حذف الدوري");
        }

        public async Task<ApiResponse> StartLeagueAsync(StartLeagueDto newLeague)
        {
            var existLeague = await _unitOfWork.GetRepository<LeagueId>()
                .Find(x => x.IsFinished == false)
                .FirstOrDefaultAsync();
            if (existLeague != null)
                return new ApiResponse(false, $"هناك دوري حالي بالفعل");

            var league = new LeagueId
            {
                Description = newLeague.Description,
                Name = newLeague.Name,
                TypeOfLeague = newLeague.TypeOfLeague,
                SystemOfLeague = newLeague.SystemOfLeague,
                IsFinished = false
            };

            var result = await _unitOfWork.GetRepository<LeagueId>().AddAsync(league);
            if (!result)
                return new ApiResponse(false, $"حدث خطأ اثناء حفظ الدوري");

            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, $"تم انشاء الدوري بنجاح");
        }

        public async Task<ApiResponse> CreateGroupsAndMatchesAsync(int leagueId)
        {
            // تحقق إذا كانت مباريات المجموعات قد أنشئت بالفعل
            var existingGroupMatches = await _unitOfWork.GetRepository<Match>()
                .Find(m => m.LeagueNumber == leagueId && m.Stage == TournamentStage.GroupStage && !m.IsDeleted)
                .AnyAsync();

            if (existingGroupMatches)
            {
                return new ApiResponse(false, "تم بالفعل بدء مرحلة المجموعات وإنشاء المباريات.");
            }

            // 1. جلب كل اللاعبين في الدوري المحدد
            var players = await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .Where(p => p.LeagueNumber == leagueId && !p.IsDeleted)
                .ToListAsync();

            if (players.Count != 16 && players.Count != 20)
                return new ApiResponse(false, "عدد اللاعبين يجب أن يكون 16 أو 20 فقط لبدء توزيع المجموعات.");

            // 2. توزيعهم عشوائيًا على 4 مجموعات
            var rnd = new Random();
            var shuffled = players.OrderBy(_ => rnd.Next()).ToList();
            int groupCount = 4;
            var groupSizes = players.Count == 16 ? [4, 4, 4, 4] : new[] { 5, 5, 5, 5 };

            int playerIndex = 0;
            for (int group = 1; group <= groupCount; group++)
            {
                for (int j = 0; j < groupSizes[group - 1]; j++)
                {
                    shuffled[playerIndex].GroupNumber = group;
                    _unitOfWork.GetRepository<Player>().Update(shuffled[playerIndex]);
                    playerIndex++;
                }
            }
            await _unitOfWork.SaveChangesAsync();

            // 3. إنشاء مباريات المجموعات (كل لاعب ضد كل لاعب في مجموعته)
            var matches = new List<Match>();
            for (int group = 1; group <= groupCount; group++)
            {
                var groupPlayers = shuffled.Where(p => p.GroupNumber == group).ToList();
                for (int i = 0; i < groupPlayers.Count; i++)
                {
                    for (int j = i + 1; j < groupPlayers.Count; j++)
                    {
                        matches.Add(new Match
                        {
                            Player1Id = groupPlayers[i].PlayerId,
                            Player2Id = groupPlayers[j].PlayerId,
                            LeagueNumber = leagueId,
                            Stage = TournamentStage.GroupStage
                        });
                    }
                }
            }

            if (matches.Any())
            {
                await _unitOfWork.GetRepository<Match>().AddRangeAsync(matches);
                await _unitOfWork.SaveChangesAsync();
            }

            return new ApiResponse(true, "تم توزيع اللاعبين على المجموعات وإنشاء مباريات دور المجموعات بنجاح.");
        }

        public async Task<ApiResponse> StartKnockoutStageAsync(int leagueId)
        {
            // Check if QuarterFinals already exist
            var existingQuarterFinals = await _unitOfWork.GetRepository<Match>()
                .Find(m => m.LeagueNumber == leagueId && m.Stage == TournamentStage.QuarterFinals && !m.IsDeleted)
                .AnyAsync();

            if (existingQuarterFinals)
            {
                return new ApiResponse(false, "تم بالفعل إنشاء مباريات دور الـ 8.");
            }

            var allGroupMatches = await _unitOfWork.GetRepository<Match>()
                .Find(m => m.LeagueNumber == leagueId && m.Stage == TournamentStage.GroupStage && !m.IsDeleted)
                .ToListAsync();

            if (allGroupMatches.Any(m => !m.IsCompleted))
                return new ApiResponse(false, "لا يمكن بدء مرحلة خروج المغلوب قبل انتهاء جميع مباريات المجموعات.");

            // 1. لكل مجموعة (1-4): احصل على أول 2 لاعبين
            var groupWinners = new Dictionary<int, List<Player>>();
            var allPlayers = await _unitOfWork.GetRepository<Player>()
                .GetAll()
                .Where(p => p.LeagueNumber == leagueId && !p.IsDeleted && p.GroupNumber != null)
                .ToListAsync();

            var rankingService = new PlayerRankingService();

            for (int group = 1; group <= 4; group++)
            {
                var groupPlayers = allPlayers.Where(p => p.GroupNumber == group).ToList();
                var groupPlayerIds = groupPlayers.Select(p => p.PlayerId).ToHashSet();
                var groupMatches = allGroupMatches.Where(m => groupPlayerIds.Contains(m.Player1Id) && groupPlayerIds.Contains(m.Player2Id)).ToList();
                var ranked = rankingService.RankPlayers(groupPlayers, groupMatches);
                groupWinners[group] = ranked.OrderBy(p => p.Rank).Take(2).ToList();
            }

            // التحقق من وجود لاعبين كافيين
            if (groupWinners.Values.Any(g => g.Count != 2))
                return new ApiResponse(false, "حدث خطأ في تحديد المتأهلين. يجب أن يكون هناك لاعبين من كل مجموعة.");

            // 2. إنشاء مباريات دور الـ 8 بنظام التصادم المباشر
            var quarterFinals = new List<Match>
            {
                // المباراة 1: الأول من المجموعة 1 vs الثاني من المجموعة 2
                new() {
                    Player1Id = groupWinners[1][0].PlayerId, // الأول من المجموعة 1
                    Player2Id = groupWinners[2][1].PlayerId, // الثاني من المجموعة 2
                    LeagueNumber = leagueId,
                    Stage = TournamentStage.QuarterFinals
                },

                // المباراة 2: الأول من المجموعة 2 vs الثاني من المجموعة 1
                new() {
                    Player1Id = groupWinners[2][0].PlayerId, // الأول من المجموعة 2
                    Player2Id = groupWinners[1][1].PlayerId, // الثاني من المجموعة 1
                    LeagueNumber = leagueId,
                    Stage = TournamentStage.QuarterFinals
                },

                // المباراة 3: الأول من المجموعة 3 vs الثاني من المجموعة 4
                new() {
                    Player1Id = groupWinners[3][0].PlayerId, // الأول من المجموعة 3
                    Player2Id = groupWinners[4][1].PlayerId, // الثاني من المجموعة 4
                    LeagueNumber = leagueId,
                    Stage = TournamentStage.QuarterFinals
                },

                // المباراة 4: الأول من المجموعة 4 vs الثاني من المجموعة 3
                new() {
                    Player1Id = groupWinners[4][0].PlayerId, // الأول من المجموعة 4
                    Player2Id = groupWinners[3][1].PlayerId, // الثاني من المجموعة 3
                    LeagueNumber = leagueId,
                    Stage = TournamentStage.QuarterFinals
                }
            };

            await _unitOfWork.GetRepository<Match>().AddRangeAsync(quarterFinals);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "تم إنشاء مباريات دور الـ 8 بنجاح.");
        }

        public async Task<ApiResponse> StartSemiFinalsAsync(int leagueId)
        {
            // Check if SemiFinals already exist
            var existingSemiFinals = await _unitOfWork.GetRepository<Match>()
                .Find(m => m.LeagueNumber == leagueId && m.Stage == TournamentStage.SemiFinals && !m.IsDeleted)
                .AnyAsync();

            if (existingSemiFinals)
            {
                return new ApiResponse(false, "تم بالفعل إنشاء مباريات نصف النهائي.");
            }

            // Get all completed QuarterFinals matches
            var quarterFinalMatches = await _unitOfWork.GetRepository<Match>()
                .Find(m => m.LeagueNumber == leagueId && m.Stage == TournamentStage.QuarterFinals && !m.IsDeleted)
                .ToListAsync();

            if (quarterFinalMatches.Count != 4 || quarterFinalMatches.Any(m => !m.IsCompleted))
                return new ApiResponse(false, "يجب أن تكتمل جميع مباريات دور الـ 8 (4 مباريات) أولاً.");

            // Get winners from each match using WinnerId
            var winners = quarterFinalMatches.Select(m => m.WinnerId ?? (m.Score1 > m.Score2 ? m.Player1Id : m.Player2Id)).ToList();
            if (winners.Count != 4)
                return new ApiResponse(false, "يجب أن يكون هناك 4 فائزين من دور الـ 8.");

            // الحصول على معلومات اللاعبين لمعرفة مجموعاتهم
            var winnerPlayers = await _unitOfWork.GetRepository<Player>()
                .Find(p => winners.Contains(p.PlayerId))
                .ToListAsync();

            // تنظيم الفائزين حسب المباريات (المباراة 1 و 3 في جانب، المباراة 2 و 4 في جانب آخر)
            var side1Winners = new List<int> { winners[0], winners[2] }; // فائز مباراة 1 و 3
            var side2Winners = new List<int> { winners[1], winners[3] }; // فائز مباراة 2 و 4

            // التحقق من عدم وجود لاعبين من نفس المجموعة في نفس الجانب
            var side1Players = winnerPlayers.Where(p => side1Winners.Contains(p.PlayerId)).ToList();
            var side2Players = winnerPlayers.Where(p => side2Winners.Contains(p.PlayerId)).ToList();

            // إذا كان هناك لاعبين من نفس المجموعة في نفس الجانب، نخلط الجانبين
            var side1Groups = side1Players.Select(p => p.GroupNumber).Distinct().Count();
            var side2Groups = side2Players.Select(p => p.GroupNumber).Distinct().Count();

            if (side1Groups < 2 || side2Groups < 2)
            {
                // خلط الفائزين لتجنب التقاء لاعبين من نفس المجموعة
                var rnd = new Random();
                var shuffled = winners.OrderBy(_ => rnd.Next()).ToList();

                side1Winners = [shuffled[0], shuffled[1]];
                side2Winners = [shuffled[2], shuffled[3]];
            }

            var semiFinals = new List<Match>
            {
                // المباراة 1: فائز مباراة 1 vs فائز مباراة 3
                new() {
                    Player1Id = side1Winners[0],
                    Player2Id = side1Winners[1],
                    LeagueNumber = leagueId,
                    Stage = TournamentStage.SemiFinals
                },

                // المباراة 2: فائز مباراة 2 vs فائز مباراة 4
                new() {
                    Player1Id = side2Winners[0],
                    Player2Id = side2Winners[1],
                    LeagueNumber = leagueId,
                    Stage = TournamentStage.SemiFinals
                }
            };

            await _unitOfWork.GetRepository<Match>().AddRangeAsync(semiFinals);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "تم إنشاء مباريات نصف النهائي بنجاح.");
        }

        public async Task<ApiResponse> StartFinalAsync(int leagueId)
        {
            // Check if Final already exists
            var existingFinal = await _unitOfWork.GetRepository<Match>()
                .Find(m => m.LeagueNumber == leagueId && m.Stage == TournamentStage.Final && !m.IsDeleted)
                .AnyAsync();

            if (existingFinal)
            {
                return new ApiResponse(false, "تم بالفعل إنشاء مباراة النهائي.");
            }

            // Get all completed SemiFinals matches
            var semiFinalMatches = await _unitOfWork.GetRepository<Match>()
                .Find(m => m.LeagueNumber == leagueId && m.Stage == TournamentStage.SemiFinals && !m.IsDeleted)
                .ToListAsync();

            if (semiFinalMatches.Count != 2 || semiFinalMatches.Any(m => !m.IsCompleted))
                return new ApiResponse(false, "يجب أن تكتمل جميع مباريات نصف النهائي (مباراتان) أولاً.");

            // Get winners from each match using WinnerId
            var winners = semiFinalMatches.Select(m => m.WinnerId ?? (m.Score1 > m.Score2 ? m.Player1Id : m.Player2Id)).ToList();
            if (winners.Count != 2)
                return new ApiResponse(false, "يجب أن يكون هناك 2 فائزين من نصف النهائي.");

            // Create the final match
            var finalMatch = new Match
            {
                Player1Id = winners[0],
                Player2Id = winners[1],
                LeagueNumber = leagueId,
                Stage = TournamentStage.Final
            };

            await _unitOfWork.GetRepository<Match>().AddAsync(finalMatch);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse(true, "تم إنشاء مباراة النهائي بنجاح.");
        }


    }
}

