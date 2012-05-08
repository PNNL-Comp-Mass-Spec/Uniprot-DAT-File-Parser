Option Strict On

' This program uses clsParseIPIDATFile to read a Uniprot (IPI) DAT file and create a 
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

	Public Const PROGRAM_DATE As String = "May 8, 2012"

    Private mMaxCharsPerColumn As Integer
    Private mInputDataFilePath As String

    Private mIncludeOrganismAndPhylogeny As Boolean
    Private mIncludeProteinSequence As Boolean
    Private mWriteFastaFile As Boolean

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
            mInputDataFilePath = String.Empty
            mIncludeOrganismAndPhylogeny = False
            mIncludeProteinSequence = False
            mWriteFastaFile = False

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
                    .IncludeOrganismAndPhylogeny = mIncludeOrganismAndPhylogeny
                    .IncludeProteinSequence = mIncludeProteinSequence
                    .WriteFastaFile = mWriteFastaFile

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
                Console.WriteLine("Error occurred in modMain->Main: " & ControlChars.NewLine & ex.Message)
            End If
            intReturnCode = -1
        End Try

    End Sub

    Private Function SetOptionsUsingCommandLineParameters(ByVal objParseCommandLine As clsParseCommandLine) As Boolean
        ' Returns True if no problems; otherwise, returns false

        Dim strValue As String = String.Empty
        Dim strValidParameters() As String = New String() {"I", "M", "S", "O", "F", "Q"}

        Try
            ' Make sure no invalid parameters are present
            If objParseCommandLine.InvalidParametersPresent(strValidParameters) Then
                Return False
            Else

                ' Query objParseCommandLine to see if various parameters are present
                With objParseCommandLine

                    If .NonSwitchParameterCount > 0 Then
                        ' Treat the first non-switch parameter as the input file
                        mInputDataFilePath = .RetrieveNonSwitchParameter(0)
                    End If

                    If .RetrieveValueForParameter("I", strValue) Then mInputDataFilePath = strValue

                    If .RetrieveValueForParameter("M", strValue) Then
                        Try
                            mMaxCharsPerColumn = CInt(strValue)
                        Catch ex As Exception
                            ' Ignore errors here
                        End Try
                    End If

                    If .RetrieveValueForParameter("O", strValue) Then mIncludeOrganismAndPhylogeny = True
                    If .RetrieveValueForParameter("S", strValue) Then mIncludeProteinSequence = True
                    If .RetrieveValueForParameter("F", strValue) Then mWriteFastaFile = True

                    If .RetrieveValueForParameter("Q", strValue) Then mQuietMode = True
                End With

                Return True
            End If

        Catch ex As System.Exception
            If mQuietMode Then
                Throw New System.Exception("Error parsing the command line parameters", ex)
            Else
                Console.WriteLine("Error parsing the command line parameters: " & ControlChars.NewLine & ex.Message)
            End If
        End Try

    End Function

    Private Sub ShowProgramHelp()

        Try

            Console.WriteLine("This program will read a Uniprot (IPI) .DAT file with protein information, then parse out the accession names and save them in a tab-delimited file. See http://www.ebi.ac.uk/IPI/FAQs.html for a list of frequently asked questions concerning Uniprot files.")
            Console.WriteLine()

            Console.WriteLine("Program syntax:" & ControlChars.NewLine & System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location) & _
                              " InputFileName.dat [/M:MaximumCharsPerColumn] /S /O /F")
            Console.WriteLine()

            Console.WriteLine("The input file name is required. If the filename contains spaces, then surround it with double quotes. ")
            Console.WriteLine("Use /M to specify the maximum number of characters to retain for each column, useful to limit the line length for each protein in the output file.")
            Console.WriteLine()

            Console.WriteLine("Use /S to include the protein sequence in the output file.  Use /O to include the organism name and phylogeny information.")
            Console.WriteLine()

            Console.WriteLine("Use /F to specify that a .Fasta file be created for the proteins.")
            Console.WriteLine()

            Console.WriteLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2007")
            Console.WriteLine()

            Console.WriteLine("E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com")
            Console.WriteLine("Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/")
            Console.WriteLine()

            ' Delay for 750 msec in case the user double clicked this file from within Windows Explorer (or started the program via a shortcut)
            System.Threading.Thread.Sleep(750)

        Catch ex As System.Exception
            Console.WriteLine("Error displaying the program syntax: " & ex.Message)
        End Try

    End Sub

End Module
