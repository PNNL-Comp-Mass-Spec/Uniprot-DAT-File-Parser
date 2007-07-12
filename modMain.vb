Option Strict On

' This program uses clsParseIPIDATFile to read an IPI DAT file and create a 
' tab-delimited text file with the information split into multiple fields
'
' -------------------------------------------------------------------------------
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
' Program started February 5, 2007
' Copyright 2007, Battelle Memorial Institute.  All Rights Reserved.

' E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com
' Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/
' -------------------------------------------------------------------------------
' 
' Licensed under the Apache License, Version 2.0; you may not use this file except
' in compliance with the License.  You may obtain a copy of the License at 
' http://www.apache.org/licenses/LICENSE-2.0
'
' Notice: This computer software was prepared by Battelle Memorial Institute, 
' hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
' Department of Energy (DOE).  All rights in the computer software are reserved 
' by DOE on behalf of the United States Government and the Contractor as 
' provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
' WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
' SOFTWARE.  This notice including this sentence must appear on any copies of 
' this computer software.

Module modMain

    Public Const PROGRAM_DATE As String = "February 20, 2007"

    Private mMaxCharsPerColumn As Integer
    Private mInputDataFilePath As String
    Private mQuietMode As Boolean

    Public Sub Main()
        Dim intReturnCode As Integer
        Dim objParseIPIDATFile As clsParseIPIDATFile
        Dim objParseCommandLine As New clsParseCommandLine

        Dim blnProceed As Boolean
        Dim blnSuccess As Boolean

        Try
            ' Set the default values
            mMaxCharsPerColumn = 0

            blnProceed = False
            If objParseCommandLine.ParseCommandLine Then
                If SetOptionsUsingCommandLineParameters(objParseCommandLine) Then blnProceed = True
            End If

            If Not blnProceed OrElse _
               objParseCommandLine.NeedToShowHelp OrElse _
               mInputDataFilePath.Length = 0 Then
                ShowProgramHelp()
                intReturnCode = -1
            Else
                objParseIPIDATFile = New clsParseIPIDATFile

                With objParseIPIDATFile
                    .ShowMessages = Not mQuietMode

                    ''If Not mParameterFilePath Is Nothing AndAlso mParameterFilePath.Length > 0 Then
                    ''    .LoadParameterFileSettings(mParameterFilePath)
                    ''End If
                End With

                blnSuccess = objParseIPIDATFile.ParseIPIDATFile(mInputDataFilePath, mMaxCharsPerColumn)

                If blnSuccess Then
                    intReturnCode = 0
                Else
                    intReturnCode = -1
                End If

            End If

        Catch ex As System.Exception
            If mQuietMode Then
                Throw ex
            Else
                MsgBox("Error occurred: " & ControlChars.NewLine & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, "Error")
            End If
            intReturnCode = -1
        End Try

    End Sub

    Private Function SetOptionsUsingCommandLineParameters(ByVal objParseCommandLine As clsParseCommandLine) As Boolean
        ' Returns True if no problems; otherwise, returns false

        Dim strValue As String
        Dim strValidParameters() As String = New String() {"I", "M", "Q"}

        Try
            ' Make sure no invalid parameters are present
            If objParseCommandLine.InvalidParametersPresent(strValidParameters) Then
                Return False
            Else

                ' Query objParseCommandLine to see if various parameters are present
                With objParseCommandLine
                    If .RetrieveValueForParameter("I", strValue) Then
                        mInputDataFilePath = strValue

                        ' Uncomment this to allow an ouptut file name
                        ' If .RetrieveValueForParameter("O", strValue) Then mOutputFileOrFolderPath = strValue
                    Else
                        ' User didn't use /I:InputFile
                        ' See if they simply provided the file name
                        If .NonSwitchParameterCount > 0 Then
                            mInputDataFilePath = .RetrieveNonSwitchParameter(0)
                        End If
                    End If

                    If .RetrieveValueForParameter("M", strValue) Then
                        Try
                            mMaxCharsPerColumn = CInt(strValue)
                        Catch ex As Exception
                            ' Ignore errors here
                        End Try
                    End If
                    If .RetrieveValueForParameter("Q", strValue) Then mQuietMode = True
                End With

                Return True
            End If

        Catch ex As System.Exception
            If mQuietMode Then
                Throw New System.Exception("Error parsing the command line parameters", ex)
            Else
                MsgBox("Error parsing the command line parameters: " & ControlChars.NewLine & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, "Error")
            End If
        End Try

    End Function

    Private Sub ShowProgramHelp()

        Dim strSyntax As String
        Dim ioPath As System.IO.Path

        Try

            strSyntax = "This program will read an IPI .DAT file with protein information, then parse out the accession names and save them in a tab-delimited file." & ControlChars.NewLine & ControlChars.NewLine
            strSyntax &= "Program syntax:" & ControlChars.NewLine & ioPath.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            strSyntax &= " InputFileName.dat [/M:MaximumCharsPerColumn]" & ControlChars.NewLine & ControlChars.NewLine

            strSyntax &= "The input file name is required. If the filename contains spaces, then surround it with double quotes. " & _
                         "Use /M to specify the maximum number of characters to retain for each column, useful to limit the line length for each protein in the output file." & ControlChars.NewLine & ControlChars.NewLine

            strSyntax &= "Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2007" & ControlChars.NewLine
            strSyntax &= "Copyright 2007, Battelle Memorial Institute.  All Rights Reserved." & ControlChars.NewLine & ControlChars.NewLine

            strSyntax &= "E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com" & ControlChars.NewLine
            strSyntax &= "Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/"

            Console.WriteLine(strSyntax)

        Catch ex As System.Exception
            Console.WriteLine("Error displaying the program syntax: " & ex.Message)
        End Try

    End Sub


    ''Private Function ParseCommandLine(Optional ByVal strSwitchStartChar As Char = "/"c) As String
    ''    ' Parses the command line, returning the filename specified (which is the argument that doesn't start with strSwitchStartChar)e
    ''    Dim strCmdLine As String

    ''    Dim strKey As String, strValue As String
    ''    Dim intCharLoc As Integer

    ''    Dim intIndex As Integer
    ''    Dim strParameters() As String

    ''    Try
    ''        Try
    ''            ' This command will fail if the program is called from a network share
    ''            strCmdLine = System.Environment.CommandLine()
    ''            strParameters = System.Environment.GetCommandLineArgs()
    ''        Catch ex As System.Exception
    ''            Windows.Forms.MessageBox.Show("This program cannot be run from a network share.  Please map a drive to the network share you are currently accessing or copy the program files and required DLL's to your local computer.", "Error", Windows.Forms.MessageBoxButtons.OK, Windows.Forms.MessageBoxIcon.Exclamation)
    ''        End Try

    ''        If strCmdLine Is Nothing OrElse strCmdLine.Length = 0 Then
    ''            Return String.Empty
    ''        ElseIf strCmdLine.IndexOf(strSwitchStartChar & "?") > 0 Or strCmdLine.ToLower.IndexOf(strSwitchStartChar & "help") > 0 Then
    ''            Return String.Empty
    ''        End If

    ''        ' Note that strParameters(0) is the path to the Executable for the calling program

    ''        If strParameters.Length > 1 Then
    ''            Return strParameters(1)
    ''        Else
    ''            Return String.Empty
    ''        End If

    ''    Catch ex As System.Exception
    ''        Throw New System.Exception("Error in ParseCommandLine", ex)
    ''    End Try

    ''End Function


End Module
