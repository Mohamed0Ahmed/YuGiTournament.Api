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
            // إعادة تعيين الرانك للجميع
            foreach (var player in players)
            {
                player.Rank = 0;
            }

            // تحقق مما إذا كانت البطولة بالمجموعات
            var hasGroups = players.Any(p => p.GroupNumber.HasValue);

            if (hasGroups)
            {
                // حساب الترتيب لكل مجموعة منفصلة
                return RankPlayersByGroups(players, matches);
            }
            else
            {
                // حساب الترتيب للبطولة العادية
                return RankPlayersNormally(players, matches);
            }
        }

        private List<Player> RankPlayersByGroups(List<Player> players, List<Match> matches)
        {
            var result = new List<Player>();

            // تجميع اللاعبين حسب المجموعات
            var groupedPlayers = players.GroupBy(p => p.GroupNumber).OrderBy(g => g.Key);

            foreach (var group in groupedPlayers)
            {
                var groupPlayers = group.ToList();
                var groupPlayerIds = groupPlayers.Select(p => p.PlayerId).ToHashSet();

                // فلترة المباريات لتشمل فقط مباريات هذه المجموعة
                var groupMatches = matches.Where(m =>
                    groupPlayerIds.Contains(m.Player1Id) &&
                    groupPlayerIds.Contains(m.Player2Id)).ToList();

                // حساب الترتيب للمجموعة الحالية
                var rankedGroupPlayers = RankPlayersNormally(groupPlayers, groupMatches);
                result.AddRange(rankedGroupPlayers);
            }

            return result;
        }

        private List<Player> RankPlayersNormally(List<Player> players, List<Match> matches)
        {
            // ترتيب أولي حسب النقاط تنازليًا
            var sortedPlayers = players.OrderByDescending(p => p.Points).ToList();

            int currentRank = 1;
            int i = 0;
            while (i < sortedPlayers.Count)
            {
                // حدد مجموعة اللاعبين المتعادلين في النقاط
                var samePointsGroup = sortedPlayers
                    .Skip(i)
                    .TakeWhile(p => p.Points == sortedPlayers[i].Points)
                    .ToList();

                if (samePointsGroup.Count == 1)
                {
                    sortedPlayers[i].Rank = currentRank;
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
                        sortedPlayers.First(p => p.PlayerId == player.PlayerId).Rank = currentRank;
                    }
                    currentRank += group.Count();
                }

                i += samePointsGroup.Count;
            }

            // ترتيب نهائي حسب الرانك
            return sortedPlayers.OrderBy(p => p.Rank).ToList();
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