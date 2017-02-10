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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace SIAC.Helpers
{
    public class Formate
    {
        public static string ParaCPF(string valor)
        {
            if (valor.Length == 11)
            {
                return $"{valor.Between(0, 3)}.{valor.Between(3, 6)}.{valor.Between(6, 9)}-{valor.Between(9, 11)}";
            }
            return string.Empty;
        }

        public static string DeCPF(string cpf)
        {
            Regex rgx = new Regex(@"\D");
            return rgx.Replace(cpf, "");
        }
    }
}