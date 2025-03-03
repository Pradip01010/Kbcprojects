using AutoMapper;
using Kbcprojects.Entities;
using Kbcprojects.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GameSession, GameResultViewModel>(); // GameSession → GameResultViewModel
        CreateMap<GameResultViewModel, GameSession>(); // GameResultViewModel → GameSession
    }
}



