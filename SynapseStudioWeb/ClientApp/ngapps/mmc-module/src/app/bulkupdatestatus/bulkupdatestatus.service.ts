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
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class BulkStatusUpdateService {
  constructor(private http: HttpClient) { }


  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }

  //Formulary/GetNextLevelStatuses
/*
ajaxGetJson('Formulary/GetNextLevelStatuses', { status: node.data.recordstatus.code },
            (data) => {
                $('#pnlUpdateProgress').hide();
                if (!data) {
                    console.error(err);
                    node.removeClass('fancytree-active');
                    node.setActive(false);
                    toastr.error('Error getting record status details.');
                    return;
                }

                $("#statuschangeformularyVersionId").val(node.data.formularyVersionId);
                $('#ddlstatus').html('');
                $("#showstatusChangeReason").hide();

                $.each(data, (k, v) => {
                    let selected = '';
                    if (v.selected)
                        selected = 'selected';

                    $('#ddlstatus').append(`<option value="${v.value}" ${selected}>${v.text}</option>`);
                });

                $('#changestatus').modal('show');
            },
            (err) => {
                $('#pnlUpdateProgress').hide();
                console.error(err);
                node.removeClass('fancytree-active');
                node.setActive(false);
                toastr.error('Error getting record status details.');
            });
*/
  getNextLevelStatuses(status: string): Observable<any> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      params: new HttpParams({ fromString: 'status=' + status })
    }

    return this.http.get<any>(this.buildUrl('/Formulary/GetNextLevelStatuses'), httpOptions);
  }

  updateStatus(request: UpdateFormularyStatusAPIRequest): Observable<{status: string | null, errors: string[] | null } | null> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
    };
    return this.http.post<{ status: string | null, errors: string[] | null } | null>(this.buildUrl('/Formulary/UpdateFormularyStatusInBulk'), request, httpOptions);
  }

  needConfirmation(ids: string[]): Observable<{ needConfirmation: boolean, msg: string | null} | null> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
    };

    return this.http.post<{ needConfirmation: boolean, msg: string | null } | null>(this.buildUrl('/Formulary/NeedUserConfirmationForUpdateForFormularyVerionIds'), ids, httpOptions);
  }
  
  /*
  updateStatus() {
    this.hasHandledChangeStatus = true;

    if (this.status != '004') {
      let updateFormularyStatusAPRequestData = {
        formularyVersionId: this.formularyVersionId,
        recordStatusCodeChangeMsg: this.reason,
        recordStatusCode: this.status
      };

      const httpOptions = {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      }

      this.http.post(this.buildUrl('/Formulary/UpdateFormularyStatus_New'), updateFormularyStatusAPRequestData, httpOptions)
        .subscribe((response) => {

          this.displayDialog = false;
        });
    }

    if (this.status == '004' && $.trim(this.reason) != '') {
      let updateFormularyStatusAPRequestData = {
        formularyVersionId: this.formularyVersionId,
        recordStatusCodeChangeMsg: this.reason,
        recordStatusCode: this.status
      };

      const httpOptions = {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      }

      this.http.post(this.buildUrl('/Formulary/UpdateFormularyStatus_New'), updateFormularyStatusAPRequestData, httpOptions)
        .subscribe((response) => {

          this.displayDialog = false;
        });
    }
  }
  */

  getFormulariesByPageNumber(pageNumber: any): Observable<any> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      params: new HttpParams({ fromString: 'pageNumber=' + pageNumber })
    }

    return this.http.get<any>(this.buildUrl('/Formulary/GetFormulariesByPageNumber'), httpOptions);
  }

  getFormulariesByPageNumberAsPromise(pageNumber: any, pageSize: any): Promise<any> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      params: new HttpParams({ fromString: `pageNumber=${pageNumber}&pageSizeIn=${pageSize}` })
    }

    return this.http.get<any>(this.buildUrl('/Formulary/GetFormulariesByPageNumber'), httpOptions).toPromise();
  }

  getFormularyChangeLogForCodesWithChangeDetailOnly(ampCodes: string[]): Observable<any> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }

    return this.http.post(this.buildUrl('/Formulary/GetFormularyChangeLogForCodesWithChangeDetailOnly'), ampCodes, httpOptions)
  }

  getFormularyChangeLogForFormularyIds(ampCodes: string[]): Observable<any> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }

    return this.http.post(this.buildUrl('/Formulary/GetFormularyChangeLogForFormularyIds'), ampCodes, httpOptions)
  }


  getFormularyChildren(searchCriteria: any): Promise<any | null> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }
    return this.http.post(this.buildUrl('/Formulary/LoadChildrenFormularies_New'), searchCriteria, httpOptions).toPromise()
  }
}
export interface UpdateFormularyStatusAPIRequest {
  requestData: UpdateFormularyStatusAPIRequestItem[]
};

export interface UpdateFormularyStatusAPIRequestItem {
  formularyVersionId: string,
  recordStatusCodeChangeMsg: string,
  recordStatusCode: string
};

declare global {
  interface Window {
    AppPath: string;
  }
}
