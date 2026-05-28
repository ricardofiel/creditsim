<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs"
    Inherits="CreditSim.Web.Default" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Credit Risk Simulator – Customer List</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css" rel="stylesheet" />
    <style>
        .low-risk    { color: #28a745; font-weight: bold; }
        .medium-risk { color: #ffc107; font-weight: bold; }
        .high-risk   { color: #dc3545; font-weight: bold; }
        .pager-row td { background: #f8f9fa; }
    </style>
</head>
<body class="bg-light">
    <form id="form1" runat="server">
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
        <div class="container">
            <a class="navbar-brand" href="#">
                <i class="bi bi-calculator"></i> Credit Risk Simulator
            </a>
            <div class="navbar-nav ms-auto">
                <a class="nav-link text-white" href="public/simulate.html">
                    <i class="bi bi-person-plus"></i> New Simulation
                </a>
            </div>
        </div>
    </nav>

    <div class="container mt-4">
        <div class="card">
            <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                <h5 class="card-title mb-0">
                    <i class="bi bi-clock-history"></i> Customer Simulations
                </h5>
                <a href="public/simulate.html" class="btn btn-light btn-sm">
                    <i class="bi bi-plus-circle"></i> Run New Simulation
                </a>
            </div>
            <div class="card-body p-0">
                <asp:GridView
                    ID="GridView1"
                    runat="server"
                    AllowPaging="true"
                    PageSize="10"
                    AutoGenerateColumns="false"
                    OnPageIndexChanging="GridView1_PageIndexChanging"
                    CssClass="table table-striped table-hover mb-0"
                    GridLines="None"
                    HeaderStyle-CssClass="table-primary">
                    <Columns>
                        <asp:BoundField DataField="Id"           HeaderText="ID"           />
                        <asp:BoundField DataField="Name"         HeaderText="Name"         />
                        <asp:BoundField DataField="Age"          HeaderText="Age"          />
                        <asp:BoundField DataField="Score"        HeaderText="Score"        />
                        <asp:BoundField DataField="RiskCategory" HeaderText="Risk"         />
                        <asp:BoundField DataField="LoanAmount"   HeaderText="Loan Amount"  DataFormatString="{0:C0}" />
                        <asp:BoundField DataField="AnnualIncome" HeaderText="Annual Income" DataFormatString="{0:C0}" />
                        <asp:BoundField DataField="CreditHistory" HeaderText="Credit History" />
                        <asp:BoundField DataField="CreatedAt"    HeaderText="Date"         DataFormatString="{0:g}" />
                    </Columns>
                    <PagerStyle CssClass="pagination justify-content-center" />
                </asp:GridView>
            </div>
        </div>
    </div>

    <footer class="bg-dark text-light text-center py-3 mt-5">
        <div class="container">
            <small>
                <i class="bi bi-info-circle"></i>
                This is a demonstration application. Do not use for actual credit decisions.
            </small>
        </div>
    </footer>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
