using System.Collections.Generic;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IPlayerRankingService
    {
        /// <summary>
        /// يرتب اللاعبين في الدوري حسب النقاط، المواجهات المباشرة، ثم نسبة الفوز.
        /// </summary>
        /// <param name="players">قائمة اللاعبين في الدوري</param>
        /// <param name="matches">قائمة المباريات في الدوري</param>
        /// <returns>قائمة اللاعبين مرتبة مع تعيين الرانك الصحيح</returns>
        List<Player> RankPlayers(List<Player> players, List<Match> matches);
    }
}