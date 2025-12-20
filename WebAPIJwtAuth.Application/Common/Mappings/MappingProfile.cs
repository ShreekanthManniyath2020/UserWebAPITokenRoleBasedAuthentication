using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIJwtAuth.Application.DTOs;
using WebAPIJwtAuth.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAPIJwtAuth.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
        }
    }
}
