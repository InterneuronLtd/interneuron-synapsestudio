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
import { Component, Input, OnInit } from '@angular/core';
import { LazyLoadEvent, PrimeNGConfig } from 'primeng/api';
import { HistoryListService } from './history-list.service';

@Component({
  selector: 'app-history-list',
  templateUrl: './history-list.component.html',
  styleUrls: ['./history-list.component.scss'],
  providers: [HistoryListService]
})
export class HistoryListComponent implements OnInit {
  _loading = true;
  _totalRecords = 100;
  _historyList: [] = [];
  _canShow = false;
  _prevFilters: any = {};

  _statuses = [{ label: 'Active', value: '003' },
  { label: 'Approved', value: '002' },
  { label: 'Archived', value: '004' },
  { label: 'Deleted', value: '006' },
  { label: 'Draft', value: '001' },
  { label: 'InActive', value: '005' }];

  @Input()
  set canShow(val: boolean) {
    this._canShow = val;
  }
  @Input() comparehistory: any;
  constructor(private historyListService: HistoryListService, private primengConfig: PrimeNGConfig) { }

  ngOnInit(): void {

  }

  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }

  async loadHistoryList(e: LazyLoadEvent) {
    this._loading = true;
    const req = this.prepareReqParams(e);
    try {
      let data: any = await this.historyListService.getHistoryList(req);
      this._historyList = data.items;
      this._totalRecords = (!req.needTotalRecords) ? this._totalRecords : data.totalRecords;
      this._loading = false;
    } catch (e) {
      console.error(e);
      this._loading = false;
    }
  }


  prepareReqParams(e: LazyLoadEvent): any {
    const req: any = {
      pageNo: 1, pageSize: 10
    };

    if (!e) return req;
    req.pageNo = (!e.first || e.first === 0) ? 1 : (e.first / (e.rows || 10)) + 1;
    req.pageSize = e.rows ? e.rows : 10;


    if (!e.filters || Object.keys(e.filters).length === 0) {
      req['filterParams'] = [JSON.stringify({ key: 'status', value: '001' }), JSON.stringify({ key: 'status', value: '003' }), JSON.stringify({ key: 'status', value: '006' }), JSON.stringify({ key: 'status', value: '004' }), JSON.stringify({ key: 'status', value: '004' })
      ];
      req['needTotalRecords'] = this.doesNeedTotalRecords(req);
      return req;
    }

    let filtersVal = [];
    if (e.filters['status'] && e.filters['status'].value) {
      filtersVal.push(JSON.stringify({ key: 'status', value: e.filters['status'].value }));
    } else {
      filtersVal = [JSON.stringify({ key: 'status', value: '001' }), JSON.stringify({ key: 'status', value: '003' }), JSON.stringify({ key: 'status', value: '006' }), JSON.stringify({ key: 'status', value: '004' }), JSON.stringify({ key: 'status', value: '002' })];
    }

    if (e.filters['name'] && e.filters['name'].value) {
      filtersVal.push(JSON.stringify({ key: 'name', value: e.filters['name'].value }));
    }

    if (e.filters['dateTime'] && e.filters['dateTime'].value) {
      filtersVal.push(JSON.stringify({ key: 'dateTime', value: e.filters['dateTime'].value }));
    }

    if (e.filters['user'] && e.filters['user'].value) {
      filtersVal.push(JSON.stringify({ key: 'user', value: e.filters['user'].value }));
    }
    req['filterParams'] = filtersVal;
    req['needTotalRecords'] = this.doesNeedTotalRecords(req);
    return req;
  }

  doesNeedTotalRecords(req: any): boolean {
    if (!(req['filterParams'] || req['filterParams'].length === 0)) {
      this._prevFilters = {};
      return true;
    }

    if (!this._prevFilters || Object.keys(this._prevFilters).length === 0) {
      this._prevFilters = {};
    }

    let hasChanged = false;

    const newFilterParams: any = {};
    for (let index = 0; index < req['filterParams'].length; index++) {

      console.log(req['filterParams'][index]);
      const item = JSON.parse(req['filterParams'][index]);

      const filterK = `__${item.key}__${item.value}__`;
      newFilterParams[filterK] = item.value;
    }

    Object.keys(newFilterParams).forEach((item) => {
      if (!this._prevFilters[item]) {
        hasChanged = true;
      }
    });

    Object.keys(this._prevFilters).forEach((item) => {
      if (!newFilterParams[item]) {
        hasChanged = true;
      }
    });

    //for (let index = 0; index < Object.keys(this._prevFilters).length; index++) {

    //  const filterK = `__${item.key}__${item.value}__`;

    //  if (this._prevFilters[filterK]) {
    //    tempPrevFilters[filterK] = item.value;
    //  } else {
    //    tempPrevFilters[filterK] = item.value;
    //    hasChanged = true;
    //  }
    //}
    this._prevFilters = newFilterParams;

    return hasChanged;
  }

  changeProps(history: any) {
    if (this.comparehistory)
      this.comparehistory(history);
    return;
  }
}
