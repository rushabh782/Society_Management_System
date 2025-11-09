<%@ Page Title="" Language="C#" MasterPageFile="~/Member/Member.Master" AutoEventWireup="true" CodeBehind="MyGateLogs.aspx.cs" Inherits="Society_Management_System.Member.MyGateLogs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h2><i class="bi bi-door-open me-2"></i>My Gate Logs</h2>
        <p>Approve pending visitors for your unit</p>
    </div>

    <!-- Success/Error Message -->
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-dismissible fade show" role="alert">
        <asp:Label ID="lblMessage" runat="server"></asp:Label>
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </asp:Panel>

    <!-- Info Card -->
    <div class="row mb-4">
        <div class="col-md-12">
            <div class="glass-card">
                <div class="row">
                    <div class="col-md-3 text-center">
                        <i class="bi bi-clock-history" style="font-size: 48px; opacity: 0.8;"></i>
                        <h3 class="mt-2"><asp:Label ID="lblPendingCount" runat="server" Text="0"></asp:Label></h3>
                        <p style="opacity: 0.8;">Pending Approvals</p>
                    </div>
                    <div class="col-md-9">
                        <h5><i class="bi bi-info-circle me-2"></i>How It Works</h5>
                        <ul style="opacity: 0.9; line-height: 2;">
                            <li>Security adds visitor details when they arrive at the gate</li>
                            <li>You receive a notification to approve the visitor</li>
                            <li>Review visitor details and click "Approve" to allow entry</li>
                            <li>Once approved, the visitor can proceed to your unit</li>
                            <li>Click "Checkout" when visitor leaves to record exit time</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Pending Gate Logs -->
    <div class="glass-card mb-4">
        <h4 class="mb-4">
            <i class="bi bi-list-check me-2"></i>Pending Visitor Approvals
        </h4>
        
        <div class="table-responsive">
            <asp:GridView ID="gvPendingLogs" runat="server" CssClass="table table-glass" 
                AutoGenerateColumns="false" DataKeyNames="gate_log_id"
                OnRowCommand="gvPendingLogs_RowCommand">
                <Columns>
                    <asp:BoundField DataField="visitor_name" HeaderText="Visitor Name" />
                    <asp:BoundField DataField="vehicle_no" HeaderText="Vehicle No" />
                    <asp:BoundField DataField="purpose" HeaderText="Purpose" />
                    <asp:BoundField DataField="check_in" HeaderText="Arrival Time" 
                        DataFormatString="{0:dd-MMM-yyyy hh:mm tt}" />
                    
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class="badge-pending">
                                <i class="bi bi-clock me-1"></i>Pending
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnApprove" runat="server" 
                                CommandName="ApproveLog" 
                                CommandArgument='<%# Eval("gate_log_id") %>' 
                                CssClass="btn btn-glass-success btn-sm"
                                OnClientClick="return confirm('Are you sure you want to approve this visitor?');">
                                <i class="bi bi-check-circle me-1"></i>Approve
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center py-5">
                        <i class="bi bi-check-circle" style="font-size: 64px; opacity: 0.5;"></i>
                        <h5 class="mt-3">No Pending Approvals</h5>
                        <p style="opacity: 0.8;">You're all caught up! There are no visitors waiting for approval.</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <!-- Recently Approved Section -->
    <div class="glass-card">
        <h4 class="mb-4">
            <i class="bi bi-check-all me-2"></i>Recently Approved Visitors
        </h4>
        
        <div class="table-responsive">
            <asp:GridView ID="gvRecentlyApproved" runat="server" CssClass="table table-glass" 
                AutoGenerateColumns="false" DataKeyNames="gate_log_id"
                OnRowCommand="gvRecentlyApproved_RowCommand">
                <Columns>
                    <asp:BoundField DataField="visitor_name" HeaderText="Visitor Name" />
                    <asp:BoundField DataField="vehicle_no" HeaderText="Vehicle No" />
                    <asp:BoundField DataField="purpose" HeaderText="Purpose" />
                    <asp:BoundField DataField="check_in" HeaderText="Check-In" 
                        DataFormatString="{0:dd-MMM-yyyy hh:mm tt}" />
                    <asp:BoundField DataField="check_out" HeaderText="Check-Out" 
                        DataFormatString="{0:dd-MMM-yyyy hh:mm tt}" />
                    
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span style="background: rgba(40, 167, 69, 0.3); color: #28a745; border: 1px solid rgba(40, 167, 69, 0.5); padding: 5px 15px; border-radius: 20px; font-weight: 500;">
                                <i class="bi bi-check-circle me-1"></i>Approved
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnCheckout" runat="server" 
                                CommandName="CheckoutLog" 
                                CommandArgument='<%# Eval("gate_log_id") %>' 
                                CssClass="btn btn-glass-warning btn-sm"
                                Visible='<%# Eval("check_out") == DBNull.Value %>'
                                OnClientClick="return confirm('Mark this visitor as checked out?');">
                                <i class="bi bi-box-arrow-right me-1"></i>Checkout
                            </asp:LinkButton>
                            <span class="badge" style="background: rgba(108, 117, 125, 0.3); color: #6c757d; border: 1px solid rgba(108, 117, 125, 0.5); padding: 5px 15px; border-radius: 20px;"
                                  runat="server" visible='<%# Eval("check_out") != DBNull.Value %>'>
                                <i class="bi bi-check-all me-1"></i>Checked Out
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center py-4">
                        <p style="opacity: 0.8;">No recently approved visitors</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>