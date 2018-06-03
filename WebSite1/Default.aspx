<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
           <asp:FileUpload ID="fpFile" runat="server" />
            <asp:Button ID="btnUpload" runat="server" Text="上传" OnClick="Button1_Click" />
            <br/>
           <span style="color:red;">只支持.txt文件
        </div>
    </form>
</body>
</html>
