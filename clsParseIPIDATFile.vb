Option Strict On

' This class reads an IPI DAT file and creates a tab-delimited text file with 
' the information split into multiple fields
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

Imports System.Collections.Generic

Public Class clsParseIPIDATFile

#Region "Constants and Enums"

	Private Const DATA_SOURCE_ENSEMBL As String = "Ensembl"
	Private Const DATA_SOURCE_ENSEMBL_Transcript As String = "Ensembl_Transcript"
	Private Const DATA_SOURCE_ENSEMBL_Protein As String = "Ensembl_Protein"
	Private Const DATA_SOURCE_ENSEMBL_Gene As String = "Ensembl_Gene"

	Private Const DATA_SOURCE_ENTREZ As String = "Entrez Gene"
	Private Const DATA_SOURCE_REFSEQ As String = "RefSeq"
	Private Const DATA_SOURCE_SWISSPROT As String = "UniProtKB/Swiss-Prot"
	Private Const DATA_SOURCE_TREMBL As String = "UniProtKB/TrEMBL"

	Private Const SKIP_COLUMN_FLAG As String = "<<SKIP_COL>>"

	Private Enum eTargetColumn
		ProteinName = 0				' Protein name
		Accession = 1				' Full list of accession numbers
		Description = 2
		REFSEQ_XP_or_NP = 3
		GI = 4
		TREMBL = 5
		ENSEMBL_Transcript = 6
		ENSEMBL_Protein = 7
		ENSEMBL_Gene = 8
		EntrezGeneID = 9
		EntrezGeneName = 10
		SwissProt = 11
		SwissProtName = 12
		Chromosome = 13
		AACount = 14
		MW = 15
		Organism = 16					' Species information
		Phylogeny = 17
		Sequence = 18
		Accession1 = 19					' First accession number
		Accession2 = 20					' Second accession number (if defined)
		Accession3 = 21					' Third accession number (if defined)
		Accession4 = 22					' Fourth accession number (if defined)
		AddnlColumnStart = 23
	End Enum

#End Region

#Region "Structures"

#End Region

#Region "Classwide Variables"
	Protected mFastaSpeciesRegex As Text.RegularExpressions.Regex
	Protected mFastaSpeciesRegexText As String = String.Empty
#End Region

#Region "Properties"
	Public Property IncludeOrganismAndPhylogeny() As Boolean
	Public Property IncludeProteinSequence() As Boolean
	Public Property WriteFastaFile() As Boolean			 ' If this is true, then IncludeOrganismAndPhylogeny and IncludeProteinSequence are auto-set to true
	Public Property FastaSpeciesFilter() As String
	Public Property FastaSpeciesFilterRegEx() As String

