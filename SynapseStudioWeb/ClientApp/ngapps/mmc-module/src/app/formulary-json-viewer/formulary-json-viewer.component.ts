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
import {  Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormularyJSONViewerService } from './formulary-json-viewer.service';

@Component({
  selector: 'app-formulary-json-viewer',
  templateUrl: './formulary-json-viewer.component.html',
  styleUrls: ['./formulary-json-viewer.component.scss'],
  providers: [FormularyJSONViewerService]
})
export class FormularyJSONViewerComponent implements OnInit {

  _data: any;
  _sectionsToShow = ['all'];

  _propsOnly: showablePropItem[] | null = [];

  _props: showableProps[] | null = [];

  @Input()
  set data(val: any) {
    if (this._data !== val || (val && this._data && val.randomVal !== this._data.randomVal)) {
      this._data = val;
      setTimeout(()=> this.buildVM(), 0);
    }
  }

  @Input()
  set sectionsToShow(val: string[]) {
    if (!val || !val.length) {
      this._sectionsToShow = ['all'];
      return;
    } else {
      this._sectionsToShow = [...val];
    }
  }
  // @Input()
  // set heightInPX(val: string) {
  //   this._customStyle.height = val;
  // }
  @ViewChild('op', { static: false }) overlaypanel: any;

  _detailKeys: string[] = [];

  constructor() { }

  ngOnInit(): void {
    //this._data = SampleData;//to test
    //this.buildVM();//to test only
  }

  buildVM() {
    this._props = new Array<showableProps>();
    this._propsOnly = [];

    if(!this._data || !this._data['Active'] || !this._data['Draft']) return;

  
    const canShowDetail = this._sectionsToShow.includes('all') || this._sectionsToShow.includes('ProductDetails') || this._sectionsToShow.includes('InValid') || this._sectionsToShow.includes('"IsDeleted"');
    const canShowPosology = this._sectionsToShow.includes('all') || this._sectionsToShow.includes('Posology');
    const canShowGuidance = this._sectionsToShow.includes('all') || this._sectionsToShow.includes('Guidance');
    const canShowFlags = this._sectionsToShow.includes('all') || this._sectionsToShow.includes('FlagsClassification');
    const detailProps = canShowDetail ? this.getProps('Detail', ['Additional Code', 'Local Licensed Use', 'Local Unlicensed Use']): null;
    const posologyProps = canShowPosology ? this.getProps('Posology', ['Ingredient', 'Local Route', 'Route', 'Excipient']): null;
    const guidanceProps = canShowGuidance ? this.getProps('Guidance') : null;
    const flagsProps = canShowFlags ? this.getProps('Flags') : null;

    if (detailProps) {
      this._props.push(detailProps);
      detailProps.properties.forEach(r=> this._propsOnly?.push(r));
    }

    if (posologyProps) {
      this._props.push(posologyProps);
      posologyProps.properties.forEach(r=> this._propsOnly?.push(r));
    }

    if (guidanceProps) {
      this._props.push(guidanceProps);
      guidanceProps.properties.forEach(r=> this._propsOnly?.push(r));
    }

    if (flagsProps) {
      this._props.push(flagsProps);
      flagsProps.properties.forEach(r=> this._propsOnly?.push(r));
    }
  }

