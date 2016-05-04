﻿using System;
using System.Configuration;
using System.Linq;

namespace SIAC.Models
{
    public partial class dbSIACEntities
    {
        public dbSIACEntities()
            : base("name=SIACEntities")
        //: base(Environment.GetEnvironmentVariable("SIAC_DB") /*?? ConfigurationManager.AppSettings["SIAC_DB"]*/)
        {

        }

        public int SaveChanges(bool alertar = true)
        {
            if (alertar)
            {
                Sistema.AlertarMudanca.AddRange(Sistema.UsuarioAtivo.Keys);
                Sistema.AlertarMudanca = Sistema.AlertarMudanca.Distinct().ToList();
                Sistema.AlertarMudanca.Remove(Helpers.Sessao.UsuarioMatricula);
            }
            return base.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Helpers.Sessao.Remover("dbSIACEntities");
            }
            base.Dispose(disposing);
        }
    }
}