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
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Audience, ChannelOptions, NotificationChannel, Notification } from '../models/notification.model';
import { NotificationLookup } from '../models/notification-lookup.model';
import { v4 as uuidv4 } from 'uuid';
import * as $ from 'jquery';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ConfirmationService } from 'primeng/api';
import { data, error } from 'jquery';
import { MessageService } from 'primeng/api';
import parsePhoneNumber from 'libphonenumber-js'

@Component({
  selector: 'app-notification-item',
  templateUrl: './notification-item.component.html',
  styleUrls: ['./notification-item.component.scss']
})
export class NotificationItemComponent implements OnInit {
  
  _allowedChannels = ['Email', 'SMS', 'Web'];

  _showAddDialog = false;
  _name = '';
  _description = '';
  _selectableChannelNames: { code: string, name: string }[] = [];
  _selectedChannels: NotificationChannel[] = [];
  _selectedChannelName?: { code: string, name: string } | null;
  _emailFullName = '';
  _emailId = '';
  _smsFullName = '';
  _sms = '';
  _smsEmailId = '';
  _notificationLookup: NotificationLookup[] = [];
  _selectedNotificationLookup?: NotificationLookup;
  _webFullName = '';
  _webEmailId = '';

  _errorName = false;
  _errorChannel = false;
  _errorEmailFullName = false;
  _errorEmailId = false;
  _errorEmailSubject = false;
  _errorEmailBody = false;

  _errorSMSFullName = false;
  _errorSMS = false;
  _errorSMSEmailId = false;
  _errorSpecificUsers = false;
  _errorWebFullName = false;
  _errorWebEmailId = false;
  _errorSMSSubject = false;
  _errorSMSContent = false;

  isChannelSelected = true;
  emailChannelAudience = true;
  smsChannelAudience = true;
  specificUsersChannelAudience = true;
  isChannelOptionSelected = true;

  notificationType?: Notification;

  _action = '';
  _notificationId = '';

  _requiredContexts: { name: string, value: string, inactive?: boolean }[] = [];
  _selectedContexts: { name: string, value: string, inactive?: boolean }[] = [];

  overlayVisible: boolean = true;

  _emailSub = '';
  _emailBody = '';

  _smsSub = '';
  _smsBody = '';

  @Input()
  set showDialog(val: boolean) {
    this._showAddDialog = val;
  }

  @Input()
  set notificationTypeModel(obj: any) {
    if (obj.notificationId && obj.action == 'edit') {
      this._action = obj.action;
      this._notificationId = obj.notificationId;
      this.populateData(obj);
    }

    if (obj.action == 'new') {
      this._name = '';
      this._description = '';
      this._notificationId = '';
      this._selectedChannels = [];
      this._selectableChannelNames = this._allowedChannels.map(item => { return { code: item, name: item } });
      this._selectedNotificationLookup = {};
    }
  }

  @Output() save = new EventEmitter<Notification>();
  @Output() cancel = new EventEmitter();

  constructor(private http: HttpClient, private confirmationService: ConfirmationService, private messageService: MessageService) { }

  ngOnInit(): void {
    this._selectableChannelNames = this._allowedChannels.map(item => { return { code: item, name: item } });

    this.getNotificationLookup();

    this._requiredContexts = [
      { name: 'Person', value: 'PersonId', inactive: true },
      { name: 'Encounter', value: 'EncounterId', inactive: true },
      { name: 'Prescription', value: 'PrescriptionId' },
    ];

    this._selectedContexts = [
      { name: 'Person', value: 'PersonId' },
      { name: 'Encounter', value: 'EncounterId'},
    ];
  }

