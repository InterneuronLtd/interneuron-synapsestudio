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
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NotificationListComponent } from './notification-list/notification-list.component';
import { createCustomElement } from "@angular/elements";
import { AppGlobalCssComponent } from './app.global.css.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { PanelModule } from 'primeng/panel';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { RippleModule } from 'primeng/ripple';
import { ToolbarModule } from 'primeng/toolbar';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { TableModule } from 'primeng/table';
import { AccordionModule } from 'primeng/accordion';
import { DropdownModule } from 'primeng/dropdown';
import { ChipModule } from 'primeng/chip';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { MultiSelectModule } from 'primeng/multiselect';
import { MessageService } from 'primeng/api';
import { DividerModule } from 'primeng/divider';
import { OverlayModule } from 'primeng/overlay';
import { FieldsetModule } from 'primeng/fieldset';
import { EditorModule } from 'primeng/editor';

import { NotificationItemComponent } from './notification-item/notification-item.component';


@NgModule({
  declarations: [
    AppComponent,
    AppGlobalCssComponent,
    NotificationListComponent,
    NotificationItemComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    CardModule,
    ButtonModule,
    BadgeModule,
    TooltipModule,
    PanelModule,
    OverlayPanelModule,
    RippleModule,
    ToastModule,
    ToolbarModule,
    DialogModule,
    InputTextareaModule,
    InputTextModule,
    CheckboxModule,
    TableModule,
    AccordionModule,
    DropdownModule,
    ChipModule,
    AutoCompleteModule,
    ConfirmDialogModule,
    MultiSelectModule,
    DividerModule,
    OverlayModule,
    FieldsetModule,
    EditorModule
  ],
  providers: [ConfirmationService, MessageService],
  bootstrap: []
})
export class AppModule {
  constructor(private injector: Injector) { }

  ngDoBootstrap(appRef: ApplicationRef) {

    const notificationContainerEl = createCustomElement(NotificationListComponent, {
      injector: this.injector
    });
    customElements.define("notification-el", notificationContainerEl);

  }
}
