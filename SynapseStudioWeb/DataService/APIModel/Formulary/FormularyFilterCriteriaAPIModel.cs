 //Interneuron synapse

//Copyright(C) 2024 Interneuron Limited

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

//See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynapseStudioWeb.DataService.APIModel
{
    public class FormularyFilterCriteriaAPIModel
    {
        public string searchTerm { get; set; }
        public bool hideArchived { get; set; }
        public List<string> recStatusCds { get; set; }
        public List<string> formularyStatusCd { get; set; }
        public List<string> Flags { get; set; }

        public bool showOnlyDuplicate { get; set; }

        public bool IncludeDeleted { get; set; } = false;
        public CategoryDiffenceFilter? CategoryDifference { get; set; }
        public string? ProductType { get; set; }
        public bool IncludeChangeDetails { get; set; } = false;
        public bool IncludeInvalid { get; set; } = false;
    }

    public record CategoryDiffenceFilter
    {
        public bool? IsDetailChanged { get; set; }
        public bool? IsPosologyChanged { get; set; }
        public bool? IsGuidanceChanged { get; set; }
        public bool? IsFlagsChanged { get; set; }
        public bool? IsInvalid { get; set; }
        public bool? IsDeleted { get; set; }

    }
}
