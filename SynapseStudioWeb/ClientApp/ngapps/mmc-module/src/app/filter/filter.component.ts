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
import { Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { CodeNameKeyValuePair } from '../Models/CodeNameKeyValuePair';
import { CdDescKeyValuePair } from '../Models/KeyValuePair';
import { TreeNode } from '../Models/TreeNode';
import { MenuItem, MessageService } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { SearchPageService } from './filter.service';
import { ContextMenu } from 'primeng/contextmenu';
import { DMDSNOMEDVersion } from './filter.component.model';
import { Task } from '../Models/Task';
import { DMDItemSessionStorageService } from './dmditemstorage.service';

declare var $: any;

@Component({
  selector: 'app-filter',
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss'],
  providers: [SearchPageService, MessageService, DMDItemSessionStorageService]
})

export class FilterComponent implements OnInit, OnDestroy {
  FORMULARY_BULK_SELECT_IDs = 'FORMULARY_BULK_SELECT_IDs';
  FORMULARY_BULK_SELECT_LEVEL = 'FORMULARY_BULK_SELECT_LEVEL';
  FORMULARY_BULK_SELECT_STATUS = 'FORMULARY_BULK_SELECT_STATUS';

  allFlags: CodeNameKeyValuePair[] = [
    { code: "BlackTriangle", name: "Has Black Triangle" },
    { code: "BloodProduct", name: "Blood Product" },
    { code: "ClinicalTrialMedication", name: "Clinical Trial Medication" },
    { code: "CriticalDrug", name: "Critical Drug" },
    { code: "CFCFree", name: "CFC Free" },
    { code: "CustomControlledDrug", name: "Custom Controlled Drug" },
    { code: "Diluent", name: "Diluent" },
    { code: "EMAAdditionalMonitoring", name: "EMA Additional Monitoring" },
    { code: "ExpensiveMedication", name: "Expensive Medication" },
    { code: "GastroResistant", name: "Gastro Resistant" },
    { code: "GlutenFree", name: "Gluten Free" },
    { code: "HighAlertMedication", name: "High Alert Medication" },
    { code: "IgnoreDuplicateWarnings", name: "Ignore Duplicate Warnings" },
    { code: "IsIndicationMandatory", name: "Indication Mandatory" },
    { code: "IVtoOral", name: "IV to Oral" },
    { code: "ModifiedRelease", name: "Modified Release" },
    { code: "NotforPRN", name: "Not for PRN" },
    { code: "OutpatientMedication", name: "Outpatient Medication" },
    { code: "Prescribable", name: "Prescribable" },
    { code: "Parallelimport", name: "Parallel import" },
    { code: "PreservativeFree", name: "Preservative Free" },
    { code: "SugarFree", name: "Sugar Free" },
    { code: "UnlicensedMedication", name: "Unlicensed Medication" },
    { code: "WitnessingRequired", name: "Witnessing Required" },
    { name: "Deleted in DMD", code: "IsDMDDeleted" },
    { name: "Invalid in DMD", code: "IsDMDInvalid" }
  ];

  vtmVMPFlags = [
    { code: "Prescribable", name: "Prescribable" },
    { code: "WitnessingRequired", name: "Witnessing Required" },
    { name: "Deleted in DMD", code: "IsDMDDeleted" },
    { name: "Invalid in DMD", code: "IsDMDInvalid" }
  ];

  _canShow = false;
  _isdmdbrowser = false;
  _isSelectAllDisabled = true;
  _selectAllProducts = false;
  _deselectAllProducts = false;
  _showLoadingSelectAllProducts = false;
  _disableSelectAllProducts = false;
  _enableimportonly = false;
  _allPagesData: any | null = null;
  _showBulkUpdateStatusView = false;

  @Input()
  set refreshsearch(val: boolean | null) {
    if (val) {
      setTimeout(() => { this.paginator?.changePage(0) }, 0);
      this.applySettings();
    }
  }

  @Input()
  set enableimportonly(val: string | null) {
    if (val && val === 'true')
      this._enableimportonly = true;
    else
      this._enableimportonly = false;
  }

  @Input()
  set isdmdbrowser(val: string | null) {
    if (val && val === 'true') {
      this._isdmdbrowser = true;
      this.overwriteLookups();
    }
    else
      this._isdmdbrowser = false;
  }

  @Input()
  set canShow(val: boolean) {
    this._canShow = val;
  }

  _formularyVersionIds: any;

  @Input() set selectFormularies(formularyVersionIds: any) {
    this._formularyVersionIds = formularyVersionIds;

    if (!this._formularyVersionIds) {
      this.deselectAllCheckbox();
      this.isCheckboxChecked = false;
      setTimeout(() => { this.paginator?.changePage(0) }, 0);
      this.applySettings();
    }
    else {
      let formularyVersionIds: any[] = []
      setTimeout(() => { this.paginator?.changePage(0) }, 0);
      this.treeNode.forEach((vtm) => {
        if (vtm.data.isChecked) {
          formularyVersionIds.push(vtm.data.formularyversionid);
        }

        vtm.children?.forEach((vmp) => {
          if (vmp.data.isChecked) {
            formularyVersionIds.push(vmp.data.formularyversionid);
          }

          vmp.children?.forEach((amp) => {
            if (amp.data.isChecked) {
              formularyVersionIds.push(amp.data.formularyversionid);
            }
          });
        });
      });

      let differences = formularyVersionIds.filter((x: any) => !this._formularyVersionIds.includes(x));

      differences.forEach((difference) => {
        this.treeNode.forEach((vtm) => {
          if (vtm.data.formularyversionid == difference) {
            vtm.data.isChecked = false;
          }

          vtm.children?.forEach((vmp) => {
            if (vmp.data.formularyversionid == difference) {
              vmp.data.isChecked = false;
            }

            vmp.children?.forEach((amp) => {
              if (amp.data.formularyversionid == difference) {
                amp.data.isChecked = false;
              }
            });
          });
        });
      });
      
      $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(this._formularyVersionIds).length);
      $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
      //sessionStorage.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(this._formularyVersionIds));

    }
  }

  @Input() editFormulary: any;

  @Input() showReImport: any;
  @Input() showCustomMedication: any;
  @Input() showHistory: any;
  @Input() showBulkEdit: any;

  search: string = '';

  recordStatuses: CdDescKeyValuePair[] = [];

  formularyStatuses: CdDescKeyValuePair[] = [];

  showFormularyStatus = true;

  showHideArchivedCntrl = true;

  flags: CodeNameKeyValuePair[] = this.allFlags;

  productTypes: CodeNameKeyValuePair[] = [];

  selectedRecordStatuses: CdDescKeyValuePair[] = [];

  selectedFormularyStatuses: CdDescKeyValuePair[] = [];

  selectedFlags: CodeNameKeyValuePair[] = [];

  selectedProductTypes?: CodeNameKeyValuePair;

  isHideArchived?: boolean | null = true;

  categories: CodeNameKeyValuePair[] = [];

  selectedCategories: CodeNameKeyValuePair[] = [];

  treeNode: TreeNode[] = [];

  statuses: CdDescKeyValuePair[] = [];

  selectedNodes: TreeNode[] = [];

  //cols: any[] = [];

  isDisabled: boolean = false;

  totalRecords: number = 0;

  recStat: string[] = [];

  items: MenuItem[] = [];

  selectedNode: TreeNode | undefined;

  differenceData: any;

  ampCodes: any[] = [];

  loading: boolean = false;

  editIcon: string = this.buildUrl('/img/Capsule.svg');

  isShowDiv = false;

  displayDialog = false;

  reason: string = '';

  showReason = false;

  status: string = '';

  formularyVersionId: string = '';

  orgStatusRowDataSelected: any;

  oldStatusValue: any;

  hasHandledChangeStatus: boolean = false;

  categoryDiffSelected: string[] = [];

  showFileUploadInterface = false;

  imgLkp: any = {
    'VTM': { src: this.buildUrl('/img/Medicationtypenotdefined.svg'), title: 'VTM' },
    'VMP': { src: this.buildUrl('/img/Capsule.svg'), title: 'VMP' },
    'AMP': { src: this.buildUrl('/img/ActualMedicinalProduct.svg'), title: 'AMP' },
    '': { src: '', title: '' },
  };

  isCheckboxChecked: boolean = false;

  isInitLoad: boolean = false;
  _dmdVersion: string | null = null;
  _dmdVersionCreated: string | null = null;
  _showSearchCriteriaError = false;
  _showDeltaInProgress = false;
  _showRefreshForDeltaCalcComplete = false;
  _deltaCalcInterval: any = null;

  @ViewChild('paginator') paginator: Paginator | undefined

  constructor(private http: HttpClient, private searchPageService: SearchPageService, private messageService: MessageService, private dmdItemSessionStorageService: DMDItemSessionStorageService) {
    if (this.dmdItemSessionStorageService)
      this.dmdItemSessionStorageService.dispose();
  }

  ngOnDestroy(): void {
    if (this._deltaCalcInterval)
      clearInterval(this._deltaCalcInterval);
    if (this.dmdItemSessionStorageService)
      this.dmdItemSessionStorageService.dispose();
  }

  ngOnInit(): void {

    this._allPagesData = null;
    this.showFormularyStatus = true;
    this.showHideArchivedCntrl = true;

    this.searchPageService.getDMDSNOMEDVersion().subscribe({
      next: (response: DMDSNOMEDVersion | null) => {
        if (response) {
          this._dmdVersion = response.dmdVersion;
          this._dmdVersionCreated = response.createdtimestamp;
        }
      },
      error: (error: any) => {
        console.error('There was an error!', error);
      }
    });

    this.productTypes = [
      { name: 'VTM Only', code: 'VTM' },
      { name: 'VMP Only', code: 'VMP' },
      { name: 'AMP Only', code: 'AMP' }
    ];

    this.getRecordStatus();

    this.getFormularyStatus();


    this.categories = [
      { name: "Product Details", code: "ProductDetails" },
      { name: "Posology", code: "Posology" },
      { name: "Guidance", code: "Guidance" },
      { name: "Flags / Classification", code: "FlagsClassification" },
      { name: "Deleted in DMD", code: "IsDeleted" },
      { name: "Invalid in DMD", code: "InValid" }
    ];

    this.statuses = [
      { desc: 'Draft', cd: '001' },
      { desc: 'Active', cd: '003' },
      { desc: 'Archived', cd: '004' },
      { desc: 'Deleted', cd: '006' },
      //{ desc: 'Ready For Review', cd: '002' }
    ];

    //this.cols = [
    //  { field: 'name', header: 'Name' },
    //  { field: 'status', header: 'Status' }
    //];

    this.isInitLoad = true;

    if (this.isdmdbrowser) {
      this.applySettings();
    }
    else {
      this.isShowDiv = true;
    }

    this._deltaCalcInterval = setInterval(() => this.indicateIfDeltaCalcInProgress(), 6000);
  }

  toggleDisplayFilter() {
    this.isShowDiv = !this.isShowDiv;
  }

  reImportMedication() {
    if (this.showReImport) this.showReImport();
  }

  addCustomMedication() {
    if (this.showCustomMedication) this.showCustomMedication();
  }

  displayHistory() {
    if (this.showHistory) this.showHistory();
  }

  displayFileUploadInterface() {
    this.showFileUploadInterface = true;
  }
  
  displayBulkEdit() {
    if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "VTM" || this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "VMP") {
      if (this.showBulkEdit)
        this.showBulkEdit();
      return;
    }
    const selectedData = this._allPagesData || this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);

    if (selectedData) {
      this.messageService.add({ key: 'bulkeditconfirm', sticky: true, severity: 'warn', summary: 'Are you sure?', detail: 'Confirm to proceed' });
    }
  }

  onImportDialogCloseDialog() {
    this.showFileUploadInterface = false;
  }

  //selectAllVMP(node: any) {

  //  if (node.data.level == "VTM") {
  //    node.expanded = true;
  //    this.getChildren(node, 'VMP');
  //  }

  //  this.treeNode = [...this.treeNode];
  //}

  //selectAllAMP(node: any) {

  //  if (node.data.level == "VMP") {
  //    node.expanded = true;

  //    this.getChildren(node, 'AMP');
  //  }

  //  if (node.data.level == "VTM") {
  //    node.expanded = true;

  //    this.getChildren(node, 'AMP');
  //  }

  //  this.treeNode = [...this.treeNode];
  //}

  //selectAllAMPsByStatus(node: any, status: string) {
  //  if (node.data.level == "VMP") {
  //    node.expanded = true;
  //    this.getAMPsByStatus(node, status);
  //  }

  //  if (node.data.level == "VTM") {
  //    node.expanded = true;
  //    this.getAMPsByStatus(node, status);
  //  }

  //  this.treeNode = [...this.treeNode];
  //}

  //selectAllReadyForReviewAMP(node: any) {
  //  if (node.data.level == "VMP") {
  //    node.expanded = true;
  //    this.getReadyForReviewAMPs(node);
  //  }
  //  this.treeNode = [...this.treeNode];
  //}

  buildUrl(actionurl: string): string {
    const basePath = (typeof (window.AppPath) !== 'undefined' && window.AppPath) || '';
    return actionurl[0] !== '/' ? basePath + "/" + actionurl : basePath + actionurl;
  }

  overwriteLookups() {
    this.allFlags = [
      { code: "CFCFree", name: "CFC Free" },
      { code: "CustomControlledDrug", name: "Custom Controlled Drug" },
      { code: "EMAAdditionalMonitoring", name: "EMA Additional Monitoring" },
      { code: "GastroResistant", name: "Gastro Resistant" },
      { code: "GlutenFree", name: "Gluten Free" },
      { code: "ModifiedRelease", name: "Modified Release" },
      { code: "Parallelimport", name: "Parallel import" },
      { code: "PreservativeFree", name: "Preservative Free" },
      { code: "SugarFree", name: "Sugar Free" },
      { code: "UnlicensedMedication", name: "Unlicensed Medication" },
      { name: "Deleted in DMD", code: "IsDMDDeleted" },
      { name: "Invalid in DMD", code: "IsDMDInvalid" }
    ];
    this.flags = this.allFlags;

    this.vtmVMPFlags = [
      { name: "Deleted in DMD", code: "IsDMDDeleted" },
      { name: "Invalid in DMD", code: "IsDMDInvalid" }
    ];
  }

  getRecordStatus() {
    return this.http.get<any>(this.buildUrl('/Formulary/GetRecordStatusLookup'))
      .subscribe({
        next: (response: any) => {

          if (!response) {
            this.recordStatuses = [];
            return;
          }
          for (var i = 0; i < response.length; i++) {
            if (response[i].cd == '005' || response[i].cd == '004') {
              response.splice(i, 1);
              i--;
            }
          }

          this.recordStatuses = response;
        },
        error: (error: any) => {
          console.error('There was an error!', error);
        }
      });
  }

  getFormularyStatus() {
    return this.http.get<any>(this.buildUrl('/Formulary/GetFormularyStatusLookup'))
      .subscribe({
        next: (response: any) => {
          this.formularyStatuses = response;
        },
        error: (error: any) => {
          console.error('There was an error!', error);
        }
      });
  }

  onNodeExpand(event: any) {

    if (event.node.children && event.node.children.length) return;

    let selNodeData: { [index: string]: any } = {};

    const selectedRecordsAsString = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);

    if (selectedRecordsAsString)
      selNodeData = JSON.parse(selectedRecordsAsString);

    const searchCriteria = {
      hideArchived: this.isHideArchived,
      formularyCode: event.node.data.code,
      formularyVersionId: event.node.data.formularyversionid
    };

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
    }

    this.http.post(this.buildUrl('/Formulary/LoadChildrenFormularies_New'), searchCriteria, httpOptions)
      .subscribe({
        next: (response: any) => {

          if (!response) {
            event.node.leaf = true;
            return;
          }
          //Could have been moved to common fn. But need to follow IN standards :(.
          const node = event.node;

          response.forEach((result: any) => {
            const treeNode: TreeNode = new TreeNode();
            const { title, key, formularyVersionId, data, formularyId } = result;
            const imageInfo = this.getImageInfo(data.Level);
            treeNode.data = {
              name: title,
              code: key,
              formularyversionid: formularyVersionId,
              formularyid: formularyId,
              status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
              level: data.Level,
              isChecked: selNodeData[formularyVersionId] ? true : false,
              isDisabled: false,
              imgSrc: imageInfo.src,
              imgTitle: imageInfo.title,
            };

            treeNode.expanded = false;
            treeNode.leaf = data.Level == 'AMP';

            if (selNodeData[formularyVersionId]) {
              this.isCheckboxChecked = true;
            }

            if (data.Level == 'AMP') {
              this.ampCodes.push(treeNode.data.code);
            }

            if ((this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) != treeNode.data.level) ||
              (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) != treeNode.data.status.desc)) {
              treeNode.data.isDisabled = true;
            }

            //tr.push(treeNode);
            if (!node.children)
              node.children = [treeNode];
            else
              node.children.push(treeNode);
          });

          //node.children = tr;

          this.http.post(this.buildUrl('/Formulary/GetFormularyChangeLogForFormularyIds'), this.ampCodes, httpOptions)
            .subscribe({
              next: (response: any) => {
                //this.differenceData = response;
                this.attachRandomValToDifferenceDataAndPopulate(response);
              },
              error: (error: any) => {
                console.error('There was an error!', error);
              }
            });

          this.treeNode = [...this.treeNode];
        },
        error: (error: any) => {
          console.error('There was an error!', error);
        }
      });
  }

  attachRandomValToDifferenceDataAndPopulate(diffData: any) {
    if (!diffData) {
      this.differenceData = diffData;
      return;
    }
    const allFormularyIdKeys = Object.keys(diffData);
    if (!allFormularyIdKeys) return;
    const randomNum = this.generateRandomNum();
    allFormularyIdKeys.forEach(formularyId => {
      try {
        diffData[formularyId] = JSON.parse(diffData[formularyId]);
      } catch { }
      diffData[formularyId]['randomVal'] = randomNum;
    });
    this.differenceData = diffData;
  }
  _isPaginationSourceApplySettings = false;

  applySettings() {

    this._showRefreshForDeltaCalcComplete = false;

    this.clearBulkEditSelections();

    this.loading = true;

    var selNodeData: { [index: string]: any } = {};

    let selectedRecordsAsString = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);

    if (selectedRecordsAsString)
      selNodeData = JSON.parse(selectedRecordsAsString);

    let recordStatuses = '';

    this._isSelectAllDisabled = true;

    if (this.selectedProductTypes) {
      if (this.selectedProductTypes.code == 'AMP') {
        if (this.selectedRecordStatuses.length > 0) {
          this._isSelectAllDisabled = (this.selectedRecordStatuses.length !== 1);
          this.selectedRecordStatuses.forEach((recordStatus) => {
            recordStatuses = recordStatuses + 'Rec|' + recordStatus + ',';
          });
        } else {
          this._isSelectAllDisabled = true;
        }
      } else {
        this._isSelectAllDisabled = false;
      }
    }

    let formularyStatuses = '';

    if (this.selectedFormularyStatuses.length > 0) {
      this.selectedFormularyStatuses.forEach((formularyStatus) => {
        formularyStatuses = formularyStatuses + 'Form|' + formularyStatus + ',';
      })
    }

    let flags = '';

    if (this.selectedFlags.length > 0) {
      this.selectedFlags.forEach((flag) => {
        flags = flags + 'Flags|' + flag + ',';
      })
    }

    let categoryDiff = '';
    this.categoryDiffSelected = [];
    if (this.selectedProductTypes && this.selectedProductTypes.code == 'AMP' && this.selectedRecordStatuses && this.selectedRecordStatuses.length > 0 && this.recStat && this.recStat.length > 0 && this.recStat[0] == '001' && this.selectedCategories.length > 0) {
      this.selectedCategories.forEach((cat) => {
        categoryDiff = categoryDiff + 'Cat|' + cat.code + ',';
        this.categoryDiffSelected.push(cat.code);
      });
    }

    let recStatusCds = '';

    if (recordStatuses != '') {
      recordStatuses = recordStatuses;
    }
    else {
      recordStatuses = '';
    }

    if (formularyStatuses != '') {
      formularyStatuses = formularyStatuses;
    }
    else {
      formularyStatuses = '';
    }

    if (flags != '') {
      flags = flags;
    }
    else {
      flags = '';
    }

    if (categoryDiff != '') {
      categoryDiff = categoryDiff;
    }
    else {
      categoryDiff = '';
    }

    recStatusCds = recordStatuses + formularyStatuses + flags + categoryDiff;

    let searchCriteria: any = {
      searchTerm: this.search,
      hideArchived: this.isHideArchived,
      recStatusCds: recStatusCds.replace(/,\s*$/, ""),
      productType: this.selectedProductTypes?.code
    };
    this._showSearchCriteriaError = false;
    if (!searchCriteria.searchTerm && !searchCriteria.recStatusCds && !searchCriteria.productType) {
      this.loading = false;
      this._showSearchCriteriaError = true;
      this.messageService.add({ severity: 'error', summary: 'Missing Search Criteria', detail: 'Please enter search text or select filter attributes.' });
      return;
    }

    //mmc-580 - no initial load
    this.isInitLoad = false;
    //if (this.isInitLoad) {
    //  searchCriteria = {
    //    searchTerm: null,
    //    hideArchived: true,//null
    //    recStatusCds: null,
    //    productType: null
    //  };
    //  this.isInitLoad = false;
    //}

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }

    this.ampCodes = [];

    if (searchCriteria && (searchCriteria.hideArchived && (!searchCriteria.searchTerm && !searchCriteria.recStatusCds && !searchCriteria.productType)))
      searchCriteria.hideArchived = true;

    this.http.post<any>(this.buildUrl('/Formulary/LoadFormularyList_New'), searchCriteria, httpOptions)
      .subscribe({
        next: (response: any) => {

          if (!response || !response.totalRecords) {
            this.treeNode = [];
            this.totalRecords = 0;
            this.loading = false;
            return;
          }

          this.totalRecords = response.totalRecords;
          this.treeNode = this.buildTreeNodeVMFromFormularies(response.results, selNodeData);

          /*
          let tr: TreeNode[] = [];

          response.results.forEach((result: any) => {

            let treeNode: TreeNode = new TreeNode();
            const { title, key, formularyVersionId, data } = result;
            const imageInfo = this.getImageInfo(data.Level);
            treeNode.data = {
              name: title,
              code: key,
              formularyversionid: formularyVersionId,
              status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
              level: data.Level,
              isChecked: selNodeData[formularyVersionId] ? true : false,
              isDisabled: false,
              imgSrc: imageInfo.src,
              imgTitle: imageInfo.title,
            };

            treeNode.expanded = false;
            treeNode.leaf = (data.Level == 'AMP' || (this.selectedProductTypes && this.selectedProductTypes.code != '')) != undefined ? true : false;

            if (selNodeData[formularyVersionId]) {
              this.isCheckboxChecked = true;
            }

            if (data.Level == 'AMP') {
              this.ampCodes.push(treeNode.data.code);
            }

            if (result.children != null && result.children.length > 0) {
              this.bindChildrenData(result.children, treeNode);
            }

            tr.push(treeNode);
          });
          this.treeNode = tr;
          */
          //this.http.post(this.buildUrl('/Formulary/GetFormularyChangeLogForCodesWithChangeDetailOnly'), this.ampCodes, httpOptions)
          //this.searchPageService.getFormularyChangeLogForCodesWithChangeDetailOnly(this.ampCodes
          this.searchPageService.getFormularyChangeLogForFormularyIds(this.ampCodes)
            .subscribe({
              next: (response: any) => {
                //this.differenceData = response;
                this.attachRandomValToDifferenceDataAndPopulate(response);
                this.loading = false;
                this._isPaginationSourceApplySettings = true;
                setTimeout(() => { this.paginator?.changePage(0) }, 0);
              },
              error: (error: any) => {
                console.error('There was an error!', error);
                this.loading = false;
              }
            });

          this.findOtherNonSimilarNodesAndDisable(this.treeNode);
        },
        error: (error: any) => {
          console.error('There was an error!', error);
          this.loading = false;
        }
      });
  }

  clearBulkEditSelections() {
    this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_STATUS);
    this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_LEVEL);
    this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_IDs);
    this.hideBulkEditOption();
    this._deselectAllProducts = false;
    this._selectAllProducts = false;
    this.isCheckboxChecked = false;
    this._disableSelectAllProducts = false;
    this.selectedNodes = [];
    this._allPagesData = null;
  }

  getImageInfo(level: any): { src: string, title: string } {
    if (!level) return { src: '', title: '' };
    return this.imgLkp[level];
  }

  bindChildrenData(children: [], treeNode: TreeNode) {

    if (children != null && children.length > 0) {

      var selNodeData: { [index: string]: any } = {};

      let selectedRecordsAsString = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);

      if (selectedRecordsAsString)
        selNodeData = JSON.parse(selectedRecordsAsString);

      children.forEach((result: any) => {
        let treeNodeChild: TreeNode = new TreeNode();

        const { title, key, formularyVersionId, data, formularyId } = result;
        const imageInfo = this.getImageInfo(data.Level);
        treeNodeChild.data = {
          name: title,
          code: key,
          formularyversionid: formularyVersionId,
          formularyid: formularyId,
          status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
          level: data.Level,
          isChecked: selNodeData[formularyVersionId] ? true : false,
          isDisabled: false,
          imgSrc: imageInfo.src,
          imgTitle: imageInfo.title,
        };

        treeNodeChild.expanded = false;
        treeNodeChild.leaf = data.Level == 'AMP';

        if (selNodeData[formularyVersionId]) {
          this.isCheckboxChecked = true;
        }

        if (data.Level == 'AMP') {
          this.ampCodes.push(treeNodeChild.data.code);
        }

        if (result.children != null && result.children.length > 0) {
          this.bindChildrenData(result.children, treeNodeChild)
        }

        //tr.push(treeNode);
        if (!treeNode.children)
          treeNode.children = [];

        treeNode.children.push(treeNodeChild);
      });

      //this.treeNode = [...this.treeNode];
    }
  }

  generateRandomNum() {
    return Math.floor(Math.random() * 100) + 1;
  }

  getOptions(statusCode: string) {
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      params: new HttpParams({ fromString: 'status=' + statusCode })
    }

    this.http.get(this.buildUrl('/Formulary/GetNextLevelStatuses'), httpOptions)
      .subscribe({
        next: (responses: any) => {
          if (responses && responses.length > 0) {
            for (var index = 0; index < this.statuses.length; ++index) {
              this.statuses[index].disabled = true;
            }
            responses.forEach((response: any) => {
              if (response.value != '' && this.statuses.filter(rec => rec.cd == response.value).length > 0) {
                let sts = this.statuses.filter(rec => rec.cd == response.value);

                sts.forEach((st) => {
                  if (st.cd == '001' || st.cd == '002' || st.cd == '003') {
                    st.cd = response.value;
                    st.desc = response.text;
                    st.disabled = true;
                  }
                  else {
                    st.cd = response.value;
                    st.desc = response.text;
                    st.disabled = false;
                  }

                  let index = this.statuses.indexOf(st);

                  if (index > -1) {
                    this.statuses.splice(index, 1);
                  }

                  this.statuses.push(st);

                })

              }
            });
          }
        },
        error: (error: any) => {
          console.error('There was an error!', error);
        }
      });
  }

  isRowSelected(rowNode: any): boolean {
    return this.selectedNodes.indexOf(rowNode.node) >= 0;
  }

  toggleRowSelection(rowNode: any): void {
    var selNodeData: { [index: string]: any } = {};

    let selectedRecordsAsString = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);

    if (selectedRecordsAsString)
      selNodeData = JSON.parse(selectedRecordsAsString);

    if (this.isRowSelected(rowNode)) {
      rowNode.node.data.isChecked = false;
      this.selectedNodes.splice(this.selectedNodes.indexOf(rowNode.node), 1);

      if (selNodeData[rowNode.node.data.formularyversionid]) {
        delete selNodeData[rowNode.node.data.formularyversionid];
      }

      if (this.selectedNodes.length == 0) {
        this.enableCheckbox(this.treeNode);
        //this.isCheckboxChecked = false;
      }
      else {
        this.findAllSimilarNodes(this.treeNode, rowNode);
      }
    }
    else {
      rowNode.node.data.isChecked = true;
      //this.isCheckboxChecked = true;
      this.selectedNodes.push(rowNode.node);
      this.findAllSimilarNodes(this.treeNode, rowNode);

      selNodeData[rowNode.node.data.formularyversionid] = { code: rowNode.node.data.code, title: rowNode.node.data.name, formularyVersionId: rowNode.node.data.formularyversionid };
    }

    if (!selNodeData || Object.keys(selNodeData).length == 0) {
      $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html('0');
      $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');
      this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_IDs);
      this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_LEVEL);
      this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_STATUS);
      this.enableCheckbox(this.treeNode);
      this.isCheckboxChecked = false;
    }
    else {
      $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(selNodeData).length);
      $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
      this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(selNodeData));
      this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_LEVEL, rowNode.node.data.level);
      this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_STATUS, rowNode.node.data.status.desc);
    }

    this.selectedNodes = [...this.selectedNodes];
  }

  showBulkEditOption(count: any) {
    $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(count);
    $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
  }

  hideBulkEditOption() {
    $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html('0');
    $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');
  }

  findAllSimilarNodes(treeNode: any, rowNode: any): any {
    for (const node of treeNode) {
      if (rowNode.node.data.level != node.data.level || rowNode.node.data.status.cd != node.data.status.cd) {
        node.data.isDisabled = true;
      }

      if (node.children) {
        this.findAllSimilarNodes(node.children, rowNode);
      }
    }

    return false;
  }

  enableCheckbox(treeNode: any): any {
    for (const node of treeNode) {
      if (node.data) {
        node.data.isDisabled = false;
        node.data.isChecked = false;
      }

      if (node.children) {
        this.enableCheckbox(node.children);
      }
    }

    return false;
  }

  paginate(event: any) {
    if (this._isPaginationSourceApplySettings) {
      this._isPaginationSourceApplySettings = false;
      return;
    }
    this.loading = true;

    let selNodeData: { [index: string]: any } = {};

    const selectedRecordsAsString = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);

    if (selectedRecordsAsString)
      selNodeData = JSON.parse(selectedRecordsAsString);

    if (Object.keys(selNodeData).length > 0) {
      $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(selNodeData).length);
      $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
    }
    else {
      $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html('0');
      $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');
    }

    /*
    let pageNumber = event.page + 1;

    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      params: new HttpParams({ fromString: 'pageNumber=' + pageNumber })
    }

    this.http.get(this.buildUrl('/Formulary/GetFormulariesByPageNumber'), httpOptions)
      */
    this.searchPageService.getFormulariesByPageNumber((event.page + 1))
      .subscribe({
        next: (responses: any) => {
          this.treeNode = this.buildTreeNodeVMFromFormularies(responses, selNodeData);
          /*
          this.treeNode = [];

          let tr: TreeNode[] = [];

          responses.forEach((result: any) => {

            let treeNode: TreeNode = new TreeNode();
            const { title, key, formularyVersionId, data } = result;
            const imageInfo = this.getImageInfo(data.Level);

            treeNode.data = {
              name: title,
              code: key,
              formularyversionid: formularyVersionId,
              status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
              level: data.Level,
              isChecked: selNodeData[formularyVersionId] ? true : false,
              isDisabled: false,
              imgSrc: imageInfo.src,
              imgTitle: imageInfo.title,
            };

            treeNode.expanded = false;
            treeNode.leaf = data.Level == 'AMP';

            if (selNodeData[formularyVersionId]) {
              this.isCheckboxChecked = true;
            }

            if (data.Level == 'AMP') {
              this.ampCodes.push(treeNode.data.code);
            }

            if (result.children != null && result.children.length > 0) {
              this.bindChildrenData(result.children, treeNode);
            }

            tr.push(treeNode);
          });
          this.treeNode = tr;
          */
          //this.http.post(this.buildUrl('/Formulary/GetFormularyChangeLogForCodesWithChangeDetailOnly'), this.ampCodes, httpOptions)
          //this.searchPageService.getFormularyChangeLogForCodesWithChangeDetailOnly(this.ampCodes)
          this.searchPageService.getFormularyChangeLogForFormularyIds(this.ampCodes)
            .subscribe({
              next: (response: any) => {
                //this.differenceData = response;
                this.attachRandomValToDifferenceDataAndPopulate(response);
              },
              error: (error: any) => {
                console.error('There was an error!', error);
                this.loading = false;
              }
            });

          this.findOtherNonSimilarNodesAndDisable(this.treeNode);
          this.loading = false;
        },
        error: (error: any) => {
          console.error('There was an error!', error);
          this.loading = false;
        }
      });
  }

  buildTreeNodeVMFromFormularies(responses: any, selNodeData: { [index: string]: any }): TreeNode[] {
    if (!responses || !responses.length) return [];

    const nodes: TreeNode[] = [];
    let isAllNodesChecked = true;

    responses.forEach((result: any) => {

      const treeNode: TreeNode = new TreeNode();
      const { title, key, formularyVersionId, data, formularyId } = result;
      const imageInfo = this.getImageInfo(data.Level);

      const isNodeChecked = selNodeData[formularyVersionId] ? true : false;

      if (!isNodeChecked)
        isAllNodesChecked = false;

      treeNode.data = {
        name: title,
        code: key,
        formularyversionid: formularyVersionId,
        formularyid: formularyId,
        status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
        level: data.Level,
        isChecked: isNodeChecked,
        isDisabled: false,
        imgSrc: imageInfo.src,
        imgTitle: imageInfo.title,
      };

      treeNode.expanded = false;
      treeNode.leaf = (data.Level == 'AMP' || (this.selectedProductTypes && this.selectedProductTypes.code != '')) != undefined ? true : false;

      if (selNodeData[formularyVersionId]) {
        this.isCheckboxChecked = true;
      }

      if (data.Level === 'AMP') {
        this.ampCodes.push(treeNode.data.code);
      }

      if (result.children != null && result.children.length > 0) {
        this.bindChildrenData(result.children, treeNode);
      }

      nodes.push(treeNode);
    });

    this.isCheckboxChecked = isAllNodesChecked;

    return nodes;
  }

  onEdit(rowNode: any) {
    this.editFormulary(rowNode.node);
  }

  onRecordStatusChange(event: any) {
    this.recStat = event.value;
    if (event.value && event.value.length == 0) {
      this.selectedCategories = [];
      this.showHideArchivedCntrl = true;
    } else if (event.value && event.value.length > 0) {
      this.showHideArchivedCntrl = false;
      if (event.value && event.value.length == 1 && event.value[0] == '001') {
        this.selectedCategories = [];
      }
    }
  }

  onContextMenuSelect(event: any, cm: ContextMenu) {
    if (this._isdmdbrowser) {
      this.items = [];
      setTimeout(() => cm.hide());
      return;
    }
    if (event.node.data.level == 'VTM' && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "VTM") {
      this.items = [
        { label: "Quit" }
      ];
    }
    else if (event.node.data.level == 'VMP' && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "VMP") {
      this.items = [
        { label: "Quit" }
      ];
    }
    else if (event.node.data.level == 'AMP' && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {
      this.items = [
        { label: "Quit" }
      ];
    }
    else if (event.node.data.level == 'VTM' && !event.node.children) {
      if (this.selectedProductTypes && this.selectedProductTypes.code != '') {
        this.items = [
          { label: "Quit" }
        ];
      }
      else if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == null) {
        this.items = [
          { label: this.getContextMenuLabel('VMP', 'Select all VMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'VMP', null, 1) },
          { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
          { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },

          { separator: true },
          { label: "Quit" }
        ];
      }
      else if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "VMP") {
        this.items = [
          { label: this.getContextMenuLabel('VMP', 'Select all VMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'VMP', null, 1) },
          { separator: true },
          { label: "Quit" }
        ];
      }
      else if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Active") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
            { separator: true },
            { label: "Quit" }
          ];
        }
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Draft") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },
            { separator: true },
            { label: "Quit" }
          ];
        }
      }
    }
    else if (event.node.data.level == 'VTM' && event.node.children && event.node.children.length > 0) {
      if (this.selectedProductTypes && this.selectedProductTypes.code != '') {
        this.items = [
          { label: "Quit" }
        ];
      }
      else if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == null) {
        this.items = [
          { label: this.getContextMenuLabel('VMP', 'Select all VMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'VMP', null, 1) },
          { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
          { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },
          { separator: true },
          { label: "Quit" }
        ];
      }
      else if (event.node.children.filter((rec: any) => rec.data.isChecked).length == 0 && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "VMP") {
        this.items = [
          { label: this.getContextMenuLabel('VMP', 'Select all VMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'VMP', null, 1) },
          { label: "Quit" }
        ];
      }
      else if (event.node.children.filter((rec: any) => rec.data.isChecked).length > 0
        && event.node.children.filter((rec: any) => rec.data.isChecked).length === event.node.children.length
        && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "VMP") {
        this.items = [
          { label: this.getContextMenuLabel('VMP', 'De-select all VMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'VMP', null, 2) },
          { separator: true },
          { label: "Quit" }
        ];
      }
      else if (event.node.children.filter((rec: any) => rec.data.isChecked).length > 0 && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "VMP") {
        this.items = [
          { label: this.getContextMenuLabel('VMP', 'Select all VMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'VMP', null, 1) },
          { label: this.getContextMenuLabel('AMP', 'De-select all VMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'VMP', null, 2) },
          { separator: true },
          { label: "Quit" }
        ];
      }
      else if (this.notCheckedAndStatusCountMatch(event.node) && this.checkedAndStatusCountMatch(event.node) && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {
        this.items = [
          { label: this.getContextMenuLabel('AMP', 'De-select all AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', null, 2) },
          { label: "Quit" }
        ];
      }
      else if (this.notCheckedAndStatusCountMatch(event.node) && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Active") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
            { separator: true },
            { label: "Quit" }
          ];
        }
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Draft") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },
            { separator: true },
            { label: "Quit" }
          ];
        }
      }
      else if (event.node.children.filter((ch: any) => ch.children.filter((gch: any) => gch.data.isChecked).length > 0).length > 0
        && this.checkedAndStatusCountMatch(event.node)
        && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {
        this.items = [
          { label: this.getContextMenuLabel('AMP', 'De-select all AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', null, 2) },
          { separator: true },
          { label: "Quit" }
        ];
      }
      else if (event.node.children.filter((ch: any) => ch.children.filter((gch: any) => gch.data.isChecked).length > 0).length > 0 && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {

        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Active") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
            { label: this.getContextMenuLabel('AMP', 'De-select all AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', null, 2) },
            { separator: true },
            { label: "Quit" }
          ];
        }
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Draft") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },
            { label: this.getContextMenuLabel('AMP', 'De-select all AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', null, 2) },
            { separator: true },
            { label: "Quit" }
          ];
        }
      }
    }
    else if (event.node.data.level == 'VMP' && !event.node.children) {
      if (this.selectedProductTypes && this.selectedProductTypes.code != '') {
        this.items = [
          { label: "Quit" }
        ];
      }
      else if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == null) {
        this.items = [
          { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
          { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },
          /*{ label: this.getContextMenuLabel('AMP', 'Select all ready for review AMP records'), escape: false, command: (event) => this.selectAllReadyForReviewAMP(this.selectedNode) },*/
          { separator: true },
          { label: "Quit" }
        ];
      }
    }
    else if (event.node.data.level == 'VMP' && event.node.children && event.node.children.length > 0) {
      if (this.selectedProductTypes && this.selectedProductTypes.code != '') {
        this.items = [
          { label: "Quit" }
        ];
      }
      else if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == null) {
        this.items = [
          { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
          { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },
          /*{ label: this.getContextMenuLabel('AMP', 'Select all ready for review AMP records'), escape: false, command: (event) => this.selectAllReadyForReviewAMP(this.selectedNode) },*/
          { separator: true },
          { label: "Quit" }
        ];
      }
      else if (event.node.children.filter((rec: any) => rec.data.isChecked).length == 0 && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Active") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
            { separator: true },
            { label: "Quit" }
          ];
        }
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Draft") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },
            { separator: true },
            { label: "Quit" }
          ];
        }
      }
      else if (event.node.children.filter((rec: any) => rec.data.isChecked).length > 0
        && event.node.children.filter((rec: any) => rec.data.isChecked).length == event.node.children.filter((rec: any) => rec.data.status.desc == this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS)).length
        && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {
        this.items = [
          { label: this.getContextMenuLabel('AMP', 'De-select all AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', null, 2) },
          { separator: true },
          { label: "Quit" }
        ];
      }
      else if (event.node.children.filter((rec: any) => rec.data.isChecked).length > 0 && this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == "AMP") {
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Active") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all active AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Active', 1) },
            { label: this.getContextMenuLabel('AMP', 'De-select all AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', null, 1) },
            { separator: true },
            { label: "Quit" }
          ];
        }
        if (this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS) == "Draft") {
          this.items = [
            { label: this.getContextMenuLabel('AMP', 'Select all draft AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', 'Draft', 1) },
            { label: this.getContextMenuLabel('AMP', 'De-select all AMP records'), escape: false, command: (event) => this.selectOrDeselectChildNodes(this.selectedNode, 'AMP', null, 2) },
            { separator: true },
            { label: "Quit" }
          ];
        }
      }
    }
    else {
      this.items = [
        {
          label: "Quit",
        }
      ];
    }
  }

  getContextMenuLabel(type: string, text?: string): string {
    return `<img  class="mr-1" src = "${this.imgLkp[type].src}" title = "${type}" />${!text ? "" : text}`;
  }

  findOtherNonSimilarNodesAndDisable(treeNode: any): any {
    let productType = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL);
    let recordStatus = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS);

    for (const tn of treeNode) {
      if ((productType && tn.data.level != productType) || (recordStatus && tn.data.status.desc != recordStatus)) {
        tn.data.isDisabled = true;
        tn.data.isChecked = false;
      }

      if (tn.children) {
        this.findOtherNonSimilarNodesAndDisable(tn.children);
      }
    }

    return false;
  }

  //deselectAllVMP(node: any) {

  //  var selNodeData: { [index: string]: any } = {};

  //  let selectedRecordsAsString = sessionStorage.getItem(this.FORMULARY_BULK_SELECT_IDs);

  //  if (selectedRecordsAsString)
  //    selNodeData = JSON.parse(selectedRecordsAsString);

  //  if (node.data.level == "VTM") {
  //    node.expanded = true;
  //    node.children.forEach((child: any, index: any) => {
  //      if (!child.data.isDisabled && child.data.isChecked) {
  //        child.data.isDisabled = false;
  //        child.data.isChecked = false;

  //        const indx = this.selectedNodes.findIndex(rec => rec.data.level == child.data.level && rec.data.code == child.data.code && rec.data.status.cd == child.data.status.cd);
  //        this.selectedNodes.splice(indx, indx >= 0 ? 1 : 0);

  //        if (selNodeData[child.data.formularyversionid]) {
  //          delete selNodeData[child.data.formularyversionid];
  //        }

  //      }
  //    });
  //  }

  //  if (!selNodeData || Object.keys(selNodeData).length == 0) {
  //    $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html('0');
  //    $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');
  //    sessionStorage.removeItem(this.FORMULARY_BULK_SELECT_IDs);
  //    sessionStorage.removeItem(this.FORMULARY_BULK_SELECT_LEVEL);
  //    sessionStorage.removeItem(this.FORMULARY_BULK_SELECT_STATUS);
  //    this.enableCheckbox(this.treeNode);
  //  }
  //  else {
  //    $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(selNodeData).length);
  //    $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
  //    sessionStorage.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(selNodeData));
  //  }

  //  this.treeNode = [...this.treeNode];
  //}

  //deselectAllAMP(node: any) {

  //  var selNodeData: { [index: string]: any } = {};

  //  let selectedRecordsAsString = sessionStorage.getItem(this.FORMULARY_BULK_SELECT_IDs);

  //  if (selectedRecordsAsString)
  //    selNodeData = JSON.parse(selectedRecordsAsString);

  //  if (node.data.level == "VMP") {
  //    node.expanded = true;
  //    node.children.forEach((child: any, index: any) => {
  //      if (!child.data.isDisabled && child.data.isChecked) {
  //        child.data.isDisabled = false;
  //        child.data.isChecked = false;

  //        const indx = this.selectedNodes.findIndex(rec => rec.data.level == child.data.level && rec.data.code == child.data.code && rec.data.status.cd == child.data.status.cd);
  //        this.selectedNodes.splice(indx, indx >= 0 ? 1 : 0);

  //        if (selNodeData[child.data.formularyversionid]) {
  //          delete selNodeData[child.data.formularyversionid];
  //        }
  //      }
  //    });
  //  }

  //  if (node.data.level == "VTM") {
  //    node.expanded = true;
  //    node.children.forEach((child: any) => {
  //      child.expanded = true;
  //      child.children.forEach((grandChild: any, index: any) => {
  //        if (!grandChild.data.isDisabled && grandChild.data.isChecked) {
  //          grandChild.data.isDisabled = false;
  //          grandChild.data.isChecked = false;

  //          const indx = this.selectedNodes.findIndex(rec => rec.data.level == grandChild.data.level && rec.data.code == grandChild.data.code && rec.data.status.cd == grandChild.data.status.cd);
  //          this.selectedNodes.splice(indx, indx >= 0 ? 1 : 0);

  //          if (selNodeData[grandChild.data.formularyversionid]) {
  //            delete selNodeData[grandChild.data.formularyversionid];
  //          }
  //        }
  //      });
  //    });
  //  }

  //  if (!selNodeData || Object.keys(selNodeData).length == 0) {
  //    $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html('0');
  //    $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');
  //    sessionStorage.removeItem(this.FORMULARY_BULK_SELECT_IDs);
  //    sessionStorage.removeItem(this.FORMULARY_BULK_SELECT_LEVEL);
  //    sessionStorage.removeItem(this.FORMULARY_BULK_SELECT_STATUS);
  //    this.enableCheckbox(this.treeNode);
  //  }
  //  else {
  //    $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(selNodeData).length);
  //    $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
  //    sessionStorage.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(selNodeData));
  //  }

  //  this.treeNode = [...this.treeNode];
  //}

  checkedAndStatusCountMatch(node: any) {
    let checkedCount = 0;
    let statusCount = 0;

    node.children.forEach((vmp: any) => {
      if (vmp.children) {
        vmp.children.forEach((amp: any) => {
          if (amp.data.isChecked) {
            checkedCount = checkedCount + 1;
          }
          if (amp.data.status.desc == this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS)) {
            statusCount = statusCount + 1;
          }
        });
      }
    });

    if (checkedCount == statusCount) {
      return true;
    }
    else {
      return false;
    }
  }

  notCheckedAndStatusCountMatch(node: any) {
    let notCheckedCount = 0;
    let statusCount = 0;

    node.children.forEach((vmp: any) => {
      if (vmp.children) {
        vmp.children.forEach((amp: any) => {
          if (!amp.data.isChecked) {
            notCheckedCount = notCheckedCount + 1;
          }
          if (amp.data.status.desc == this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS)) {
            statusCount = statusCount + 1;
          }
        });
      }
    });

    if (notCheckedCount == statusCount) {
      return true;
    }
    else {
      return false;
    }
  }

  deselectAllCheckbox() {
    this.treeNode.forEach((vtm) => {
      vtm.data.isChecked = false;
      vtm.data.isDisabled = false;

      vtm.children?.forEach((vmp) => {
        vmp.data.isChecked = false;
        vmp.data.isDisabled = false;

        vmp.children?.forEach((amp) => {
          amp.data.isChecked = false;
          amp.data.isDisabled = false;
        });
      });
    });

    $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html('0');
    $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');
    this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_IDs);
    this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_LEVEL);
    this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_STATUS);
    this.selectedNodes = [];
  }

  selectAllNodes() {

    var selNodeData: { [index: string]: any } = {};

    //let selectedRecordsAsString = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);
    let selectedRecordsAsString = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);
    if (this._allPagesData)
      selectedRecordsAsString = JSON.stringify(this._allPagesData);

    if (selectedRecordsAsString)
      selNodeData = JSON.parse(selectedRecordsAsString);

    this.treeNode.forEach((vtm, index) => {

      if (index == 0) {
        this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_LEVEL, vtm.data.level);
        this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_STATUS, vtm.data.status.desc);
        this.findOtherNonSimilarNodesAndDisable(this.treeNode);
      }
      if (!vtm.data.isDisabled && !vtm.data.isChecked && vtm.data.status.desc == this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS)) {
        vtm.data.isChecked = true;
        vtm.data.isDisabled = false;
        this.selectedNodes.push(vtm);

        selNodeData[vtm.data.formularyversionid] = { code: vtm.data.code, title: vtm.data.name, formularyVersionId: vtm.data.formularyversionid };
      }

      vtm.children?.forEach((vmp, index) => {

        if (index == 0) {
          this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_LEVEL, vmp.data.level);
          this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_STATUS, vmp.data.status.desc);
          this.findOtherNonSimilarNodesAndDisable(this.treeNode);
        }
        if (!vmp.data.isDisabled && !vmp.data.isChecked && vmp.data.status.desc == this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS)) {
          vmp.data.isChecked = true;
          vmp.data.isDisabled = false;
          this.selectedNodes.push(vmp);

          selNodeData[vmp.data.formularyversionid] = { code: vmp.data.code, title: vmp.data.name, formularyVersionId: vmp.data.formularyversionid };
        }

        vmp.children?.forEach((amp, index) => {

          if (index == 0) {
            this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_LEVEL, amp.data.level);
            this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_STATUS, amp.data.status.desc);
            this.findOtherNonSimilarNodesAndDisable(this.treeNode);
          }
          if (!amp.data.isDisabled && !amp.data.isChecked && amp.data.status.desc == this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS)) {
            amp.data.isChecked = true;
            amp.data.isDisabled = false;
            this.selectedNodes.push(amp);

            selNodeData[amp.data.formularyversionid] = { code: amp.data.code, title: amp.data.name, formularyVersionId: amp.data.formularyversionid };
          }
        });
      });
    });

    $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(selNodeData).length);
    $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
    this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(selNodeData));
  }

  convertToJson(str: string) {
    return JSON.parse(str);
  }

  convertFormularyToTreeNode(formulary: any): TreeNode | null {
    if (!formulary) return null;

    const treeNode: TreeNode = new TreeNode();

    const { title, key, formularyVersionId, data, formularyId } = formulary;
    const imageInfo = this.getImageInfo(data.Level);

    treeNode.data = {
      name: title,
      code: key,
      formularyversionid: formularyVersionId,
      formularyid: formularyId,
      status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
      level: data.Level,
      isChecked: false,
      isDisabled: false,
      imgSrc: imageInfo.src,
      imgTitle: imageInfo.title,
    };

    treeNode.expanded = false;
    treeNode.leaf = data.Level === 'AMP';

    return treeNode;
  }


  async getChildrenForNode(currentNode: TreeNode): Promise<TreeNode[] | null> {

    if (!currentNode || currentNode.leaf || !currentNode.data) return null;
    if (currentNode && currentNode.children) return currentNode.children;

    const searchCriteria = {
      hideArchived: this.isHideArchived,
      formularyCode: currentNode.data.code,
      formularyVersionId: currentNode.data.formularyversionid
    };

    const searchResults = await this.searchPageService.getFormularyChildren(searchCriteria);

    if (!searchResults) return null;

    const childNodes: TreeNode[] = [];
    searchResults.forEach((result: any, index: any) => {
      const childNode = this.convertFormularyToTreeNode(result);
      if (childNode) {
        if (!currentNode.children)
          currentNode.children = [];
        currentNode.children.push(childNode);
        childNodes.push(childNode);
      }
    });
    this.treeNode = [...this.treeNode];

    return childNodes;
  }

  disableOrEnableOtherUncheckedNodes(nodes: TreeNode[], enableOrDisableAllSelection: number | null) {
    if (!nodes || !nodes.length) return;
    //enableOrDisableAllSelection - 1 - enable all, 2 - disable all
    const setAllNodesDisabledAsDefault = !enableOrDisableAllSelection ? null : (enableOrDisableAllSelection === 1 ? false : (enableOrDisableAllSelection === 2) ? true : null);

    const treeNodes = nodes;
    const targetNodeLevel = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL);
    const targetNodeStatus = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS);

    //if no nodes selected and also default is not passed - then ignore disabling

    for (const currentNode of treeNodes) {
      if (currentNode) {

        if (setAllNodesDisabledAsDefault !== null) {
          currentNode.data.isDisabled = setAllNodesDisabledAsDefault;
        } else {
          if (!targetNodeLevel) {//enable all nodes
            currentNode.data.isDisabled = false;
          } else if (!currentNode.data.isChecked) {
            if (currentNode.data && targetNodeLevel === currentNode.data.level && targetNodeStatus && currentNode.data.status && targetNodeStatus === currentNode.data.status.desc) {
              currentNode.data.isDisabled = false;
            }
            else {
              currentNode.data.isDisabled = true;
            }
          }
        }
        if (currentNode.children && currentNode.children.length)
          this.disableOrEnableOtherUncheckedNodes(currentNode.children, enableOrDisableAllSelection);
      }
    }
  }

  selectOrDeselectChildNodes(currentNode: TreeNode | undefined, targetNodeLevel: string, targetNodeStatus: string | null, action: number) {
    this._allPagesData = null;
    if (!currentNode || !currentNode.data || !targetNodeLevel) return;

    //action: 1= select, 2=deselect
    //if deselect the child nodes already exists
    if (action === 2) {
      this.deselectChildNodes(currentNode, targetNodeLevel, targetNodeStatus);
      return;
    }

    const prevTargetNodeLevel = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL);
    const prevTargetStatus = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS);

    let selNodeData: { [index: string]: any } = {};
    const selectedRecordsAsString = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);

    if (selectedRecordsAsString)
      selNodeData = JSON.parse(selectedRecordsAsString);

    //if has records already - check if of same level - else do not allow
    if (selNodeData && Object.keys(selNodeData).length) {
      if (!((prevTargetNodeLevel === targetNodeLevel) && (!targetNodeStatus || !prevTargetStatus || (targetNodeStatus === prevTargetStatus)))) {
        return;//do not allow
      } else {
        this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_LEVEL, targetNodeLevel);
        if (targetNodeStatus)
          this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_STATUS, targetNodeStatus);
      }
    } else {
      this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_IDs);
      this.selectedNodes = [];//to be checked --not sure
      this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_LEVEL, targetNodeLevel);

      if (targetNodeStatus)
        this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_STATUS, targetNodeStatus);
    }

    this.selectChildNodes(currentNode).then(() => {
      const newSelNodeData = this.convertTreeNodeToSelectedBulkData(this.selectedNodes);

      if (!newSelNodeData) {
        $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html('0');
        $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');
        this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_IDs);
        this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_LEVEL);
        this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_STATUS);
      } else {

        $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(newSelNodeData).length);
        $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
        this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(newSelNodeData));
      }

      //disable other nodes here
      this.disableOrEnableOtherUncheckedNodes(this.treeNode, null);
      this.treeNode = [...this.treeNode];
    });
  }

  deselectChildNodes(currentNode: TreeNode, targetNodeLevel: string, targetNodeStatus: string | null) {
    if (!currentNode || !currentNode.data || !targetNodeLevel) return;

    if (!currentNode.children || !currentNode.children.length) return;

    const children = currentNode.children;

    for (const child of children) {
      if (child.data.level === targetNodeLevel) {
        if (!targetNodeStatus || (targetNodeStatus && child.data.status && targetNodeStatus === child.data.status.desc)) {//this is the level
          child.data.isDisabled = false;
          child.data.isChecked = false;

          const indx = this.selectedNodes.findIndex(rec => rec.data.level === child.data.level && rec.data.code === child.data.code && rec.data.status.cd === child.data.status.cd);
          this.selectedNodes.splice(indx, indx >= 0 ? 1 : 0);

        } else {
          //some error cannot occur
          console.error('status not matching to deselect')
        }

      } else if (child.children && child.children.length) {
        this.deselectChildNodes(child, targetNodeLevel, targetNodeStatus);
      }
    }

    //post-process
    const newSelNodeData = this.convertTreeNodeToSelectedBulkData(this.selectedNodes);
    if (!newSelNodeData) {
      $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html('0');
      $('#iBulkEdit, #iBulkEditFloat').removeClass('enabled').addClass('disabled');
      this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_IDs);
      this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_LEVEL);
      this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_STATUS);
    } else {
      $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(newSelNodeData).length);
      $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
      this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(newSelNodeData));
    }

    //disable other nodes here
    this.disableOrEnableOtherUncheckedNodes(this.treeNode, null);
  }

  convertTreeNodeToSelectedBulkData(treeNodes: TreeNode[]) {

    if (!treeNodes || !treeNodes.length) return null;
    const selNodeData: { [index: string]: any } = {};

    for (const node of treeNodes) {
      selNodeData[node.data.formularyversionid] = { code: node.data.code, title: node.data.name, formularyVersionId: node.data.formularyversionid };
    }
    return selNodeData;
  }

  async selectChildNodes(currentNodeInput: TreeNode) {
    if (!currentNodeInput || !currentNodeInput.data) return;

    const currentNode = currentNodeInput;

    const childrenForNode = await this.getChildrenForNode(currentNode);

    const targetNodeLevel = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL);
    const targetStatus = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_STATUS);

    if (!childrenForNode || !childrenForNode.length) return;

    currentNode.expanded = true;

    for (const child of childrenForNode) {

      if (child.data.level === targetNodeLevel) {
        if (!targetStatus || !child.data.status || targetStatus === child.data.status.desc) {
          child.data.isDisabled = false;
          child.data.isChecked = true;

          if (!(this.selectedNodes.findIndex(rec => rec.data.level === child.data.level && rec.data.code === child.data.code && rec.data.status.cd === child.data.status.cd) >= 0))
            this.selectedNodes.push(child); //perform selection of the children
        }
      } else {
        await this.selectChildNodes(child);
      }
    }
  }


  //_selNodeData: { [index: string]: any } = {};

  //getChildren(node: any, level: string, isRecursive: boolean = false) {

  //  if (!isRecursive) {
  //    this._selNodeData = {};
  //  }

  //  let selectedRecordsAsString = sessionStorage.getItem(this.FORMULARY_BULK_SELECT_IDs);

  //  if (selectedRecordsAsString)
  //    this._selNodeData = JSON.parse(selectedRecordsAsString);

  //  let searchCriteria = {
  //    hideArchived: this.isHideArchived,
  //    formularyCode: node.data.code,
  //  };

  //  const httpOptions = {
  //    headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
  //  }

  //  this.http.post(this.buildUrl('/Formulary/LoadChildrenFormularies_New'), searchCriteria, httpOptions)
  //    .subscribe({
  //      next: (response: any) => {
  //        let tr: TreeNode[] = [];

  //        node.children = [];

  //        response.forEach((result: any, index: any) => {

  //          let treeNode: TreeNode = new TreeNode();

  //          const { title, key, formularyVersionId, data } = result;
  //          const imageInfo = this.getImageInfo(data.Level);

  //          treeNode.data = {
  //            name: title,
  //            code: key,
  //            formularyversionid: formularyVersionId,
  //            status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
  //            level: data.Level,
  //            isChecked: this._selNodeData[formularyVersionId] ? true : false,
  //            isDisabled: false,
  //            imgSrc: imageInfo.src,
  //            imgTitle: imageInfo.title,
  //          };

  //          treeNode.expanded = false;
  //          treeNode.leaf = data.Level == 'AMP';

  //          if (this._selNodeData[formularyVersionId]) {
  //            this.isCheckboxChecked = true;
  //          }

  //          if (!treeNode.data.isDisabled && !treeNode.data.isChecked && treeNode.data.level == level
  //            && (sessionStorage.getItem(this.FORMULARY_BULK_SELECT_STATUS) == null || treeNode.data.status.desc == sessionStorage.getItem(this.FORMULARY_BULK_SELECT_STATUS)))
  //          {
  //            treeNode.data.isDisabled = false;
  //            treeNode.data.isChecked = true;

  //            this.selectedNodes.push(treeNode);

  //            this._selNodeData[treeNode.data.formularyversionid] = { code: treeNode.data.code, title: treeNode.data.name, formularyVersionId: treeNode.data.formularyversionid };

  //            if (sessionStorage.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == null) {
  //              sessionStorage.setItem(this.FORMULARY_BULK_SELECT_LEVEL, treeNode.data.level);
  //              sessionStorage.setItem(this.FORMULARY_BULK_SELECT_STATUS, treeNode.data.status.desc);
  //            }

  //            if (treeNode.data.level == "VMP") {
  //              this.findOtherNonSimilarNodesAndDisable(this.treeNode);
  //            }

  //          }
  //          else if (!treeNode.data.isDisabled && treeNode.data.isChecked && treeNode.data.level == level
  //            && (sessionStorage.getItem(this.FORMULARY_BULK_SELECT_STATUS) == null || treeNode.data.status.desc == sessionStorage.getItem(this.FORMULARY_BULK_SELECT_STATUS))) {
  //            treeNode.data.isDisabled = false;
  //            treeNode.data.isChecked = true;
  //            }
  //          else {
  //            treeNode.data.isDisabled = true;
  //            treeNode.data.isChecked = false;
  //            this.findOtherNonSimilarNodesAndDisable(this.treeNode);
  //          }

  //          tr.push(treeNode);

  //        });

  //        $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(this._selNodeData).length);
  //        $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
  //        sessionStorage.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(this._selNodeData));

  //        node.children = tr;

  //        node.children.forEach((child: any) => {
  //          if (child.data.level != level) {
  //            child.expanded = true;
  //            this.getChildren(child, level, true);
  //          }
  //        })

  //        this.treeNode = [...this.treeNode];
  //      },
  //      error: error => {
  //        console.error('There was an error!', error);
  //      }
  //    });
  //}

  ////_selectedNodeData: { [index: string]: any } = {};

  //getAMPsByStatus(node: any, status: string, isRecursive: boolean = false) {

  //  if (!isRecursive) {
  //    this._selNodeData = {};
  //  }

  //  let selectedRecordsAsString = sessionStorage.getItem(this.FORMULARY_BULK_SELECT_IDs);

  //  if (selectedRecordsAsString)
  //    this._selNodeData = JSON.parse(selectedRecordsAsString);

  //  let searchCriteria = {
  //    hideArchived: this.isHideArchived,
  //    formularyCode: node.data.code,
  //  };

  //  const httpOptions = {
  //    headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
  //  }

  //  this.http.post(this.buildUrl('/Formulary/LoadChildrenFormularies_New'), searchCriteria, httpOptions)
  //    .subscribe({
  //      next: (response: any) => {
  //        let tr: TreeNode[] = [];

  //        node.children = [];

  //        response.forEach((result: any, index: any) => {

  //          let treeNode: TreeNode = new TreeNode();
  //          const { title, key, formularyVersionId, data } = result;
  //          const imageInfo = this.getImageInfo(data.Level);

  //          treeNode.data = {
  //            name: title,
  //            code: key,
  //            formularyversionid: formularyVersionId,
  //            status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
  //            level: data.Level,
  //            isChecked: this._selNodeData[formularyVersionId] ? true : false,
  //            isDisabled: false,
  //            imgSrc: imageInfo.src,
  //            imgTitle: imageInfo.title,
  //          };

  //          treeNode.expanded = false;
  //          treeNode.leaf = data.Level == 'AMP';

  //          if (this._selNodeData[formularyVersionId]) {
  //            this.isCheckboxChecked = true;
  //          }

  //          if (!treeNode.data.isDisabled && !treeNode.data.isChecked && treeNode.data.status.cd == status && treeNode.data.level == 'AMP') {
  //            treeNode.data.isDisabled = false;
  //            treeNode.data.isChecked = true;
  //            this.selectedNodes.push(treeNode);

  //            this._selNodeData[treeNode.data.formularyversionid] = { code: treeNode.data.code, title: treeNode.data.name, formularyVersionId: treeNode.data.formularyversionid };

  //            if (sessionStorage.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == null) {
  //              sessionStorage.setItem(this.FORMULARY_BULK_SELECT_LEVEL, treeNode.data.level);
  //              sessionStorage.setItem(this.FORMULARY_BULK_SELECT_STATUS, treeNode.data.status.desc);
  //            }

  //            if (treeNode.data.level == "VMP") {
  //              this.findOtherNonSimilarNodesAndDisable(this.treeNode);
  //            }
  //          }
  //          else if (!treeNode.data.isDisabled && treeNode.data.isChecked && treeNode.data.status.cd == status && treeNode.data.level == 'AMP') {
  //            treeNode.data.isDisabled = false;
  //            treeNode.data.isChecked = true;
  //          }
  //          else {
  //            treeNode.data.isDisabled = true;
  //            treeNode.data.isChecked = false;
  //            this.findOtherNonSimilarNodesAndDisable(this.treeNode);
  //          }

  //          tr.push(treeNode);

  //        });

  //        $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(this._selNodeData).length);
  //        $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
  //        sessionStorage.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(this._selNodeData));

  //        node.children = tr;

  //        node.children.forEach((child: any) => {
  //          if (child.data.level != "AMP") {
  //            child.expanded = true;
  //            this.getAMPsByStatus(child, status, true);
  //          }
  //        })

  //        this.treeNode = [...this.treeNode];
  //      },
  //      error: error => {
  //        console.error('There was an error!', error);
  //      }
  //    });
  //}

  //getReadyForReviewAMPs(node: any) {

  //  var selNodeData: { [index: string]: any } = {};

  //  let selectedRecordsAsString = sessionStorage.getItem(this.FORMULARY_BULK_SELECT_IDs);

  //  if (selectedRecordsAsString)
  //    selNodeData = JSON.parse(selectedRecordsAsString);

  //  let searchCriteria = {
  //    hideArchived: this.isHideArchived,
  //    formularyCode: node.data.code,
  //  };

  //  const httpOptions = {
  //    headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
  //  }

  //  this.http.post(this.buildUrl('/Formulary/LoadChildrenFormularies_New'), searchCriteria, httpOptions)
  //    .subscribe({
  //      next: (response: any) => {
  //        let tr: TreeNode[] = [];

  //        node.children = [];

  //        response.forEach((result: any, index: any) => {

  //          let treeNode: TreeNode = new TreeNode();

  //          const { title, key, formularyVersionId, data } = result;
  //          const imageInfo = this.getImageInfo(data.Level);

  //          treeNode.data = {
  //            name: title,
  //            code: key,
  //            formularyversionid: formularyVersionId,
  //            status: { 'cd': data.recordstatus.code, 'desc': data.recordstatus.description, 'orgCd': data.recordstatus.code },
  //            level: data.Level,
  //            isChecked: selNodeData[formularyVersionId] ? true : false,
  //            isDisabled: false,
  //            imgSrc: imageInfo.src,
  //            imgTitle: imageInfo.title,
  //          };

  //          treeNode.expanded = false;
  //          treeNode.leaf = data.Level == 'AMP';

  //          if (selNodeData[formularyVersionId]) {
  //            this.isCheckboxChecked = true;
  //          }

  //          if (!treeNode.data.isDisabled && !treeNode.data.isChecked && treeNode.data.status.cd == '002') {
  //            treeNode.data.isDisabled = false;
  //            treeNode.data.isChecked = true;
  //            this.selectedNodes.push(treeNode);

  //            selNodeData[treeNode.data.formularyversionid] = { code: treeNode.data.code, title: treeNode.data.name, formularyVersionId: treeNode.data.formularyversionid };

  //            if (sessionStorage.getItem(this.FORMULARY_BULK_SELECT_LEVEL) == null) {
  //              sessionStorage.setItem(this.FORMULARY_BULK_SELECT_LEVEL, treeNode.data.level);
  //              sessionStorage.setItem(this.FORMULARY_BULK_SELECT_STATUS, treeNode.data.status.desc);
  //            }
  //          }
  //          else {
  //            treeNode.data.isDisabled = true;
  //            treeNode.data.isChecked = false;
  //            this.findOtherNonSimilarNodesAndDisable(this.treeNode);
  //          }

  //          tr.push(treeNode);

  //        });

  //        node.children = tr;

  //        $('#lblBulkEditNumber, #lblBulkEditNumberFloat').html(Object.keys(selNodeData).length);
  //        $('#iBulkEdit, #iBulkEditFloat').removeClass('disabled').addClass('enabled');
  //        sessionStorage.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(selNodeData));

  //        this.treeNode = [...this.treeNode];
  //      },
  //      error: error => {
  //        console.error('There was an error!', error);
  //      }
  //    });
  //}

  changeStatus(event: any, rowData: any, oldValue: any) {

    this.orgStatusRowDataSelected = rowData;
    this.oldStatusValue = oldValue;
    this.status = event.value;
    this.formularyVersionId = rowData.formularyversionid;
    this.displayDialog = true;

    if (event.value == '004') {
      this.showReason = true;
    }
    else {
      this.showReason = false;
    }
  }

  onClearProductTypes() {
    this.selectedRecordStatuses = [];
    this.selectedCategories = [];
    this.showFormularyStatus = true;
    this.showHideArchivedCntrl = true;
  }

  onChangeProductTypes(event: any) {
    if (event && event.value) {
      if (event.value.code === 'VTM' || event.value.code === 'VMP') {
        this.selectedRecordStatuses = [];
        this.selectedCategories = [];
        this.flags = this.vtmVMPFlags;
        this.selectedFlags = [];
        this.selectedFormularyStatuses = [];
        this.showFormularyStatus = false;
        this.showHideArchivedCntrl = false;
        this.handleDirty();
        return;
      }
    }
    this.flags = this.allFlags;
    this.selectedFlags = [];
    this.showFormularyStatus = true;
    this.showHideArchivedCntrl = true;
    this.handleDirty();
  }

  closeDialog() {
    if (!this.hasHandledChangeStatus) {
      this.displayDialog = false;
      this.orgStatusRowDataSelected.status.cd = this.oldStatusValue;
    }
    this.hasHandledChangeStatus = false;
  }

  confirmChangeStatus() {
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
        .subscribe((response: any) => {

          this.displayDialog = false;
          this.applySettings();
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
        .subscribe((response: any) => {

          this.displayDialog = false;
          this.applySettings();
        });
    }
  }

  rejectChangeStatus() {
    this.hasHandledChangeStatus = true;
    this.displayDialog = false;
    this.orgStatusRowDataSelected.status.cd = this.oldStatusValue;
  }

  onCheckboxChecked(event: any) {
    this._allPagesData = null;
    if (!event.checked) {
      this.deselectAllCheckbox();
      this._selectAllProducts = false;
      this._deselectAllProducts = false;
    }

    if (event.checked) {
      if (this.selectedProductTypes && (this.selectedProductTypes.code == 'AMP' || this.selectedProductTypes.code == 'VMP' || this.selectedProductTypes.code == 'VTM')) {
        if (this.selectedProductTypes.code !== 'AMP') {
          this.selectAllNodes();
        } else if (this.selectedProductTypes.code === 'AMP') {
          if (this.selectedRecordStatuses && this.selectedRecordStatuses.length === 1) {
            this._selectAllProducts = true;
            this._deselectAllProducts = false;
            this.selectAllNodes();
          } else {
            this._selectAllProducts = false;
            this._deselectAllProducts = false;
            this.deselectAllCheckbox();
            this.isCheckboxChecked = false;
            this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_IDs);
            this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_LEVEL);
            this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_STATUS);
          }
        }
      }
    }
  }

  async onSelectAllProducts(e: any) {
    this._allPagesData = null;
    if (this.selectedProductTypes && (this.selectedProductTypes.code === 'AMP' || this.selectedProductTypes.code === 'VMP' || this.selectedProductTypes.code === 'VTM')) {
      this._selectAllProducts = false;
      this._disableSelectAllProducts = true;
      //FORMULARY_BULK_SELECT_LEVEL" and FORMULARY_BULK_SELECT_STATUS will be set already
      this._showLoadingSelectAllProducts = true;
      const data = await this.getFormulariesInBatches(1);
      this._showLoadingSelectAllProducts = false;
      this._disableSelectAllProducts = false;
      if (!data || !Object.keys(data).length)
        this.hideBulkEditOption();
      else {
        try {
          //sessionStorage.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(data));//to be commented for mmc-477
          this._allPagesData = data;
          //this.showBulkEditOption(Object.keys(data).length);//mmc-477 - to be shown only when user opts bulkedit
          this.isCheckboxChecked = true;
          this._deselectAllProducts = true;
        } catch {
          this.hideBulkEditOption();
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Too many records selected. Please reduce the number of records by filtering.', sticky: true });
        }
      }
    }
    this.selectAllNodes();
  }



  async getFormulariesInBatches(pageNo: any): Promise<any> {
    try {
      const results = await this.searchPageService.getFormulariesByPageNumberAsPromise(pageNo, 1000);
      if (!results || !results.length) return null;

      let selNodeData: any = {};
      for (let result of results) {
        const { key, title, formularyVersionId } = result;
        selNodeData[result.formularyVersionId] = { code: key, title, formularyVersionId };
      }
      const data = await this.getFormulariesInBatches(pageNo + 1);

      if (data && Object.keys(data).length)
        selNodeData = { ...data, ...selNodeData };
      return selNodeData;

    } catch (err) {
      console.error('Error invoking getFormulariesByPageNumber', err);
    }

    return null;
  }

  onDeSelectAllProducts(e: any) {
    this._allPagesData = null;
    this.isCheckboxChecked = false;
    this._deselectAllProducts = false;
    if (this.selectedProductTypes && (this.selectedProductTypes.code === 'AMP' || this.selectedProductTypes.code === 'VMP' || this.selectedProductTypes.code === 'VTM')) {
      this.dmdItemSessionStorageService.removeItem(this.FORMULARY_BULK_SELECT_IDs);
      this.deselectAllCheckbox();
      this.hideBulkEditOption();
      this._selectAllProducts = true;
    }
    this.deselectAllCheckbox();
  }

  onKeyUp(event: any) {
    if (event.keyCode == 13) {
      this.applySettings();
    }
  }

  gettest(data: any): string | null {
    if (!data) return '';
    return JSON.stringify(data);
  }

  searchInput() {
    this.handleDirty();
  }

  formularyStatusChange() {
    this.handleDirty();
  }

  flagsChange() {
    this.handleDirty();
  }

  handleDirty() {
    this._showSearchCriteriaError = false;
    if ((!this.selectedFlags || !this.selectedFlags.length) && (!this.selectedProductTypes || !this.selectedProductTypes.code) && (!this.selectedFormularyStatuses || !this.selectedFormularyStatuses.length) && (!this.selectedRecordStatuses || !this.selectedRecordStatuses.length) && !this.search)
      this._showSearchCriteriaError = true;
  }

  onBulkEditConfirmClose() {
    this.messageService.clear('bulkeditconfirm');
  }

  onBulkEdit() {
    this.messageService.clear('bulkeditconfirm');
    let dataInSession = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);
    let data = dataInSession && JSON.parse(dataInSession);
    if (this._allPagesData)
      data = this._allPagesData;

    if (!data || !Object.keys(data).length)
      this.hideBulkEditOption();
    else if (Object.keys(data).length > 2500) {
      this.messageService.add({ severity: 'error', summary: 'Exceeded Selection Limit', detail: 'The number of records selected cannot be more than 2500.' });
      return;
    }
    else {
      try {
        //this.dmdItemSessionStorageService.setItem(this.FORMULARY_BULK_SELECT_IDs, JSON.stringify(data));
        this.showBulkEditOption(Object.keys(data).length);
        if (this.showBulkEdit)
          this.showBulkEdit();

      } catch (e: any) {
        console.error('Error putting bulk data in session', e);
        this.hideBulkEditOption();
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Too many records selected. Please reduce the number of records by filtering.', sticky: true });
      }
    }
  }
  _bulkUpdateStatusData: { currentStatus: any | null, level: string | null, ids: any } | null = null;

  onBulkStatusChange() {
    this.messageService.clear('bulkeditconfirm');
    let dataInSession = this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_IDs);
    //if (dataInSession)
      //this._allPagesData = JSON.parse(dataInSession);

    const firstSelectedNode = this.selectedNodes[0];
    console.log('firstSelectedNode===', firstSelectedNode, "this.selectedNodes==", this.selectedNodes, "this._allPagesData==", this._allPagesData, "dataInSession==", dataInSession);
    const currentStatusVal = { 'cd': firstSelectedNode.data.status.cd, 'desc': firstSelectedNode.data.status.desc, 'orgCd': firstSelectedNode.data.status.orgCd };

    const data = {
      currentStatus: currentStatusVal,
      level: this.dmdItemSessionStorageService.getItem(this.FORMULARY_BULK_SELECT_LEVEL),
      ids: dataInSession
    };
    this._bulkUpdateStatusData = data;
    this._showBulkUpdateStatusView = true;
  }

  onBulkStatusEditClose(e: any) {
    console.log('onBulkStatusEditClose', e)
    if (e && e.isUpdated)
      this.applySettings();

    this._bulkUpdateStatusData = null;
    this._showBulkUpdateStatusView = false;
  }



  indicateIfDeltaCalcInProgress() {

    this.searchPageService.getTasksByName(['logformularydelta', 'importdmdtoformulary'])
      .subscribe(
        (result: Task[] | null) => {
          //console.log('getTasksByName result=', result);

          if (!result || !result.length) {
            if (this._showDeltaInProgress)
              this._showRefreshForDeltaCalcComplete = true;

            this._showDeltaInProgress = false;
            return;
          }
          const inProgress = result.filter(rec => rec.statusCd === 2 || rec.statusCd === 1);
          console.log('getTasksByName inProgress=', inProgress);
          if (inProgress && inProgress.length) {
            this._showRefreshForDeltaCalcComplete = false;
            this._showDeltaInProgress = true;
          }
          else {
            if (this._showDeltaInProgress)
              this._showRefreshForDeltaCalcComplete = true;

            this._showDeltaInProgress = false;
          }

        }, (error: any) => {
          this._showDeltaInProgress = false;
          console.error(error);
        });
  }

  refreshOnDeltaCalc(e: any) {
    this.applySettings();
  }


  //getChildren(node: TreeNode, sourceProductType: any, targetProductType: any, productStatus: any) {
  //  if (!node || !sourceProductType || !!targetProductType) {
  //    return;
  //  }

  //  if (node.children) { }
  //}

}
