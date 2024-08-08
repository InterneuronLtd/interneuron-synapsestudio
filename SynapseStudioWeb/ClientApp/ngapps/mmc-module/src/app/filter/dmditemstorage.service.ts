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
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DataCompressionService } from '../shared/datacompression.service';

/*
@Injectable()
export class DMDItemStorageService {
  private dmdItemsStorage: Record<string, any> = {};
  constructor() { }

  setItem(keyName: string, obj: any) {
    this.dmdItemsStorage[keyName] = obj;
  }

  getItem(keyName: string): any {
    if (!(keyName in this.dmdItemsStorage)) return null;
    return this.dmdItemsStorage[keyName];
  }

  removeItem(keyName: string) {
    if (keyName in this.dmdItemsStorage)
      delete this.dmdItemsStorage[keyName];
  }

  dispose() {
    this.dmdItemsStorage = {};
  }
}
  */
@Injectable()
export class DMDItemSessionStorageService {
  constructor(private dataCompressionService: DataCompressionService) { }

  setItem(keyName: string, data: string) {
    if (data == null) return;
    sessionStorage.setItem(keyName, this.dataCompressionService.compressString(data));
  }

  getItem(keyName: string): string | null {
    const data = sessionStorage.getItem(keyName);
    if (data == null) return null;
    const decomObj = this.dataCompressionService.decompressString(data);
    return decomObj;
  }

  removeItem(keyName: string) {
    sessionStorage.removeItem(keyName);
  }

  dispose() {
    return;
  }
}