  addEmailRecord(event: any) {

    this._errorEmailFullName = ((!this._emailFullName) || (this._emailFullName && this._emailFullName.trim() == '')) == true ? true : false;

    this._errorEmailId = ((!this._emailId) || (this._emailId && this._emailId.trim() == '')) == true ? true : false;

    let regex = /^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;

    if (regex.test(this._emailId)) {
      this._errorEmailId = false;
    }
    else {
      this._errorEmailId = true;
    }

    if (this._errorEmailFullName || this._errorEmailId) {
      event.stopPropagation();
      return;
    }

    this._selectedChannels.forEach((channel) => {
      channel.isEnabled = true;
      if (channel.name == 'Email') {
        let aud: Audience = new Audience();
        aud.name = this._emailFullName;
        aud.email = this._emailId;

        if (channel.channelAudiences && channel.channelAudiences.length > 0) {
          if (channel.channelAudiences.filter(rec => rec.email.toLowerCase() == aud.email.toLowerCase()).length <= 0) {
            channel.channelAudiences.push(aud);
          }

          channel.channelAudiences.sort((a, b) => {
            let x = a.email.toLowerCase();
            let y = b.email.toLowerCase();
            if (x < y) { return -1; }
            if (x > y) { return 1; }
            return 0;
          });
        }
        else {
          channel.channelAudiences = [];
          channel.channelAudiences.push(aud);

          channel.channelAudiences.sort((a, b) => {
            let x = a.email.toLowerCase();
            let y = b.email.toLowerCase();
            if (x < y) { return -1; }
            if (x > y) { return 1; }
            return 0;
          });
        }
        this._emailFullName = '';
        this._emailId = '';
        this.emailChannelAudience = true;
      }
    });
  }

  addSMSRecord(event: any) {

   this._errorSMSFullName = ((!this._smsFullName) || (this._smsFullName && this._smsFullName.trim() == '')) == true ? true : false;

   this._errorSMS = ((!this._sms) || (this._sms && this._sms.trim() == '')) == true ? true : false;

    this._errorSMSEmailId = ((!this._smsEmailId) || (this._smsEmailId && this._smsEmailId.trim() == '')) == true ? true : false;

    let regex = /^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;

    if (regex.test(this._smsEmailId)) {
      this._errorSMSEmailId = false;
    }
    else {
      this._errorSMSEmailId = true;
    }

    //let regexMobile = /^(\+\d{1,3}[- ]?)?\d{10}$/;

    //if (regexMobile.test(this._sms)) {
    //  this._errorSMS = false;
    //}
    //else {
    //  this._errorSMS = true;
    //}

    const phoneNumber = parsePhoneNumber(this._sms);

    if (phoneNumber) {
      this._errorSMS = false;
    }
    else {
      this._errorSMS = true;
    }

    if (this._errorSMSFullName || this._errorSMS || this._errorSMSEmailId) {
      event.stopPropagation();
      return;
    }

    this._selectedChannels.forEach((channel) => {
      channel.isEnabled = true;

      if (channel.name == 'SMS') {
        let aud: Audience = new Audience();
        aud.name = this._smsFullName;
        aud.phoneNo = this._sms;
        aud.email = this._smsEmailId;

        if (channel.channelAudiences && channel.channelAudiences.length > 0) {
          if (channel.channelAudiences.filter(rec => rec.phoneNo.toLowerCase() == aud.phoneNo.toLowerCase()).length <= 0) {
            channel.channelAudiences.push(aud);
          }

          channel.channelAudiences.sort((a, b) => {
            let x = a.name.toLowerCase();
            let y = b.name.toLowerCase();
            if (x < y) { return -1; }
            if (x > y) { return 1; }
            return 0;
          });
        }
        else {
          channel.channelAudiences = [];
          channel.channelAudiences.push(aud);

          channel.channelAudiences.sort((a, b) => {
            let x = a.name.toLowerCase();
            let y = b.name.toLowerCase();
            if (x < y) { return -1; }
            if (x > y) { return 1; }
            return 0;
          });
        }
        this._smsFullName = '';
        this._sms = '';
        this._smsEmailId = '';
        this.smsChannelAudience = true;
      }
    });
  }

