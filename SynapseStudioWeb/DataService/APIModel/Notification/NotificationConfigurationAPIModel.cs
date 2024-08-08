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
﻿using System.Collections.Generic;

namespace SynapseStudioWeb.DataService.APIModel.Notification
{
    public class NotificationConfigurationAPIModel
    {
        public List<NotificationTypeAPIModel> NotificationTypes { get; set; }
    }

    public class NotificationTypeAPIModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public long Status { get; set; }
        public List<ChannelDetailAPIModel> ChannelDetails { get; set; }
    }

    public class ChannelDetailAPIModel
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public List<ChannelOptionAPIModel> ChannelOptions { get; set; }
        public List<ChannelAudienceAPIModel> ChannelAudiences { get; set; }
        /// <summary>
        /// This property is applicable only for the Web Channel
        /// </summary>
        public bool IsModuleAgnostic { get; set; } = false;
        /// <summary>
        /// This property is applicable only for the Web Channel
        /// </summary>
        public bool IsPersonAgnostic { get; set; } = false;
        /// <summary>
        /// This property is applicable only for the Web Channel
        /// </summary>
        public string? MsgPropNameOrMsg { get; set; }

        public string? EmailBody { get; set; }

        public string? EmailSubject { get; set; }

        public string? SMSContent { get; set; }

        public string? SMSSubject { get; set; }
    }

    public class ChannelAudienceAPIModel
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
    }

    public class ChannelOptionAPIModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
