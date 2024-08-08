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
import { MessageService } from 'primeng/api';
import { PrimeNGConfig } from 'primeng/api';
import { Observable } from 'rxjs';
import { FormularyAdditionalCode } from '../../Models/FormularyAdditionalCode';
import { GroupedClassification } from '../../Models/GroupedClassification';
import { SelectList } from '../../Models/SelectList';
import { ClassificationContainerService } from './classification.container.service';

@Component({
  selector: 'classification-container',
  templateUrl: './classification.container.component.html',
  styleUrls: ['./classification.container.component.scss'],
  providers: [MessageService, ClassificationContainerService]
})
export class ClassificationContainerComponent implements OnInit {

  _existingadditionalcode: FormularyAdditionalCode[] = [];
  _groupedClassificationCodes: GroupedClassification[] = [];
  _classificationtypes: SelectList[] = [];
  _newClassificationTypeSelected: string | null = null;
  _newClassificationCode: any | null = null;
  _newClassificationCodeNemeAsArr: any[] | null = null;
  _showAddNewFields = false;
  _canAddNew = false;
  _allowAllDelete = false;
  _productType = '';
  _isdmdbrowser = false;

  _newClassificationDesc: string | null = '';
  _hideClassificationDesc = false;
  _classificationSuggestions: any[] = [];
  //_disableClassificationDesc = false;
  _showNewClassificationCodeAsReadOnly = false;

  @Input()
  set isdmdbrowser(val: string | null) {
    if (val && val === 'true')
      this._isdmdbrowser = true;
    else
      this._isdmdbrowser = false;
  }

  @Input()
  set productType(val: string) {
    this._productType = val;
    this._canAddNew = (val === 'AMP');
  }

  @Input()
  set allowAllDelete(val: boolean) {
    this._allowAllDelete = val;
  }

  get existingadditionalcode(): string {
    const addlCode = this._existingadditionalcode;
    if (addlCode)
      return JSON.stringify(this._existingadditionalcode);
    return '';
  }

  @Input()
  set existingadditionalcode(data: string) {
    console.log('existingadditionalcodeVal=');
    console.log(data);
    this._existingadditionalcode = [];
    if (data !== null && data !== '') {
      this._existingadditionalcode = JSON.parse(data);
      this.convertToGroupedClassCode();
    }
  }

  get classificationtypes(): SelectList[] {
    return this._classificationtypes;
  }
  @Input()
  set classificationtypes(data: any) {
    this._classificationtypes = [];
    if (data) {
      const input: SelectList[] = JSON.parse(data);
      this._classificationtypes = input.map(rec => {
        if (rec.Value === "customgroup" || this._allowAllDelete)
          rec.Disabled = false;
        return rec;
      });
    }
  }

  constructor(private messageService: MessageService, private primengConfig: PrimeNGConfig, private classificationContainerService: ClassificationContainerService) { }
  

  ngOnInit(): void {
    this.primengConfig.ripple = true;
  }


  showDialogToAddNew(e: any) {
    this._showAddNewFields = true;
    this._showNewClassificationCodeAsReadOnly = false;
    e.preventDefault();
  }

  cancelNewCodesSave(e: any) {
    this._newClassificationTypeSelected = null;
    this._newClassificationCode = null;
    this._newClassificationDesc = null;
    this._showAddNewFields = false;
    e.preventDefault();
  }

  saveNewCodes(e: any) {
    const msgs = [];
    if (!this._newClassificationTypeSelected) {
      msgs.push({ severity: 'error', summary: 'Error', detail: 'Please select the Type' });
    }
    if (!this._newClassificationCode) {
      msgs.push({ severity: 'error', summary: 'Error', detail: 'Please enter Code' });
    }
    if (!this._newClassificationDesc) {
      msgs.push({ severity: 'error', summary: 'Error', detail: 'Please enter Description' });
    }
    if (msgs && msgs.length > 0) {
      this.messageService.addAll(msgs);
      return;
    }
    if (!this.addNewElement(e)) {
      e.preventDefault(); return;
    }
    this._newClassificationTypeSelected = null;
    this._newClassificationCode = null;
    this._newClassificationDesc = null;
    this._showAddNewFields = false;
    e.preventDefault();
  }

