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
import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Task, TaskStep } from '../Models/Task';

@Component({
  selector: 'app-import',
  templateUrl: './import.component.html',
  styleUrls: ['./import.component.scss'],
  providers: [MessageService, ConfirmationService]

})
export class ImportComponent implements OnInit, OnDestroy {
  title = 'import-mmc-module';
  _showFileUploader = false;
  _selectedFiles: any[] = [];
  _loading = false;
  _tasks: Task[] = [];
  _activeIndex = 0;
  _intervalId: any;
  _executionNote = '';
  _showProgressIndicator = false;

  myfile: any = [];


  @Input()
  set showFileUploader(val: boolean) {
    this._showFileUploader = val;
    if (val) {
      this.loadTasksStatusList(null);
      this._intervalId = setInterval(() => this.refreshPendingTasks(), 100000);
    }
  }

  @Output() closeDialog = new EventEmitter<void>();

  @ViewChild('op', { static: false }) overlaypanel: any;
  @ViewChild('fileupload', { static: false }) fileupload: any;

  constructor(private messageService: MessageService, private httpClient: HttpClient, private confirmationService: ConfirmationService) { }


  hideDialog() {
    this.closeDialog.emit();
    this._showFileUploader = false;
    if (this._intervalId) {
      clearInterval(this._intervalId);
    }
  }
  ngOnDestroy(): void {
    if (this._intervalId) {
      clearInterval(this._intervalId);
    }
  }

  ngOnInit(): void {

  }

  loadTasksStatusList(e: any) {
    const tempTasks: Task[] = [];

    this.httpClient.get(this.buildUrl('Formulary/GetUploadFilesStatusHistory'))
      .subscribe(
        (result: any) => {
          console.log('result=', result);

          if (!result) return;
          result.forEach((res: any) => {

            const task: Task = { ...res, steps: [] };

            res.steps?.forEach((step: any, index: number) => {
              task.steps.push({ ...step });
            });

            tempTasks.push(task);
          });

          this._tasks = tempTasks;
          console.log('this._tasks=', this._tasks);

        }, (error: any) => {
          console.error(error);
        });
  }

  showNote(event: Event, note: string) {

    this._executionNote = note;
    this.overlaypanel.show(event);
  }

  refreshPendingTasks() {
    const pendingTasks = this._tasks?.filter(t => t.isStillRunning === true);

    pendingTasks?.forEach(p => {
      this.refreshTask(p);
    });
  }

  refreshTask(task: Task) {
    this.httpClient.get(this.buildUrl(`Formulary/GetFileUploadTasksStatus?taskId=${task.taskId}`))
      .subscribe(
        (result: any) => {
          console.log('result=', result);

          const taskIndex = this._tasks.findIndex(t => t.taskId === task.taskId);
          if (!result && taskIndex > -1) {
            this._tasks.splice(taskIndex, 1);
            return;
          }

          const newTask: Task = { ...result, steps: [] };

          result.steps?.forEach((step: any, index: number) => {
            newTask.steps.push({ ...step });
          });

          if (taskIndex === -1) {
            this._tasks.push(newTask);
          } else {
            this._tasks.splice(taskIndex, 1);
            this._tasks.splice(taskIndex, 0, newTask);
          }

          console.log('this._tasks=', this._tasks);

        }, (error: any) => {
          console.error(error);
        });
  }


  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }


  onUploadConfirm() {
    this.messageService.clear('c');

    if (!this._selectedFiles || !this._selectedFiles.length) return;
    console.log('this._selectedFiles', this._selectedFiles);

    const formData = new FormData();
    this._selectedFiles.forEach((file: any) => formData.append(file.name, file));
    this._showProgressIndicator = true;

    this.httpClient.post(this.buildUrl('Formulary/SyncDMDUsingFile'), formData)
      .subscribe(
        (result: any) => {
          this.loadTasksStatusList(null);
          this._showProgressIndicator = false;
          this.fileupload?.clear();
          console.log('result=', result);
          const msg = 'Note: This process is a long running process. Please click on the refresh button to check the status of upload.';
          const bodyContentInLower = result?.toLowerCase();
          if (bodyContentInLower) {
            if (bodyContentInLower.includes("success")) {
              this.messageService.add({ severity: 'success', summary: 'Success', detail: result });
              this.messageService.add({ severity: 'success', summary: 'Success', detail: msg });
            } else if (bodyContentInLower.includes("error")) {
              this.messageService.add({ severity: 'error', summary: 'Error', detail: result });
            } else {
              this.messageService.add({ severity: 'success', summary: 'Success', detail: msg });
            }
          }
        }, (error: any) => {
          this.loadTasksStatusList(null);
          this._showProgressIndicator = false;
          this.fileupload?.clear();
          console.error(error);
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Unknown error. Please check if there is any network connectivity issue and try uploading again after sometime.', sticky: true });
        });
  }

  onUploadReject() {
    this.messageService.clear('c');
  }
  dmdUploader(event: any) {

    console.log('event', event);

    if (!event || !event.files) {
      this._selectedFiles = [];
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please select files to upload.', sticky: true });
      return;
    }
    this._selectedFiles = [...event.files];

    this.messageService.clear();
    this.messageService.add({ key: 'c', sticky: true, severity: 'warn', summary: 'Are you sure you want to upload?', detail: 'Confirm to proceed' });

  }

}

