using Microsoft.AspNetCore.Mvc;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Models;
using YuGiTournament.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Abstractions;

namespace YuGiTournament.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly IMatchServiceSelector _matchServiceSelector;
        private readonly IUnitOfWork _unitOfWork;

        public MatchController(IMatchServiceSelector matchServiceSelector, IUnitOfWork unitOfWork)
        {
            _matchServiceSelector = matchServiceSelector;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Match>>> GetMatches()
        {
            var league = await _unitOfWork.GetRepository<LeagueId>()
                .Find(x => x.IsFinished == false)
                .FirstOrDefaultAsync();
            if (league == null)
                return Ok(new List<Match>());
            var service = _matchServiceSelector.GetMatchService(league.SystemOfLeague);
            var matches = await ((dynamic)service).GetAllMatchesAsync();
            return Ok(matches);
        }

        [HttpGet("matches/all")]
        public async Task<IActionResult> GetAllLeaguesMatches()
        {
            // سنستخدم النظام الحالي للدوري النشط فقط
            var league = await _unitOfWork.GetRepository<LeagueId>()
                .Find(x => x.IsDeleted == false)
                .FirstOrDefaultAsync();
            if (league == null)
                return Ok(new List<object>());
            var service = _matchServiceSelector.GetMatchService(league.SystemOfLeague);
            var ranking = await ((dynamic)service).GetAllLeaguesWithMatchesAsync();
            return Ok(ranking);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Match>> GetMatch(int id)
        {
            var match = await _unitOfWork.GetRepository<Match>().Find(m => m.MatchId == id).FirstOrDefaultAsync();
            if (match == null)
                return NotFound("Match not found.");
            var league = await _unitOfWork.GetRepository<LeagueId>().Find(l => l.Id == match.LeagueNumber).FirstOrDefaultAsync();
            if (league == null)
                return NotFound("League not found.");
            var service = _matchServiceSelector.GetMatchService(league.SystemOfLeague);
            var matchVm = await ((dynamic)service).GetMatchByIdAsync(id);
            return Ok(matchVm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{matchId}/result")]
        public async Task<IActionResult> UpdateMatchResult(int matchId, [FromBody] MatchResultDto resultDto)
        {
            var match = await _unitOfWork.GetRepository<Match>().Find(m => m.MatchId == matchId).FirstOrDefaultAsync();
            if (match == null)
                return NotFound("Match not found.");
            var league = await _unitOfWork.GetRepository<LeagueId>().Find(l => l.Id == match.LeagueNumber).FirstOrDefaultAsync();
            if (league == null)
                return NotFound("League not found.");
            var service = _matchServiceSelector.GetMatchService(league.SystemOfLeague);
            var result = await ((dynamic)service).UpdateMatchResultAsync(matchId, resultDto);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("reset/{matchId}")]
        public async Task<IActionResult> ResetMatches(int matchId)
        {
            var match = await _unitOfWork.GetRepository<Match>().Find(m => m.MatchId == matchId).FirstOrDefaultAsync();
            if (match == null)
                return NotFound("Match not found.");
            var league = await _unitOfWork.GetRepository<LeagueId>().Find(l => l.Id == match.LeagueNumber).FirstOrDefaultAsync();
            if (league == null)
                return NotFound("League not found.");
            var service = _matchServiceSelector.GetMatchService(league.SystemOfLeague);
            var result = await ((dynamic)service).ResetMatchByIdAsync(matchId);
            return Ok(result);
        }
    }
}
