<%--
Interneuron Synapse

Copyright(C) 2023  Interneuron Holdings Ltd



This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  

See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
--%>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavBar.ascx.cs" Inherits="SynapseStudio.NavBar" %>

<ul class="navbar-nav navbar-right">   
    <li class="nav-item dropdown">
        <a href="#" class="nav-link dropdown-toggle" data-bs-toggle="dropdown"><i class="fa fa-user"></i>&nbsp;<asp:Label ID="lblUserFullName" runat="server"></asp:Label></a>
        <ul class="dropdown-menu">
            <li><a class="dropdown-item" href="logout.aspx"><i class="fa fa-power-off"></i>&nbsp;Log Out</a></li>
        </ul>
    </li>
</ul>