  addWebRecord(event: any) {
    this._errorWebFullName = ((!this._webFullName) || (this._webFullName && this._webFullName.trim() == '')) == true ? true : false;

    if (!this._errorWebFullName) {
      this._webFullName = this._webFullName;
    }

    this._errorWebEmailId = ((!this._webEmailId) || (this._webEmailId && this._webEmailId.trim() == '')) == true ? true : false;

    if (!this._errorWebEmailId) {
      this._webEmailId = this._webEmailId;
    }

    if (this._errorWebFullName || this._errorWebEmailId) {
      event.stopPropagation();
      return;
    }

    this._selectedChannels.forEach((channel) => {
      channel.isEnabled = true;

      if (channel.name === 'Web') {
        const aud: Audience = {
          name: this._webFullName,
          email: this._webEmailId,
          userId: '',
          phoneNo: ''
        };


        if (channel.channelAudiences && channel.channelAudiences.length > 0) {
          if (channel.channelAudiences.filter(rec => rec.email.toLowerCase() == aud.email.toLowerCase()).length <= 0) {
            channel.channelAudiences.push(aud);
          }

          channel.channelAudiences.sort((a, b) => {
            let x = a.email.toLowerCase();
            let y = b.email.toLowerCase();
            if (x < y) { return -1; }
            if (x > y) { return 1; }
            return 0;
          });
        }
        else {
          channel.channelAudiences = [];
          channel.channelAudiences.push(aud);

          channel.channelAudiences.sort((a, b) => {
            let x = a.email.toLowerCase();
            let y = b.email.toLowerCase();
            if (x < y) { return -1; }
            if (x > y) { return 1; }
            return 0;
          });
        }
        this._webFullName = '';
        this._webEmailId = '';
        this.specificUsersChannelAudience = true;
      }
    });
  }

  populateData(obj:any): void {
    this._name = '';
    this._description = '';
    this._selectedChannels = [];

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }

