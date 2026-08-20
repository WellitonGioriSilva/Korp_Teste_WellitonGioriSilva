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
            CreateMap<ItemNotaFiscalCreateDTO, ItemNotaFiscal>();
            CreateMap<NotaFiscalCreateDTO, NotaFiscal>()
                .ForMember(dest => dest.Numero, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }
}
