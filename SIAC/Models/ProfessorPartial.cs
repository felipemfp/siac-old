﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SIAC.Models
{
    public partial class Professor
    {
        [NotMapped]
        public Instituicao Instituicao => this.TurmaDiscProfHorario.OrderBy(t => t.AnoLetivo).LastOrDefault()?.Turma.Curso.Diretoria.Campus.Instituicao;

        [NotMapped]
        public Campus Campus => this.TurmaDiscProfHorario.OrderBy(t => t.AnoLetivo).LastOrDefault()?.Turma.Curso.Diretoria.Campus;

        public bool Leciona(int codDisciplina) => this.Disciplina.FirstOrDefault(d => d.CodDisciplina == codDisciplina) != null;

        private static Contexto contexto => Repositorio.GetInstance();

        public static Professor ListarPorMatricula(string matricula) => contexto.Professor.FirstOrDefault(p => p.MatrProfessor == matricula);

        public static Professor ListarPorCodigo(int codProfessor) => contexto.Professor.Find(codProfessor);

        public static void Inserir(Professor professor)
        {
            contexto.Professor.Add(professor);
            contexto.SaveChanges();
        }

        public static bool ProfessorLeciona(int codProfessor, int codDisciplina) =>
            (bool)contexto.Professor.Find(codProfessor)?.Leciona(codDisciplina);

        public static List<Disciplina> ObterDisciplinas(int codProfessor) => contexto.Professor.FirstOrDefault(p => p.CodProfessor == codProfessor)?.Disciplina.OrderBy(d => d.Descricao).ToList();

        public static List<Disciplina> ObterDisciplinas(string matrProfessor) => contexto.Professor.FirstOrDefault(p => p.MatrProfessor == matrProfessor)?.Disciplina.OrderBy(d => d.Descricao).ToList();

        public static List<Professor> ListarOrdenadamente() => contexto.Professor.OrderBy(p => p.Usuario.PessoaFisica.Nome).ToList();

        public override string ToString()
        {
            return this.Usuario.PessoaFisica.Nome;
        }
    }
}