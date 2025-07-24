using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services
{
    public class MatchServiceSelector : IMatchServiceSelector
    {
        private readonly IPointMatchService _pointMatchService;
        private readonly IClassicMatchService _classicMatchService;

        public MatchServiceSelector(IPointMatchService pointMatchService, IClassicMatchService classicMatchService)
        {
            _pointMatchService = pointMatchService;
            _classicMatchService = classicMatchService;
        }

        public object GetMatchService(SystemOfLeague systemOfLeague)
        {
            return systemOfLeague switch
            {
                SystemOfLeague.Points => _pointMatchService,
                SystemOfLeague.Classic => _classicMatchService,
                _ => _pointMatchService // fallback
            };
        }
    }
} 