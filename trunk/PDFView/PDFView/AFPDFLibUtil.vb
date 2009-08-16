﻿Imports System.Drawing
Imports System.Windows.Forms
Imports System.Drawing.Printing

Public Class AFPDFLibUtil

  'This uses an XPDF wrapper written by Jose Antonio Sandoval Soria of Guadalajara, México
  'The source is available at http://www.codeproject.com/KB/files/xpdf_csharp.aspx
  '
  'I have ported over to VB.NET select functionality from the C# PDF viewer in the above project

  Const RENDER_DPI As Integer = 200
  Const PRINT_DPI As Integer = 300

  Public Shared Function GetOptimalDPI(ByRef pdfDoc As PDFLibNet.PDFWrapper, ByRef oPictureBox As PictureBox) As Integer
    GetOptimalDPI = 0
    If pdfDoc IsNot Nothing Then
      If pdfDoc.PageWidth > 0 And pdfDoc.PageHeight > 0 Then
        Dim DPIScalePercent As Single = 72 / pdfDoc.RenderDPI
        Dim HScale As Single = oPictureBox.Width / (pdfDoc.PageWidth * DPIScalePercent)
        Dim VScale As Single = oPictureBox.Height / (pdfDoc.PageHeight * DPIScalePercent)
        If HScale < VScale Then
          GetOptimalDPI = Math.Floor(72 * HScale)
        Else
          GetOptimalDPI = Math.Floor(72 * VScale)
        End If
      End If
    End If
  End Function

  Public Shared Function GetImageFromPDF(ByRef pdfDoc As PDFLibNet.PDFWrapper, ByVal PageNumber As Integer, Optional ByVal DPI As Integer = RENDER_DPI) As System.Drawing.Image
    GetImageFromPDF = Nothing
    Try
      If pdfDoc IsNot Nothing Then
        pdfDoc.CurrentPage = PageNumber
        pdfDoc.CurrentX = 0
        pdfDoc.CurrentY = 0
        If DPI < 1 Then DPI = RENDER_DPI
        pdfDoc.RenderDPI = DPI
        Dim oPictureBox As New PictureBox
        pdfDoc.RenderPage(oPictureBox.Handle)
        GetImageFromPDF = Render(pdfDoc)
        oPictureBox.Dispose()
      End If
    Catch ex As Exception
      Throw ex
    End Try
  End Function

  Public Shared Function Render(ByRef pdfDoc As PDFLibNet.PDFWrapper) As System.Drawing.Bitmap
    Try
      If pdfDoc IsNot Nothing Then
        Dim backbuffer As System.Drawing.Bitmap = New Bitmap(pdfDoc.PageWidth, pdfDoc.PageHeight)
        pdfDoc.ClientBounds = New Rectangle(0, 0, pdfDoc.PageWidth, pdfDoc.PageHeight)
        Dim g As Graphics = Graphics.FromImage(backbuffer)
        Using g
          Dim hdc As IntPtr = g.GetHdc()
          pdfDoc.DrawPageHDC(hdc)
          g.ReleaseHdc()
        End Using
        g.Dispose()
        Return backbuffer
      End If
    Catch ex As Exception
      Throw ex
      Return Nothing
    End Try
    Return Nothing
  End Function
  Public Shared Sub ExportPDF(ByRef pdfDoc As PDFLibNet.PDFWrapper, ByVal fileName As String, Optional ByVal startPage As Integer = 1, Optional ByVal endPage As Integer = 0)
    If Not Nothing Is pdfDoc Then
      If endPage = 0 Or endPage > pdfDoc.PageCount Then
        endPage = pdfDoc.PageCount
      End If
      Try
        If fileName.EndsWith(".ps") Then
          pdfDoc.PrintToFile(fileName, startPage, endPage)
        ElseIf fileName.EndsWith(".jpg") Then
          pdfDoc.ExportJpg(fileName, 70)
        ElseIf fileName.EndsWith(".txt") Then
          pdfDoc.ExportText(fileName, startPage, endPage, True, True)
        ElseIf fileName.EndsWith(".html") Then
          pdfDoc.ExportHtml(fileName, startPage, endPage, True, True, False)
        End If
      Catch ex As Exception
        MessageBox.Show(ex.ToString())
      End Try
    End If
  End Sub

  Public Shared Function FillTree(ByRef tvwOutline As TreeView, ByRef pdfDoc As PDFLibNet.PDFWrapper) As Boolean
    FillTree = False
    tvwOutline.Nodes.Clear()
    For Each ol As PDFLibNet.OutlineItem In pdfDoc.Outline
      FillTree = True
      Dim tn As New TreeNode(ol.Title)
      tn.Tag = ol
      tvwOutline.Nodes.Add(tn)
      If ol.KidsCount > 0 Then
        FillTreeRecursive(ol.Childrens, tn)
      End If
    Next
  End Function

  Public Shared Sub FillTreeRecursive(ByVal olParent As PDFLibNet.OutlineItemCollection(Of PDFLibNet.OutlineItem), ByVal treeNode As TreeNode)
    For Each ol As PDFLibNet.OutlineItem In olParent
      Dim tn As New TreeNode(ol.Title)
      tn.Tag = ol
      treeNode.Nodes.Add(tn)
      If ol.KidsCount > 0 Then
        FillTreeRecursive(ol.Childrens, tn)
      End If
    Next
  End Sub

  Public Shared Sub FillHTMLTreeRecursive(ByVal olParent As PDFLibNet.OutlineItemCollection(Of PDFLibNet.OutlineItem), ByVal htmlString As String)
    htmlString &= "<ul>"
    For Each ol As PDFLibNet.OutlineItem In olParent
      htmlString &= "<li><a href=""javascript:changeImage('images/page" & ol.Destination.Page & ".png')"">" & Web.HttpUtility.HtmlEncode(ol.Title) & "</a></li>"
      If ol.KidsCount > 0 Then
        FillHTMLTreeRecursive(ol.Childrens, htmlString)
      End If
    Next
    htmlString &= "</ul>"
  End Sub

End Class

Public Class PDFOutline

  Public Title As String
  Public Item As PDFLibNet.OutlineItem
  Friend _doc As PDFLibNet.PDFWrapper = Nothing
  Friend _children As List(Of PDFOutline)

  Public ReadOnly Property Children() As List(Of PDFOutline)
    Get
      Return _children
    End Get
  End Property

  Friend Sub New(ByVal title__1 As String, ByVal outlineItem As PDFLibNet.OutlineItem, ByVal doc As PDFLibNet.PDFWrapper)
    Title = title__1
    Item = outlineItem
    _doc = doc
  End Sub

End Class

Public Class SearchArgs
  Inherits EventArgs
  Public Text As String
  Public FromBegin As Boolean
  Public Exact As Boolean
  Public WholeDoc As Boolean
  Public FindNext As Boolean
  Public Up As Boolean

  Friend Sub New(ByVal text__1 As String, ByVal frombegin__2 As Boolean, ByVal exact__3 As Boolean, ByVal wholedoc__4 As Boolean, ByVal findnext__5 As Boolean, ByVal up__6 As Boolean)
    Text = text__1
    FromBegin = frombegin__2
    Exact = exact__3
    WholeDoc = wholedoc__4
    FindNext = findnext__5
    Up = up__6
  End Sub
End Class

