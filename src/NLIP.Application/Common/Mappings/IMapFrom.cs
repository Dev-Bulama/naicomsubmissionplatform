using AutoMapper;

namespace NLIP.Application.Common.Mappings;

/// <summary>Implement on a DTO to declare "map me from T" — picked up automatically by
/// MappingProfile via reflection so individual profiles don't need to be hand-written per DTO.</summary>
public interface IMapFrom<T>
{
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}
