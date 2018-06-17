<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MatrixOperations.aspx.cs" Inherits="PredictNumber.Pages.MatrixOperations" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2 style="color:saddlebrown;">Steps to follow:</h2>
           <ul>
               <li>Browse a text file containing sample data (history data).</li>
               <li>Click on <i>Read File</i> button to get model ready</li>
               <li>Once model is ready, you can enter predictor vaiables(comma separated) in textbox.</li>
                <li>Click on <i>Predict Value</i> button to get predicted output number.</li>
           </ul>
        </div>
        <br />
        <hr />
    <div>
        <div style="margin-left: 400px;">
            <h2> Step-1 : </h2>
        <asp:Label ID="lblSampleUpload" runat="server" Text="Upload Sample data file(.txt) to Test"></asp:Label><br/>
    <asp:FileUpload ID="FileUpload1" runat="server" />
<asp:Button ID="btnImport" runat="server" Text="Read File" OnClick="ImportMatrixFile" />

<asp:GridView ID="GridView1" runat="server">
</asp:GridView>
                    <asp:Label ID="lblTranspose" style="color:darkgreen;" runat="server" Text=""></asp:Label>
            </div>

        <br />
        <hr />
          <br />
        <div style="margin-left: 400px;">
             <h2> Step-2 : </h2>
         <asp:Label ID="lblMatrix" runat="server" Text="Enter comma separated predictor numbers : "></asp:Label>
        <asp:TextBox ID="txtArr" style="width:500px;" runat="server" Placeholder="Enter numbers here"></asp:TextBox>
        <asp:Button ID="btnRead" Text="Predict Value" runat="server" OnClick="ReadNumbers"/>
        <br/> <br/>

             </div>
        <br /> <br/> <br/>  <hr />

            <h2 style="margin-left: 400px;"> Output : </h2>
         <asp:Label ID="Label1" style="color:darkgreen;margin-left: 400px;" runat="server" Text=""></asp:Label>
    </div>
       <%-- <asp:Button ID="btnDtable" runat="server" OnClick="GenerateTable" />--%>
    </form>
</body>
</html>