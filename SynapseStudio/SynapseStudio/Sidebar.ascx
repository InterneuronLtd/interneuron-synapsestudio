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

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Sidebar.ascx.cs" Inherits="SynapseStudio.Sidebar" %>

<ul id="active" class="navbar-nav side-nav">
    <li class="nav-item" style="height: 10px;">&nbsp;</li>
    <li class="nav-item"><a class="nav-link" href="Default.aspx"><i class="fa fa-home"></i>&nbsp;Home</a></li>
    <li class="nav-item"><a class="nav-link" href="EntityManagerList.aspx"><i class="fa fa-database"></i>&nbsp;Entity Manager</a></li>
    <li class="nav-item"><a class="nav-link" href="BaseViewManagerList.aspx"><i class="fa fa-anchor"></i>&nbsp;Baseviews</a></li>
    <li class="nav-item"><a class="nav-link" href="ListManagerList.aspx"><i class="fa fa-list"></i>&nbsp;Lists</a></li>
    <li class="nav-item"><a class="nav-link" href="BoardManagerList.aspx"><i class="fa fa-columns"></i>&nbsp;Boards</a></li>
    <li class="nav-item"><a class="nav-link" href="SchemaExport.aspx"><i class="fa fa-refresh"></i>&nbsp;Schema Migration</a></li>
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" data-bs-toggle="dropdown" href="#">&nbsp;DevOps</a>
        <ul class="dropdown-menu">
            <li><a class="dropdown-item" href="DevOpsDashboard.aspx"><i class="fa fa-dashboard"></i>&nbsp;Dashboard</a> </li>
            <li><a class="dropdown-item" href="#" onclick="javascript:window.open('<%= ConfigurationManager.AppSettings["JenkinsURL"] %>');return false;" target="_blank">
                <i class="fa fa-handshake"></i>&nbsp;CI \ CD</a> </li>
        </ul>
    </li>
    <li class="nav-item"><a class="nav-link" href="SecurityManager.aspx"><i class="fa fa-server"></i>&nbsp;Security</a></li>
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle" data-bs-toggle="dropdown" href="#">&nbsp;Applications</a>
        <ul class="dropdown-menu">
            <li><a class="dropdown-item" href="ApplicationManager.aspx"><i class="fa fa-map-marker"></i>&nbsp;Applications Module</a> </li>
            <li><a class="dropdown-item" href="ApplicationListManager.aspx"><i class="fa fa-map-marker"></i>&nbsp;Applications List</a> </li>
        </ul>
    </li>
    <li class="nav-item"><a class="nav-link" href="logout.aspx"><i class="fa fa-power-off"></i>&nbsp;Log Out</a></li>
</ul>