  selectAdditionalCode(e: any) {
    const changedAdditionalCode: FormularyAdditionalCode = e;

    if (changedAdditionalCode) {
      const existingCodes = [...this._existingadditionalcode];

      for (const rec of existingCodes) {
        if (rec.AdditionalCodeSystem === changedAdditionalCode.AdditionalCodeSystem) {
          rec.IsDefault = false;

          if (rec.AdditionalCode === changedAdditionalCode.AdditionalCode) {
            rec.IsDefault = true;
          }
        }
      }
      this._existingadditionalcode = [];
      this._existingadditionalcode = [...existingCodes];
    }
  }

  convertToGroupedClassCode() {
    this._groupedClassificationCodes = [];
    if (!this._groupedClassificationCodes) return;
    //get unique classification codes in the list
    const uniqueCodeSystems = [...new Set(this._existingadditionalcode.map(item => item.AdditionalCodeSystem))];

    if (uniqueCodeSystems) {
      for (let uniqueCodeIndex = 0; uniqueCodeIndex < uniqueCodeSystems.length; uniqueCodeIndex++) {
        const uniqueCodeSystem = uniqueCodeSystems[uniqueCodeIndex];
        const uniqCodeSysVals = this._existingadditionalcode.filter(rec => rec.AdditionalCodeSystem === uniqueCodeSystem);
        let canDelete = false;
        if ((this._productType === 'AMP' && uniqueCodeSystem === 'CustomGroup') || this._allowAllDelete) {
          canDelete = true;
        }
        const defaultObj = uniqCodeSysVals.find(rec => rec.IsDefault === true);
        const additionalPropsAsStr = uniqCodeSysVals.map(rec => `${rec.AdditionalCode} - ${rec.AdditionalProps}`).join(', ');

        const grouped: GroupedClassification = {
          groupName: uniqueCodeSystem, additionalCodes: uniqCodeSysVals, isDeletable: canDelete, isDisabled: true, selectedCode: defaultObj == null ? undefined : defaultObj, additonalPropsForGrp: additionalPropsAsStr
        };
        this._groupedClassificationCodes.push(grouped);
      }
    }
  }

  deleteElement(elementToDelete: GroupedClassification) {
    if (!elementToDelete) return;

    const groupRec = this._groupedClassificationCodes.find(rec => rec.groupName === elementToDelete.groupName);
    const groupRecIndex = this._groupedClassificationCodes.findIndex(rec => rec.groupName === elementToDelete.groupName);

    const deletableAddnlCodes = groupRec?.additionalCodes;

    if (deletableAddnlCodes) {
      deletableAddnlCodes.forEach(rec => {
        const addnlCodeIndex = this._existingadditionalcode.findIndex(existingRec => existingRec.AdditionalCode === rec.AdditionalCode && existingRec.AdditionalCodeSystem === rec.AdditionalCodeSystem);
        if (addnlCodeIndex !== -1) {
          this._existingadditionalcode.splice(addnlCodeIndex, 1);
        }
      });
    }
    if (groupRecIndex !== -1) {
      this._groupedClassificationCodes.splice(groupRecIndex, 1);
    }
    return;
  }

  deleteAssociation(obj: { group: GroupedClassification | null, additionalCode: FormularyAdditionalCode }) {
    
    if (!obj || !obj.group || !obj.additionalCode) return;

    const addnlCodeIndex = this._existingadditionalcode.findIndex(existingRec => existingRec.AdditionalCode === obj.additionalCode.AdditionalCode && existingRec.AdditionalCodeSystem === obj.additionalCode.AdditionalCodeSystem);

    if (addnlCodeIndex !== -1) {
      this._existingadditionalcode.splice(addnlCodeIndex, 1);
      this.convertToGroupedClassCode();
    }
    return;
  }

  getNewAdditionalCode(): string {
    if (typeof this._newClassificationCode === 'string' || this._newClassificationCode instanceof String)
      return `${this._newClassificationCode}`;

    return this._newClassificationCode.code.split('|')[0];
  }

