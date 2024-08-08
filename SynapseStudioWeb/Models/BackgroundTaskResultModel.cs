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

namespace SynapseStudioWeb.Models
{
    public class BackgroundTaskResultModel
    {
        public string TaskId { get; set; }
        public string Name { get; set; }
        public short StatusCd { get; set; }
        public string Status { get; set; }
        public List<Step> Steps { get; set; }
        public string? Detail { get; set; }
        public string? FileName { get; set; }
        public List<string>? AllFileNames { get; set; }

        public bool? IsStillRunning { get; set; }
        public string? TaskExecutionNote { get; set; }
        public DateTime? Createdtimestamp { get; set; }
        public DateTime? Createddate { get; set; }
        public string? Createdby { get; set; }
        public DateTime? Updatedtimestamp { get; set; }
        public DateTime? Updateddate { get; set; }
        public string? Updatedby { get; set; }

        public short? SubTaskStatusCd { get; set; }
        public string SubTaskStatus { get; set; }
    }

    public class Step
    {
        public int StepCd { get; set; }
        public string Name { get; set; }
        public bool? IsCurrentStep { get; set; }

        public bool? IsPending { get; set; }
        public bool? IsSuccess { get; set; }
        public bool? IsFail { get; set; }

    }
}
