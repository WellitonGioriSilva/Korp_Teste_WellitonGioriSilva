using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using faturamento_api.DTOs;
using faturamento_api.Models;

namespace faturamento_api.Profiles
{
    public class NotaFiscalProfile : Profile
    {
        public NotaFiscalProfile()
        {
            CreateMap<NotaFiscalCreateDTO, NotaFiscal>();
        }
    }
}