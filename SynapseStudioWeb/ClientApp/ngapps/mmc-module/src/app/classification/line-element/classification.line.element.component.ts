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
import { ConfirmationService, MessageService } from 'primeng/api';
import { FormularyAdditionalCode } from '../../Models/FormularyAdditionalCode';
import { GroupedClassification } from '../../Models/GroupedClassification';
import { SelectList } from '../../Models/SelectList';

@Component({
  selector: 'classification-line-el',
  templateUrl: './classification.line.element.component.html',
  styleUrls: ['./classification.line.element.component.scss'],
  providers: [MessageService, ConfirmationService]
})
export class ClassificationLineElementComponent implements OnInit {

  _groupedClassification: GroupedClassification | null = null;
  _classificationtypes: SelectList[] | null = [];
  _productType = '';
  _allowAllDelete = false;

  get groupedClassification(): GroupedClassification | null {
    return this._groupedClassification;
  }

  @Input()
  set productType(val: string) {
    this._productType = val;
  }

  @Input()
  set allowAllDelete(val: boolean) {
    this._allowAllDelete = val;
  }

  @Input()
  set groupedClassification(data: GroupedClassification | null) {
    console.log('groupedClassification========', data)
    this._groupedClassification = data;
  }

  get classificationtypes(): SelectList[] | null {
    return this._classificationtypes;
  }
  @Input()
  set classificationtypes(data: SelectList[] | null) {
    this._classificationtypes = data;
    if (data) {
      const input: SelectList[] = data;
      this._classificationtypes = input.map(rec => {
        if (rec.Value === "customgroup")
          rec.Disabled = false;
        return rec;
      });
    }
  }

  @Output() selectDefaultAdditionalCodeEvent = new EventEmitter<FormularyAdditionalCode>();
  @Output() deleteElementEvent = new EventEmitter<GroupedClassification>();
  @Output() deleteAssociationEvent = new EventEmitter<{ group: GroupedClassification | null, additionalCode: FormularyAdditionalCode }>();

  constructor(private confirmationService: ConfirmationService, private messageService: MessageService) { }

  ngOnInit(): void {
  }

  //selectAdditionalCode(e: any) {
  selectAdditionalCode(e: FormularyAdditionalCode) {
    if (this._groupedClassification)
      this._groupedClassification.selectedCode = e;

    const changedAdditionalCode: FormularyAdditionalCode = e;// e.value;
    this.selectDefaultAdditionalCodeEvent?.emit(changedAdditionalCode);
    return;
  }


  deleteElement(paramVal: any) {
    if (!paramVal || !paramVal.groupedClassification) return;

    this.messageService.clear();
    this.messageService.add({ data: { event: 'delElem', ...paramVal }, key: 'c', sticky: true, severity: 'warn', summary: 'Are you sure you want to delete?', detail: 'Confirm to delete' });
    paramVal.e.preventDefault();
    return;
  }

  onConfirm(paramVal: any) {
    this.messageService.clear('c');
    if (paramVal.event === 'delElem')
      this.deleteElementEvent?.emit(paramVal.groupedClassification);
    else if (paramVal.event === 'delAssoc')
      this.deleteAssociationEvent?.emit({ group: this._groupedClassification, additionalCode: paramVal.item });
  }

  onReject() {
    this.messageService.clear('c');
  }

  deleteAssociation(paramVal: { e: any, item: FormularyAdditionalCode }) {
    //if (!item || !this.groupedClassification || !this.groupedClassification.additionalCodes) return;
    //this.deleteAssociationEvent?.emit({ group: this._groupedClassification, additionalCode: item });
    //return;
    if (!paramVal || !paramVal.item || !this.groupedClassification || !this.groupedClassification.additionalCodes) return
    this.messageService.clear();
    this.messageService.add({ data: { event: 'delAssoc', item: paramVal.item }, key: 'c', sticky: true, severity: 'warn', summary: 'Are you sure you want to delete?', detail: 'Confirm to delete' });
    paramVal.e.preventDefault();
    return;
  }
}

