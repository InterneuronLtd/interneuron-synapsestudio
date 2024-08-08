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
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MessageService } from 'primeng/api';
import { BulkStatusUpdateService, UpdateFormularyStatusAPIRequest, UpdateFormularyStatusAPIRequestItem } from './bulkupdatestatus.service';

@Component({
  selector: 'app-bulk-updatestatus',
  templateUrl: './bulkupdatestatus.component.html',
  styleUrls: ['./bulkupdatestatus.component.scss'],
  providers: [BulkStatusUpdateService]
})
export class BulkUpdateStatusComponent {
  _currentStatus: string | null = null;
  _currentStatusCd: string | null = null;
  _showEditDialog = false;
  _statuses: RecordStatus[] = [];
  _selectedStatus: RecordStatus | null = null;
  _data: { currentStatus: string | null, level: string | null, ids: any } | null = null;
  _errors: string[] | null = []
  _isUpdateDisabled = true;
  _archivedReason = '';
  _errorArchivedReason = false;
  _errorNewStatus = false;
  _userConfirmMsg = '';
  _isUpdating = false;

  @Output() onHide: EventEmitter<{ isUpdated: boolean | null } | null> = new EventEmitter<{ isUpdated: boolean | null } | null>();
  @Input()
  set isVisible(visible: boolean) {
    this._showEditDialog = visible;
  }
  @Input()
  set data(val: { currentStatus: any | null, level: string | null, ids: any } | null) {
    this._data = val;
    this._errors =[]
    this._isUpdateDisabled = true;
    this._archivedReason = '';
    this._errorArchivedReason = false;
    this._userConfirmMsg = '';

    if (val && val.currentStatus) {
      console.log('bulkStatusUpdateService val==', val);

      this._currentStatus = val.currentStatus.desc;
      this._currentStatusCd = val.currentStatus.cd;
      this._statuses = [];
      const statuses: RecordStatus[] | null  = [];
      this.bulkStatusUpdateService.getNextLevelStatuses(val.currentStatus.cd)
        .subscribe({
          next: (response: any | null) => {
            if (response && response.length) {
              console.log('bulkStatusUpdateService response==', response);
              response.forEach((rec: any) => {
                if (!rec.value) {
                  this._selectedStatus = {
                    code: rec.value,
                    name: rec.text
                  };
                }
                console.log('bulkStatusUpdateService response1111==', rec);

                statuses.push({
                  code: rec.value,
                  name: rec.text
                });
              });
              this._statuses = statuses;
              console.log('bulkStatusUpdateService response233111==', this._statuses);

            }
          },
          error: (err: any) => {
            console.error('There was an error!', err);
          }
        });

    }
  }

  constructor(private bulkStatusUpdateService: BulkStatusUpdateService, private messageService: MessageService) {
   }

  onDialogHide() {
    this.onHide?.emit();
  }

  onUpdate(e: any) {
    this._errorArchivedReason = false;
    this._errorNewStatus = false;
    this._errors = [];

    if (!this._selectedStatus || !this._selectedStatus.code) {
      this._errorNewStatus = true;
      return;
    }

    if (this._selectedStatus && (this._selectedStatus.code === '004' || this._selectedStatus.code === '006')) {
      if (!this._archivedReason || !this._archivedReason.trim()) {
        this._errorArchivedReason = true;
        return;
      }
    }
    this._isUpdating = true;
    const ids = this._data && this._data.ids ? Object.keys(JSON.parse(this._data.ids)) : [];
    console.log('data=====', this._data, ids);
    this._userConfirmMsg = '';
    this.bulkStatusUpdateService.needConfirmation(ids)
      .subscribe((response) => {
        console.log('NeedUserConfirmationForUpdateForFormularyVerionIds==', response);
        if (response && response.needConfirmation) {
          this._userConfirmMsg = response.msg ?? "Some records being moved to ACTIVE have been DELETED in dm+d and will be DELETED in the MMC";
          this._userConfirmMsg = this._userConfirmMsg.replace("<li>", "").replace("</li>", "");
          this.messageService.add({ key: 'needUserConfirm', sticky: true, severity: 'warn' });
        } else {
          this.updateRecordStatus();
        }
      }, err => {
        console.error('NeedUserConfirmationForUpdateForFormularyVerionIds error', err);
        this._errors = ['UKNOWN ERROR: Error updating the status'];
      });
  }

  onStatusSelectionChange(e: any) {
    console.log('onStatusSelectionChange', e);
    this._isUpdateDisabled = (e && e.value && e.value.code) ? false: true;
  }

  onUserConfirmClose() {
    this.messageService.clear('needUserConfirm');
    this._isUpdating = false;
  }

  onUserConfirmYes() {
    this.messageService.clear('needUserConfirm');
    this.updateRecordStatus();
  }

  updateRecordStatus() {
    this._errors = [];
    const ids = this._data && this._data.ids ? Object.keys(JSON.parse(this._data.ids)) : [];

    const requestItems: UpdateFormularyStatusAPIRequestItem[] = ids.map((id: string) => {
      const item: UpdateFormularyStatusAPIRequestItem = { formularyVersionId: id, recordStatusCode: this._selectedStatus ? this._selectedStatus.code : '', recordStatusCodeChangeMsg: this._archivedReason.trim() };
      return item;
    });

    const request: UpdateFormularyStatusAPIRequest = { requestData: requestItems };

    this.bulkStatusUpdateService.updateStatus(request)
      .subscribe((response) => {
        console.log('UpdateFormularyStatusInBulk==', response);
        this._isUpdating = false;
        if (response && response.errors && response.errors.length) {
          this._errors = [...response.errors];
          return;
        }
        this.onHide?.emit({ isUpdated: true });
      }, err => {
        this._isUpdating = false;
        console.error('UpdateFormularyStatusInBulk error', err);
        this._errors = ['UKNOWN ERROR: Error updating the status'];
      });
  }
}

interface RecordStatus {
  name: string;
  code: string;
}
