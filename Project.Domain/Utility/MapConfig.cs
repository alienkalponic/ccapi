using AutoMapper;
using Project.Domain.Dto.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Utility
{
    public class MapConfig:Profile
    {
        public MapConfig()
        {
            CreateMap<LoginUserDetails, LoginUserDto>().ReverseMap();
        }
    }
}
