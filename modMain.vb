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

    Public Const PROGRAM_DATE As String = "January 5, 2015"

    Private mMaxCharsPerColumn As Integer
	Private mInputFilePath As String

	Private mIncludeOrganismAndPhylogeny As Boolean
	Private mIncludeProteinSequence As Boolean
	Private mWriteFastaFile As Boolean

	Private mFastaSpeciesFilter As String
	Private mFastaSpeciesFilterRegEx As String
    Private mOrganismFilterFilePath As String

	Public Function Main() As Integer
		Dim intReturnCode As Integer
		Dim objParseIPIDATFile As clsParseIPIDATFile
		Dim objParseCommandLine As New clsParseCommandLine

		Dim blnProceed As Boolean
		Dim blnSuccess As Boolean

		Try
			' Set the default values
			mMaxCharsPerColumn = 0
			mInputFilePath = String.Empty
			mIncludeOrganismAndPhylogeny = False
			mIncludeProteinSequence = False
			mWriteFastaFile = False

			mFastaSpeciesFilter = String.Empty
            mFastaSpeciesFilterRegEx = String.Empty
            mOrganismFilterFilePath = String.Empty

			blnProceed = False
			If objParseCommandLine.ParseCommandLine Then
				If SetOptionsUsingCommandLineParameters(objParseCommandLine) Then blnProceed = True
			End If

			If Not blnProceed OrElse _
			   objParseCommandLine.NeedToShowHelp OrElse _
			   objParseCommandLine.ParameterCount + objParseCommandLine.NonSwitchParameterCount = 0 OrElse _
			   mInputFilePath.Length = 0 Then
				ShowProgramHelp()
				intReturnCode = -1
			Else
				objParseIPIDATFile = New clsParseIPIDATFile

				With objParseIPIDATFile
					.IncludeOrganismAndPhylogeny = mIncludeOrganismAndPhylogeny
					.IncludeProteinSequence = mIncludeProteinSequence
					.WriteFastaFile = mWriteFastaFile

					.FastaSpeciesFilter = mFastaSpeciesFilter
					.FastaSpeciesFilterRegEx = mFastaSpeciesFilterRegEx

                    .OrganismFilterFilePath = mOrganismFilterFilePath

					''If Not mParameterFilePath Is Nothing AndAlso mParameterFilePath.Length > 0 Then
					''    .LoadParameterFileSettings(mParameterFilePath)
					''End If
				End With

				blnSuccess = objParseIPIDATFile.ParseIPIDATFile(mInputFilePath, mMaxCharsPerColumn)

				If blnSuccess Then
					intReturnCode = 0
				Else
					intReturnCode = -1
				End If

			End If

		Catch ex As System.Exception
			ShowErrorMessage("Error occurred in modMain->Main: " & System.Environment.NewLine & ex.Message)
			intReturnCode = -1
		End Try

		Return intReturnCode

	End Function

	Private Function GetAppVersion() As String
		Return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() & " (" & PROGRAM_DATE & ")"
	End Function

	Private Function SetOptionsUsingCommandLineParameters(ByVal objParseCommandLine As clsParseCommandLine) As Boolean
		' Returns True if no problems; otherwise, returns false

		Dim strValue As String = String.Empty
        Dim strValidParameters() As String = New String() {"I", "M", "S", "O", "F", "Q", "Species", "SpeciesRegEx", "OrgFile"}

		Try
			' Make sure no invalid parameters are present
			If objParseCommandLine.InvalidParametersPresent(strValidParameters) Then
				Return False
			Else

				' Query objParseCommandLine to see if various parameters are present
				With objParseCommandLine
					' Query objParseCommandLine to see if various parameters are present
					If .RetrieveValueForParameter("I", strValue) Then
						mInputFilePath = strValue
					ElseIf .NonSwitchParameterCount > 0 Then
						' Treat the first non-switch parameter as the input file
						mInputFilePath = .RetrieveNonSwitchParameter(0)
					End If

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

					If .RetrieveValueForParameter("Species", strValue) Then mFastaSpeciesFilter = strValue
                    If .RetrieveValueForParameter("SpeciesRegEx", strValue) Then mFastaSpeciesFilterRegEx = strValue

                    If .RetrieveValueForParameter("OrgFile", strValue) Then mOrganismFilterFilePath = strValue

				End With

				Return True
			End If

		Catch ex As Exception
			ShowErrorMessage("Error parsing the command line parameters: " & System.Environment.NewLine & ex.Message)
		End Try

		Return False

	End Function

	Private Sub ShowErrorMessage(ByVal strMessage As String)
		Dim strSeparator As String = "------------------------------------------------------------------------------"

		Console.WriteLine()
		Console.WriteLine(strSeparator)
		Console.WriteLine(strMessage)
		Console.WriteLine(strSeparator)
		Console.WriteLine()

	End Sub

	Private Sub ShowProgramHelp()

		Try

			Console.WriteLine("This program will read a Uniprot (IPI) .DAT file with protein information, then parse out the accession names and save them in a tab-delimited file. See http://www.ebi.ac.uk/IPI/FAQs.html for a list of frequently asked questions concerning Uniprot files.")
			Console.WriteLine()

			Console.WriteLine("Program syntax:" & ControlChars.NewLine & System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location) & _
			   " InputFileName.dat [/M:MaximumCharsPerColumn] [/S] [/O]")
            Console.WriteLine("  [/F] [/Species:FilterText] [/SpeciesRegEx:""RegEx""]")
            Console.WriteLine("  [/OrgFile:FilePath]")
			Console.WriteLine()

			Console.WriteLine("The input file name is required. If the filename contains spaces, then surround it with double quotes. ")
            Console.WriteLine("Use /M to specify the maximum number of characters to retain for each column, useful to limit the line length for each protein in the output file")
			Console.WriteLine()

            Console.WriteLine("Use /S to include the protein sequence in the output file (ignored if using /F)")
            Console.WriteLine("Use /O to include the organism name and phylogeny information (ignored if using /F)")
			Console.WriteLine()

            Console.WriteLine("Use /F to specify that a .Fasta file be created for the proteins")
            Console.WriteLine()

			Console.WriteLine("Use /Species:FilterText to only write entries to the fasta file if the Species tag contains FilterText")
            Console.WriteLine("Use /SpeciesRegEx:""RegEx"" to only write entries to the fasta file if the Species tag matches the specified regular expression")
            Console.WriteLine()
            Console.WriteLine("Use /OrgFile:FilePath to specify the path to a file containing organism names to filter on (one name per line).  The organism names must be exact matches to the organism names listed in the _OrganismSummary.txt file that is created by this program")
            Console.WriteLine()

			Console.WriteLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2007")
			Console.WriteLine("Version: " & GetAppVersion())
			Console.WriteLine()

			Console.WriteLine("E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com")
			Console.WriteLine("Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov")
			Console.WriteLine()

			' Delay for 750 msec in case the user double clicked this file from within Windows Explorer (or started the program via a shortcut)
			System.Threading.Thread.Sleep(750)

		Catch ex As Exception
			ShowErrorMessage("Error displaying the program syntax: " & ex.Message)
		End Try

	End Sub

End Module
