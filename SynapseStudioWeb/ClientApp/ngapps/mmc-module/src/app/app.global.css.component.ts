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
import { Component, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-global-css',
  template: ``,
  styleUrls: [
    '../../node_modules/primeicons/primeicons.css',
    '../../node_modules/primeflex/primeflex.min.css',
    '../../node_modules/primeng/resources/themes/saga-blue/theme.css',
    '../../node_modules/primeng/resources/primeng.min.css',
    //"../../node_modules/bootstrap/dist/css/bootstrap.min.css"
    //'../../node_modules/primeicons/fonts/primeicons.woff2',
    //'../../node_modules/primeicons/fonts/primeicons.woff',
    //'../../node_modules/primeicons/fonts/primeicons.ttf'
  ],
  encapsulation: ViewEncapsulation.None
})
export class AppGlobalCssComponent {
}
