Imports Microsoft.VisualBasic
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Public Class PhotoUtils

    ''' <summary>
    ''' Handles the logic for uploading a photo using a fileupload control
    ''' </summary>
    ''' <param name="postedfile"></param>
    ''' <param name="targetfilename"></param>   
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function UploadPhoto(ByVal postedfile As HttpPostedFile, ByVal targetfilename As String, Optional ByVal maxsize As Integer = 262144, Optional folder As String = "", Optional hmindimension As Integer = 0) As String
        Dim result As String = ""
        Dim ctx As HttpContext = HttpContext.Current
        Dim path As String = ctx.Server.MapPath("~/uploaded_images/")
        If folder <> "" Then
            path += folder + "\"
        End If
        Dim fileOK As Boolean = False
        Dim fileSize As Integer = postedfile.ContentLength
        Dim maxk As String = Math.Round(maxsize / 1024).ToString
        If fileSize > 0 And targetfilename.Length > 0 Then
            Dim fileExtension As String
            fileExtension = System.IO.Path.GetExtension(postedfile.FileName).ToLower()
            Dim allowedExtensions As String() = {".jpg", ".jpeg"}
            For i As Integer = 0 To allowedExtensions.Length - 1
                If fileExtension = allowedExtensions(i) Then
                    fileOK = True
                End If
            Next
            If fileOK Then
                If (fileSize < maxsize) Then
                    Try
                        result = ResizePhotoUpload(postedfile.InputStream, targetfilename + fileExtension, path, hmindimension)
                    Catch ex As Exception
                        result = "ERROR: File could not be uploaded to " + path + "<br>" + ex.Message
                    End Try
                Else
                    result = "ERROR: File is larger than " + maxk + "K. Please upload a smaller image"
                End If
            Else
                result = "ERROR: Cannot accept files of this type."
            End If
        Else
            result = "ERROR: Cannot upload photos without valid targetfilename."
        End If
        Return result
    End Function

    ''' <summary>
    ''' Normalizes the size of uploaded images
    ''' </summary>
    ''' <param name="inputfilestream"></param>
    ''' <param name="finalfilename"></param>
    ''' <param name="folderpath"></param>
    ''' <param name="hmindimension"></param> 
    ''' <returns>Text message with operation result</returns>
    ''' <remarks>
    ''' Images will be scaled up or down depending on original size.
    ''' </remarks>
    Public Function ResizePhotoUpload(ByVal inputfilestream As Stream, ByVal finalfilename As String, ByVal folderpath As String, ByVal hmindimension As Integer) As String
        Dim result As String = ""
        Dim newStillWidth, newStillHeight As Integer ' new width/height for the new resized uploaded image 
        Dim ori1 As Integer ' used for calculation of new dimensions
        Dim originalimg As System.Drawing.Image ' used to hold the original image 
        Try
            originalimg = System.Drawing.Image.FromStream(inputfilestream)
            ' determines larger side and scales up/down the image to 1200 pixels on the smaller side
            ' maintaining the image ratio
            If (originalimg.Width) > (originalimg.Height) Then
                ' landscape image rules
                ' set img height to hmindimension and scale width
                ori1 = originalimg.Height
                newStillHeight = hmindimension
                newStillWidth = originalimg.Width * (hmindimension / ori1)
            Else
                ' portrait rules image rules
                ' set img height to hmindimension and scale width
                ori1 = originalimg.Width
                newStillHeight = hmindimension
                newStillWidth = newStillHeight * (originalimg.Width / originalimg.Height)
            End If
            Dim still As New Bitmap(newStillWidth, newStillHeight)
            ' Create a graphics object 
            Dim gr_dest_still As Graphics = Graphics.FromImage(still)
            ' force the background of new image to white 
            Dim sb = New SolidBrush(System.Drawing.Color.White)
            gr_dest_still.FillRectangle(sb, 0, 0, still.Width, still.Height)
            ' Re-draw the image to target normalized dimensions
            gr_dest_still.DrawImage(originalimg, 0, 0, still.Width, still.Height)
            Try
                ' define quality parameters for new jpeg image
                Dim codecencoder As ImageCodecInfo = GetEncoder("image/jpeg")
                Dim quality As Integer = 90
                Dim encodeparams As EncoderParameters = New EncoderParameters(1)
                Dim qualityparam As EncoderParameter = New EncoderParameter(Encoder.Quality, quality)
                encodeparams.Param(0) = qualityparam
                still.SetResolution(96, 96)
                If Not folderpath.EndsWith("\") Then
                    folderpath += "\"
                End If
                still.Save(folderpath + finalfilename, codecencoder, encodeparams)
                result = "OK: File uploaded!"
            Catch ex As Exception
                result = "ERROR: there was a problem saving the image. " + ex.Message
            End Try
            ' dispose used image resources
            If Not still Is Nothing Then
                still.Dispose()
                still = Nothing
            End If
        Catch ex As Exception
            result = "ERROR: that was not an image we could process. " + ex.Message
        End Try
        Return result
    End Function

    ''' <summary>
    ''' Creates and saves a cropped/scaled version of the image identified by name and folder
    ''' using the coordinates provided in the input parameters
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <param name="folderpath"></param>
    ''' <param name="X">x coordinate for crop origin</param>
    ''' <param name="Y">y coordinate for crop origin</param>
    ''' <param name="W">width of cropped image</param>
    ''' <param name="H">height of cropped image</param>
    ''' <returns>Text message with operation result</returns>
    ''' <remarks>
    '''  For now we don't validate that the coordinates are valid.  
    '''  This can be incorporated later if desired .
    '''  </remarks>
    Public Function CropImage(ByVal filename As String, ByVal folderpath As String, ByVal X As Integer, ByVal Y As Integer, ByVal W As Integer, ByVal H As Integer) As String
        Dim result As String = ""
        ' target width for all final cropped files
        Dim croppedfinalh As Integer = Convert.ToInt32(Global.Resources.AppSettings.passheight)
        Dim croppedfinalw As Integer = Convert.ToInt32(Global.Resources.AppSettings.passwidth)
        Try
            If Not folderpath.EndsWith("\") Then
                folderpath += "\"
            End If
            Dim image1 As Bitmap = CType(Image.FromFile(folderpath + filename, True), Bitmap)
            Dim rect As New Rectangle(X, Y, W, H)
            Dim cropped As Bitmap = image1.Clone(rect, image1.PixelFormat)
            ' dispose image1 we don't needed anymore and if we don't dispose it now we won't 
            ' be able to overwrite inthe code below if that is desired
            If Not image1 Is Nothing Then
                image1.Dispose()
                image1 = Nothing
            End If
            ' resize cropped to standard size
            Dim finalcropped As New Bitmap(croppedfinalw, croppedfinalh)
            Dim gr_finalcropped As Graphics = Graphics.FromImage(finalcropped)
            Dim sb = New SolidBrush(System.Drawing.Color.White)
            gr_finalcropped.FillRectangle(sb, 0, 0, finalcropped.Width, finalcropped.Height)
            gr_finalcropped.DrawImage(cropped, 0, 0, finalcropped.Width, finalcropped.Height)
            Try
                ' define quality parameters for new jpeg image
                Dim codecencoder As ImageCodecInfo = GetEncoder("image/jpeg")
                Dim quality As Integer = 92
                Dim encodeparams As EncoderParameters = New EncoderParameters(1)
                Dim qualityparam As EncoderParameter = New EncoderParameter(Encoder.Quality, quality)
                encodeparams.Param(0) = qualityparam
                finalcropped.SetResolution(240, 240)
                ' refactor this to remove dependency to hard typed folders
                folderpath = folderpath.Replace("raw", "cropped")
                finalcropped.Save(folderpath + filename, codecencoder, encodeparams)
                result = "OK - File cropped"
            Catch ex As Exception
                result = "ERROR: there was a problem saving the image. " + ex.Message
            End Try
            If Not cropped Is Nothing Then
                cropped.Dispose()
                cropped = Nothing
            End If
            If Not finalcropped Is Nothing Then
                finalcropped.Dispose()
                finalcropped = Nothing
            End If
        Catch ex As Exception
            result = "ERROR: that was not an image we could process. " + ex.Message
        End Try
        Return result
    End Function

    ''' <summary>
    ''' Returns the proper codec for the specified image mime type
    ''' </summary>
    ''' <param name="mimetype"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function GetEncoder(ByVal mimetype As String) As ImageCodecInfo
        Dim result As ImageCodecInfo = Nothing
        For Each codec As ImageCodecInfo In ImageCodecInfo.GetImageEncoders()
            If (codec.MimeType = mimetype) Then
                result = codec
            End If
        Next
        Return result
    End Function

    ''' <summary>
    ''' Given a fixed height, recalculates the width of an image to maintain ratio
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <param name="folderpath"></param>
    ''' <param name="newh"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CalculateResizedWidth(ByVal filename As String, ByVal folderpath As String, ByVal newh As Integer) As Integer
        If Not folderpath.EndsWith("\") Then
            folderpath += "\"
        End If
        Dim image1 As Bitmap = CType(Image.FromFile(folderpath + filename, True), Bitmap)
        Dim result As Integer = 0
        If Not image1 Is Nothing Then
            result = newh * image1.Width / image1.Height
            image1.Dispose()
        End If
        Return result
    End Function

End Class
