﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SIAC.Models;
using Newtonsoft.Json;

namespace SIAC.ViewModels
{
    public class GerenciaDisciplinasViewModel
    {
        public List<Disciplina> Disciplinas { get; set; } = new List<Disciplina>();
    }
}