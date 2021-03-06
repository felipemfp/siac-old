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
using SIAC.Helpers;
using SIAC.Models;
using SIAC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SIAC.Controllers
{
    [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.PROFESSOR, Categoria.COLABORADOR }, SomenteOcupacaoSimulado = true)]
    public class GerenciaController : Controller
    {
        // GET: simulado/gerencia
        public ActionResult Index() => View();

        // GET: simulado/gerencia/dados
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult Dados() => View();

        #region Blocos

        // GET: simulado/gerencia/blocos
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult Blocos()
        {
            var viewModel = new GerenciaBlocosViewModel();
            viewModel.Campi = Campus.ListarOrdenadamente();
            viewModel.Blocos = Bloco.ListarOrdenadamente();

            return View(viewModel);
        }

        // POST: simulado/gerencia/novobloco
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult NovoBloco(FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar cadastrar um novo bloco.";

            if (form.HasKeys())
            {
                string campusCodComposto = form["ddlCampus"].Trim();
                string descricao = form["txtDescricao"].Trim();
                string sigla = form["txtSigla"].Trim();
                string refLocal = form["txtRefLocal"].Trim();
                string observacao = form["txtObservacao"].Trim();
                if (!StringExt.IsNullOrWhiteSpace(campusCodComposto, descricao, sigla))
                {
                    var bloco = new Bloco();
                    bloco.Campus = Campus.ListarPorCodigo(campusCodComposto);
                    bloco.Descricao = descricao;
                    bloco.Sigla = String.IsNullOrWhiteSpace(sigla) ? null : sigla;
                    bloco.RefLocal = String.IsNullOrWhiteSpace(refLocal) ? null : refLocal;
                    bloco.Observacao = String.IsNullOrWhiteSpace(observacao) ? null : observacao;

                    Bloco.Inserir(bloco);

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Novo bloco \"{bloco.Descricao}\" cadastrado com sucesso.";
                }
                else
                {
                    mensagem = "É necessário Campus, Descrição e Sigla para cadastrar um novo bloco.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Blocos");
        }

        // POST: simulado/gerencia/carregarbloco
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult CarregarBloco(int bloco)
        {
            var viewModel = new GerenciaEditarBlocoViewModel();
            viewModel.Campi = Campus.ListarOrdenadamente();
            viewModel.Bloco = Bloco.ListarPorCodigo(bloco);

            return PartialView("_CarregarBloco", viewModel);
        }

        // POST: simulado/gerencia/editarbloco
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult EditarBloco(int codigo, FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar editar um bloco.";

            Bloco bloco = Bloco.ListarPorCodigo(codigo);

            if (bloco != null && form.HasKeys())
            {
                string campusCodComposto = form["ddlCampus"].Trim();
                string descricao = form["txtDescricao"].Trim();
                string sigla = form["txtSigla"].Trim();
                string refLocal = form["txtRefLocal"].Trim();
                string observacao = form["txtObservacao"].Trim();

                if (!StringExt.IsNullOrWhiteSpace(campusCodComposto, descricao, sigla))
                {
                    bloco.Campus = Campus.ListarPorCodigo(campusCodComposto);
                    bloco.Descricao = descricao;
                    bloco.Sigla = String.IsNullOrWhiteSpace(sigla) ? null : sigla;
                    bloco.RefLocal = String.IsNullOrWhiteSpace(refLocal) ? null : refLocal;
                    bloco.Observacao = String.IsNullOrWhiteSpace(observacao) ? null : observacao;

                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Bloco \"{bloco.Descricao}\" editado com sucesso.";
                }
                else
                {
                    mensagem = "É necessário Campus, Descrição e Sigla para editar um bloco.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Blocos");
        }

        // POST: simulado/gerencia/excluirbloco
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public void ExcluirBloco(int codigo)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar excluir um bloco.";

            Bloco bloco = Bloco.ListarPorCodigo(codigo);

            if (bloco != null)
            {
                if (bloco.Sala.Count == 0)
                {
                    Repositorio.GetInstance().Bloco.Remove(bloco);
                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Bloco \"{bloco.Descricao}\" excluído com sucesso.";
                }
                else
                {
                    lembrete = Lembrete.NEGATIVO;
                    mensagem = $"É necessário excluir primeiro as salas do Bloco \"{bloco.Descricao}\". Este bloco contém {bloco.Sala.Count} salas relacionadas.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
        }

        #endregion Blocos

        #region Salas

        // GET: simulado/gerencia/salas
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult Salas()
        {
            var viewModel = new GerenciaSalasViewModel();
            viewModel.Campi = Campus.ListarOrdenadamente();
            viewModel.Blocos = Bloco.ListarOrdenadamente();
            viewModel.Salas = Sala.ListarOrdenadamente();

            return View(viewModel);
        }

        // POST: simulado/gerencia/novasala
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult NovaSala(FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar cadastrar uma nova sala.";

            if (form.HasKeys())
            {
                string bloco = form["ddlBloco"].Trim();
                string descricao = form["txtDescricao"].Trim();
                string sigla = form["txtSigla"].Trim();
                string capacidade = form["txtCapacidade"].Trim();
                string refLocal = form["txtRefLocal"].Trim();
                string observacao = form["txtObservacao"].Trim();
                if (!StringExt.IsNullOrWhiteSpace(bloco, descricao, sigla, capacidade))
                {
                    var sala = new Sala();
                    sala.Bloco = Bloco.ListarPorCodigo(int.Parse(bloco));
                    sala.Descricao = descricao;
                    sala.Sigla = sigla;
                    sala.Capacidade = int.Parse(capacidade);
                    sala.RefLocal = String.IsNullOrWhiteSpace(refLocal) ? null : refLocal;
                    sala.Observacao = String.IsNullOrWhiteSpace(observacao) ? null : observacao;

                    Sala.Inserir(sala);

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Novo sala \"{sala.Descricao}\" cadastrada com sucesso.";
                }
                else
                {
                    mensagem = "É necessário Bloco, Descrição, Sigla e Capacidade para cadastrar uma nova sala.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Salas");
        }

        // POST: simulado/gerencia/carregarsala
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult CarregarSala(int sala)
        {
            var viewModel = new GerenciaEditarSalaViewModel();
            viewModel.Campi = Campus.ListarOrdenadamente();
            viewModel.Blocos = Bloco.ListarOrdenadamente();
            viewModel.Sala = Sala.ListarPorCodigo(sala);

            return PartialView("_CarregarSala", viewModel);
        }

        // POST: simulado/gerencia/editarsala
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult EditarSala(int codigo, FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar editar uma sala.";

            Sala sala = Sala.ListarPorCodigo(codigo);

            if (sala != null && form.HasKeys())
            {
                string bloco = form["ddlBloco"].Trim();
                string descricao = form["txtDescricao"].Trim();
                string sigla = form["txtSigla"].Trim();
                string capacidade = form["txtCapacidade"].Trim();
                string refLocal = form["txtRefLocal"].Trim();
                string observacao = form["txtObservacao"].Trim();
                if (!StringExt.IsNullOrWhiteSpace(bloco, descricao, sigla, capacidade))
                {
                    sala.Bloco = Bloco.ListarPorCodigo(int.Parse(bloco));
                    sala.Descricao = descricao;
                    sala.Sigla = sigla;
                    sala.Capacidade = int.Parse(capacidade);
                    sala.RefLocal = String.IsNullOrWhiteSpace(refLocal) ? null : refLocal;
                    sala.Observacao = String.IsNullOrWhiteSpace(observacao) ? null : observacao;

                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Sala \"{sala.Descricao}\" editada com sucesso.";
                }
                else
                {
                    mensagem = "É necessário Bloco, Descrição, Sigla e Capacidade para editar uma sala.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Salas");
        }

        // POST: simulado/gerencia/excluirsala
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public void ExcluirSala(int codigo)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar excluir uma sala.";

            Sala sala = Sala.ListarPorCodigo(codigo);

            if (sala != null)
            {
                Repositorio.GetInstance().Sala.Remove(sala);
                Repositorio.Commit();

                lembrete = Lembrete.POSITIVO;
                mensagem = $"Sala \"{sala.Descricao}\" excluída com sucesso.";
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
        }

        #endregion Salas

        #region Disciplinas

        // GET: simulado/gerencia/disciplinas
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult Disciplinas() => View(new GerenciaDisciplinasViewModel()
        {
            Disciplinas = Disciplina.ListarOrdenadamente()
        });

        // POST: simulado/gerencia/novadisciplina
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult NovaDisciplina(FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar cadastrar uma nova disciplina.";

            if (form.HasKeys())
            {
                string descricao = form["txtDescricao"].Trim();
                string sigla = form["txtSigla"].Trim();
                if (!StringExt.IsNullOrWhiteSpace(descricao, sigla))
                {
                    var disciplina = new Disciplina()
                    {
                        Descricao = descricao,
                        Sigla = sigla,
                        Tema = new List<Tema> {
                            new Tema()
                            {
                                CodTema = 1,
                                Descricao = "Simulado"
                            }
                        }
                    };

                    Disciplina.Inserir(disciplina);

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Nova disciplina \"{disciplina.Descricao}\" cadastrada com sucesso.";
                }
                else
                {
                    mensagem = "É necessário Descrição e Sigla para cadastrar uma nova disciplina.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Disciplinas");
        }

        // POST: simulado/gerencia/carregardisciplina
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult CarregarDisciplina(int disciplina) => PartialView("_CarregarDisciplina", Disciplina.ListarPorCodigo(disciplina));

        // POST: simulado/gerencia/editardisciplina
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult EditarDisciplina(int codigo, FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar editar uma disciplina.";

            Disciplina disciplina = Disciplina.ListarPorCodigo(codigo);

            if (disciplina != null && form.HasKeys())
            {
                string descricao = form["txtDescricao"].Trim();
                string sigla = form["txtSigla"].Trim();

                if (!StringExt.IsNullOrWhiteSpace(descricao, sigla))
                {
                    disciplina.Descricao = descricao;
                    disciplina.Sigla = sigla;

                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Disciplina \"{disciplina.Descricao}\" editada com sucesso.";
                }
                else
                {
                    mensagem = "É necessário Descrição e Sigla para editar uma disciplina.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Disciplinas");
        }

        // POST: simulado/gerencia/excluirdisciplina
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public void ExcluirDisciplina(int codigo)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar excluir uma disciplina.";

            Disciplina disciplina = Disciplina.ListarPorCodigo(codigo);

            if (disciplina != null)
            {
                if (disciplina.Tema.Count == 0 || (disciplina.Tema.Count == 1 && disciplina.Tema.First().Descricao.ToLower() == "simulado"))
                {
                    Repositorio.GetInstance().Disciplina.Remove(disciplina);
                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Disciplina \"{disciplina.Descricao}\" excluída com sucesso.";
                }
                else
                {
                    lembrete = Lembrete.NEGATIVO;
                    mensagem = $"É necessário excluir primeiro os temas da disciplina \"{disciplina.Descricao}\". Esta disciplina contém {disciplina.Tema.Count} temas relacionados.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
        }

        #endregion Disciplinas

        #region Professores

        // GET: simulado/gerencia/professores
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult Professores() => View(new GerenciaProfessoresViewModel()
        {
            Professores = Professor.ListarOrdenadamente(),
            Disciplinas = Disciplina.ListarOrdenadamente()
        });

        // POST: simulado/gerencia/novoprofessor
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult NovoProfessor(FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar cadastrar um(a) novo(a) professor(a).";

            if (form.HasKeys())
            {
                string nome = form["txtNome"].Trim();
                string matricula = form["txtMatricula"].Trim();
                string senha = form["txtSenha"];
                string senhaConfirmacao = form["txtSenhaConfirmacao"];

                string[] disciplinas = form["ddlDisciplina"]?.Split(',');
                if (!StringExt.IsNullOrWhiteSpace(nome, matricula, senha, senha) && disciplinas?.Length > 0)
                {
                    if (Usuario.ListarPorMatricula(matricula) == null)
                    {
                        if (senha == senhaConfirmacao)
                        {
                            int codPessoa = Pessoa.Inserir(new Pessoa() { TipoPessoa = Pessoa.FISICA });

                            var pf = new PessoaFisica();
                            pf.CodPessoa = codPessoa;
                            pf.Nome = nome;
                            pf.Categoria.Add(Categoria.ListarPorCodigo(Categoria.PROFESSOR));

                            int codPessoaFisica = PessoaFisica.Inserir(pf);

                            var usuario = new Usuario();
                            usuario.Matricula = matricula;
                            usuario.CodPessoaFisica = codPessoaFisica;
                            usuario.CodCategoria = Categoria.PROFESSOR;
                            usuario.Senha = Criptografia.RetornarHash(senhaConfirmacao);

                            int codUsuario = Usuario.Inserir(usuario);

                            var professor = new Professor();
                            professor.MatrProfessor = usuario.Matricula;

                            foreach (var disciplina in disciplinas)
                            {
                                professor.Disciplina.Add(Disciplina.ListarPorCodigo(int.Parse(disciplina)));
                            }

                            Professor.Inserir(professor);

                            lembrete = Lembrete.POSITIVO;
                            mensagem = $"Novo(a) professor(a) \"{pf.Nome}\" cadastrado(a) com sucesso.";
                        }
                        else
                        {
                            mensagem = "A Senha informada deve ser igual à Confirmação da Senha.";
                        }
                    }
                    else
                    {
                        mensagem = $"Já existe um usuário com a matrícula \"{matricula}\".";
                    }
                }
                else
                {
                    mensagem = "Todos os campos são necessário para cadastrar um(a) novo(a) professor(a).";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Professores");
        }

        // POST: gerencia/carregarprofessor
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult CarregarProfessor(string professor) => PartialView("_CarregarProfessor", new GerenciaEditarProfessorViewModel()
        {
            Professor = Professor.ListarPorMatricula(professor),
            Disciplinas = Disciplina.ListarOrdenadamente()
        });

        // POST: simulado/gerencia/carregarprofessordisciplinas
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult CarregarProfessorDisciplinas(string professor) => PartialView("_CarregarProfessorDisciplinas", Professor.ListarPorMatricula(professor));

        // POST: simulado/gerencia/editarprofessor
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult EditarProfessor(string codigo, FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar editar um(a) professor(a).";

            Professor professor = Professor.ListarPorMatricula(codigo);

            if (professor != null && form.HasKeys())
            {
                string nome = form["txtNome"].Trim();
                string[] disciplinas = String.IsNullOrEmpty(form["ddlDisciplina"]) ? new string[0] : form["ddlDisciplina"].Split(',');

                if (!StringExt.IsNullOrWhiteSpace(nome))
                {
                    professor.Usuario.PessoaFisica.Nome = nome;

                    professor.Disciplina.Clear();

                    foreach (var disciplina in disciplinas)
                    {
                        professor.Disciplina.Add(Disciplina.ListarPorCodigo(int.Parse(disciplina)));
                    }

                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Professor(a) \"{nome}\" editado(a) com sucesso.";
                }
                else
                {
                    mensagem = "Todos os campos são necessário para editar um(a) professor(a).";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Professores");
        }

        // POST: simulado/gerencia/excluirprofessor
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public void ExcluirProfessor(string codigo)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar excluir um(a) professor(a).";

            Professor professor = Professor.ListarPorMatricula(codigo);

            if (professor != null)
            {
                try
                {
                    Repositorio.GetInstance().Professor.Remove(professor);
                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Professor(a) \"{professor.Usuario.PessoaFisica.Nome}\" excluído(a) com sucesso.";
                }
                catch
                {
                    Repositorio.Dispose();
                    Repositorio.Commit();

                    lembrete = Lembrete.NEGATIVO;
                    mensagem = $"Não é possível excluir este(a) professor(a).";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
        }

        #endregion Professores

        #region Colaboradores

        // GET: simulado/gerencia/colaboradores
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult Colaboradores() => View(new GerenciaColaboradoresViewModel()
        {
            Colaboradores = Colaborador.ListarOrdenadamente(),
            Ocupacoes = new List<Ocupacao>()
            {
                Ocupacao.ListarPorCodigo(Ocupacao.COORDENADOR_SIMULADO),
                Ocupacao.ListarPorCodigo(Ocupacao.COLABORADOR_SIMULADO)
            }
        });

        // POST: simulado/gerencia/novocolaborador
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult NovoColaborador(FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar cadastrar um(a) novo(a) professor(a).";

            if (form.HasKeys())
            {
                string nome = form["txtNome"].Trim();
                string matricula = form["txtMatricula"].Trim();
                string senha = form["txtSenha"];
                string senhaConfirmacao = form["txtSenhaConfirmacao"];
                string strOcupacao = form["ddlOcupacao"];

                int ocupacao = -1;

                int.TryParse(strOcupacao, out ocupacao);

                if (!StringExt.IsNullOrWhiteSpace(nome, matricula, senha, senha))
                {
                    if (Usuario.ListarPorMatricula(matricula) == null)
                    {
                        if (senha == senhaConfirmacao)
                        {
                            int codPessoa = Pessoa.Inserir(new Pessoa() { TipoPessoa = Pessoa.FISICA });

                            var pf = new PessoaFisica();
                            pf.CodPessoa = codPessoa;
                            pf.Nome = nome;
                            pf.Categoria.Add(Categoria.ListarPorCodigo(Categoria.COLABORADOR));

                            int codPessoaFisica = PessoaFisica.Inserir(pf);

                            var usuario = new Usuario();
                            usuario.Matricula = matricula;
                            usuario.CodPessoaFisica = codPessoaFisica;
                            usuario.CodCategoria = Categoria.COLABORADOR;
                            usuario.Senha = Criptografia.RetornarHash(senhaConfirmacao);

                            int codUsuario = Usuario.Inserir(usuario);

                            var colaborador = new Colaborador();
                            colaborador.MatrColaborador = usuario.Matricula;

                            Colaborador.Inserir(colaborador);

                            if (new[] { Ocupacao.COLABORADOR_SIMULADO, Ocupacao.COORDENADOR_SIMULADO }.Contains(ocupacao))
                            {
                                PessoaFisica.AdicionarOcupacao(codPessoaFisica, ocupacao);
                            }

                            lembrete = Lembrete.POSITIVO;
                            mensagem = $"Novo(a) colaborador(a) \"{pf.Nome}\" cadastrado(a) com sucesso.";
                        }
                        else
                        {
                            mensagem = "A Senha informada deve ser igual à Confirmação da Senha.";
                        }
                    }
                    else
                    {
                        mensagem = $"Já existe um usuário com a matrícula \"{matricula}\".";
                    }
                }
                else
                {
                    mensagem = "Todos os campos são necessário para cadastrar um(a) novo(a) colaborador(a).";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Colaboradores");
        }

        // POST: gerencia/carregarcolaborador
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult CarregarColaborador(string colaborador) => PartialView("_CarregarColaborador", new GerenciaEditarColaboradorViewModel()
        {
            Colaborador = Colaborador.ListarPorMatricula(colaborador)
        });

        // POST: simulado/gerencia/editarcolaborador
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult EditarColaborador(string codigo, FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar editar um(a) colaborador(a).";

            Colaborador colaborador = Colaborador.ListarPorMatricula(codigo);

            if (colaborador != null && form.HasKeys())
            {
                string nome = form["txtNome"].Trim();

                if (!StringExt.IsNullOrWhiteSpace(nome))
                {
                    colaborador.Usuario.PessoaFisica.Nome = nome;

                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Colaborador(a) \"{nome}\" editado(a) com sucesso.";
                }
                else
                {
                    mensagem = "Todos os campos são necessário para editar um(a) colaborador(a).";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Colaboradores");
        }

        // POST: simulado/gerencia/excluircolaborador
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public void ExcluirColaborador(string codigo)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar excluir um(a) colaborador(a).";

            Colaborador colaborador = Colaborador.ListarPorMatricula(codigo);

            if (colaborador != null)
            {
                try
                {
                    if (colaborador.AvalAvi.Count > 0)
                        mensagem = $"Não é possível excluir este(a) colaborador(a), pois ele tem vínculo com Avaliação Institucional.";
                    else if (colaborador.Diretoria.Count > 0)
                        mensagem = $"Não é possível excluir este(a) colaborador(a), pois ele tem vínculo com Diretoria.";
                    else if (colaborador.Campus.Count > 0)
                        mensagem = $"Não é possível excluir este(a) colaborador(a), pois ele tem vínculo com Campus.";
                    else if (colaborador.ProReitoria.Count > 0)
                        mensagem = $"Não é possível excluir este(a) colaborador(a), pois ele tem vínculo com Pró-Reitoria.";
                    else if (colaborador.Reitoria.Count > 0)
                        mensagem = $"Não é possível excluir este(a) colaborador(a), pois ele tem vínculo com Reitoria.";
                    else if (colaborador.Simulado.Count > 0)
                        mensagem = $"Não é possível excluir este(a) colaborador(a), pois ele tem vínculo com Simulado.";
                    else if (colaborador.Curso.Count > 0)
                        mensagem = $"Não é possível excluir este(a) colaborador(a), pois ele tem vínculo com Curso.";
                    else
                    {
                        string nome = colaborador.Usuario.PessoaFisica.Nome;
                        Colaborador.Remover(colaborador);
                        Repositorio.Commit();

                        lembrete = Lembrete.POSITIVO;
                        mensagem = $"Colaborador(a) \"{nome}\" excluído(a) com sucesso.";
                    }
                }
                catch
                {
                    Repositorio.Dispose();
                    Repositorio.Commit();

                    lembrete = Lembrete.NEGATIVO;
                    mensagem = $"Não é possível excluir este(a) colaborador(a).";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
        }

        #endregion Colaboradores

        #region Campi

        // GET: simulado/gerencia/campi
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult Campi() => View(new GerenciaCampiViewModel()
        {
            Campi = Campus.ListarOrdenadamente(),
            Instituicoes = Instituicao.ListarOrdenadamente(),
            Colaboradores = Colaborador.ListarOrdenadamente()
        });

        // POST: simulado/gerencia/novocampus
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult NovoCampus(FormCollection form)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar cadastrar um novo campus.";

            if (form.HasKeys())
            {
                string razaoSocial = form["txtRazaoSocial"];
                string nomeFantasia = form["txtNomeFantasia"];
                string portal = form["txtPortal"];
                string sigla = form["txtSigla"];
                string strCodInstituicao = form["ddlInstituicao"];
                string strCodDiretor = form["ddlDiretor"];

                int codInstituicao = -1;
                int codDiretor = -1;

                int.TryParse(strCodInstituicao, out codInstituicao);
                int.TryParse(strCodDiretor, out codDiretor);

                if (!StringExt.IsNullOrWhiteSpace(razaoSocial, nomeFantasia, portal, sigla) && codInstituicao > -1 && codDiretor > -1)
                {
                    var campus = new Campus();
                    campus.PessoaJuridica = new PessoaJuridica();
                    campus.PessoaJuridica.Pessoa = new Pessoa();

                    //Pessoa
                    campus.PessoaJuridica.Pessoa.TipoPessoa = Pessoa.JURIDICA;

                    //PessoaJuridica
                    campus.PessoaJuridica.RazaoSocial = razaoSocial;
                    campus.PessoaJuridica.NomeFantasia = nomeFantasia;
                    campus.PessoaJuridica.Portal = portal;

                    //Campus
                    campus.CodInstituicao = codInstituicao;
                    campus.Colaborador = Colaborador.ListarPorCodigo(codDiretor);
                    campus.Sigla = sigla;

                    Campus.Inserir(campus);
                    PessoaFisica.AdicionarOcupacao(campus.Colaborador.Usuario.CodPessoaFisica, Ocupacao.DIRETOR_GERAL);

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Novo campus \"{nomeFantasia}\" cadastrado com sucesso.";
                }
                else
                {
                    mensagem = "Todos os campos são necessário para cadastrar um novo campus.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Campi");
        }

        // POST: gerencia/carregarcampus
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult CarregarCampus(string campus) => PartialView("_CarregarCampus", new GerenciaEditarCampusViewModel()
        {
            Campus = Campus.ListarPorCodigo(campus),
            Instituicoes = Instituicao.ListarOrdenadamente(),
            Colaboradores = Colaborador.ListarOrdenadamente()
        });

        // POST: simulado/gerencia/editarcampus
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public ActionResult EditarCampus(string codigo, FormCollection form)
        {
            codigo = codigo.Replace('-', '.');
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar editar um campus.";

            Campus campus = Campus.ListarPorCodigo(codigo);

            if (campus != null && form.HasKeys())
            {
                string razaoSocial = form["txtRazaoSocial"];
                string nomeFantasia = form["txtNomeFantasia"];
                string portal = form["txtPortal"];
                string sigla = form["txtSigla"];
                string strCodInstituicao = form["ddlInstituicao"];
                string strCodDiretor = form["ddlDiretor"];

                int codInstituicao = -1;
                int codDiretor = -1;

                int.TryParse(strCodInstituicao, out codInstituicao);
                int.TryParse(strCodDiretor, out codDiretor);

                if (!StringExt.IsNullOrWhiteSpace(razaoSocial, nomeFantasia, portal, sigla) && codInstituicao > -1 && codDiretor > -1)
                {
                    //PessoaJuridica
                    campus.PessoaJuridica.RazaoSocial = razaoSocial;
                    campus.PessoaJuridica.NomeFantasia = nomeFantasia;
                    campus.PessoaJuridica.Portal = portal;

                    //Campus
                    campus.CodInstituicao = codInstituicao;
                    campus.Sigla = sigla;

                    Campus.TrocarDiretor(campus, codDiretor);

                    Repositorio.Commit();

                    lembrete = Lembrete.POSITIVO;
                    mensagem = $"Campus \"{nomeFantasia}\" editado com sucesso.";
                }
                else
                {
                    mensagem = "Todos os campos são necessário para editar um campus.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
            return RedirectToAction("Campi");
        }

        // POST: simulado/gerencia/excluircampus
        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR })]
        public void ExcluirCampus(string codigo)
        {
            string lembrete = Lembrete.NEGATIVO;
            string mensagem = "Ocorreu um erro ao tentar excluir um(a) colaborador(a).";

            Colaborador colaborador = Colaborador.ListarPorMatricula(codigo);
            Campus campus = Campus.ListarPorCodigo(codigo);

            if (campus != null)
            {
                try
                {
                    if (campus.Bloco.Count > 0)
                        mensagem = $"Não é possível excluir este campus, pois ele tem vínculo com Bloco.";
                    else if (campus.Diretoria.Count > 0)
                        mensagem = $"Não é possível excluir este campus, pois ele tem vínculo com Diretoria.";
                    else if (campus.Pessoas.Count > 1)
                        mensagem = $"Não é possível excluir este campus, pois ele tem vínculo com Pessoas.";
                    else
                    {
                        string nome = campus.PessoaJuridica.NomeFantasia;
                        Campus.Remover(campus);
                        Repositorio.Commit();

                        lembrete = Lembrete.POSITIVO;
                        mensagem = $"Campus \"{nome}\" excluído com sucesso.";
                    }
                }
                catch
                {
                    Repositorio.Dispose();
                    Repositorio.Commit();

                    lembrete = Lembrete.NEGATIVO;
                    mensagem = $"Não é possível excluir este campus.";
                }
            }

            Lembrete.AdicionarNotificacao(mensagem, lembrete);
        }

        #endregion Campi

        #region Configuracoes

        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR }, Ocupacoes = new[] { Ocupacao.SUPERUSUARIO, Ocupacao.REITOR, Ocupacao.DIRETOR_GERAL, Ocupacao.COORDENADOR_SIMULADO })]
        public ActionResult Configuracoes() => View(new GerenciaConfiguracoesViewModel()
        {
            SmtpEnderecoHost = Parametro.Obter().SmtpEnderecoHost,
            SmtpPorta = Parametro.Obter().SmtpPorta,
            SmptFlagSSL = Parametro.Obter().SmtpFlagSSL,
            SmtpUsuario = Criptografia.Base64Decode(Parametro.Obter().SmtpUsuario),
            SmtpSenha = Criptografia.Base64Decode(Parametro.Obter().SmtpSenha)
        });

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR }, Ocupacoes = new[] { Ocupacao.SUPERUSUARIO, Ocupacao.REITOR, Ocupacao.DIRETOR_GERAL, Ocupacao.COORDENADOR_SIMULADO })]
        public ActionResult Configuracoes(GerenciaConfiguracoesViewModel model)
        {
            Parametro p = Parametro.Obter();

            p.SmtpEnderecoHost = model.SmtpEnderecoHost;
            p.SmtpPorta = model.SmtpPorta;
            p.SmtpFlagSSL = model.SmptFlagSSL;
            p.SmtpUsuario = Criptografia.Base64Encode(model.SmtpUsuario);
            p.SmtpSenha = Criptografia.Base64Encode(model.SmtpSenha);

            Parametro.Atualizar(p);

            Lembrete.AdicionarNotificacao("Configurações atualizadas com sucesso.", Lembrete.POSITIVO);

            return RedirectToAction("Configuracoes");
        }

        #endregion Configuracoes

        #region Provas

        // GET: simulado/gerencia/provas
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.PROFESSOR })]
        public ActionResult Provas()
        {
            var model = new GerenciaProvasViewModel();

            List<SimProva> provas = SimProva.ListarPorProfessor(Sessao.UsuarioMatricula);

            foreach (var prova in provas)
            {
                Simulado sim = prova.SimDiaRealizacao.Simulado;
                if (!sim.FlagSimuladoEncerrado && !sim.FlagProvaEncerrada && prova.SimDiaRealizacao.DtRealizacao > DateTime.Now)
                {
                    if (model.Provas.Keys.Count == 0 || model.Provas.Keys.FirstOrDefault(s => s.Ano == sim.Ano && s.NumIdentificador == sim.NumIdentificador) == null)
                    {
                        model.Provas[sim] = new List<SimProva>() { prova };
                    }
                    else
                    {
                        model.Provas[sim].Add(prova);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.PROFESSOR })]
        public ActionResult CarregarProvaConfigurar(string simulado, int dia, int prova)
        {
            if (!String.IsNullOrWhiteSpace(simulado))
            {
                Simulado sim = Simulado.ListarPorCodigo(simulado);
                if (sim != null)
                {
                    SimProva simProva = sim.SimDiaRealizacao.FirstOrDefault(d => d.CodDiaRealizacao == dia)?.SimProva.FirstOrDefault(p => p.CodProva == prova);
                    if (simProva != null)
                    {
                        return PartialView("_CarregarProvaConfigurar", simProva);
                    }
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.PROFESSOR })]
        public ActionResult ProvaConfigurarTrocar(string simulado, int dia, int prova, int questao, int indice)
        {
            if (!String.IsNullOrWhiteSpace(simulado))
            {
                Simulado sim = Simulado.ListarPorCodigo(simulado);
                if (sim != null)
                {
                    SimProva simProva = sim.SimDiaRealizacao.FirstOrDefault(d => d.CodDiaRealizacao == dia)?.SimProva.FirstOrDefault(p => p.CodProva == prova);
                    if (simProva != null)
                    {
                        if (!TempData.Keys.Contains($"SimuladoTrocarQuestoes{sim.Codigo}"))
                        {
                            TempData[$"SimuladoTrocarQuestoes{sim.Codigo}"] = sim.TodasQuestoesPorDisciplina(simProva.CodDisciplina).Select(q => q.CodQuestao).ToList();
                        }

                        var simuladoQuestoes = (List<int>)TempData[$"SimuladoTrocarQuestoes{sim.Codigo}"];
                        TempData.Keep();

                        Questao questaoTrocada = simProva.SimProvaQuestao.FirstOrDefault(q => q.CodQuestao == questao)?.Questao;

                        if (questaoTrocada != null)
                        {
                            int? novaQuestao = Questao.RetornarCodigoAleatorio(simProva.CodDisciplina, codTipoQuestao: questaoTrocada.CodTipoQuestao, evite: simuladoQuestoes.ToArray());
                            if (novaQuestao.HasValue && !simuladoQuestoes.Contains(novaQuestao.Value))
                            {
                                simProva.RemoverQuestao(questao);
                                simuladoQuestoes.Remove(questao);

                                simProva.AdicionarQuestao(novaQuestao.Value);
                                simuladoQuestoes.Add(novaQuestao.Value);

                                TempData[$"SimuladoTrocarQuestaoCodigo{sim.Codigo}{indice}"] = questao;
                                TempData[$"SimuladoTrocarQuestoes{sim.Codigo}"] = simuladoQuestoes;

                                ViewData["Index"] = indice;
                                return PartialView("_QuestaoConfigurar", Questao.ListarPorCodigo(novaQuestao.Value));
                            }
                            else
                            {
                                Lembrete.AdicionarNotificacao("Parece não haver mais questões disponíveis. Professor, cadastre mais algumas.", Lembrete.INFO, 10);
                                return new HttpStatusCodeResult(HttpStatusCode.Accepted, "Accepted");
                            }
                        }
                    }
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.PROFESSOR })]
        public ActionResult ProvaConfigurarDesfazer(string simulado, int dia, int prova, int questao, int indice)
        {
            if (!String.IsNullOrWhiteSpace(simulado))
            {
                Simulado sim = Simulado.ListarPorCodigo(simulado);
                if (sim != null)
                {
                    SimProva simProva = sim.SimDiaRealizacao.FirstOrDefault(d => d.CodDiaRealizacao == dia)?.SimProva.FirstOrDefault(p => p.CodProva == prova);
                    if (simProva != null)
                    {
                        Questao questaoDesfazer = simProva.SimProvaQuestao.FirstOrDefault(q => q.CodQuestao == questao)?.Questao;
                        if (questaoDesfazer != null)
                        {
                            Questao questaoTrocada = Questao.ListarPorCodigo((int)TempData[$"SimuladoTrocarQuestaoCodigo{sim.Codigo}{indice}"]);
                            TempData.Keep();
                            if (questaoTrocada?.CodQuestao != questao)
                            {
                                var simuladoQuestoes = (List<int>)TempData[$"SimuladoTrocarQuestoes{sim.Codigo}"];
                                TempData.Keep();

                                if (!simuladoQuestoes.Contains(questaoTrocada.CodQuestao))
                                {
                                    simProva.RemoverQuestao(questao);
                                    simuladoQuestoes.Remove(questao);

                                    simProva.AdicionarQuestao(questaoTrocada.CodQuestao);
                                    simuladoQuestoes.Add(questaoTrocada.CodQuestao);

                                    TempData[$"SimuladoTrocarQuestaoCodigo{sim.Codigo}{indice}"] = questao;
                                    TempData[$"SimuladoTrocarQuestoes{sim.Codigo}"] = simuladoQuestoes;

                                    ViewData["Index"] = indice;
                                    return PartialView("_QuestaoConfigurar", questaoTrocada);
                                }
                            }
                        }
                    }
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.PROFESSOR })]
        public ActionResult ProvaConfigurarRecarregarQuestoes(string simulado, int dia, int prova)
        {
            if (!String.IsNullOrWhiteSpace(simulado))
            {
                Simulado sim = Simulado.ListarPorCodigo(simulado);
                if (sim != null)
                {
                    SimProva simProva = sim.SimDiaRealizacao.FirstOrDefault(d => d.CodDiaRealizacao == dia)?.SimProva.FirstOrDefault(p => p.CodProva == prova);
                    if (simProva != null)
                    {
                        List<int> simuladoQuestoes = sim.TodasQuestoesPorDisciplina(simProva.CodDisciplina, simProva.CodDiaRealizacao, simProva.CodProva).Select(q => q.CodQuestao).ToList();
                        List<int> questoesCodigos = Simulado.ObterQuestoesCodigos(simProva.CodDisciplina, simProva.QteQuestoes, simProva.TipoQuestoes, simuladoQuestoes);

                        simProva.SimProvaQuestao.Clear();

                        foreach (var codQuestao in questoesCodigos)
                        {
                            simProva.SimProvaQuestao.Add(new SimProvaQuestao()
                            {
                                Questao = Questao.ListarPorCodigo(codQuestao)
                                //CodQuestao = codQuestao
                            });
                        }

                        TempData[$"SimuladoTrocarQuestoes{sim.Codigo}"] = questoesCodigos;
                        TempData.Keep();

                        if (questoesCodigos.Count < simProva.QteQuestoes)
                        {
                            Lembrete.AdicionarNotificacao("Foi gerada uma quantidade menor de questões para a prova deste simulado.", Lembrete.NEGATIVO);
                        }
                        else
                        {
                            Lembrete.AdicionarNotificacao("As questões para esta prova foram recarregadas com sucesso.", Lembrete.POSITIVO);
                        }

                        Repositorio.Commit();
                    }
                }
            }
            return RedirectToAction("Provas");
        }

        #endregion Provas

        #region Permissoes

        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR }, Ocupacoes = new[] { Ocupacao.SUPERUSUARIO, Ocupacao.REITOR, Ocupacao.DIRETOR_GERAL, Ocupacao.COORDENADOR_SIMULADO })]
        public ActionResult Permissoes()
        {
            var model = new GerenciaPermissoesViewModel();
            var contexto = Repositorio.GetInstance();

            model.Coordenadores = contexto.Ocupacao
                .Find(Ocupacao.COORDENADOR_SIMULADO)
                .PessoaFisica
                .Where(p => p.Usuario.FirstOrDefault(u => u.Colaborador.Count > 0 && u.Matricula != Sessao.UsuarioMatricula) != null)
                .ToList();

            model.Colaboradores = contexto.Ocupacao
                .Find(Ocupacao.COLABORADOR_SIMULADO)
                .PessoaFisica
                .Where(p => p.Usuario.FirstOrDefault(u => u.Colaborador.Count > 0 && u.Matricula != Sessao.UsuarioMatricula) != null)
                .ToList();

            return View(model);
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR }, Ocupacoes = new[] { Ocupacao.SUPERUSUARIO, Ocupacao.REITOR, Ocupacao.DIRETOR_GERAL, Ocupacao.COORDENADOR_SIMULADO })]
        public ActionResult ListarPessoas(string pesquisa)
        {
            if (!String.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.Trim().ToLower();
                var lstPessoas = PessoaFisica.Listar().Where(p =>
                    (p.Usuario.FirstOrDefault(u => u.Colaborador.Count > 0) != null) &&
                    (p.Nome.ToLower().Contains(pesquisa) ||
                    (!String.IsNullOrEmpty(p.Cpf) && p.Cpf.Contains(pesquisa)) ||
                    p.Usuario.FirstOrDefault(u => u.Matricula.ToLower().Contains(pesquisa)) != null)
                );
                return Json(lstPessoas.Select(p => new { CodPessoa = p.CodPessoa, Nome = p.Nome }));
            }
            return null;
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR }, Ocupacoes = new[] { Ocupacao.SUPERUSUARIO, Ocupacao.REITOR, Ocupacao.DIRETOR_GERAL, Ocupacao.COORDENADOR_SIMULADO })]
        public void AdicionarCoordenador(int codPessoaFisica)
        {
            var pessoa = PessoaFisica.ListarPorCodigo(codPessoaFisica);
            if (pessoa != null && pessoa.Usuario.FirstOrDefault(u => u.Colaborador.Count > 0) != null)
            {
                PessoaFisica.AdicionarOcupacao(codPessoaFisica, Ocupacao.COORDENADOR_SIMULADO);
            }
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR }, Ocupacoes = new[] { Ocupacao.SUPERUSUARIO, Ocupacao.REITOR, Ocupacao.DIRETOR_GERAL, Ocupacao.COORDENADOR_SIMULADO })]
        public void AdicionarColaborador(int codPessoaFisica)
        {
            var pessoa = PessoaFisica.ListarPorCodigo(codPessoaFisica);
            if (pessoa != null && pessoa.Usuario.FirstOrDefault(u => u.Colaborador.Count > 0) != null)
            {
                PessoaFisica.AdicionarOcupacao(codPessoaFisica, Ocupacao.COLABORADOR_SIMULADO);
            }
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR }, Ocupacoes = new[] { Ocupacao.SUPERUSUARIO, Ocupacao.REITOR, Ocupacao.DIRETOR_GERAL, Ocupacao.COORDENADOR_SIMULADO })]
        public void RemoverCoordenador(int[] codPessoaFisica)
        {
            foreach (var codPessoa in codPessoaFisica)
            {
                PessoaFisica.RemoverOcupacao(codPessoa, Ocupacao.COORDENADOR_SIMULADO);
            }
        }

        [HttpPost]
        [Filters.AutenticacaoFilter(Categorias = new[] { Categoria.SUPERUSUARIO, Categoria.COLABORADOR }, Ocupacoes = new[] { Ocupacao.SUPERUSUARIO, Ocupacao.REITOR, Ocupacao.DIRETOR_GERAL, Ocupacao.COORDENADOR_SIMULADO })]
        public void RemoverColaborador(int[] codPessoaFisica)
        {
            foreach (var codPessoa in codPessoaFisica)
            {
                PessoaFisica.RemoverOcupacao(codPessoa, Ocupacao.COLABORADOR_SIMULADO);
            }
        }

        #endregion Permissoes
    }
}