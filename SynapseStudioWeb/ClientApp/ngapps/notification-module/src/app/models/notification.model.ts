//BEGIN LICENSE BLOCK 
//Interneuron Synapse

//Copyright(C) 2024  Interneuron Limited

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
//END LICENSE BLOCK 
export class Notification {
  id?: string;
  name?: string;
  description?: string;
  channelDetails?: NotificationChannel[];
  status?: number;
  //allowedChannelDetails?: NotificationChannel[];
}

export class NotificationChannel {
  name = '';
  isEnabled?= false;
  channelOptions?: ChannelOptions[] = [];
  channelAudiences?: Audience[] = [];
  //allowedChannelOptions?: ChannelOptions[] = [];
  requiredClientContexts?: RequiredClientContexts[] = [];
  isModuleAgnostic?: boolean = false
  isPersonAgnostic?: boolean = false;
  msgPropNameOrMsg?: string;
  emailBody?: string;
  emailSubject?: string;
  smsSubject?: string;
  smsContent?: string;
}

export class ChannelOptions {
  name = '';
  value = '';
}

export class Audience {
  name = '';
  userId = '';
  email = '';
  phoneNo = '';
}

export class RequiredClientContexts {
  name = '';
  value = '';
}
