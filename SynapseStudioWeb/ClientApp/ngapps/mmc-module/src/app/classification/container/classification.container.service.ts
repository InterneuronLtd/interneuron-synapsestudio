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
import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable()
export class ClassificationContainerService {

  constructor(private http: HttpClient) { }


  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }

  searchFDBTherapeuticClasses(q: string): Observable<any> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      params: new HttpParams({ fromString: `q=${q}` })
    }

    return this.http.get<any>(this.buildUrl('/Formulary/SearchFDBTherapeuticClasses'), httpOptions);
  }

  searchATCLookups(q: string): Observable<any> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      params: new HttpParams({ fromString: `q=${q}` })
    }

    return this.http.get<any>(this.buildUrl('/Formulary/SearchATCLookups'), httpOptions);
  }

  searchBNFLookups(q: string): Observable<any> {

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      params: new HttpParams({ fromString: `q=${q}` })
    }

    return this.http.get<any>(this.buildUrl('/Formulary/SearchBNFLookups'), httpOptions);
  }

  //getFormulariesByPageNumberAsPromise(pageNumber: any, pageSize: any): Promise<any> {

  //  const httpOptions = {
  //    headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
  //    params: new HttpParams({ fromString: `pageNumber=${pageNumber}&pageSizeIn=${pageSize}` })
  //  }

  //  return this.http.get<any>(this.buildUrl('/Formulary/GetFormulariesByPageNumber'), httpOptions).toPromise();
  //}
}
