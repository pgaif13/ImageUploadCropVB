Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Class _Default
    Inherits System.Web.UI.Page

    Dim myphotoutils As New PhotoUtils
    Dim hminRaw As Integer = Convert.ToInt32(Global.Resources.AppSettings.uploadheight)
    Dim imgUploadPreview As Integer = Convert.ToInt32(Global.Resources.AppSettings.uploadpreviewh)
    Dim maxSize As Integer = Convert.ToInt32(Global.Resources.AppSettings.maxuploadsize)
    Dim hminCropped As Integer = Convert.ToInt32(Global.Resources.AppSettings.passheight)
    Dim wminCropped As Integer = Convert.ToInt32(Global.Resources.AppSettings.passwidth)
    Dim prevw As Integer = Convert.ToInt32(Global.Resources.AppSettings.previewwidth)
    Dim prevh As Integer = Convert.ToInt32(Global.Resources.AppSettings.previewheight)

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' preset previewdiv and preview img dimensions
        If Not IsPostBack Then
            preview.Width = New Unit(prevw)
            preview.Height = New Unit(prevh)
            previewdiv.Attributes("style") = previewdiv.Attributes("style").Replace("120", prevw.ToString).Replace("160", prevh.ToString)
        End If
    End Sub

    ''' <summary>
    ''' Handles uploading photo and updating related data
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub UploadPhoto(ByVal sender As Object, ByVal e As System.EventArgs)
        errorLiteral.Text = ""
        Dim path As String = Server.MapPath("~/uploaded_images/raw/")
        Dim fileOK As Boolean = False
        Dim fileSize As Integer = FileUpload1.PostedFile.ContentLength
        ' for demo/testing imagefile is a prefix value
        ' change this to accomodate a real life application
        Dim testfilename As String = "image_upload_test"
        Dim resultstr As String = ""
        Dim fileExtension As String = System.IO.Path.GetExtension(FileUpload1.FileName).ToLower()
        If FileUpload1.HasFile Then
            Dim opresult As String = myphotoutils.UploadPhoto(FileUpload1.PostedFile, testfilename, 262144, "raw", hminRaw)
            resultstr = opresult
            If opresult = "OK: File uploaded!" Then
                imagenameLiteral.Text = testfilename + fileExtension
                ViewState("imagename") = testfilename + fileExtension
                Image1.ImageUrl = "~/uploaded_images/raw/" + testfilename + fileExtension
                ' prefix the height of the source image in the UI
                Image1.Height = New Unit(imgUploadPreview)
                Dim img1w As Integer = myphotoutils.CalculateResizedWidth(ViewState("imagename"), path, imgUploadPreview)
                Image1.Width = New Unit(img1w)
                preview.ImageUrl = "~/uploaded_images/raw/" + testfilename + fileExtension
                ' update preview JS parameters for width and height of the uploaded image
                Dim jsWidth As Integer = myphotoutils.CalculateResizedWidth(ViewState("imagename"), path, hminRaw)
                UpdatePreviewJsWH(jsWidth, hminRaw, path, testfilename + fileExtension)
                croppedpreviewLiteral.Visible = True
            Else
                resultstr = opresult
                errorLiteral.Text = resultstr
                croppedpreviewLiteral.Visible = False
            End If
        Else
            resultstr = "ERROR: Cannot upload photos without valid image file."
            errorLiteral.Text = resultstr
        End If
    End Sub

    ''' <summary>
    ''' Handles the crop button click action
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub btnCrop_Click(sender As Object, e As EventArgs)
        Dim path As String = Server.MapPath("~/uploaded_images/raw/")
        ' if this is a crop of a just cropped image (after a postback) we need to change the folder url
        ' for the images to the cropped folder
        If Image1.ImageUrl.Contains("cropped") Then
            path = Server.MapPath("~/uploaded_images/cropped/")
        End If
        ' call CropImage function
        Dim res As String = myphotoutils.CropImage(ViewState("imagename"), path, CInt(X1value.Value), CInt(Y1value.Value), CInt(Wvalue.Value), CInt(Hvalue.Value))
        If res.StartsWith("OK") Then
            ' refresh UI
            Image1.ImageUrl = "~/uploaded_images/cropped/" + ViewState("imagename")
            Image1.Height = New Unit(imgUploadPreview)
            Dim img1w As Integer = myphotoutils.CalculateResizedWidth(ViewState("imagename"), Server.MapPath("~/uploaded_images/cropped/"), imgUploadPreview)
            Image1.Width = New Unit(img1w)
            preview.ImageUrl = "~/uploaded_images/cropped/" + ViewState("imagename")
            ' update JS paramaters for refresh function
            UpdatePreviewJsWH(wminCropped, hminCropped, path.Replace("raw", "cropped"), ViewState("imagename"))
        Else
            errorLiteral.Text = res
        End If
    End Sub

    ''' <summary>
    ''' Re-writes the Javascript function based on the size of the specified image in
    ''' the input parameters so the preview function has the proper width and height values
    ''' </summary>
    ''' <param name="folderpath"></param>
    ''' <param name="filename"></param>
    ''' <remarks></remarks>
    Sub UpdatePreviewJsWH(ByVal neww As Integer, ByVal newh As Integer, ByVal folderpath As String, ByVal filename As String)
        ' todo review we don;t need this image here
        ' change width based on source image
        Dim startselect As Integer = jcropLiteral.Text.IndexOf("width: Math.round")
        Dim startvalue As Integer = jcropLiteral.Text.IndexOf("(", startselect)
        Dim endvalue As Integer = jcropLiteral.Text.IndexOf(")", startvalue)
        Dim selectvalue As String = jcropLiteral.Text.Substring(startvalue, endvalue - startvalue + 1)
        Dim newvalue As String = "(rx*" + neww.ToString + ")"
        jcropLiteral.Text = jcropLiteral.Text.Replace(selectvalue, newvalue)
        ' change height based on source image
        startselect = jcropLiteral.Text.IndexOf("height: Math.round")
        startvalue = jcropLiteral.Text.IndexOf("(", startselect)
        endvalue = jcropLiteral.Text.IndexOf(")", startvalue)
        selectvalue = jcropLiteral.Text.Substring(startvalue, endvalue - startvalue + 1)
        newvalue = "(ry*" + newh.ToString + ")"
        jcropLiteral.Text = jcropLiteral.Text.Replace(selectvalue, newvalue)
        ' Configure Explicit Resizing 
        startselect = jcropLiteral.Text.IndexOf("trueSize:")
        startvalue = jcropLiteral.Text.IndexOf("[", startselect)
        endvalue = jcropLiteral.Text.IndexOf("]", startvalue)
        selectvalue = jcropLiteral.Text.Substring(startvalue, endvalue - startvalue + 1)
        newvalue = "[" + neww.ToString + ", " + newh.ToString + "]"
        jcropLiteral.Text = jcropLiteral.Text.Replace(selectvalue, newvalue)
        ' determine fixed image ratio from Appsettings
        Dim ratio As String = Global.Resources.AppSettings.passwidth + "/" + Global.Resources.AppSettings.passheight
        startselect = jcropLiteral.Text.IndexOf("aspectRatio:")
        startvalue = jcropLiteral.Text.IndexOf(" ", startselect)
        endvalue = jcropLiteral.Text.IndexOf(",", startvalue)
        selectvalue = jcropLiteral.Text.Substring(startvalue, endvalue - startvalue + 1)
        newvalue = " " + ratio + ","
        jcropLiteral.Text = jcropLiteral.Text.Replace(selectvalue, newvalue)
        ' configure default values for rx            
        startselect = jcropLiteral.Text.IndexOf("var rx")
        startvalue = jcropLiteral.Text.IndexOf("=", startselect)
        endvalue = jcropLiteral.Text.IndexOf(";", startvalue)
        selectvalue = jcropLiteral.Text.Substring(startvalue, endvalue - startvalue + 1)
        newvalue = "= " + Global.Resources.AppSettings.previewwidth + " / c.w;"
        jcropLiteral.Text = jcropLiteral.Text.Replace(selectvalue, newvalue)
        ' configure default values for ry 
        startselect = jcropLiteral.Text.IndexOf("var ry ")
        startvalue = jcropLiteral.Text.IndexOf("=", startselect)
        endvalue = jcropLiteral.Text.IndexOf(";", startvalue)
        selectvalue = jcropLiteral.Text.Substring(startvalue, endvalue - startvalue + 1)
        newvalue = "= " + Global.Resources.AppSettings.previewheight + " / c.h;"
        jcropLiteral.Text = jcropLiteral.Text.Replace(selectvalue, newvalue)
    End Sub

    
End Class
