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
import { DMDSNOMEDVersion } from './filter.component.model';

@Injectable()
export class SearchPageService {
  constructor(private http: HttpClient) { }


  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }

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

  getDMDSNOMEDVersion(): Observable<DMDSNOMEDVersion | null> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }
    return this.http.get<DMDSNOMEDVersion | null>(this.buildUrl('/Formulary/GetDMDSNOMEDVersion'), httpOptions)
  }

  getFormularyChildren(searchCriteria: any): Promise<any | null> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }
    return this.http.post(this.buildUrl('/Formulary/LoadChildrenFormularies_New'), searchCriteria, httpOptions).toPromise()
  }

  getTasksByName(taskNames: string[]): Observable<any | null> {
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }
    return this.http.post(this.buildUrl('/Formulary/GetTasksByName'), taskNames, httpOptions);
  }
}


declare global {
  interface Window {
    AppPath: string;
  }
}