#End Region

	Private Sub AppendToText(ByRef strText As String, ByVal strNewText As String, ByVal strSeparator As String)
		If strNewText Is Nothing Then
			strNewText = String.Empty
		End If

		If String.IsNullOrEmpty(strText) Then
			strText = String.Copy(strNewText)
		Else
			strText &= strSeparator & strNewText
		End If
	End Sub

	Private Sub ClearArray(ByRef strArray() As String)
		For intIndex As Integer = 0 To strArray.Length - 1
			strArray(intIndex) = String.Empty
		Next
	End Sub

	Private Function FlattenArray(ByRef strArray() As String) As String
		Return FlattenArray(strArray, ControlChars.Tab)
	End Function

	Private Function FlattenArray(ByRef strArray() As String, ByVal chSepChar As Char) As String
		If strArray Is Nothing Then
			Return String.Empty
		Else
			Return FlattenArray(strArray, strArray.Length, chSepChar)
		End If
	End Function

	Private Function FlattenArray(ByRef strArray() As String, ByVal intDataCount As Integer, ByVal chSepChar As Char) As String
		Dim intIndex As Integer
		Dim strResult As String

		If strArray Is Nothing Then
			Return String.Empty
		ElseIf strArray.Length = 0 OrElse intDataCount <= 0 Then
			Return String.Empty
		Else
			If intDataCount > strArray.Length Then
				intDataCount = strArray.Length
			End If

			strResult = strArray(0)
			If strResult Is Nothing Then strResult = String.Empty

			For intIndex = 1 To intDataCount - 1
				If strArray(intIndex) Is Nothing Then
					strResult &= chSepChar
				ElseIf strArray(intIndex) <> SKIP_COLUMN_FLAG Then
					strResult &= chSepChar & strArray(intIndex)
				End If
			Next intIndex
			Return strResult
		End If
	End Function

	Public Function ParseIPIDATFile(ByVal strInputFileName As String, ByVal intMaxCharsPerColumnToWrite As Integer) As Boolean

		Dim ioSourceFile As IO.FileInfo
		Dim strOutputFileName As String = String.Empty
		Dim strColumnSummaryFilename As String = String.Empty

		Dim strLineIn As String

		Dim blnDataPresent As Boolean
		Dim strData() As String
		Dim intMaxCharsWritten() As Integer

		Dim strKey As String = String.Empty, strItem As String = String.Empty
		Dim strSubKey As String = String.Empty, strSubItem As String = String.Empty
		Dim strSubKey2 As String = String.Empty, strSubItem2 As String = String.Empty
		Dim strSubKey3 As String = String.Empty, strSubItem3 As String = String.Empty
		Dim strSubKey4 As String = String.Empty, strSubItem4 As String = String.Empty

		Dim strEnsembleNames() As String

		Dim lstHeaderColumns As List(Of String)

		Dim lstAddnlColumns As SortedSet(Of String)
		Dim dctAddnlColumnNameAndIndex As Dictionary(Of String, Integer)

		Dim intSpaceIndex As Integer
		Dim intCharIndex As Integer

		Dim intRowsProcessed As Integer
		Dim intEntryCount As Integer

		Dim srInFile As IO.StreamReader
		Dim swOutFile As IO.StreamWriter

		Dim sbSequence As Text.StringBuilder = New Text.StringBuilder
		Dim blnReadingSequence As Boolean = False

		Try
			ioSourceFile = New IO.FileInfo(strInputFileName)
			If Not ioSourceFile.Exists Then
				Console.WriteLine("File not found: " & strInputFileName)
				Return False
			End If

			ReportProgress("Reading " & ioSourceFile.Name)

			lstAddnlColumns = New SortedSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
			dctAddnlColumnNameAndIndex = New Dictionary(Of String, Integer)(StringComparer.CurrentCultureIgnoreCase)

			If Me.WriteFastaFile Then
				Me.IncludeOrganismAndPhylogeny = True
				Me.IncludeProteinSequence = True
			Else
				' Prescan the file
				PrescanFileForAddnlColumns(ioSourceFile.FullName, lstAddnlColumns, intEntryCount)
			End If

			If lstAddnlColumns.Count > 0 Then
				' Remove columns from lstAddnlColumns() that match the standard columns typically searched for below
				For Each strAddnlColumn As String In lstAddnlColumns
					If strAddnlColumn.ToUpper.StartsWith(DATA_SOURCE_REFSEQ.ToUpper) Then
						lstAddnlColumns.Remove(strAddnlColumn)
						Exit For
					End If
				Next

				If lstAddnlColumns.Contains(DATA_SOURCE_ENSEMBL) Then lstAddnlColumns.Remove(DATA_SOURCE_ENSEMBL)
				If lstAddnlColumns.Contains(DATA_SOURCE_ENTREZ) Then lstAddnlColumns.Remove(DATA_SOURCE_ENTREZ)
				If lstAddnlColumns.Contains(DATA_SOURCE_SWISSPROT) Then lstAddnlColumns.Remove(DATA_SOURCE_SWISSPROT)
				If lstAddnlColumns.Contains(DATA_SOURCE_TREMBL) Then lstAddnlColumns.Remove(DATA_SOURCE_TREMBL)

			End If

		Catch ex As Exception
			Console.WriteLine("Error in ParseIPIDataFile (Prescan file): " & ex.Message)
			Return False
		End Try

		Try
			srInFile = New IO.StreamReader(New IO.FileStream(ioSourceFile.FullName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite))
		Catch ex As Exception
			Console.WriteLine("Error opening the input file (" & strInputFileName & ") in ParseIPIDataFile: " & ex.Message)
			Return False
		End Try

		Try
			strOutputFileName = IO.Path.GetFileNameWithoutExtension(ioSourceFile.Name) & "_output"
			If Me.WriteFastaFile Then
				strOutputFileName &= ".fasta"
			Else
				strOutputFileName &= ".txt"
			End If

			swOutFile = New IO.StreamWriter(New IO.FileStream(IO.Path.Combine(ioSourceFile.DirectoryName, strOutputFileName), IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read))
		Catch ex As Exception
			Console.WriteLine("Error opening the output file (" & strOutputFileName & ") in ParseIPIDataFile: " & ex.Message)
			Return False
		End Try

		Try
			If Me.WriteFastaFile Then
				' Writing out a fasta file; no header to write
				lstHeaderColumns = New List(Of String)
			Else
				' Writing a tab-delimited file
				lstHeaderColumns = WriteDelimitedFileHeader(swOutFile, lstAddnlColumns, dctAddnlColumnNameAndIndex)
			End If

			ReDim strData(eTargetColumn.AddnlColumnStart + lstAddnlColumns.Count - 1)
			ReDim intMaxCharsWritten(eTargetColumn.AddnlColumnStart + lstAddnlColumns.Count - 1)

			ClearArray(strData)

			intRowsProcessed = 0
			blnDataPresent = False
			Do While srInFile.Peek > -1
				strLineIn = srInFile.ReadLine

				If Not String.IsNullOrEmpty(strLineIn) Then
					strLineIn = strLineIn.TrimEnd

					If strLineIn = "//" Then
						' Write out the previous row
						WriteCachedData(strData, sbSequence, blnDataPresent, swOutFile, intMaxCharsWritten, intMaxCharsPerColumnToWrite, strData(eTargetColumn.Organism))
						blnReadingSequence = False

						intRowsProcessed += 1

						If intRowsProcessed Mod 1000 = 0 Then
							If intEntryCount = 0 Then
								ReportProgress("Working: " & intRowsProcessed & " entries processed")
							Else
								ReportProgress("Working: " & intRowsProcessed & " / " & intEntryCount & " entries processed")
							End If
						End If

					ElseIf blnReadingSequence Then
						If Me.IncludeProteinSequence Then
							sbSequence.Append(strLineIn.Trim.Replace(" "c, String.Empty))
						End If
					ElseIf strLineIn.Length > 2 AndAlso Char.IsUpper(strLineIn.Chars(0)) AndAlso Char.IsUpper(strLineIn.Chars(1)) Then

						If SplitLine(strLineIn, strKey, strItem) Then
							Select Case strKey.Substring(0, 2).ToUpper()
								Case "ID"
									intSpaceIndex = strItem.IndexOf(" "c)
									If intSpaceIndex >= 3 Then
										strItem = strItem.Substring(0, intSpaceIndex)
									End If
									strData(eTargetColumn.ProteinName) = String.Copy(strItem)
									blnDataPresent = True
								Case "AC"
									strData(eTargetColumn.Accession) = String.Copy(strItem)
									If SplitLine(strItem, strSubKey, strSubItem, ";") Then
										strData(eTargetColumn.Accession1) = String.Copy(strSubKey)
										If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
											strData(eTargetColumn.Accession2) = String.Copy(strSubKey2)
											If SplitLine(strSubItem2, strSubKey3, strSubItem3, ";") Then
												strData(eTargetColumn.Accession3) = String.Copy(strSubKey3)
												If SplitLine(strSubItem3, strSubKey4, strSubItem4, ";") Then
													strData(eTargetColumn.Accession4) = String.Copy(strSubKey4)
												Else
													strData(eTargetColumn.Accession4) = String.Copy(strSubItem3)
												End If
											Else
												strData(eTargetColumn.Accession3) = String.Copy(strSubItem2)
											End If
										Else
											strData(eTargetColumn.Accession2) = String.Copy(strSubItem)
										End If
									Else
										strData(eTargetColumn.Accession1) = String.Copy(strItem)
									End If
									blnDataPresent = True

								Case "DE"
									' Alternate Names, aka AltName
									AppendToText(strData(eTargetColumn.Description), strItem, " ")
									blnDataPresent = True

								Case "DR"
									If SplitLine(strItem, strSubKey, strSubItem, ";") Then
										If strSubKey.ToUpper().StartsWith(DATA_SOURCE_REFSEQ.ToUpper()) Then
											If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
												AppendToText(strData(eTargetColumn.REFSEQ_XP_or_NP), strSubKey2, "; ")

												intCharIndex = strSubItem2.IndexOf(";")
												If intCharIndex >= 0 Then
													strSubItem2 = strSubItem2.Substring(0, intCharIndex)
												End If

												strSubItem2 = Replace(strSubItem2, "GI:", "")
												strSubItem2 = TrimCommonEndChars(strSubItem2)

												AppendToText(strData(eTargetColumn.GI), strSubItem2, "; ")
											Else
												AppendToText(strData(eTargetColumn.REFSEQ_XP_or_NP), strSubItem, "; ")
											End If
											blnDataPresent = True
										Else

											Select Case strSubKey.ToUpper()
												Case DATA_SOURCE_TREMBL.ToUpper()

													strSubItem = TrimCommonEndChars(strSubItem)

													If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
														AppendToText(strData(eTargetColumn.TREMBL), strSubKey2, "; ")
													Else
														AppendToText(strData(eTargetColumn.TREMBL), strSubItem, "; ")
													End If
													blnDataPresent = True

												Case DATA_SOURCE_ENSEMBL.ToUpper()
													strSubItem = TrimCommonEndChars(strSubItem)

													' Split strSubItem on a semicolon and parse each of the entries
													strEnsembleNames = strSubItem.Split(";"c)

													For Each strEnsembleName As String In strEnsembleNames
														strEnsembleName = strEnsembleName.Trim()
														If strEnsembleName.StartsWith("ENST") Then
															AppendToText(strData(eTargetColumn.ENSEMBL_Transcript), strEnsembleName, "; ")
														ElseIf strEnsembleName.StartsWith("ENSP") Then
															AppendToText(strData(eTargetColumn.ENSEMBL_Protein), strEnsembleName, "; ")
														ElseIf strEnsembleName.StartsWith("ENSG") Then
															AppendToText(strData(eTargetColumn.ENSEMBL_Gene), strEnsembleName, "; ")
														Else
															AppendToText(strData(eTargetColumn.ENSEMBL_Protein), strEnsembleName, "; ")
														End If
													Next

													blnDataPresent = True

												Case DATA_SOURCE_ENTREZ.ToUpper()
													If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
														AppendToText(strData(eTargetColumn.EntrezGeneID), strSubKey2, "; ")

														intCharIndex = strSubItem2.IndexOf(";")
														If intCharIndex >= 0 Then
															strSubItem2 = strSubItem2.Substring(0, intCharIndex)
														End If
														AppendToText(strData(eTargetColumn.EntrezGeneName), strSubItem2, "; ")
													Else
														AppendToText(strData(eTargetColumn.EntrezGeneID), strSubItem, "; ")
													End If
													blnDataPresent = True

												Case DATA_SOURCE_SWISSPROT.ToUpper()
													If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
														AppendToText(strData(eTargetColumn.SwissProt), strSubKey2, "; ")

														intCharIndex = strSubItem2.IndexOf(";")
														If intCharIndex >= 0 Then
															strSubItem2 = strSubItem2.Substring(0, intCharIndex)
														End If
														AppendToText(strData(eTargetColumn.SwissProtName), strSubItem2, "; ")

													Else
														AppendToText(strData(eTargetColumn.SwissProt), strSubItem, "; ")
													End If
													blnDataPresent = True

												Case Else
													' See if strSubKey matches any of the entries in lstAddnlColumns()
													' dctAddnlColumnNameAndIndex was instantiated with a case-insensitive comparer
													Dim intTargetDataIndex As Integer
													If dctAddnlColumnNameAndIndex.TryGetValue(strSubKey, intTargetDataIndex) Then

														strSubItem = TrimCommonEndChars(strSubItem)

														AppendToText(strData(intTargetDataIndex), strSubItem, "; ")
														blnDataPresent = True
													End If

											End Select
										End If
									End If

								Case "CC"
									If SplitLine(strItem, strSubKey, strSubItem, ":") Then
										If strSubKey = "-!- CHROMOSOME" Then
											strSubItem = strSubItem.TrimEnd("."c)
											strData(eTargetColumn.Chromosome) = String.Copy(strSubItem)
											blnDataPresent = True
										End If
									End If
								Case "SQ"
									If SplitLine(strItem, strSubKey, strSubItem, " ") Then
										If strSubKey = "SEQUENCE" Then
											If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
												strSubKey2 = strSubKey2.Replace("AA", String.Empty).Trim
												strData(eTargetColumn.AACount) = String.Copy(strSubKey2)

												If SplitLine(strSubItem2, strSubKey2, strSubItem2, ";") Then
													strSubKey2 = strSubKey2.Replace("MW", String.Empty).Trim
													strData(eTargetColumn.MW) = String.Copy(strSubKey2)
												End If

											Else
												AppendToText(strData(eTargetColumn.AACount), strSubItem, "; ")
											End If
											blnDataPresent = True
										End If
									End If

									sbSequence.Length = 0
									blnReadingSequence = True

								Case "OS"
									If Me.IncludeOrganismAndPhylogeny Then
										' Organism lines tend to end in a period; remove it if found
										strItem = TrimEnd(strItem, ".")

										AppendToText(strData(eTargetColumn.Organism), strItem, " ")
										blnDataPresent = True
									Else
										strData(eTargetColumn.Organism) = SKIP_COLUMN_FLAG
									End If

								Case "OC"
									If Me.IncludeOrganismAndPhylogeny Then
										AppendToText(strData(eTargetColumn.Phylogeny), strItem, " ")
										blnDataPresent = True
									Else
										strData(eTargetColumn.Phylogeny) = SKIP_COLUMN_FLAG
									End If

								Case Else
									' Ignore this code
							End Select
						End If
					End If
				End If
			Loop

			' Write out the previous row
			WriteCachedData(strData, sbSequence, blnDataPresent, swOutFile, intMaxCharsWritten, intMaxCharsPerColumnToWrite, strData(eTargetColumn.Organism))
			blnReadingSequence = False

			Try

				If Not Me.WriteFastaFile Then
					' Write out a summary of the column names and the maximum characters in each column

					strColumnSummaryFilename = IO.Path.GetFileNameWithoutExtension(ioSourceFile.Name) & "_Columns.txt"
					Using swColumnSummary As IO.StreamWriter = New IO.StreamWriter(New IO.FileStream(IO.Path.Combine(ioSourceFile.DirectoryName, strColumnSummaryFilename), IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read))

						swColumnSummary.WriteLine("Column_Name" & ControlChars.Tab & "Max_Column_Length")
						For intIndex As Integer = 0 To intMaxCharsWritten.Length - 1
							If intIndex < lstHeaderColumns.Count Then
								If lstHeaderColumns.Item(intIndex) <> SKIP_COLUMN_FLAG Then
									swColumnSummary.WriteLine(lstHeaderColumns.Item(intIndex) & ControlChars.Tab & intMaxCharsWritten(intIndex))
								End If
							Else
								swColumnSummary.WriteLine("Unknown_Column_" & intIndex & ControlChars.Tab & intMaxCharsWritten(intIndex))
							End If
						Next
					End Using

				End If

			Catch ex As Exception
				Console.WriteLine("Error writing to the column summary file (" & strColumnSummaryFilename & ") in ParseIPIDataFile: " & ex.Message)
				Return False
			End Try

		Catch ex As Exception
			Console.WriteLine("Error in ParseIPIDataFile (parse contents): " & ex.Message)
			Return False
		Finally
			If Not srInFile Is Nothing Then srInFile.Close()
			If Not swOutFile Is Nothing Then swOutFile.Close()
		End Try

		ReportProgress("Done")

		Return True

	End Function

	Private Sub PrescanFileForAddnlColumns(ByVal strFilePath As String, ByRef lstAddnlColumns As SortedSet(Of String), ByRef intEntryCount As Integer)

		Dim strLineIn As String
		Dim strKey As String = String.Empty
		Dim strSubKey As String = String.Empty

		Dim strItem As String = String.Empty
		Dim strSubItem As String = String.Empty

		Try
			intEntryCount = 0

			lstAddnlColumns.Clear()

			ReportProgress("Pre-scanning file to find additional references")

			Using srInFile As IO.StreamReader = New IO.StreamReader(New IO.FileStream(strFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite))

				Do While srInFile.Peek > -1
					strLineIn = srInFile.ReadLine

					If Not String.IsNullOrEmpty(strLineIn) Then
						strLineIn = strLineIn.TrimEnd

						If strLineIn = "//" Then
							intEntryCount += 1

							If intEntryCount Mod 1000 = 0 Then
								ReportProgress("Pre-scanning: " & intEntryCount & " rows read")
							End If

						ElseIf strLineIn.Length > 2 AndAlso Char.IsUpper(strLineIn.Chars(0)) AndAlso Char.IsUpper(strLineIn.Chars(1)) Then

							If SplitLine(strLineIn, strKey, strItem) Then
								If strKey.StartsWith("DR") Then
									If SplitLine(strItem, strSubKey, strSubItem, ";") Then
										If Not lstAddnlColumns.Contains(strSubKey) Then
											lstAddnlColumns.Add(strSubKey)
										End If
									End If
								End If
							End If
						End If
					End If
				Loop

			End Using

			ReportProgress("Done Pre-scanning")


		Catch ex As Exception
			Console.WriteLine("Error in PrescanFileForAddnlColumns: " & ex.Message)
		End Try


	End Sub

	Private Sub ReportProgress(ByVal strProgress As String)
		Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") & " " & strProgress)
	End Sub

	Private Function SplitLine(ByVal strInputText As String, ByRef strKey As String, ByRef strItem As String, Optional ByVal chDelimiter As String = " "c) As Boolean
		Dim intCharIndex As Integer

		If strInputText Is Nothing Then
			strKey = String.Empty
			Return False
		End If

		intCharIndex = strInputText.IndexOf(chDelimiter)

		If intCharIndex > 0 Then
			strKey = strInputText.Substring(0, intCharIndex)
			strItem = strInputText.Substring(intCharIndex + 1).Trim
			Return True
		Else
			strKey = String.Copy(strInputText)
			Return False
		End If
	End Function

	Private Function TrimCommonEndChars(ByVal strText As String) As String

		strText = TrimEnd(strText, "; M.")
		strText = TrimEnd(strText, "; -.")
		strText = TrimEnd(strText, "; -")
		strText = TrimEnd(strText, ".")
		strText = strText.Replace(ControlChars.Tab, ", ")

		Return strText
	End Function

	Private Function TrimEnd(ByVal strText As String, ByVal strTextToTrim As String) As String
		If strTextToTrim Is Nothing Then
			Return String.Empty
		Else
			If strText.EndsWith(strTextToTrim) Then
				Return strText.Substring(0, strText.Length - strTextToTrim.Length)
			Else
				Return strText
			End If
		End If
	End Function

	Private Sub WriteCachedData(ByRef strData() As String,
	 ByRef sbSequence As Text.StringBuilder,
	 ByRef blnDataPresent As Boolean,
	 ByRef swOutFile As IO.StreamWriter,
	 ByRef intMaxCharsWritten() As Integer,
	 ByVal intMaxCharsPerColumnToWrite As Integer,
	 ByVal strSpecies As String)

		If Me.IncludeProteinSequence Then
			strData(eTargetColumn.Sequence) = sbSequence.ToString
		Else
			strData(eTargetColumn.Sequence) = SKIP_COLUMN_FLAG
		End If

		' Write out the previous row
		If blnDataPresent Then
			If Me.WriteFastaFile Then
				WriteFastaFileEntry(swOutFile, strData, strSpecies)
			Else
				WriteData(swOutFile, strData, intMaxCharsWritten, intMaxCharsPerColumnToWrite)
			End If
			ClearArray(strData)
			blnDataPresent = False
		End If

	End Sub

	Private Sub WriteData(ByRef swOutFile As IO.StreamWriter, ByRef strData() As String)
		Dim intMaxCharsWritten() As Integer
		Const intMaxCharsPerColumnToWrite As Integer = 0

		ReDim intMaxCharsWritten(strData.Length)
		WriteData(swOutFile, strData, intMaxCharsWritten, intMaxCharsPerColumnToWrite)
	End Sub

	Private Sub WriteData(ByRef swOutFile As IO.StreamWriter, ByRef strData() As String, ByRef intMaxCharsWritten() As Integer, ByVal intMaxCharsPerColumnToWrite As Integer)
		Const MINIMUM_MAX_CHARS_PER_COLUMN As Integer = 5

		Dim intIndex As Integer

		If intMaxCharsPerColumnToWrite > 0 Then
			If intMaxCharsPerColumnToWrite < MINIMUM_MAX_CHARS_PER_COLUMN Then
				intMaxCharsPerColumnToWrite = MINIMUM_MAX_CHARS_PER_COLUMN
			End If

			For intIndex = 0 To strData.Length - 1
				If Not strData(intIndex) Is Nothing AndAlso strData(intIndex).Length > intMaxCharsPerColumnToWrite Then
					strData(intIndex) = strData(intIndex).Substring(0, intMaxCharsPerColumnToWrite - 3) + "..."
				End If
			Next intIndex
		End If

		For intIndex = 0 To strData.Length - 1
			If Not String.IsNullOrEmpty(strData(intIndex)) Then
				intMaxCharsWritten(intIndex) = Math.Max(strData(intIndex).Length, intMaxCharsWritten(intIndex))
			End If
		Next

		swOutFile.WriteLine(FlattenArray(strData))

	End Sub

	Private Function WriteDelimitedFileHeader(ByRef swOutFile As IO.StreamWriter, _
	 ByRef lstAddnlColumns As SortedSet(Of String), _
	 ByRef dctAddnlColumnNameAndIndex As Dictionary(Of String, Integer)) As List(Of String)

		Dim intIndex As Integer
		Dim strData() As String
		Dim lstHeaderColumns As List(Of String)
		lstHeaderColumns = New List(Of String)

		' Write the header line
		ReDim strData(eTargetColumn.AddnlColumnStart + lstAddnlColumns.Count - 1)

		strData(eTargetColumn.ProteinName) = "Protein_Name"
		strData(eTargetColumn.Accession) = "Accession"
		strData(eTargetColumn.Description) = "Description"
		strData(eTargetColumn.REFSEQ_XP_or_NP) = "RefSeq"
		strData(eTargetColumn.GI) = "GI"
		strData(eTargetColumn.TREMBL) = "TREMBL"
		strData(eTargetColumn.ENSEMBL_Transcript) = "Ensembl_Transcript"
		strData(eTargetColumn.ENSEMBL_Protein) = "Ensembl_Protein"
		strData(eTargetColumn.ENSEMBL_Gene) = "Ensembl_Gene"
		strData(eTargetColumn.EntrezGeneID) = "Entrez_GeneID"
		strData(eTargetColumn.EntrezGeneName) = "Entrez_GeneName"
		strData(eTargetColumn.SwissProt) = "SwissProt"
		strData(eTargetColumn.SwissProtName) = "SwissProt_Name"
		strData(eTargetColumn.Chromosome) = "Chromosome"
		strData(eTargetColumn.AACount) = "Sequence_AA_Count"
		strData(eTargetColumn.MW) = "MW"
		strData(eTargetColumn.Accession1) = "Accession1"
		strData(eTargetColumn.Accession2) = "Accession2"
		strData(eTargetColumn.Accession3) = "Accession3"
		strData(eTargetColumn.Accession4) = "Accession4"

		If Me.IncludeOrganismAndPhylogeny Then
			strData(eTargetColumn.Organism) = "Organism"
			strData(eTargetColumn.Phylogeny) = "Phylogeny"
		Else
			strData(eTargetColumn.Organism) = SKIP_COLUMN_FLAG
			strData(eTargetColumn.Phylogeny) = SKIP_COLUMN_FLAG
		End If

		If Me.IncludeProteinSequence Then
			strData(eTargetColumn.Sequence) = "Sequence"
		Else
			strData(eTargetColumn.Sequence) = SKIP_COLUMN_FLAG
		End If

		dctAddnlColumnNameAndIndex.Clear()

		' Append the additional column names
		intIndex = 0
		For Each strAddnlColumn As String In lstAddnlColumns
			strData(eTargetColumn.AddnlColumnStart + intIndex) = strAddnlColumn
			dctAddnlColumnNameAndIndex.Add(strAddnlColumn, eTargetColumn.AddnlColumnStart + intIndex)
			intIndex += 1
		Next

		For intIndex = 0 To strData.Length - 1
			lstHeaderColumns.Add(strData(intIndex))
		Next

		WriteData(swOutFile, strData)

		Return lstHeaderColumns

	End Function

	Private Sub WriteFastaFileEntry(ByRef swOutFile As IO.StreamWriter, ByRef strData() As String, ByVal strSpecies As String)

		Const RESIDUES_PER_LINE As Integer = 60

		Dim strProteinName As String
		Dim sbProteinDescription As New Text.StringBuilder()

		Dim intStartIndex As Integer
		Dim intLength As Integer

		Dim blnValidSpecies As Boolean = True

		If Not String.IsNullOrEmpty(Me.FastaSpeciesFilter) Then
			If Not strSpecies.ToLower.Contains(Me.FastaSpeciesFilter.ToLower) Then
				blnValidSpecies = False
			End If
		End If

		If Not String.IsNullOrEmpty(Me.FastaSpeciesFilterRegEx) Then
			If mFastaSpeciesRegex Is Nothing OrElse Not Me.FastaSpeciesFilterRegEx.Equals(mFastaSpeciesRegexText) Then
				mFastaSpeciesRegex = New Text.RegularExpressions.Regex(Me.FastaSpeciesFilterRegEx, Text.RegularExpressions.RegexOptions.Compiled Or Text.RegularExpressions.RegexOptions.IgnoreCase)
				mFastaSpeciesRegexText = String.Copy(Me.FastaSpeciesFilterRegEx)
			End If

			blnValidSpecies = mFastaSpeciesRegex.IsMatch(strSpecies)
		End If

		If Not blnValidSpecies Then Exit Sub

		If Not String.IsNullOrEmpty(strData(eTargetColumn.ProteinName)) AndAlso Not String.IsNullOrEmpty(strData(eTargetColumn.Sequence)) Then
			strProteinName = strData(eTargetColumn.ProteinName)

			sbProteinDescription.Clear()

			If Not strData(eTargetColumn.Description) Is Nothing Then
				sbProteinDescription.Append(TrimEnd(strData(eTargetColumn.Description), "."))
			End If

			If Not String.IsNullOrEmpty(strData(eTargetColumn.Accession)) Then
				' Make sure we don't append a semicolon when one already is present
				If Not sbProteinDescription.Chars(sbProteinDescription.Length - 1) = ";"c Then
					sbProteinDescription.Append(";"c)
				End If
				sbProteinDescription.Append(" Accession=" & strData(eTargetColumn.Accession))
			End If

			If Not strData(eTargetColumn.Organism) Is Nothing Then
				sbProteinDescription.Append(" [" & strData(eTargetColumn.Organism) & "]")
			End If

			' When writing out, replace "RecName: Full=" with ""
			swOutFile.WriteLine(">" & strProteinName & " " & sbProteinDescription.ToString().Replace("RecName: Full=", ""))

			' Now write out the residues
			intStartIndex = 0
			intLength = strData(eTargetColumn.Sequence).Length
			Do While intStartIndex < intLength
				If intStartIndex + RESIDUES_PER_LINE <= intLength Then
					swOutFile.WriteLine(strData(eTargetColumn.Sequence).Substring(intStartIndex, RESIDUES_PER_LINE))
				Else
					swOutFile.WriteLine(strData(eTargetColumn.Sequence).Substring(intStartIndex))
				End If
				intStartIndex += RESIDUES_PER_LINE
			Loop

		End If


	End Sub
End Class