    this.http.post(this.buildUrl('/Notification/GetNotificationConfigurationById'), JSON.stringify(obj.notificationId), httpOptions)
      .subscribe({
        next: (response) => {
          this.notificationType = response;

          this._name = this.notificationType?.name!;
          this._description = this.notificationType?.description!;
          this._selectedChannels = this.notificationType?.channelDetails!;

          this._selectableChannelNames = this._allowedChannels.map(item => { return { code: item, name: item } });

          this._selectedChannels.forEach((channel) => {
            let selectedChannel = this._selectableChannelNames.filter(rec => rec.name == channel.name);

            if (selectedChannel.length > 0) {
              const ind = this._selectableChannelNames.findIndex(rec => rec.name == channel.name);
              this._selectableChannelNames.splice(ind, 1);
            }

            if (channel.name === 'Web') {
              if (channel && channel.channelOptions && channel.channelOptions.length) {
                let firstChannelOption = channel.channelOptions[0].value;
                let notificationLookupMatch = this._notificationLookup.find(rec => rec.code == firstChannelOption);
                this._selectedNotificationLookup = notificationLookupMatch;
                
              }
            }
          })
        },
        error: error => {
          this.messageService.add({ severity: 'error', summary: 'Status', detail: 'Error while retrieving notification' });
          console.error('There was an error!', error);
        }
      });
  }

  getNotificationLookup() {
    this.http.get(this.buildUrl('/Notification/GetAllNotificationLookup')).subscribe({
      next: (response:any) => {
        this._notificationLookup = response;
      },
      error: error => {
        this.messageService.add({ severity: 'error', summary: 'Status', detail: 'Error while retrieving notifications' });
        console.error('There was an error!', error);
      }
    });
  }

  saveNotification(event: any) {
    this._errorName = ((!this._name) || (this._name && this._name.trim() == '')) == true ? true : false;

    if (this._errorName) {
      event.stopPropagation();
      return;
    }

    if (!this._selectedChannels || !this._selectedChannels.length) {
      this.isChannelSelected = false;
      event.stopPropagation();
      return;
    }
    else {
      this.isChannelSelected = true;
    }

    this._selectedChannels.forEach((channel) => {

      this.validateChannel(channel);

      if (channel.isEnabled && channel.name == 'Web' && this._selectedNotificationLookup?.code == 'specific_users') {
        if (channel.channelAudiences == undefined || (channel.channelAudiences && channel.channelAudiences.length == 0)) {
          this.specificUsersChannelAudience = false;
        }
        else {
          this.specificUsersChannelAudience = true;
        }
      }
    });

    if (!this.emailChannelAudience || !this.smsChannelAudience || !this.specificUsersChannelAudience || this._errorSMSSubject || this._errorSMSContent || this._errorEmailSubject || this._errorEmailBody) {
      event.stopPropagation();
      return;
    }

    let webChannel = this._selectedChannels.filter(rec => rec.name == 'Web');

    if (webChannel && webChannel.length > 0 && webChannel[0].isEnabled && $.isEmptyObject(this._selectedNotificationLookup) || (!$.isEmptyObject(this._selectedNotificationLookup) && this._selectedNotificationLookup?.code?.trim() == '')) {
      this.isChannelOptionSelected = false;
      event.stopPropagation();
      return;
    }
    else {
      this.isChannelOptionSelected = true;
    }

    const notification = new Notification();
    notification.name = this._name;
    notification.description = this._description;
    notification.channelDetails = this._selectedChannels;
    notification.status = 1;
    notification.id = this._action !== 'edit' ? uuidv4() : this._notificationId;
    //this._showAddDialog = false;
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }

    this.http.post(this.buildUrl('/Notification/AddNotificationConfiguration'), { notificationTypes: [notification] }, httpOptions).subscribe(
      {
        next: any => {
          this._showAddDialog = false;
          if (this.save) {
            this.save.emit(notification);
          }
        },
        error: error => {
          this.messageService.add({ severity: 'error', summary: 'Status', detail: `${error?.error}`, life: 6000 });
          console.warn('There was an error!', error);
        }
      });
  }

  private validateChannel(channel: NotificationChannel | null) {
    if (!channel) return;

    if (channel.isEnabled && channel.name == 'Email') {
      //console.log(channel.name);
      if (channel.channelAudiences == undefined || (channel.channelAudiences && channel.channelAudiences.length == 0))
        this.emailChannelAudience = false;
      else 
        this.emailChannelAudience = true;

      if (channel.emailSubject) 
        this._errorEmailSubject = false;
      else 
        this._errorEmailSubject = true;

      if (channel.emailBody)
        this._errorEmailBody = false;
      else
        this._errorEmailBody = true;
    }

    if (channel.isEnabled && channel.name == 'SMS') {
      if (channel.channelAudiences == undefined || (channel.channelAudiences && channel.channelAudiences.length == 0)) {
        this.smsChannelAudience = false;
      }
      else {
        this.smsChannelAudience = true;
      }

      let regex = /^[a-zA-Z\d]+$/;

      if (channel.smsSubject && regex.test(channel.smsSubject!)) {
        this._errorSMSSubject = false;
      }
      else {
        this._errorSMSSubject = true;
      }
      if (channel.smsContent) {
        this._errorSMSContent = false;
      }
      else {
        this._errorSMSContent = true;
      }
    }
  }

  hideDialog() {
    this._showAddDialog = false;
    if (this.cancel) this.cancel.emit();

  }

  selectChannel(ev: any) {

    if ((!this._selectedChannelName) || (this._selectedChannelName && this._selectedChannelName.code && this._selectedChannelName.code.trim() == '')) {
      this.isChannelSelected = false;
      ev.stopPropagation();
      return;
    }
    else {
      this.isChannelSelected = true;
    }

    if (!this._selectedChannelName) return;

    const ind = this._selectableChannelNames.findIndex(rec => rec.code === this._selectedChannelName?.code);

    if (ind > -1) {
      this._selectedChannels.push({ name: this._selectedChannelName.name, isEnabled: true });
      this._selectableChannelNames.splice(ind, 1);
      this._selectedChannelName = null;
    }
  }

  disableChannel(e: any, channel: NotificationChannel) {
    this.confirmationService.confirm({
      message: 'Do you want to disable this record?',
      accept: () => {
 
        let tempSelectedChannels = [...this._selectedChannels];

        tempSelectedChannels.filter(rec => rec.name == channel.name)?.forEach((ch) => {
          ch.isEnabled = false;
        });

        if (channel.name == 'Email') {
          this.emailChannelAudience = true;
        }

        if (channel.name == 'SMS') {
          this.smsChannelAudience = true;
        }

        if (channel.name == 'Web') {
          this.specificUsersChannelAudience = true;
        }

        this._selectedChannels = tempSelectedChannels;
      }
    });
  }

  enableChannel(e: any, channel: NotificationChannel) {
    this.confirmationService.confirm({
      message: 'Do you want to enable this record?',
      accept: () => {
        
        let tempSelectedChannels = [...this._selectedChannels];

        tempSelectedChannels.filter(rec => rec.name == channel.name)?.forEach((ch) => {
          ch.isEnabled = true;
        });

        this._selectedChannels.filter(rec => rec.name == channel.name).forEach((channel) => {
          channel.isEnabled = true;
        });

        if (channel.name == 'Email') {
          this.emailChannelAudience = true;
        }

        if (channel.name == 'SMS') {
          this.smsChannelAudience = true;
        }

        if (channel.name == 'Web') {
          this.specificUsersChannelAudience = true;
        }

        this._selectedChannels = tempSelectedChannels;
      }
    });
  }

  selectChannelOption(ev: any, channel: NotificationChannel) {
    let label = this._notificationLookup.find(x => x.code == ev.value.code)!;
    channel.isEnabled = true;
    channel.channelOptions = [];
    if (label)
      channel.channelOptions.push({ name: label.name!, value: label.code! });

    this.isChannelOptionSelected = true;

    if (channel.name == 'Web' && ev.value.code != 'specific_users' && channel.channelAudiences && channel.channelAudiences.length > 0) {
      channel.channelAudiences = [];
    }
  }
    
  removeAudience(e: any, aud: Audience, channel: NotificationChannel) {
     if (channel.channelAudiences) {
      const ind = channel.channelAudiences.findIndex(rec => rec.email === aud.email);
      if (ind > -1) {
        channel.channelAudiences.splice(ind, 1);
      }
    }
  }

  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }

  onWebContextPropChange(e: any, propName: string, channelObj?: any) {
    this.confirmationService.confirm({
      message: 'This has impact on how the notification is handled in the application. Are you sure that you want to perform this action?',
      reject: () => {
        channelObj[propName] = !channelObj[propName];
      }
    });
    e.preventDefault(); // kill the click
  }

  deleteChannel(e: any, channel: NotificationChannel) {
    this.confirmationService.confirm({
      message: 'Do you want to delete this record?',
      accept: () => {

        const selectedChannel = this._selectedChannels?.filter(rec => rec.name === channel.name);

        if (selectedChannel && selectedChannel.length) {
          const ind = this._selectedChannels.findIndex(rec => rec.name === channel.name);
          this._selectedChannels.splice(ind, 1);
        }

        if (channel.name === 'Email') {
          this.emailChannelAudience = true;
        }

        if (channel.name === 'SMS') {
          this.smsChannelAudience = true;
        }

        if (channel.name === 'Web') {
          this.specificUsersChannelAudience = true;
        }

        this._selectableChannelNames.push({ code: channel.name, name: channel.name });
      }
    });

  }

}

declare global {
  interface Window {
    AppPath: string;
  }
}
