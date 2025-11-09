<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManageAmenities.aspx.cs" Inherits="Society_Management_System.Admin.ManageAmenities" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageTitle" runat="server">
    Manage Amenities
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="glass-effect p-6 rounded-2xl shadow-lg">
        <h2 class="text-xl font-semibold mb-4 text-white">Add / Edit Amenity</h2>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <div>
                <label class="block mb-2">Select Society</label>
                <asp:DropDownList ID="ddlSociety" runat="server" CssClass="glass-input w-full"></asp:DropDownList>
            </div>

            <div>
                <label class="block mb-2">Amenity Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="glass-input w-full" placeholder="Enter Amenity Name"></asp:TextBox>
            </div>

            <div class="flex items-center space-x-2 mt-6">
                <asp:CheckBox ID="chkBookingRequired" runat="server" CssClass="form-check-input" />
                <label for="chkBookingRequired">Booking Required</label>
            </div>
        </div>

        <div class="flex space-x-4">
            <asp:Button ID="btnAdd" runat="server" Text="Add Amenity" CssClass="btn btn-primary" OnClick="btnAdd_Click" />
            <asp:Button ID="btnUpdate" runat="server" Text="Update Amenity" CssClass="btn btn-primary" Visible="false" OnClick="btnUpdate_Click" />
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary" Visible="false" OnClick="btnCancel_Click" />
        </div>
    </div>

    <div class="glass-effect mt-8 p-6 rounded-2xl shadow-lg">
        <h2 class="text-xl font-semibold mb-4 text-white">Amenities List</h2>
        <asp:GridView ID="gvAmenities" runat="server" AutoGenerateColumns="False" CssClass="glass-grid"
            OnRowCommand="gvAmenities_RowCommand">
            <Columns>
                <asp:BoundField DataField="amenity_id" HeaderText="ID" ReadOnly="True" />
                <asp:BoundField DataField="society_name" HeaderText="Society" />
                <asp:BoundField DataField="name" HeaderText="Amenity Name" />
                <asp:TemplateField HeaderText="Booking Required">
                    <ItemTemplate>
                        <%# (bool)Eval("booking_required") ? "Yes" : "No" %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkEdit" runat="server" CommandName="EditAmenity" CommandArgument='<%# Eval("amenity_id") %>' CssClass="grid-link">
                            <i class="bi bi-pencil-square"></i> Edit
                        </asp:LinkButton>
                        <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteAmenity" CommandArgument='<%# Eval("amenity_id") %>' CssClass="grid-link delete-link" OnClientClick="return confirm('Are you sure you want to delete this amenity?');">
                            <i class="bi bi-trash"></i> Delete
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>

