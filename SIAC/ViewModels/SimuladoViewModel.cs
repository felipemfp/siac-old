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
using SIAC.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SIAC.ViewModels
{
    public class SimuladoProvaViewModel
    {
        public Simulado Simulado { get; set; }
        public SimProva Prova { get; set; } = new SimProva();
        public List<Disciplina> Disciplinas { get; set; }

        public string DisciplinaProfessoresEmJson
        {
            get
            {
                Dictionary<int, IEnumerable> dict = new Dictionary<int, IEnumerable>();
                foreach (var disc in this.Disciplinas)
                {
                    dict[disc.CodDisciplina] = disc.Professor.Select(p => new
                    {
                        p.CodProfessor,
                        p.MatrProfessor,
                        p.Usuario.PessoaFisica.Nome
                    });
                }
                return JsonConvert.SerializeObject(dict);
            }
        }
    }

    public class SimuladoSalasViewModel
    {
        public Simulado Simulado { get; set; }
        public List<Campus> Campi { get; set; } = new List<Campus>();
        public List<Bloco> Blocos { get; set; } = new List<Bloco>();
        public List<Sala> Salas { get; set; } = new List<Sala>();

        public string BlocosEmJson => JsonConvert.SerializeObject(Blocos.Select(b => new
        {
            Campus = b.Campus.CodComposto,
            CodBloco = b.CodBloco,
            Descricao = b.Descricao,
            Sigla = b.Sigla,
            RefLocal = b.RefLocal,
            Observacao = b.Observacao
        }));

        public string SalasEmJson => JsonConvert.SerializeObject(Salas.Select(s => new
        {
            CodBloco = s.Bloco.CodBloco,
            CodSala = s.CodSala,
            Descricao = s.Descricao,
            Sigla = s.Sigla,
            RefLocal = s.RefLocal,
            Observacao = s.Observacao,
            Capacidade = s.Capacidade
        }));
    }

    public class SimuladoRespostasViewModel
    {
        public Simulado Simulado { get; set; }
        public List<SimProva> Provas { get; set; }
        public List<SimCandidato> Candidatos { get; set; }
    }

    public class SimuladoRespostasCandidatoViewModel
    {
        public Simulado Simulado { get; set; }
        public Candidato Candidato { get; set; }
        public SimProva Prova { get; set; }
        public List<SimProvaQuestao> Questoes { get; set; }
        public List<SimCandidatoQuestao> Respostas { get; set; }
        public SimCandidatoProva CandidatoProva { get; set; }
    }

    public class SimuladoClassificacaoViewModel
    {
        public Simulado Simulado { get; set; }
        public List<KeyValuePair<int, SimCandidato>> Classificacao { get; set; } = new List<KeyValuePair<int, SimCandidato>>();
        public List<SimCandidato> Faltosos { get; set; } = new List<SimCandidato>();
    }

    public class SimuladoPontuacoesViewModel
    {
        public Simulado Simulado { get; set; }
        public List<SimProva> Provas { get; set; }
        public List<SimCandidato> Candidatos { get; set; }
    }

    public class SimuladoPontuacoesCandidatoViewModel
    {
        public Simulado Simulado { get; set; }
        public Candidato Candidato { get; set; }
        public SimProva Prova { get; set; }
        public List<SimProvaQuestao> Questoes { get; set; }
        public List<SimCandidatoQuestao> Respostas { get; set; }
        public SimCandidatoProva CandidatoProva { get; set; }
    }
}