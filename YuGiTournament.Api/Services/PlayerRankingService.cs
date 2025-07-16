using System;
using System.Collections.Generic;
using System.Linq;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class PlayerRankingService : IPlayerRankingService
    {
        private const double Tolerance = 1e-6;
        public List<Player> RankPlayers(List<Player> players, List<Match> matches)
        {
            // نسخة من اللاعبين حتى لا نعدل القائمة الأصلية
            var rankedPlayers = players.Select(p => new Player
            {
                PlayerId = p.PlayerId,
                FullName = p.FullName,
                Wins = p.Wins,
                Losses = p.Losses,
                Draws = p.Draws,
                Points = p.Points,
                MatchesPlayed = p.MatchesPlayed,
                Rank = 0,
                WinRate = p.WinRate,
                LeagueNumber = p.LeagueNumber
            }).ToList();

            // ترتيب أولي حسب النقاط تنازليًا
            rankedPlayers = rankedPlayers.OrderByDescending(p => p.Points).ToList();

            int currentRank = 1;
            int i = 0;
            while (i < rankedPlayers.Count)
            {
                // حدد مجموعة اللاعبين المتعادلين في النقاط
                var samePointsGroup = rankedPlayers
                    .Skip(i)
                    .TakeWhile(p => p.Points == rankedPlayers[i].Points)
                    .ToList();

                if (samePointsGroup.Count == 1)
                {
                    rankedPlayers[i].Rank = currentRank;
                    currentRank++;
                    i++;
                    continue;
                }

                // لو أكثر من لاعب متعادلين في النقاط، طبق المواجهات المباشرة أولاً
                var headToHeadRanks = RankByHeadToHead(samePointsGroup, matches);

                // مجموعة نهائية سنبنيها بعد فض التعادل بالـ WinRate عند الحاجة
                var finalRanks = new List<Player>();

                // ابحث عن المجموعات التي ما زالت متعادلة بعد المواجهات المباشرة
                var headGroups = headToHeadRanks.GroupBy(p => headToHeadRanks.First(h => h.PlayerId == p.PlayerId).Rank);

                foreach (var hg in headGroups)
                {
                    var groupList = hg.ToList();
                    if (groupList.Count == 1)
                    {
                        finalRanks.AddRange(groupList);
                        continue;
                    }

                    // لو أكتر من لاعب بنفس الـ Rank بعد المواجهة المباشرة، اكسر التعادل بنسة الفوز
                    var winRateRanks = RankByWinRate(groupList);
                    finalRanks.AddRange(winRateRanks);
                }

                // عيّن الرانك النهائي بناءً على الترتيب داخل finalRanks مع مراعاة المتعادلين بعد كل الخطوات
                foreach (var group in finalRanks.GroupBy(p => p.Rank))
                {
                    foreach (var player in group)
                    {
                        rankedPlayers.First(p => p.PlayerId == player.PlayerId).Rank = currentRank;
                    }
                    currentRank += group.Count();
                }

                i += samePointsGroup.Count;
            }

            // ترتيب نهائي حسب الرانك
            // تأكد من أن اللاعبين المتعادلين تماماً في النقاط و WinRate يحصلون على نفس الرتبة
            for (int a = 0; a < rankedPlayers.Count; a++)
            {
                for (int b = a + 1; b < rankedPlayers.Count; b++)
                {
                    var p1 = rankedPlayers[a];
                    var p2 = rankedPlayers[b];

                    if (Math.Abs(p1.Points - p2.Points) <= Tolerance &&
                        Math.Abs(p1.WinRate - p2.WinRate) <= Tolerance)
                    {
                        var minRank = Math.Min(p1.Rank, p2.Rank);
                        p1.Rank = p2.Rank = minRank;
                    }
                }
            }

            return rankedPlayers.OrderBy(p => p.Rank).ToList();
        }

        private List<Player> RankByHeadToHead(List<Player> players, List<Match> matches)
        {
            if (players.Count <= 1)
                return players;

            // لكل لاعب، احسب مجموع نقاطه في المواجهات المباشرة مع المتعادلين فقط
            var headToHeadScores = players.ToDictionary(p => p.PlayerId, p => 0.0);
            foreach (var match in matches)
            {
                if (!players.Any(p => p.PlayerId == match.Player1Id) || !players.Any(p => p.PlayerId == match.Player2Id))
                    continue;
                headToHeadScores[match.Player1Id] += match.Score1;
                headToHeadScores[match.Player2Id] += match.Score2;
            }
            // رتب اللاعبين حسب مجموع نقاط المواجهات المباشرة
            var ordered = players.OrderByDescending(p => headToHeadScores[p.PlayerId]).ToList();

            // تحقق من وجود تعادل بعد المواجهات المباشرة
            double? prevScore = null;
            int rank = 1;
            int sameRankCount = 0;
            foreach (var player in ordered)
            {
                if (prevScore == null || Math.Abs(headToHeadScores[player.PlayerId] - prevScore.Value) > Tolerance)
                {
                    rank += sameRankCount;
                    player.Rank = rank;
                    sameRankCount = 1;
                }
                else
                {
                    player.Rank = rank;
                    sameRankCount++;
                }
                prevScore = headToHeadScores[player.PlayerId];
            }
            return ordered;
        }

        private List<Player> RankByWinRate(List<Player> players)
        {
            if (players.Count <= 1)
                return players;

            // رتب حسب WinRate
            var ordered = players.OrderByDescending(p => p.WinRate).ToList();
            double? prevRate = null;
            int rank = 1;
            int sameRankCount = 0;
            foreach (var player in ordered)
            {
                if (prevRate == null || Math.Abs(player.WinRate - prevRate.Value) > Tolerance)
                {
                    rank += sameRankCount;
                    player.Rank = rank;
                    sameRankCount = 1;
                }
                else
                {
                    player.Rank = rank;
                    sameRankCount++;
                }
                prevRate = player.WinRate;
            }
            return ordered;
        }
    }
}