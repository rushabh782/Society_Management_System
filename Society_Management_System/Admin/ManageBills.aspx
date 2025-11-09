<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManageBills.aspx.cs" Inherits="Society_Management_System.Admin.ManageBills" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    Manage Maintenance Bills
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="glass-effect p-6 max-w-5xl mx-auto mb-8">
        <h2 class="text-xl font-semibold mb-4">Add or Edit Bill</h2>
        <asp:Label ID="LblMessage" runat="server" CssClass="text-danger fw-bold mb-2 d-block" EnableViewState="false" />
        <asp:ValidationSummary runat="server" CssClass="alert alert-danger" />

        <div class="row">
            <div class="col-md-4 mb-3">
                <label>Society</label>
                <asp:DropDownList ID="DdlSociety" runat="server" CssClass="glass-input w-100"
                    AutoPostBack="true" OnSelectedIndexChanged="DdlSociety_SelectedIndexChanged" />
            </div>
            <div class="col-md-4 mb-3">
                <label>Unit</label>
                <asp:DropDownList ID="DdlUnit" runat="server" CssClass="glass-input w-100" />
            </div>
            <div class="col-md-4 mb-3">
                <label>Bill Month</label>
                <asp:TextBox ID="TxtBillMonth" runat="server" CssClass="glass-input w-100" TextMode="Date" placeholder="YYYY-MM-01" />
            </div>
            <div class="col-md-4 mb-3">
                <label>Due Date</label>
                <asp:TextBox ID="TxtDueDate" runat="server" CssClass="glass-input w-100" TextMode="Date" placeholder="YYYY-MM-DD"
                    AutoPostBack="true" OnTextChanged="TxtDueDate_TextChanged" />
            </div>
            <div class="col-md-4 mb-3">
                <label>Total Amount</label>
                <asp:TextBox ID="TxtTotalAmount" runat="server" CssClass="glass-input w-100" TextMode="Number" ReadOnly="true" />
            </div>
        </div>
        <div class="mb-3">
            <asp:Button ID="BtnSave" runat="server" Text="Save Bill" CssClass="btn btn-primary me-2" OnClick="BtnSave_Click" />
            <asp:Button ID="BtnCancel" runat="server" Text="Clear" CssClass="btn btn-outline-secondary" OnClick="BtnCancel_Click" />
            <asp:HiddenField ID="HfBillId" runat="server" />
        </div>

        <div class="mb-3">
            <h4 class="mt-4">Bill Items</h4>
            <div class="d-flex gap-2">
                <asp:TextBox ID="TxtItemDesc" runat="server" CssClass="glass-input" placeholder="Description" />
                <asp:TextBox ID="TxtItemAmount" runat="server" CssClass="glass-input" TextMode="Number" placeholder="Amount" Width="120px" />
                <asp:Button ID="BtnAddItem" runat="server" Text="Add Item" CssClass="btn btn-primary" OnClick="BtnAddItem_Click" />
            </div>
            <asp:GridView ID="GvBillItems" runat="server" CssClass="glass-grid mt-2"
                AutoGenerateColumns="False"
                DataKeyNames="ItemId"
                OnRowDeleting="GvBillItems_RowDeleting">
                <Columns>
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:F2}" />
                    <asp:CommandField ShowDeleteButton="True" DeleteText="<i class='bi bi-trash3-fill text-danger'></i>" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <div class="glass-effect p-6 max-w-6xl mx-auto">
        <h2 class="mb-4 text-xl font-semibold">All Maintenance Bills</h2>
        <asp:GridView ID="GvBills" runat="server" CssClass="glass-grid" AutoGenerateColumns="False"
            DataKeyNames="bill_id"
            OnRowEditing="GvBills_RowEditing"
            OnRowDeleting="GvBills_RowDeleting"
            OnRowCancelingEdit="GvBills_RowCancelingEdit"
            OnRowUpdating="GvBills_RowUpdating">
            <Columns>
                <asp:BoundField DataField="bill_id" HeaderText="Bill ID" ReadOnly="True" />
                <asp:BoundField DataField="name" HeaderText="Society" />
                <asp:BoundField DataField="unit_no" HeaderText="Unit" />
                <asp:BoundField DataField="bill_month" HeaderText="Bill Month" DataFormatString="{0:yyyy-MM}" />
                <asp:BoundField DataField="due_date" HeaderText="Due Date" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="total_amount" HeaderText="Total" DataFormatString="{0:F2}" />
                <asp:BoundField DataField="status" HeaderText="Status" />
                <asp:CommandField ShowEditButton="True" EditText="<i class='bi bi-pencil-square'></i>"
                    ShowDeleteButton="True" DeleteText="<i class='bi bi-trash3 text-danger'></i>" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>