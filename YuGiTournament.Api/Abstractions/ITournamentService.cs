namespace YuGiTournament.Api.Abstractions
{
    public interface ITournamentService
    {

        Task StartTournament();
        Task EndTournament();
    }
}

