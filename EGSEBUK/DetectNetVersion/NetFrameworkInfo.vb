' -----------------------------------------------------------------------
' <copyright file="NetFrameworkInfo.vb" company="IKI RSSI, laboratory №711">
'     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
' </copyright>
' <author>Коробейщиков Иван</author>
' -----------------------------------------------------------------------

Imports System
Imports System.IO
Imports System.Security
Imports System.Text.RegularExpressions

Namespace DotNetFramework
    ''' <summary>
    ''' Предоставляет информацию по установленным версиям DotNet Framework.
    ''' </summary>
    ''' <remarks>
    ''' Пример использования:
    ''' Dim Info As New DotNetFramework.NetFrameworkInfo
    ''' Console.WriteLine(Info.HighestFrameworkVersion)
    ''' Console.WriteLine(Info.NetFrameworkInstallationPath)
    ''' Dim FrameworkVersionCollection As New Collection
    ''' FrameworkVersionCollection = Info.FrameworkVersions
    ''' For Each FrameworkVersion As String In FrameworkVersionCollection
    ''' Console.WriteLine(FrameworkVersion)
    ''' </remarks>
    Public Class NetFrameworkInfo
        ''' <summary>
        ''' Путь установки DotNet, по-умолчанию.
        ''' </summary>
        ''' <remarks></remarks>
        Private Const FrameworkPath As String = "\Microsoft.NET\Framework"
        ''' <summary>
        ''' Наименование для системного каталога.
        ''' </summary>
        ''' <remarks>см. также <c>WinDir2</c>.</remarks>
        Private Const WinDir1 As String = "windir"
        ''' <summary>
        ''' Наименование для системного каталога.
        ''' </summary>
        ''' <remarks>см. также <c>WinDir1</c>.</remarks>
        Private Const WinDir2 As String = "SystemRoot"
        Private _versions As New List(Of String)

        Public ReadOnly Property FrameworkVersions() As List(Of String)
            Get
                Try
                    GetVersions(NetFrameworkInstallationPath)
                Catch e1 As SecurityException
                    Return Nothing
                End Try
                Return _versions
            End Get
        End Property

        Public ReadOnly Property HighestFrameworkVersion() As String
            Get
                Try
                    Return GetHighestVersion(NetFrameworkInstallationPath)
                Catch e1 As SecurityException
                    Return "Unknown"
                End Try
            End Get
        End Property

        Private Sub GetVersions(ByVal installationPath As String)

            Dim versions() As String = Directory.GetDirectories(installationPath, "v*")
            Dim VersionNumber As String
            _versions.Clear()

            For i As Integer = versions.Length - 1 To 0 Step -1
                VersionNumber = ExtractVersion(versions(i))

                If IsFrameworkVersionFormat(VersionNumber) Then
                    _versions.Add(VersionNumber)
                End If
            Next i

        End Sub

        Private Function GetHighestVersion(ByVal installationPath As String) As String
            Dim versions() As String = Directory.GetDirectories(installationPath, "v*")
            Dim version As String = "Unknown"

            For i As Integer = versions.Length - 1 To 0 Step -1
                version = ExtractVersion(versions(i))

                If IsFrameworkVersionFormat(version) Then
                    Return version
                End If
            Next i

            Return version
        End Function

        Private Function ExtractVersion(ByVal directory As String) As String
            Dim startIndex As Integer = directory.LastIndexOf("\") + 2
            Return directory.Substring(startIndex, directory.Length - startIndex)
        End Function

        Private Shared Function IsFrameworkVersionFormat(ByVal str As String) As Boolean
            Return New Regex("^[0-9]+\.?[0-9]+\.?[0-9]*$").IsMatch(str)
        End Function

        Public ReadOnly Property NetFrameworkInstallationPath() As String
            Get
                Return WindowsPath & FrameworkPath
            End Get
        End Property
        ''' <summary>
        ''' Получает путь к системному каталогу.
        ''' </summary>
        ''' <value>Строка, представляющая путь к системному каталогу.</value>
        ''' <returns>Путь к системному каталогу.</returns>
        ''' <remarks>см. также <c>WinDir1</c> и <c>WinDir2</c></remarks>
        Public ReadOnly Property WindowsPath() As String
            Get
                Dim winDir As String = Environment.GetEnvironmentVariable(WinDir1)
                If String.IsNullOrEmpty(winDir) Then
                    winDir = Environment.GetEnvironmentVariable(WinDir2)
                End If

                Return winDir
            End Get
        End Property
    End Class
End Namespace
