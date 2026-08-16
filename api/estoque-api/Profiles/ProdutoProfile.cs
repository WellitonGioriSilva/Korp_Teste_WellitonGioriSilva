using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using estoque_api.DTOs;
using estoque_api.Models;
using AutoMapper;

namespace estoque_api.Profiles
{
    public class ProdutoProfile : Profile
    {
        public ProdutoProfile()
        {
            CreateMap<ProdutoCreateDTO, Produto>();
            CreateMap<ProdutoUpdateDTO, Produto>();
        }
    }
}