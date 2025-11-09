<%@ Page Title="Manage Gate Logs" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManageGateLogs.aspx.cs" Inherits="Society_Management_System.Admin.ManageGateLogs" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h2><i class="bi bi-door-open me-2"></i>Manage Gate Logs</h2>
        <p>Add, edit, and manage visitor gate logs for all societies</p>
    </div>

    <!-- Success/Error Message -->
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-dismissible fade show" role="alert">
        <asp:Label ID="lblMessage" runat="server"></asp:Label>
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </asp:Panel>

    <!-- Add/Edit Form -->
    <div class="glass-card mb-4">
        <h4 class="mb-4">
            <i class="bi bi-plus-circle me-2"></i>
            <asp:Label ID="lblFormTitle" runat="server" Text="Add New Gate Log"></asp:Label>
        </h4>
        
        <div class="row g-3">
            <div class="col-md-4">
                <label class="form-label">Society *</label>
                <asp:DropDownList ID="ddlSociety" runat="server" CssClass="form-select" 
                    AutoPostBack="true" OnSelectedIndexChanged="ddlSociety_SelectedIndexChanged">
                    <asp:ListItem Value="">-- Select Society --</asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvSociety" runat="server" 
                    ControlToValidate="ddlSociety" InitialValue="" 
                    ErrorMessage="Society is required" 
                    CssClass="text-warning d-block mt-1" Display="Dynamic" />
            </div>

            <div class="col-md-4">
                <label class="form-label">Building *</label>
                <asp:DropDownList ID="ddlBuilding" runat="server" CssClass="form-select" 
                    AutoPostBack="true" OnSelectedIndexChanged="ddlBuilding_SelectedIndexChanged">
                    <asp:ListItem Value="">-- Select Building --</asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvBuilding" runat="server" 
                    ControlToValidate="ddlBuilding" InitialValue="" 
                    ErrorMessage="Building is required" 
                    CssClass="text-warning d-block mt-1" Display="Dynamic" />
            </div>

            <div class="col-md-4">
                <label class="form-label">Unit *</label>
                <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-select">
                    <asp:ListItem Value="">-- Select Unit --</asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvUnit" runat="server" 
                    ControlToValidate="ddlUnit" InitialValue="" 
                    ErrorMessage="Unit is required" 
                    CssClass="text-warning d-block mt-1" Display="Dynamic" />
            </div>

            <div class="col-md-6">
                <label class="form-label">Visitor Name *</label>
                <asp:TextBox ID="txtVisitorName" runat="server" CssClass="form-control" 
                    placeholder="Enter visitor name" MaxLength="120"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvVisitorName" runat="server" 
                    ControlToValidate="txtVisitorName" 
                    ErrorMessage="Visitor name is required" 
                    CssClass="text-warning d-block mt-1" Display="Dynamic" />
            </div>

            <div class="col-md-6">
                <label class="form-label">Vehicle Number</label>
                <asp:TextBox ID="txtVehicleNo" runat="server" CssClass="form-control" 
                    placeholder="Enter vehicle number" MaxLength="20"></asp:TextBox>
            </div>

            <div class="col-md-12">
                <label class="form-label">Purpose</label>
                <asp:TextBox ID="txtPurpose" runat="server" CssClass="form-control" 
                    placeholder="Enter purpose of visit" MaxLength="80"></asp:TextBox>
            </div>

            <div class="col-md-12">
                <asp:HiddenField ID="hfGateLogId" runat="server" Value="0" />
                <asp:Button ID="btnSave" runat="server" Text="Save Gate Log" 
                    CssClass="btn btn-glass-primary me-2" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" 
                    CssClass="btn btn-glass" OnClick="btnCancel_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <!-- Gate Logs Grid -->
    <div class="glass-card">
        <h4 class="mb-4"><i class="bi bi-list-ul me-2"></i>All Gate Logs</h4>
        
        <div class="table-responsive">
            <asp:GridView ID="gvGateLogs" runat="server" CssClass="table table-glass" 
                AutoGenerateColumns="false" DataKeyNames="gate_log_id"
                OnRowCommand="gvGateLogs_RowCommand" OnRowDataBound="gvGateLogs_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="visitor_name" HeaderText="Visitor Name" />
                    <asp:BoundField DataField="vehicle_no" HeaderText="Vehicle No" />
                    <asp:BoundField DataField="purpose" HeaderText="Purpose" />
                    <asp:BoundField DataField="society_name" HeaderText="Society" />
                    <asp:BoundField DataField="building_name" HeaderText="Building" />
                    <asp:BoundField DataField="unit_no" HeaderText="Unit No" />
                    <asp:BoundField DataField="check_in" HeaderText="Check-In" 
                        DataFormatString="{0:dd-MMM-yyyy hh:mm tt}" />
                    <asp:BoundField DataField="check_out" HeaderText="Check-Out" 
                        DataFormatString="{0:dd-MMM-yyyy hh:mm tt}" />
                    
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:Label ID="lblStatus" runat="server" 
                                Text='<%# Eval("status") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnEdit" runat="server" 
                                CommandName="EditLog" 
                                CommandArgument='<%# Eval("gate_log_id") %>' 
                                CssClass="btn btn-glass btn-sm me-1" 
                                CausesValidation="false">
                                <i class="bi bi-pencil"></i>
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnDelete" runat="server" 
                                CommandName="DeleteLog" 
                                CommandArgument='<%# Eval("gate_log_id") %>' 
                                CssClass="btn btn-glass-danger btn-sm" 
                                OnClientClick="return confirm('Are you sure you want to delete this gate log?');"
                                CausesValidation="false">
                                <i class="bi bi-trash"></i>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center py-4">
                        <i class="bi bi-inbox" style="font-size: 48px; opacity: 0.5;"></i>
                        <p class="mt-2">No gate logs found. Add your first gate log above.</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>