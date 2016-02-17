﻿using System;
using System.Web;
using SIAC.Models;

namespace SIAC.Helpers
{
    public class Sessao
    {
        private static HttpContext context => HttpContext.Current;

        public static bool RealizandoAvaliacao
        {
            get
            {
                foreach (string avaliacao in Sistema.AvaliacaoUsuario.Keys)
                    if (Sistema.AvaliacaoUsuario[avaliacao].Contains(UsuarioMatricula))
                        return true;
                return false;
            }
        }

        public static bool Apresentacao => Retornar("Apresentacao") != null ? (bool)Retornar("Apresentacao") : false;

        public static bool AjudaEstado => Retornar("AjudaEstado") != null ? (bool)Retornar("AjudaEstado") : false;

        public static string UsuarioMatricula => (string)Retornar("UsuarioMatricula") ?? String.Empty;

        public static string UsuarioNome => (string)Retornar("UsuarioNome") ?? String.Empty;

        public static string UsuarioCategoria => (string)Retornar("UsuarioCategoria") ?? String.Empty;

        public static int UsuarioCategoriaCodigo => (int)Retornar("UsuarioCategoriaCodigo");

        public static bool UsuarioSenhaPadrao => Retornar("UsuarioSenhaPadrao") != null ? (bool)Retornar("UsuarioSenhaPadrao") : false;

        public static void Inserir(string chave, object valor) { if (context != null) context.Session[chave] = valor; }

        public static object Retornar(string chave)
        {
            if (context?.Session != null)
            {
                return context?.Session[chave];
            }
            return null;
        }

        public static void Limpar() => context?.Session.Clear();
    }
}