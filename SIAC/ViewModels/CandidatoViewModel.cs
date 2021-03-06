﻿/*
This file is part of SIAC.

Copyright (C) 2016 Felipe Mateus Freire Pontes <felipemfpontes@gmail.com>
Copyright (C) 2016 Francisco Bento da Silva Júnior <francisco.bento.jr@hotmail.com>

SIAC is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details. 
*/
using Newtonsoft.Json;
using SIAC.Helpers;
using SIAC.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIAC.ViewModels
{
    public class CandidatoIndexViewModel
    {
        public Candidato Candidato => Sessao.Candidato;
        public List<Simulado> Inscritos { get; set; } = new List<Simulado>();
        public List<Simulado> Passados { get; set; } = new List<Simulado>();
    }

    public class CandidatoAcessarViewModel
    {
        public string Cpf { get; set; }
        public string Senha { get; set; }
        public string Mensagem { get; set; }
        public bool TemMensagem => !String.IsNullOrEmpty(Mensagem);
    }

    public class CandidatoInscricoesViewModel
    {
        public const int QtePorPagina = 10;
        public List<Simulado> Simulados { get; set; }
        public int PaginaAtual { get; set; }
        public bool TemProxima { get; set; }
        public bool TemAnterior => PaginaAtual > 1;
    }

    public class CandidatoEsqueceuSenhaViewModel
    {
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Mensagem { get; set; }
        public bool TemMensagem => !String.IsNullOrEmpty(Mensagem);
    }

    public class CandidatoCadastrarViewModel
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string SenhaConfirmacao { get; set; }
        public string Mensagem { get; set; }
        public bool TemMensagem => !String.IsNullOrEmpty(Mensagem);
    }

    public class CandidatoAlterarSenhaViewModel
    {
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string Confirmacao { get; set; }
        public string Mensagem { get; set; }
        public bool TemMensagem => !String.IsNullOrEmpty(Mensagem);
        public bool Ok { get; set; }
    }

    public class CandidatoPerfilViewModel
    {
        public Candidato Candidato => Helpers.Sessao.Candidato;

        public string Nome { get; set; }
        public string Email { get; set; }

        public int RgNumero { get; set; }
        public string RgOrgao { get; set; }
        public DateTime RgDtExpedicao { get; set; }

        public DateTime? DtNascimento { get; set; }
        public string Sexo { get; set; }

        public string Matricula { get; set; }

        public string TelefoneFixo { get; set; }
        public string TelefoneCelular { get; set; }

        public int Pais { get; set; }
        public int Estado { get; set; }
        public int Municipio { get; set; }

        public bool Adventista { get; set; }

        public bool NecessidadeEspecial { get; set; }
        public string DescricaoNecessidadeEspecial { get; set; }

        public string Mensagem { get; set; }
        public List<Pais> Paises { get; set; } = new List<Pais>();
        public List<Estado> Estados { get; set; } = new List<Estado>();
        public List<Municipio> Municipios { get; set; } = new List<Municipio>();

        public string EstadosEmJson => JsonConvert.SerializeObject(Estados.Select(e => new
        {
            Pais = e.CodPais,
            Estado = e.CodEstado,
            Descricao = e.Descricao,
            Sigla = e.Sigla
        }));

        public string MunicipiosEmJson => JsonConvert.SerializeObject(Municipios.Select(m => new
        {
            Estado = m.CodEstado,
            Municipio = m.CodMunicipio,
            Descricao = m.Descricao
        }));
    }
}