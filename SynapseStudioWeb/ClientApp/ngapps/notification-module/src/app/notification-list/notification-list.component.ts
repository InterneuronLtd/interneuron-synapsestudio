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
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit, ViewChild } from '@angular/core';
import { Notification } from '../models/notification.model';
import { ConfirmationService, LazyLoadEvent } from 'primeng/api';
import { MessageService } from 'primeng/api';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-notification-list',
  templateUrl: './notification-list.component.html',
  styleUrls: ['./notification-list.component.scss']
})
export class NotificationListComponent implements OnInit {

  _showAddDialog = false;

  notificationTypes: Notification[] = [];

  _notificationTypeModel = {};
  _loadingConfigs = false
  _totalRecords = 0;

  @ViewChild('dt') dt: Table | undefined;

  constructor(private http: HttpClient, private confirmationService: ConfirmationService, private messageService: MessageService) { }

  ngOnInit(): void {
    this.loadNotificationConfigurations();
  }



  loadNotificationConfigurations() {
    //console.log(event);
    this._loadingConfigs = true;
    this._totalRecords = 0;
    this.http.get(this.buildUrl('/Notification/GetAllNotificationConfiguration')).subscribe(
      {
        next: (response: any) => {
          this.notificationTypes = response;
          this._totalRecords = this.notificationTypes ? this.notificationTypes.length : 0;
          //console.log(this.notificationTypes);
          this._loadingConfigs = false;
        },
        error: error => {
          this.messageService.add({ severity: 'error', summary: 'Status', detail: 'Error while retrieving notifications' });
          console.error('There was an error!', error);
        }
      });
  }

  openNew() {

    this._notificationTypeModel = {action:'new', notificationId : ''};

    this._showAddDialog = true;
  }

  edit(id: string | undefined) {
    if (id != null || id != undefined || id != '') {
      this._notificationTypeModel = { action: 'edit', notificationId: id };
      this._showAddDialog = true;
    }
  }

  delete(id: string | undefined) {
    this.confirmationService.confirm({
      message: 'Do you want to delete this record?',
      accept: () => {
        const httpOptions = {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
        }

        this.http.post(this.buildUrl('/Notification/DeleteNotificationConfigurationById'), JSON.stringify(id), httpOptions).subscribe(
          {
            next: (response: any) => {
              if (response) {
                let index = this.notificationTypes.findIndex(rec => rec.id == id)

                if (index > -1) {
                  this.notificationTypes.splice(index, 1);
                }
              }
            },
            error: error => {
              this.messageService.add({ severity: 'error', summary: 'Status', detail: 'Error while retrieving notifications' });
              console.error('There was an error!', error);
            }
          });
      }
    });
    
  }

  saveNotification(notification: Notification) {
    if (!notification) return;

    this.messageService.add({ severity: 'success', summary: 'Status', detail: 'Notification saved successfully' });

    this._showAddDialog = false;

    if (!this.notificationTypes)
      this.notificationTypes = [];

    const index = this.notificationTypes.findIndex(rec => rec.id === notification.id);

    if (index > -1) {
      this.notificationTypes.splice(index, 1);
    }

    this.notificationTypes.push(notification);

    this.notificationTypes.sort((a, b) => {
      let x = a.name ? a.name.toLowerCase() : '';
      let y = b.name ? b.name.toLowerCase() : '';
      if (x < y) { return -1; }
      if (x > y) { return 1; }
      return 0;
    });
  }

  cancelNotification() {
    this._showAddDialog = false;
  }

  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }

  applyFilterGlobal(event: any, stringVal: string) {
    //console.log((event.target as HTMLInputElement).value);
    //console.log(this.dt);
    this.dt?.filterGlobal((event.target as HTMLInputElement).value, stringVal);
  }
}


