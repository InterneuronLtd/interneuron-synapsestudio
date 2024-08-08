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
import { ApplicationRef, Injector, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ClassificationContainerComponent } from './classification/container/classification.container.component';
import { createCustomElement } from "@angular/elements";
import { FormsModule } from '@angular/forms';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { PanelModule } from 'primeng/panel';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { RippleModule } from 'primeng/ripple';
import { StepsModule } from 'primeng/steps';
import { SkeletonModule } from 'primeng/skeleton';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { CheckboxModule } from 'primeng/checkbox';
import { MultiSelectModule } from 'primeng/multiselect';
import { TreeTableModule } from 'primeng/treetable';
import { PaginatorModule } from 'primeng/paginator';
import { TreeModule } from 'primeng/tree';
import { SplitterModule } from 'primeng/splitter';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ContextMenuModule } from 'primeng/contextmenu';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputSwitchModule } from 'primeng/inputswitch';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { ChipModule } from 'primeng/chip';
import { ChipsModule } from 'primeng/chips';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppGlobalCssComponent } from './app.global.css.component';
import { ClassificationLineElementComponent } from './classification/line-element/classification.line.element.component';
import { MessagesModule } from 'primeng/messages';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { HistoryListComponent } from './formulary-history/history-list/history-list.component';

import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { CalendarModule } from 'primeng/calendar';
import { FileUploadModule } from 'primeng/fileupload';
import { ImportComponent } from './import/import.component';
import { FilterComponent } from './filter/filter.component';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { FormularyJSONViewerComponent } from './formulary-json-viewer/formulary-json-viewer.component';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { CommonInterceptor } from './shared/common.Interceptor';
import { MessageModule } from 'primeng/message';
import { BulkUpdateStatusComponent } from './bulkupdatestatus/bulkupdatestatus.component';
import { DataCompressionService } from './shared/datacompression.service';

@NgModule({
  declarations: [
    AppComponent,
    AppGlobalCssComponent,
    ClassificationContainerComponent,
    ClassificationLineElementComponent,
    HistoryListComponent,
    ImportComponent,
    FilterComponent,
    FormularyJSONViewerComponent,
    BulkUpdateStatusComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    DropdownModule,
    InputTextModule,
    CardModule,
    BadgeModule,
    TooltipModule,
    PanelModule,
    OverlayPanelModule,
    ButtonModule,
    ToastModule,
    RippleModule,
    MessagesModule,
    ConfirmDialogModule,
    TableModule,
    DialogModule,
    CalendarModule,
    FileUploadModule,
    StepsModule,
    SkeletonModule,
    ConfirmPopupModule,
    CheckboxModule,
    MultiSelectModule,
    TreeTableModule,
    PaginatorModule,
    TreeModule,
    NgxJsonViewerModule,
    SplitterModule,
    ScrollPanelModule,
    ContextMenuModule,
    InputTextareaModule,
    InputSwitchModule,
    ProgressSpinnerModule,
    AutoCompleteModule,
    ChipModule,
    ChipsModule,
    MessageModule,
    BsDropdownModule.forRoot(),
  ],
  providers: [
    DataCompressionService,
    { provide: HTTP_INTERCEPTORS, useClass: CommonInterceptor, multi: true }
  ],
  //bootstrap: [AppComponent]
})
export class AppModule {
  constructor(private injector: Injector) { }

  ngDoBootstrap(appRef: ApplicationRef) {

    const classificationContainerEl = createCustomElement(ClassificationContainerComponent, {
      injector: this.injector
    });
    customElements.define("classification-el", classificationContainerEl);

    const historyEl = createCustomElement(HistoryListComponent, {
      injector: this.injector
    });
    customElements.define("formulary-history-el", historyEl);

    const importEl = createCustomElement(ImportComponent, {
      injector: this.injector
    });
    customElements.define("mmc-import-el", importEl);

    const filterEl = createCustomElement(FilterComponent, {
      injector: this.injector
    });
    customElements.define("filter-el", filterEl);
    const formularyJSONViewerEl = createCustomElement(FormularyJSONViewerComponent, {
      injector: this.injector
    });
    customElements.define("jsonviewer-el", formularyJSONViewerEl);
  }

}
