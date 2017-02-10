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
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SIAC.Hubs
{
    public class ReposicaoHub : Hub
    {
        #region Override

        public override Task OnDisconnected(bool stopCalled)
        {
            var connId = Context.ConnectionId;

            var lstAval = avaliacoes.ListarReposicoes();

            var aval = String.Empty;

            foreach (var key in lstAval.Keys)
            {
                if (lstAval[key].ListarConnectionIdAvaliados().Contains(connId))
                {
                    aval = key;
                    break;
                }
                else if (lstAval[key].SelecionarConnectionIdProfessor() == connId)
                {
                    aval = key;
                    break;
                }
            }
            if (!String.IsNullOrEmpty(aval))
            {
                var mapping = avaliacoes.SelecionarReposicao(aval);
                mapping.DesconectarPorConnectionId(connId);
                var matr = mapping.SelecionarMatriculaPorAvaliado(connId);

                if (!String.IsNullOrEmpty(matr))
                {
                    if (Models.Sistema.AvaliacaoUsuario.ContainsKey(aval.ToLower()))
                    {
                        Models.Sistema.AvaliacaoUsuario[aval.ToLower()].Remove(matr.ToLower());
                    }
                }

                if (mapping.SeTodosDesconectados())
                {
                    avaliacoes.RemoverReposicao(aval);
                    if (Models.Sistema.AvaliacaoUsuario.ContainsKey(aval.ToLower()))
                    {
                        Models.Sistema.AvaliacaoUsuario.Remove(aval.ToLower());
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(matr))
                    {
                        if (!mapping.SeAvaliadoFinalizou(matr))
                        {
                            mapping.InserirEvento(matr, "red power", "Desconectou");

                            if (!String.IsNullOrEmpty(mapping.SelecionarConnectionIdProfessor()))
                            {
                                Clients.Client(mapping.SelecionarConnectionIdProfessor()).desconectarAvaliado(matr);
                            }
                        }
                    }
                }
            }
            return base.OnDisconnected(stopCalled);
        }

        #endregion Override

        private readonly static ReposicaoMapping avaliacoes = new ReposicaoMapping();

        public void Realizar(string codAvaliacao)
        {
            Groups.Add(Context.ConnectionId, codAvaliacao);
        }

        public void Liberar(string codAvaliacao, bool flag)
        {
            if (flag)
            {
                Clients.Group(codAvaliacao).liberar(codAvaliacao);
            }
            else
            {
                Clients.Group(codAvaliacao).bloquear(codAvaliacao);
            }
        }

        public void ProfessorConectou(string codAvaliacao, string usrMatricula)
        {
            avaliacoes.InserirReposicao(codAvaliacao);
            var mapping = avaliacoes.SelecionarReposicao(codAvaliacao);
            mapping.InserirProfessor(usrMatricula, Context.ConnectionId);

            foreach (var alnMatricula in mapping.ListarMatriculaAvaliados())
            {
                foreach (var codQuestao in mapping.ListarQuestaoRespondidasPorAvaliado(alnMatricula))
                {
                    Clients.Client(mapping.SelecionarConnectionIdProfessor()).respondeuQuestao(alnMatricula, codQuestao, true);
                }
                Clients.Client(mapping.SelecionarConnectionIdProfessor()).listarChat(alnMatricula, mapping.ListarChat(alnMatricula));

                Clients.Client(mapping.SelecionarConnectionIdProfessor()).atualizarProgresso(alnMatricula, mapping.ListarQuestaoRespondidasPorAvaliado(alnMatricula).Count);
                Clients.Client(mapping.SelecionarConnectionIdProfessor()).conectarAvaliado(alnMatricula);
                if (mapping.SeAvaliadoFinalizou(alnMatricula))
                {
                    Clients.Client(mapping.SelecionarConnectionIdProfessor()).avaliadoFinalizou(alnMatricula);
                }
            }
        }

        public void AvaliadoConectou(string codAvaliacao, string usrMatricula)
        {
            avaliacoes.InserirReposicao(codAvaliacao);
            if (!Models.Sistema.AvaliacaoUsuario.ContainsKey(codAvaliacao.ToLower()))
            {
                Models.Sistema.AvaliacaoUsuario.Add(codAvaliacao.ToLower(), new List<string>());
            }
            Models.Sistema.AvaliacaoUsuario[codAvaliacao.ToLower()].Add(usrMatricula.ToLower());

            var mapping = avaliacoes.SelecionarReposicao(codAvaliacao);

            if (mapping.ListarMatriculaAvaliados().Contains(usrMatricula))
            {
                mapping.InserirEvento(usrMatricula, "sign in", "Reconectou");
                mapping.InserirAvaliado(usrMatricula, Context.ConnectionId);
            }
            else
            {
                mapping.InserirAvaliado(usrMatricula, Context.ConnectionId);
                mapping.InserirEvento(usrMatricula, "green sign in", "Conectou");
            }

            if (!String.IsNullOrEmpty(mapping.SelecionarConnectionIdProfessor()))
            {
                foreach (var codQuestao in mapping.ListarQuestoes())
                {
                    Clients.Client(mapping.SelecionarConnectionIdProfessor()).respondeuQuestao(usrMatricula, codQuestao, false);
                }
                Clients.Client(mapping.SelecionarConnectionIdProfessor()).atualizarProgresso(usrMatricula, mapping.ListarQuestaoRespondidasPorAvaliado(usrMatricula).Count);
                Clients.Client(mapping.SelecionarConnectionIdProfessor()).conectarAvaliado(usrMatricula);
            }
        }

        public void RequererAval(string codAvaliacao, string usrMatricula)
        {
            Clients.Client(avaliacoes.SelecionarReposicao(codAvaliacao).SelecionarConnectionIdPorAvaliado(usrMatricula)).enviarAval(codAvaliacao);
        }

        public void AvalEnviada(string codAvaliacao, string alnMatricula)
        {
            avaliacoes.SelecionarReposicao(codAvaliacao).InserirEvento(alnMatricula, "send outline", "Enviou printscreen");

            Clients.Client(avaliacoes.SelecionarReposicao(codAvaliacao).SelecionarConnectionIdProfessor()).receberAval(alnMatricula);
        }

        public void ResponderQuestao(string codAvaliacao, string usrMatricula, int questao, bool flag)
        {
            var mapping = avaliacoes.SelecionarReposicao(codAvaliacao);

            if (flag)
            {
                if (mapping.ListarQuestaoRespondidasPorAvaliado(usrMatricula).Contains(questao))
                {
                    mapping.InserirEvento(usrMatricula, "refresh", "Mudou a resposta da questão " + mapping.SelecionarIndiceQuestao(questao));
                }
                else
                {
                    mapping.InserirEvento(usrMatricula, "write", "Respondeu a questão " + mapping.SelecionarIndiceQuestao(questao));
                }
            }
            else
            {
                mapping.InserirEvento(usrMatricula, "erase", "Retirou resposta da questão " + mapping.SelecionarIndiceQuestao(questao));
            }

            mapping.AlterarAvaliadoQuestao(usrMatricula, questao, flag);

            if (!string.IsNullOrEmpty(mapping.SelecionarConnectionIdProfessor()))
            {
                Clients.Client(mapping.SelecionarConnectionIdProfessor()).atualizarProgresso(usrMatricula, mapping.ListarQuestaoRespondidasPorAvaliado(usrMatricula).Count);
                Clients.Client(mapping.SelecionarConnectionIdProfessor()).respondeuQuestao(usrMatricula, questao, flag);
            }
        }

        public void Alertar(string codAvaliacao, string mensagem, string alnMatricula)
        {
            if (!String.IsNullOrWhiteSpace(mensagem))
            {
                if (String.IsNullOrEmpty(alnMatricula))
                {
                    foreach (var matr in avaliacoes.SelecionarReposicao(codAvaliacao).ListarMatriculaAvaliados())
                    {
                        avaliacoes.SelecionarReposicao(codAvaliacao).InserirEvento(matr, "announcement", "Recebeu alerta geral");
                    }
                    Clients.Clients(avaliacoes.SelecionarReposicao(codAvaliacao).ListarConnectionIdAvaliados()).alertar(mensagem);
                }
                else
                {
                    avaliacoes.SelecionarReposicao(codAvaliacao).InserirEvento(alnMatricula, "announcement", "Recebeu alerta específico");
                    Clients.Client(avaliacoes.SelecionarReposicao(codAvaliacao).SelecionarConnectionIdPorAvaliado(alnMatricula)).alertar(mensagem);
                }
            }
        }

        public void Feed(string codAvaliacao, string alnMatricula)
        {
            if (avaliacoes.SelecionarReposicao(codAvaliacao).ListarMatriculaAvaliados().Contains(alnMatricula))
            {
                var lstEvento = avaliacoes.SelecionarReposicao(codAvaliacao).ListarFeed(alnMatricula).Select(e => new { Icone = e.Icone, Descricao = e.Descricao, DataCompleta = e.Data.ToBrazilianString(), Data = e.Data.ToElapsedTimeString() });
                if (!String.IsNullOrEmpty(avaliacoes.SelecionarReposicao(codAvaliacao).SelecionarConnectionIdProfessor()))
                {
                    Clients.Client(avaliacoes.SelecionarReposicao(codAvaliacao).SelecionarConnectionIdProfessor()).atualizarFeed(alnMatricula, lstEvento);
                }
            }
        }

        public void FocoAvaliacao(string codAvaliacao, string alnMatricula, bool flag)
        {
            if (flag)
            {
                avaliacoes.SelecionarReposicao(codAvaliacao).InserirEvento(alnMatricula, "warning sign", "Estabeleceu o foco na avaliação");
            }
            else
            {
                avaliacoes.SelecionarReposicao(codAvaliacao).InserirEvento(alnMatricula, "red warning sign", "Perdeu o foco na avaliação");
            }
            Feed(codAvaliacao, alnMatricula);
        }

        public void ChatProfessorEnvia(string codAvaliacao, string usrMatricula, string mensagem)
        {
            avaliacoes.SelecionarReposicao(codAvaliacao).InserirMensagem(usrMatricula, mensagem, false);
            Clients.Client(avaliacoes.SelecionarReposicao(codAvaliacao).SelecionarConnectionIdPorAvaliado(usrMatricula)).chatAvaliadoRecebe(mensagem);
        }

        public void ChatAvaliadoEnvia(string codAvaliacao, string usrMatricula, string mensagem)
        {
            avaliacoes.SelecionarReposicao(codAvaliacao).InserirMensagem(usrMatricula, mensagem, true);
            Clients.Client(avaliacoes.SelecionarReposicao(codAvaliacao).SelecionarConnectionIdProfessor()).chatProfessorRecebe(usrMatricula, mensagem);
        }

        public void AvaliadoVerificando(string codAvaliacao, string usrMatricula)
        {
            var mapping = avaliacoes.SelecionarReposicao(codAvaliacao);
            mapping.InserirEvento(usrMatricula, "write square", "Verificando respostas");
        }

        public void AvaliadoFinalizou(string codAvaliacao, string usrMatricula)
        {
            var mapping = avaliacoes.SelecionarReposicao(codAvaliacao);
            mapping.InserirEvento(usrMatricula, "red sign out", "Finalizou");
            mapping.AlterarAvaliadoFlagFinalizou(usrMatricula);
            if (!String.IsNullOrEmpty(mapping.SelecionarConnectionIdProfessor()))
            {
                Clients.Client(mapping.SelecionarConnectionIdProfessor()).avaliadoFinalizou(usrMatricula);
            }
        }

        public void Prorrogar(string codAvaliacao, int duracao, string observacao)
        {
            if (duracao > 0)
            {
                var mapping = avaliacoes.SelecionarReposicao(codAvaliacao);
                using (var e = new Models.Contexto())
                {
                    int numIdentificador = 0;
                    int semestre = 0;
                    int ano = 0;

                    if (codAvaliacao.Length == 13)
                    {
                        string codigo = codAvaliacao;
                        int.TryParse(codigo.Substring(codigo.Length - 4), out numIdentificador);
                        codigo = codigo.Remove(codigo.Length - 4);
                        int.TryParse(codigo.Substring(codigo.Length - 1), out semestre);
                        codigo = codigo.Remove(codigo.Length - 1);
                        int.TryParse(codigo.Substring(codigo.Length - 4), out ano);
                        codigo = codigo.Remove(codigo.Length - 4);
                        int codTipoAvaliacao = e.TipoAvaliacao.FirstOrDefault(t => t.Sigla == codigo).CodTipoAvaliacao;

                        Models.Avaliacao aval = e.Avaliacao.FirstOrDefault(acad => acad.Ano == ano && acad.Semestre == semestre && acad.NumIdentificador == numIdentificador && acad.CodTipoAvaliacao == codTipoAvaliacao);

                        var prorrogacao = new Models.AvaliacaoProrrogacao();
                        prorrogacao.DtProrrogacao = DateTime.Now;
                        prorrogacao.Observacao = String.IsNullOrWhiteSpace(observacao) ? null : observacao;
                        prorrogacao.DuracaoAnterior = aval.Duracao.Value;
                        prorrogacao.DuracaoNova = prorrogacao.DuracaoAnterior + duracao;
                        aval.Duracao = prorrogacao.DuracaoNova;
                        aval.AvaliacaoProrrogacao.Add(prorrogacao);

                        e.SaveChanges();

                        Clients.Clients(mapping.ListarConnectionIdAvaliados()).prorrogar(duracao);
                    }
                }
            }
        }
    }

    public class ReposicaoMapping
    {
        private readonly Dictionary<string, Avaliacao> reposicoes = new Dictionary<string, Avaliacao>();

        public Dictionary<string, Avaliacao> ListarReposicoes()
        {
            return reposicoes;
        }

        public void InserirReposicao(string codAvaliacao)
        {
            lock (reposicoes)
            {
                if (!reposicoes.ContainsKey(codAvaliacao))
                {
                    reposicoes.Add(codAvaliacao, new Avaliacao());
                    using (var e = new Models.Contexto())
                    {
                        int numIdentificador = 0;
                        int semestre = 0;
                        int ano = 0;

                        if (codAvaliacao.Length == 13)
                        {
                            string codigo = codAvaliacao;
                            int.TryParse(codigo.Substring(codigo.Length - 4), out numIdentificador);
                            codigo = codigo.Remove(codigo.Length - 4);
                            int.TryParse(codigo.Substring(codigo.Length - 1), out semestre);
                            codigo = codigo.Remove(codigo.Length - 1);
                            int.TryParse(codigo.Substring(codigo.Length - 4), out ano);
                            codigo = codigo.Remove(codigo.Length - 4);
                            int codTipoAvaliacao = e.TipoAvaliacao.FirstOrDefault(t => t.Sigla == codigo).CodTipoAvaliacao;

                            Models.AvalAcadReposicao avalReposicao = e.AvalAcadReposicao.FirstOrDefault(acad => acad.Ano == ano && acad.Semestre == semestre && acad.NumIdentificador == numIdentificador && acad.CodTipoAvaliacao == codTipoAvaliacao);

                            reposicoes[codAvaliacao].MapearQuestao(avalReposicao.Avaliacao.Questao.Select(q => q.CodQuestao).ToList());
                        }
                    }
                }
            }
        }

        public void RemoverReposicao(string codAvaliacao)
        {
            lock (reposicoes)
            {
                if (reposicoes.ContainsKey(codAvaliacao))
                {
                    reposicoes.Remove(codAvaliacao);
                }
            }
        }

        public Avaliacao SelecionarReposicao(string codAvaliacao)
        {
            if (reposicoes.ContainsKey(codAvaliacao))
            {
                return reposicoes[codAvaliacao];
            }
            return null;
        }
    }
}