  getProps(sectionName: string, arrPropToLookFor?: string[]) {
    const detailProps = new showableProps();

    detailProps.header = sectionName;

    let activeKeysInDetail: string[] = [];
    let draftKeysInDetail: string[] = [];

    const detailActiveObj = this._data?.Active[sectionName];
    const detailDraftObj = this._data?.Draft[sectionName];

    if(!detailActiveObj && !detailDraftObj) return null;
    
    if (detailActiveObj)
      activeKeysInDetail = Object.keys(detailActiveObj);
    if (detailDraftObj)
      draftKeysInDetail = Object.keys(detailDraftObj);

    let detailKeys = [...activeKeysInDetail, ...draftKeysInDetail];
    //console.log(detailKeys);
    if (!detailKeys) return null;

    detailKeys = [...new Set(detailKeys)];

    detailProps.properties = [];
    detailKeys.forEach(k => {
      const prop = new showablePropItem();
      prop.name = k;
      if (detailActiveObj && detailActiveObj[k]) {
        
        if (!prop.actives) prop.actives = [];

        if (arrPropToLookFor && arrPropToLookFor.includes(k)) {
          if (detailActiveObj[k] && detailActiveObj[k].length)
            detailActiveObj[k].forEach((r: any) => {
              const recObjKeys = Object.keys(r);
              if (recObjKeys && recObjKeys.length)
                prop.actives.push(recObjKeys.map(k => `${k}:${r[k]}`).join('|'));
            });
        }
        else
          prop.actives.push(detailActiveObj[k]);
      }
      if (detailDraftObj && detailDraftObj[k]) {
        
        if (!prop.drafts) prop.drafts = [];

        if (arrPropToLookFor && arrPropToLookFor.includes(k)) {
          if (detailDraftObj[k] && detailDraftObj[k].length)
            detailDraftObj[k].forEach((r: any) => {
              const recObjKeys = Object.keys(r);
              if (recObjKeys && recObjKeys.length)
                prop.drafts.push(recObjKeys.map(k => `${k}:${r[k]}`).join('|'));
            });
        }
        else
          prop.drafts.push(detailDraftObj[k]);
      }
      detailProps.properties.push(prop);
    });
    return detailProps;
  }


  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }
  showOverlay(e: any) {
    this.overlaypanel.show(e);
  }

  hideOverlay(e: any) {

  }

}

export class showableProps {
  header: string = '';
  //property: { [key: string]: string[] } = {};
  properties: showablePropItem[] = [];
}

export class showablePropItem {
  name: string = '';
  actives: string[] = [];
  drafts: string[] = [];
}




export const SampleData: any = JSON.parse('{"Active":{"Detail":{"ParentCode":"443311001","VmpId":"443311001","BasisOfPreferredNameCd":"1","FormularyAdditionalCode":[{"AdditionalCode":"0203020X0AAAAAA","AdditionalCodeSystem":"BNF","AdditionalCodeDesc":"Dronedarone 400mg tablets","Source":"DMD","CodeType":"Classification"},{"AdditionalCode":"C01BD07","AdditionalCodeSystem":"ATC","AdditionalCodeDesc":"dronedarone","Source":"DMD","CodeType":"Classification"}]},"Guidance":{"ControlledDrugCategoryCd":"0"},"Posology":{"FormularyIngredient":[{"IngredientCd":"703121007","BasisOfPharmaceuticalStrengthCd":"2","StrengthValueNumerator":"400","StrengthValueNumeratorUnitCd":"258684004","StrengthValueDenominator":null,"StrengthValueDenominatorUnitCd":null,"IngredientName":null,"BasisOfPharmaceuticalStrengthDesc":null,"StrengthValueNumeratorUnitDesc":null,"StrengthValueDenominatorUnitDesc":null}],"FormularyRouteDetail":[{"RouteCd":"26643006","RouteFieldTypeCd":"003","Source":"DMD","RouteDesc":null}]}},"Draft":{"Detail":{"ParentCode":"2242434","VmpId":"39691711000001107","BasisOfPreferredNameCd":"2","FormularyAdditionalCode":[{"AdditionalCode":"23232","AdditionalCodeSystem":"BNF","AdditionalCodeDesc":"Dronedarone 400mg tablets","Source":"DMD","CodeType":"Classification"}]},"Guidance":{"ControlledDrugCategoryCd":"2"},"Posology":{"FormularyIngredient":[{"IngredientCd":"weew22323","BasisOfPharmaceuticalStrengthCd":"2","StrengthValueNumerator":"400","StrengthValueNumeratorUnitCd":"258684004","StrengthValueDenominator":null,"StrengthValueDenominatorUnitCd":null,"IngredientName":null,"BasisOfPharmaceuticalStrengthDesc":null,"StrengthValueNumeratorUnitDesc":null,"StrengthValueDenominatorUnitDesc":null}],"FormularyRouteDetail":[{"RouteCd":"2","RouteFieldTypeCd":"003","Source":"DMD","RouteDesc":null}]}}}');
