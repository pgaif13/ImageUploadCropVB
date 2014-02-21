<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="js/jquery.min.js"></script>
    <script src="js/jquery.Jcrop.min.js"></script>
    <link rel="stylesheet" href="css/jquery.Jcrop.css" type="text/css" />    
</head>
<body>
    <form id="form1" runat="server">
        
    <div>
    <h3>Proof of Concept - Online Image Cropping</h3>
       Select an image using the "Browse..." button below and click the "Upload Image" when ready.
       <br />
        <span style="font-size:14px; color:Red; font-weight:bold">
           <asp:Literal ID="errorLiteral" runat="server"></asp:Literal>
        </span>
        <br />
        <asp:FileUpload ID="FileUpload1" runat="server" />
        <asp:Button ID="uploadfileButton" runat="server" Text="Upload Image" OnClick="UploadPhoto" />
        <br />
        <br />
        <table>
            <tr>
                <td style="width:400px;">
                    <asp:Literal ID="imagenameLiteral" runat="server">invalid.jpg</asp:Literal> 
                    <br /><br />
                    <div>
                         <asp:Image ID="Image1" runat="server" ImageUrl="~/uploaded_images/raw/invalid.jpg" /><br />
                    </div>           
                </td>
                <td style="width:400px;">
                    <asp:Literal ID="croppedpreviewLiteral" runat="server" Visible="false">Preview image:</asp:Literal>
                    <div id="previewdiv" style="width:120px;height:160px;overflow:hidden;background-color:#ffffff" runat="server">
                      <asp:Image ID="preview" Width="120" Height="160" ImageUrl="~/uploaded_images/raw/invalid.jpg" runat="server" />	       
                    </div>
                </td>
            </tr>
        </table> 

        <asp:HiddenField ID="X1value" runat="server" />        
        <asp:HiddenField ID="Y1value" runat="server" />       
        <asp:HiddenField ID="X2value" runat="server" />        
        <asp:HiddenField ID="Y2value" runat="server" />        
        <asp:HiddenField ID="Wvalue" runat="server" />        
        <asp:HiddenField ID="Hvalue" runat="server" />        
        <div id="cropinstructions" style="width:400px">              
        Move or rearrange the selection tool inside the original image until the preview selection displays 
        an image that you want to keep. When ready click the crop button and your selection will be
        saved permanently.<br /><br />
        <asp:Button ID="cropimageButton" runat="server" Text="Crop Image" onclick="btnCrop_Click" />             
        </div>
    </div>        
        <asp:Literal ID="jcropLiteral" runat="server">
          <script lang="Javascript">
              jQuery(document).ready(function () {
                  jQuery('#Image1').Jcrop({
                      onChange: showCoords,
                      onSelect: showCoords,
                      bgOpacity: .4,
                      setSelect: [0, 0, 900, 1200],
                      aspectRatio: 3 / 4,
                      trueSize: [120, 160]
                  });
              });
              // Simple event handler, called from onChange and onSelect
              // event handlers, as per the Jcrop invocation above
              function showCoords(c) {
                  jQuery('#X1value').val(Math.round(c.x));
                  jQuery('#Y1value').val(Math.round(c.y));
                  jQuery('#X2value').val(Math.round(c.x2));
                  jQuery('#Y2value').val(Math.round(c.y2));
                  jQuery('#Wvalue').val(Math.round(c.w));
                  jQuery('#Hvalue').val(Math.round(c.h));
                  var rx = 120 / c.w;
                  var ry = 160 / c.h;
                  $('#preview').css({
                      width: Math.round(rx * 0) + 'px',
                      height: Math.round(ry * 0) + 'px',
                      marginLeft: '-' + Math.round(rx * c.x) + 'px',
                      marginTop: '-' + Math.round(ry * c.y) + 'px'
                  });
              };
          </script>
        </asp:Literal>    
    </form>
    
</body>
</html>
