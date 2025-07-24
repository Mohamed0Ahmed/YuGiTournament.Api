using YuGiTournament.Api.Models;
using YuGiTournament.Api.Services.Abstractions;

namespace YuGiTournament.Api.Services.Abstractions
{
    public interface IMatchServiceSelector
    {
        object GetMatchService(SystemOfLeague systemOfLeague);
    }
} 