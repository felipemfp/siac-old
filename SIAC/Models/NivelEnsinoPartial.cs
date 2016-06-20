﻿using System.Collections.Generic;
using System.Linq;

namespace SIAC.Models
{
    public partial class NivelEnsino
    {
        private static Contexto contexto => Repositorio.GetInstance();

        public static List<NivelEnsino> ListarOrdenadamente() => contexto.NivelEnsino.OrderBy(ne => ne.Descricao).ToList();
    }
}