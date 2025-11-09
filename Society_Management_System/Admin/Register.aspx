<%@ Page Title="Register" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true"
    CodeBehind="Register.aspx.cs" Inherits="Society_Management_System.Admin.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&display=swap" rel="stylesheet" />

    <style>
        body {
            font-family: 'Poppins', sans-serif;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 2rem;
        }
        .register-container {
            position: relative;
            z-index: 1;
            width: 100%;
            max-width: 600px;
        }
        .glass-card {
            background: rgba(255, 255, 255, 0.15);
            backdrop-filter: blur(20px);
            border-radius: 25px;
            border: 1px solid rgba(255, 255, 255, 0.3);
            padding: 3rem;
            box-shadow: 0 15px 45px rgba(0, 0, 0, 0.2);
        }
        .register-header { text-align: center; margin-bottom: 2rem; color: #fff; }
        .btn-register {
            width: 100%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: none;
            border-radius: 50px;
            padding: 1rem;
            font-size: 1.1rem;
            font-weight: 600;
            color: #fff;
            margin-top: 1rem;
        }
        .error-message, .success-message {
            border-radius: 15px;
            padding: 1rem;
            margin-bottom: 1.5rem;
            text-align: center;
            color: #fff;
        }
        .error-message { background: rgba(220, 53, 69, 0.2); }
        .success-message { background: rgba(40, 167, 69, 0.2); }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="register-container">
        <div class="glass-card">
            <div class="register-header">
                <h1>Create Account</h1>
                <p>Join our community today</p>
            </div>

            <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-message">
                <asp:Label ID="lblError" runat="server"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="success-message">
                <asp:Label ID="lblSuccess" runat="server"></asp:Label>
            </asp:Panel>

            <asp:ValidationSummary ID="valSummary" runat="server" CssClass="validation-message"
                ValidationGroup="RegisterVG" DisplayMode="BulletList" />

            <div class="form-group">
                <label>Full Name</label>
                <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" Placeholder="John Doe" />
                <asp:RequiredFieldValidator ID="reqFullName" runat="server" ControlToValidate="txtFullName"
                    ErrorMessage="Full name is required." CssClass="validation-message" ValidationGroup="RegisterVG" />
            </div>

            <div class="form-group">
                <label>Username</label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" Placeholder="Choose a username" />
                <asp:RequiredFieldValidator ID="reqUsername" runat="server" ControlToValidate="txtUsername"
                    ErrorMessage="Username is required." CssClass="validation-message" ValidationGroup="RegisterVG" />
            </div>

            <div class="form-group">
                <label>Email</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" Placeholder="you@example.com" />
                <asp:RequiredFieldValidator ID="reqEmail" runat="server" ControlToValidate="txtEmail"
                    ErrorMessage="Email is required." CssClass="validation-message" ValidationGroup="RegisterVG" />
            </div>

            <div class="form-group">
                <label>Mobile Number</label>
                <asp:TextBox ID="txtMobile" runat="server" CssClass="form-control" Placeholder="+1234567890" />
                <asp:RequiredFieldValidator ID="reqMobile" runat="server" ControlToValidate="txtMobile"
                    ErrorMessage="Mobile number is required." CssClass="validation-message" ValidationGroup="RegisterVG" />
            </div>

            <asp:ScriptManager ID="ScriptManager1" runat="server" />


            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="form-group">
                        <label>Society</label>
                        <asp:DropDownList ID="ddlSociety" runat="server" CssClass="form-control"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlSociety_SelectedIndexChanged" />
                    </div>

                    <div class="form-group">
                        <label>Building</label>
                        <asp:DropDownList ID="ddlBuilding" runat="server" CssClass="form-control"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlBuilding_SelectedIndexChanged" />
                    </div>

                    <div class="form-group">
                        <label>Unit / Flat No.</label>
                        <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-control" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

            <div class="form-group">
                <label>Occupancy Type</label>
                <asp:DropDownList ID="ddlOccupancyType" runat="server" CssClass="form-control" />
            </div>

            <div class="form-group">
                <label>Password</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
            </div>

            <div class="form-group">
                <label>Confirm Password</label>
                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" />
                <asp:CompareValidator ID="cmpPasswords" runat="server"
                    ControlToValidate="txtConfirmPassword" ControlToCompare="txtPassword"
                    ErrorMessage="Passwords do not match." CssClass="validation-message" ValidationGroup="RegisterVG" />
            </div>

            <asp:Button ID="btnRegister" runat="server" CssClass="btn-register" Text="Create Account"
                OnClick="btnRegister_Click" ValidationGroup="RegisterVG" />

            <div class="form-footer text-center mt-3">
                <asp:HyperLink ID="lnkLogin" runat="server" NavigateUrl="~/Admin/AdminDashboard.aspx">Go Back</asp:HyperLink>
            </div>
        </div>
    </div>
</asp:Content>
