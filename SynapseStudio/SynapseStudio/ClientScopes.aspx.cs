 //Interneuron synapse

//Copyright(C) 2024 Interneuron Limited

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
﻿//Interneuron Synapse

//Copyright(C) 2023  Interneuron Holdings Ltd

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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SynapseStudio
{
    public partial class ClientScopes : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string id = Request.QueryString["id"].ToString();

                    if (!String.IsNullOrEmpty(id))
                    {
                        this.hdnClientID.Value = id;
                    }

                    if (!string.IsNullOrEmpty(GetClientName(this.hdnClientID.Value)) || GetClientName(this.hdnClientID.Value) != "An error occurred")
                    {
                        this.lblH3Client.Text = GetClientName(this.hdnClientID.Value);
                        this.lblClient.Text = GetClientName(this.hdnClientID.Value);
                        this.lblClientName.Text = GetClientName(this.hdnClientID.Value);
                    }
                }
                catch
                {
                    Response.Redirect("Error.aspx");
                }

                this.dgClientScope.Columns[0].Visible = false;
                this.lblError.Text = string.Empty;
                this.lblError.Visible = false;
                this.lblSuccess.Visible = false;

                BindDropDownList();
                BindEntityGrid();
            }

            setupClientScript();
        }

        private void BindEntityGrid()
        {
            try
            {
                string sql = "SELECT \"Id\", \"Scope\", \"ClientId\" FROM public.\"ClientScopes\" WHERE \"ClientId\" = CAST(@ClientId AS INT) ORDER BY \"Id\";";
                var paramList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("ClientId", this.hdnClientID.Value)
                };

                DataSet ds = DataServices.DataSetFromSQL(sql, paramList, dbConnection: SynapseHelpers.DBConnections.PGSQLConnectionSIS);
                DataTable dt = ds.Tables[0];

                this.dgClientScope.DataSource = dt;
                this.dgClientScope.DataBind();

                this.lblResultCount.Text = dt.Rows.Count.ToString();
            }
            catch(Exception ex)
            {
                this.lblError.Text = ex.ToString();
                this.lblError.Visible = true;
                return;
            }
        }

        private void BindDropDownList()
        {
            try
            {
                string sql = "SELECT \"Name\" FROM public.\"IdentityResources\" UNION SELECT \"Name\" FROM public.\"ApiScopes\" ORDER BY \"Name\";";

                DataSet ds = DataServices.DataSetFromSQL(sql, null, dbConnection: SynapseHelpers.DBConnections.PGSQLConnectionSIS);
                DataTable dt = ds.Tables[0];

                ddlClientScope.DataSource = dt;
                ddlClientScope.DataTextField = "Name";
                ddlClientScope.DataValueField = "Name";
                ddlClientScope.DataBind();
                ddlClientScope.Items.Insert(0, new ListItem("Please select ...", "0"));
            }
            catch(Exception ex)
            {
                this.lblError.Text = ex.ToString();
                this.lblError.Visible = true;
                return;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearAttributesForm();
        }

        private void ClearAttributesForm()
        {
            this.lblError.Text = string.Empty;
            this.lblError.Visible = false;
            this.lblSuccess.Visible = false;
            this.lblSuccess.Text = string.Empty;
            this.ddlClientScope.Enabled = true;
            this.ddlClientScope.SelectedIndex = 0;
            this.pnlClientScope.CssClass = "form-group";
        }

        protected void btnAddClientScope_Click(object sender, EventArgs e)
        {
            AddNewClientScope();
        }

        private void AddNewClientScope()
        {
            try
            {
                string sql = "SELECT * FROM \"ClientScopes\" WHERE \"ClientId\" = CAST(@ClientId AS INT) AND \"Scope\" = @Scope";

                var param = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("ClientId", this.hdnClientID.Value),
                    new KeyValuePair<string, string>("Scope", this.ddlClientScope.SelectedValue)
                };

                DataSet ds = DataServices.DataSetFromSQL(sql, param, dbConnection: SynapseHelpers.DBConnections.PGSQLConnectionSIS);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    this.lblError.Text = "Scope already exists";
                    this.lblError.Visible = true;
                    this.pnlClientScope.CssClass = "form - group has - error";
                    return;
                }
                else
                {
                    string insertSql = "INSERT INTO \"ClientScopes\" (\"Scope\", \"ClientId\") VALUES(@Scope, CAST(@ClientId AS INT));";

                    var paramList = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("Scope", this.ddlClientScope.SelectedValue),
                        new KeyValuePair<string, string>("ClientId", this.hdnClientID.Value)
                    };

                    DataServices.ExcecuteNonQueryFromSQL(insertSql, paramList, dbConnection: SynapseHelpers.DBConnections.PGSQLConnectionSIS);

                    ClearAttributesForm();
                    this.lblSuccess.Text = "Client scope added";
                    this.lblSuccess.Visible = true;

                    BindEntityGrid();
                }
            }
            catch(Exception ex)
            {
                this.lblError.Text = ex.ToString();
                this.lblError.Visible = true;
                return;
            }
        }

        protected void btnClientClaims_Click(object sender, EventArgs e)
        {
            Response.Redirect("ClientClaims.aspx?id=" + this.hdnClientID.Value);
        }

        protected void btnClientRedirectUris_Click(object sender, EventArgs e)
        {
            Response.Redirect("ClientRedirectUris.aspx?id=" + this.hdnClientID.Value);
        }

        protected void btnClientGrantTypes_Click(object sender, EventArgs e)
        {
            Response.Redirect("ClientGrantTypes.aspx?id=" + this.hdnClientID.Value);
        }

        protected void btnClientSecrets_Click(object sender, EventArgs e)
        {
            Response.Redirect("ClientSecrets.aspx?id=" + this.hdnClientID.Value);
        }

        private string GetClientName(string id)
        {
            try
            {
                string sql = "SELECT \"ClientId\" FROM public.\"Clients\" WHERE \"Id\" = CAST(@Id AS INT);";

                var paramList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Id", id)
                };

                DataSet ds = DataServices.DataSetFromSQL(sql, paramList, dbConnection: SynapseHelpers.DBConnections.PGSQLConnectionSIS);

                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                this.lblError.Text = ex.ToString();
                this.lblError.Visible = true;
                return "An error occurred";
            }
        }

        protected void dgClientScope_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            if (e.CommandName == "Remove")
            {
                try
                {
                    string sql = "DELETE FROM \"ClientScopes\" WHERE \"Id\" = CAST(@Id AS INT);";

                    var paramList = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("Id", e.Item.Cells[0].Text.ToString())
                    };

                    DataServices.ExcecuteNonQueryFromSQL(sql, paramList, dbConnection: SynapseHelpers.DBConnections.PGSQLConnectionSIS);

                    ClearAttributesForm();
                    this.lblSuccess.Text = "Client scope removed";
                    this.lblSuccess.Visible = true;

                    BindEntityGrid();
                }
                catch (Exception ex)
                {
                    this.lblError.Text = ex.ToString();
                    this.lblError.Visible = true;
                    return;
                }

            }
        }

        private void setupClientScript()
        {
            string js = @"
               <script language=JavaScript>
                  function ConfirmRemoval()
                  {
                      return confirm('Are you sure you wish to remove this record?');
                  }
               </script>";
            //Register the script
            if (!ClientScript.IsClientScriptBlockRegistered("ConfirmRemoval"))
            {
                ClientScript.RegisterClientScriptBlock(GetType(), "ConfirmRemoval", js);
            }
        }

        protected void dgClientScope_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.SelectedItem)
            {
                e.Item.Cells[2].Attributes.Add("onClick", "return ConfirmRemoval();");
            }
        }
    }
}