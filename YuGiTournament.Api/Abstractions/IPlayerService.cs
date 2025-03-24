using YuGiTournament.Api.DTOs;

namespace YuGiTournament.Api.Abstractions
{
    public interface IPlayerService
    {

        Task<PlayerDto> AddPlayer(string id);
        Task DeletePlayer(string id);
        
    }
}