  addNewElement(e: any): boolean {
    const newEl = new FormularyAdditionalCode();
    newEl.AdditionalCode = this.getNewAdditionalCode();// this._newClassificationCode ?? '';
    newEl.AdditionalCodeDesc = this._newClassificationDesc ?? '';
    newEl.AdditionalCodeSystem = this._newClassificationTypeSelected ?? '';
    newEl.Source = 'RNOH';
    if (this._existingadditionalcode && this._existingadditionalcode.length > 0) {
      //check if the code entered already exists
      if (this.IsAdditionalCodeExists()) { e.preventDefault(); return false; }
      newEl.FormularyVersionId = this._existingadditionalcode[0].FormularyVersionId;
    } else {
      this._existingadditionalcode = [];
    }
    this._existingadditionalcode.push(newEl);
    this.convertToGroupedClassCode();
    //this.existingadditionalcode = JSON.stringify(this._existingadditionalcode);
    e.preventDefault();
    return true;
  }

  IsAdditionalCodeExists(): boolean {
    //const doesExistIndex = this._existingadditionalcode.findIndex(rec => rec.AdditionalCodeSystem === this._newClassificationTypeSelected && (this._newClassificationCode && rec.AdditionalCode && this._newClassificationCode.toLowerCase() === rec.AdditionalCode.toLowerCase()));
    const newClassificationCode = this.getNewAdditionalCode()
    const doesExistIndex = this._existingadditionalcode.findIndex(rec => rec.AdditionalCodeSystem === this._newClassificationTypeSelected && (newClassificationCode && rec.AdditionalCode && newClassificationCode.toLowerCase() === rec.AdditionalCode.toLowerCase()));

    if (doesExistIndex > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'This code already exists for this type' });
      return true;
    }
    return false;
  }

  

  searchClassification(event: any) {
    this._newClassificationDesc = '';
    //this._disableClassificationDesc = false;
    this._hideClassificationDesc = false;

    if (!this._newClassificationTypeSelected) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please select the Classificaton Type' });
      return ;
    }

    //this._classificationSuggestions = [];
    const serviceClassFn = this.invokeClassificationServiceFn(event.query);
    if (!serviceClassFn) { return; };

    serviceClassFn
      .subscribe({
        next: (response: any) => {
          console.log(response);
          if (!response || !response.length) {
            this._classificationSuggestions = [];
            return;
          }
          const tempSuggestions = [];
          for (const classification of response) {
            const { id, name } = classification;
            tempSuggestions.push({ code: `${id}|${name}`, name: `${id} - ${name}` });
          }
          this._classificationSuggestions = tempSuggestions;
          console.log(this._classificationSuggestions);

        }, error: error => {
          console.error(error);
          this._classificationSuggestions = [];
        }
      });
  }

  selectClassfication(event: any) {
    if (!event || !event.code) return;
    //this._disableClassificationDesc = true;
    this._hideClassificationDesc = true;

    const codeWithName = event.code.split('|');
    this._newClassificationDesc = codeWithName[1];
    this._showNewClassificationCodeAsReadOnly = true;
    this._newClassificationCodeNemeAsArr = [this._newClassificationCode.name];
  }

  invokeClassificationServiceFn(q: string): Observable<any>| null {
    switch (this._newClassificationTypeSelected) {
      case 'FDB':
        return this.classificationContainerService['searchFDBTherapeuticClasses'](q);
      case 'ATC':
        return this.classificationContainerService['searchATCLookups'](q);
      case 'BNF':
        return this.classificationContainerService['searchBNFLookups'](q);
      default:
        return null;
    }
  }

  removeNewClassificationCodeAsReadOnly(e: any) {
    this._showNewClassificationCodeAsReadOnly = false;
    this._newClassificationCode = null;
    this._newClassificationDesc = null;
    this._newClassificationCodeNemeAsArr = [];
  }

  //onKeyUp(event: KeyboardEvent) {
  //  if (event.key == "Enter") {
  //    let tokenInput = event.srcElement as any;
  //    if (tokenInput.value) {
  //      this._selectedClassification = tokenInput.value;//.push(tokenInput.value);
  //      tokenInput.value = "";
  //    }
  //  }
  //}
}
