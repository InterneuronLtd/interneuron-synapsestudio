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
using SynapseStudioWeb.DataService.APIModel;

namespace SynapseStudioWeb.Models
{
    public class FormularyTreeModel
    {
        public bool? Checkbox { get; set; }
        public List<FormularyTreeModel> Children { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public bool? Expanded { get; set; }
        public bool? Icon { get; set; }
        public string Key { get; set; }
        public bool? Selected { get; set; }
        public string FormularyId { get; set; }
        public string FormularyVersionId { get; set; }
        public string Title { get; set; }
        public string Tooltip { get; set; }
        public bool? Folder { get; set; }
        public bool? Lazy { get; set; }
        public string? DMDJSONActiveData { get; set; }
        public string? DMDJSONDraftData { get; set; }

    }
}
