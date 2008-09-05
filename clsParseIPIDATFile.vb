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


Public Class clsParseIPIDATFile

#Region "Constants and Enums"

    Private Const DATA_SOURCE_ENSEMBL As String = "ENSEMBL"
    Private Const DATA_SOURCE_ENTREZ As String = "Entrez Gene"
    Private Const DATA_SOURCE_REFSEQ As String = "REFSEQ"
    Private Const DATA_SOURCE_SWISSPROT As String = "UniProtKB/Swiss-Prot"
    Private Const DATA_SOURCE_TREMBL As String = "UniProtKB/TrEMBL"

    Private Const SKIP_COLUMN_FLAG As String = "<<SKIP_COL>>"

    Private Enum eTargetColumn
        IPI = 0
        Accession = 1
        Description = 2
        REFSEQ_XP_or_NP = 3
        GI = 4
        TREMBL = 5
        ENSEMBL = 6
        EntrezGeneID = 7
        EntrezGeneName = 8
        SwissProt = 9
        SwissProtName = 10
        Chromosome = 11
        AACount = 12
        MW = 13
        Organism = 14
        Phylogeny = 15
        Sequence = 16
        IPI1 = 17
        IPI2 = 18
        IPI3 = 19
        IPI4 = 20
        AddnlColumnStart = 21
    End Enum

#End Region

#Region "Structures"

#End Region

#Region "Classwide Variables"
    Private mShowMessages As Boolean
    Private mIncludeOrganismAndPhylogeny As Boolean
    Private mIncludeProteinSequence As Boolean
    Private mWriteFastaFile As Boolean              ' If this is true, then mIncludeOrganismAndPhylogeny and mIncludeProteinSequence are auto-set to true
#End Region

#Region "Properties"
    Public Property IncludeOrganismAndPhylogeny() As Boolean
        Get
            Return mIncludeOrganismAndPhylogeny
        End Get
        Set(ByVal value As Boolean)
            mIncludeOrganismAndPhylogeny = value
        End Set
    End Property
    Public Property IncludeProteinSequence() As Boolean
        Get
            Return mIncludeProteinSequence
        End Get
        Set(ByVal value As Boolean)
            mIncludeProteinSequence = value
        End Set
    End Property

    Public Property ShowMessages() As Boolean
        Get
            Return mShowMessages
        End Get
        Set(ByVal Value As Boolean)
            mShowMessages = Value
        End Set
    End Property

    Public Property WriteFastaFile() As Boolean
        Get
            Return mWriteFastaFile
        End Get
        Set(ByVal value As Boolean)
            mWriteFastaFile = value
        End Set
    End Property
#End Region

    Private Sub AppendToText(ByRef strText As String, ByVal strNewText As String, ByVal strSeparator As String)
        If strNewText Is Nothing Then
            strNewText = String.Empty
        End If

        If strText Is Nothing OrElse strText.Length = 0 Then
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

    Public Function ParseIPIDATFile(ByVal strInputFileName As String, ByVal intMaxCharsPerColumn As Integer) As Boolean

        Dim strOutputFileName As String = String.Empty

        Dim strLineIn As String

        Dim blnDataPresent As Boolean
        Dim strData() As String

        Dim strKey As String = String.Empty, strItem As String = String.Empty
        Dim strSubKey As String = String.Empty, strSubItem As String = String.Empty
        Dim strSubKey2 As String = String.Empty, strSubItem2 As String = String.Empty
        Dim strSubKey3 As String = String.Empty, strSubItem3 As String = String.Empty
        Dim strSubKey4 As String = String.Empty, strSubItem4 As String = String.Empty

        Dim intAddnlColumnCount As Integer
        Dim strAddnlColumns() As String

        Dim intIndex As Integer
        Dim intIndex2 As Integer

        Dim intSpaceIndex As Integer
        Dim intCharIndex As Integer

        Dim intRowsProcessed As Integer
        Dim intEntryCount As Integer

        Dim srInFile As System.IO.StreamReader
        Dim srOutFile As System.IO.StreamWriter

        Dim sbSequence As System.Text.StringBuilder = New System.Text.StringBuilder
        Dim blnReadingSequence As Boolean = False

        Try
            If Not System.IO.File.Exists(strInputFileName) Then
                Console.WriteLine("File not found: " & strInputFileName)
                Exit Function
            End If

            ReportProgress("Reading " & strInputFileName)

            If mWriteFastaFile Then
                ReDim strAddnlColumns(-1)
                intAddnlColumnCount = 0

                mIncludeOrganismAndPhylogeny = True
                mIncludeProteinSequence = True
            Else
                ' Prescan the file
                PrescanFileForAddnlColumns(strInputFileName, strAddnlColumns, intAddnlColumnCount, intEntryCount)
            End If

            If intAddnlColumnCount > 0 Then
                ' Remove columns from strAddnlColumns() that match the standard columns typically searched for below
                For intIndex = 0 To intAddnlColumnCount - 1
                    If strAddnlColumns(intIndex).ToUpper.StartsWith(DATA_SOURCE_REFSEQ.ToUpper) Then
                        strAddnlColumns(intIndex) = String.Empty
                    Else
                        Select Case strAddnlColumns(intIndex).ToUpper
                            Case DATA_SOURCE_ENSEMBL.ToUpper
                                strAddnlColumns(intIndex) = String.Empty
                            Case DATA_SOURCE_ENTREZ.ToUpper
                                strAddnlColumns(intIndex) = String.Empty
                            Case DATA_SOURCE_REFSEQ.ToUpper
                                strAddnlColumns(intIndex) = String.Empty
                            Case DATA_SOURCE_SWISSPROT.ToUpper
                                strAddnlColumns(intIndex) = String.Empty
                            Case DATA_SOURCE_TREMBL.ToUpper
                                strAddnlColumns(intIndex) = String.Empty
                        End Select

                    End If
                Next intIndex

                ' Compress the array (copy in place)
                intIndex = 0
                For intIndex2 = 0 To intAddnlColumnCount - 1
                    If strAddnlColumns(intIndex2).Length > 0 Then
                        If intIndex <> intIndex2 Then
                            strAddnlColumns(intIndex) = String.Copy(strAddnlColumns(intIndex2))
                        End If
                        intIndex += 1
                    End If
                Next intIndex2
                intAddnlColumnCount = intIndex
            End If

        Catch ex As Exception
            Console.WriteLine("Error in ParseIPIDataFile (Prescan file): " & ex.Message)
            Exit Function
        End Try


        Try
            srInFile = New System.IO.StreamReader(New System.IO.FileStream(strInputFileName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read))
        Catch ex As Exception
            Console.WriteLine("Error opening the input file (" & strInputFileName & ") in ParseIPIDataFile: " & ex.Message)
            Exit Function
        End Try

        Try
            strOutputFileName = System.IO.Path.GetFileNameWithoutExtension(strInputFileName) & "_output"
            If mWriteFastaFile Then
                strOutputFileName &= ".fasta"
            Else
                strOutputFileName &= ".txt"
            End If

            srOutFile = New System.IO.StreamWriter(New System.IO.FileStream(strOutputFileName, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read))
        Catch ex As Exception
            Console.WriteLine("Error opening the output file (" & strOutputFileName & ") in ParseIPIDataFile: " & ex.Message)
            Exit Function
        End Try

        Try
            If mWriteFastaFile Then
                ' Writing out a fasta file; no header to write
            Else
                ' Writing a tab-delimited file
                WriteDelimitedFileHeader(srOutFile, intAddnlColumnCount, strAddnlColumns, intMaxCharsPerColumn)
            End If

            ReDim strData(eTargetColumn.AddnlColumnStart + intAddnlColumnCount)
            ClearArray(strData)

            intRowsProcessed = 0
            blnDataPresent = False
            Do While srInFile.Peek >= 0
                strLineIn = srInFile.ReadLine

                If Not strLineIn Is Nothing AndAlso strLineIn.Length > 0 Then
                    strLineIn = strLineIn.TrimEnd

                    If strLineIn = "//" Then

                        blnReadingSequence = False
                        If mIncludeProteinSequence Then
                            strData(eTargetColumn.Sequence) = sbSequence.ToString
                        End If

                        ' Write out the previous row
                        If blnDataPresent Then
                            If mWriteFastaFile Then
                                WriteFastaFileEntry(srOutFile, strData)
                            Else
                                WriteData(srOutFile, strData, intMaxCharsPerColumn)
                            End If
                            ClearArray(strData)
                            blnDataPresent = False
                        End If

                        intRowsProcessed += 1

                        If intRowsProcessed Mod 1000 = 0 Then
                            ReportProgress("Working: " & intRowsProcessed & " / " & intEntryCount & " entries processed")
                        End If

                    ElseIf blnReadingSequence Then
                        If mIncludeProteinSequence Then
                            sbSequence.Append(strLineIn.Trim.Replace(" "c, String.Empty))
                        End If
                    ElseIf strLineIn.Length > 2 AndAlso Char.IsUpper(strLineIn.Chars(0)) AndAlso Char.IsUpper(strLineIn.Chars(1)) Then

                        If SplitLine(strLineIn, strKey, strItem) Then
                            Select Case strKey.Substring(0, 2)
                                Case "ID"
                                    intSpaceIndex = strItem.IndexOf(" "c)
                                    If intSpaceIndex >= 3 Then
                                        strItem = strItem.Substring(0, intSpaceIndex)
                                    End If
                                    strData(eTargetColumn.IPI) = String.Copy(strItem)
                                    blnDataPresent = True
                                Case "AC"
                                    strData(eTargetColumn.Accession) = String.Copy(strItem)
                                    If SplitLine(strItem, strSubKey, strSubItem, ";") Then
                                        strData(eTargetColumn.IPI1) = String.Copy(strSubKey)
                                        If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
                                            strData(eTargetColumn.IPI2) = String.Copy(strSubKey2)
                                            If SplitLine(strSubItem2, strSubKey3, strSubItem3, ";") Then
                                                strData(eTargetColumn.IPI3) = String.Copy(strSubKey3)
                                                If SplitLine(strSubItem3, strSubKey4, strSubItem4, ";") Then
                                                    strData(eTargetColumn.IPI4) = String.Copy(strSubKey4)
                                                Else
                                                    strData(eTargetColumn.IPI4) = String.Copy(strSubItem3)
                                                End If
                                            Else
                                                strData(eTargetColumn.IPI3) = String.Copy(strSubItem2)
                                            End If
                                        Else
                                            strData(eTargetColumn.IPI2) = String.Copy(strSubItem)
                                        End If
                                    Else
                                        strData(eTargetColumn.IPI1) = String.Copy(strItem)
                                    End If
                                    blnDataPresent = True

                                Case "DE"
                                    AppendToText(strData(eTargetColumn.Description), strItem, " ")
                                    blnDataPresent = True

                                Case "DR"
                                    If SplitLine(strItem, strSubKey, strSubItem, ";") Then
                                        If strSubKey.StartsWith(DATA_SOURCE_REFSEQ) Then
                                            If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
                                                AppendToText(strData(eTargetColumn.REFSEQ_XP_or_NP), strSubKey2, "; ")

                                                intCharIndex = strSubItem2.IndexOf(";")
                                                If intCharIndex >= 0 Then
                                                    strSubItem2 = strSubItem2.Substring(0, intCharIndex)
                                                End If

                                                strSubItem2 = Replace(strSubItem2, "GI:", "")
                                                strSubItem2 = TrimEnd(strSubItem2, "; M.")
                                                strSubItem2 = TrimEnd(strSubItem2, "; -.")

                                                AppendToText(strData(eTargetColumn.GI), strSubItem2, "; ")
                                            Else
                                                AppendToText(strData(eTargetColumn.REFSEQ_XP_or_NP), strSubItem, "; ")
                                            End If
                                            blnDataPresent = True
                                        Else

                                            Select Case strSubKey
                                                Case DATA_SOURCE_TREMBL
                                                    strSubItem = TrimEnd(strSubItem, "; M.")
                                                    strSubItem = TrimEnd(strSubItem, "; -.")

                                                    If SplitLine(strSubItem, strSubKey2, strSubItem2, ";") Then
                                                        AppendToText(strData(eTargetColumn.TREMBL), strSubKey2, "; ")
                                                    Else
                                                        AppendToText(strData(eTargetColumn.TREMBL), strSubItem, "; ")
                                                    End If
                                                    blnDataPresent = True

                                                Case DATA_SOURCE_ENSEMBL
                                                    strSubItem = TrimEnd(strSubItem, "; M.")
                                                    strSubItem = TrimEnd(strSubItem, "; -.")
                                                    AppendToText(strData(eTargetColumn.ENSEMBL), strSubItem, "; ")
                                                    blnDataPresent = True

                                                Case DATA_SOURCE_ENTREZ
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

                                                Case DATA_SOURCE_SWISSPROT
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
                                                    ' See if strSubKey matches any of the entries in strAddnlColumns()
                                                    intIndex = Array.BinarySearch(strAddnlColumns, 0, intAddnlColumnCount, strSubKey)
                                                    If intIndex >= 0 Then
                                                        strSubItem = TrimEnd(strSubItem, "; M.")
                                                        strSubItem = TrimEnd(strSubItem, "; -.")
                                                        strSubItem = TrimEnd(strSubItem, "; -")

                                                        AppendToText(strData(eTargetColumn.AddnlColumnStart + intIndex), strSubItem, "; ")
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
                                    If mIncludeOrganismAndPhylogeny Then
                                        ' Organism lines tend to end in a period; remove it if found
                                        strItem = TrimEnd(strItem, ".")

                                        AppendToText(strData(eTargetColumn.Organism), strItem, " ")
                                        blnDataPresent = True
                                    Else
                                        strData(eTargetColumn.Organism) = SKIP_COLUMN_FLAG
                                    End If

                                Case "OC"
                                    If mIncludeOrganismAndPhylogeny Then
                                        AppendToText(strData(eTargetColumn.Phylogeny), strItem, " ")
                                        blnDataPresent = True
                                    Else
                                        strData(eTargetColumn.Phylogeny) = SKIP_COLUMN_FLAG
                                    End If
                                Case Else
                            End Select
                        End If
                    End If
                End If
            Loop

            If blnDataPresent Then
                blnReadingSequence = False
                If mIncludeProteinSequence Then
                    strData(eTargetColumn.Sequence) = sbSequence.ToString
                End If

                ' Write out the previous row
                If blnDataPresent Then
                    If mWriteFastaFile Then
                        WriteFastaFileEntry(srOutFile, strData)
                    Else
                        WriteData(srOutFile, strData, intMaxCharsPerColumn)
                    End If
                    ClearArray(strData)
                    blnDataPresent = False
                End If

            End If

        Catch ex As Exception
            Console.WriteLine("Error in ParseIPIDataFile (parse contents): " & ex.Message)
        Finally
            If Not srInFile Is Nothing Then srInFile.Close()
            If Not srOutFile Is Nothing Then srOutFile.Close()
        End Try

        ReportProgress("Done")
    End Function

    Private Sub PrescanFileForAddnlColumns(ByVal strFilePath As String, ByRef strAddnlColumns() As String, ByRef intAddnlColumnCount As Integer, ByRef intEntryCount As Integer)

        Dim srInFile As System.IO.StreamReader

        Dim strLineIn As String
        Dim strKey As String = String.Empty
        Dim strSubKey As String = String.Empty

        Dim strItem As String = String.Empty
        Dim strSubItem As String = String.Empty

        Try
            intAddnlColumnCount = 0
            ReDim strAddnlColumns(9)                    ' 0-based array

            intEntryCount = 0

            ReportProgress("Pre-scanning file to find additional references")

            srInFile = New System.IO.StreamReader(New System.IO.FileStream(strFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read))

            Do While srInFile.Peek >= 0
                strLineIn = srInFile.ReadLine

                If Not strLineIn Is Nothing AndAlso strLineIn.Length > 0 Then
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
                                    If Array.BinarySearch(strAddnlColumns, 0, intAddnlColumnCount, strSubKey) < 0 Then
                                        ' Add strSubKey to strAddnlColumns (when adding, keep sorted alphabetically)

                                        If intAddnlColumnCount = 0 Then
                                            strAddnlColumns(0) = strSubKey
                                            intAddnlColumnCount = 1
                                        Else
                                            ' Expand the array if needed
                                            If intAddnlColumnCount >= strAddnlColumns.Length Then
                                                ReDim Preserve strAddnlColumns(strAddnlColumns.Length * 2 - 1)
                                            End If

                                            strAddnlColumns(intAddnlColumnCount) = strSubKey
                                            intAddnlColumnCount += 1

                                            ' Re-sort the array
                                            Array.Sort(strAddnlColumns, 0, intAddnlColumnCount)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Loop

            ReportProgress("Done Pre-scanning")


        Catch ex As Exception
            Console.WriteLine("Error in PrescanFileForAddnlColumns: " & ex.Message)
        Finally
            If Not srInFile Is Nothing Then srInFile.Close()
        End Try


    End Sub

    Private Sub ReportProgress(ByVal strProgress As String)
        Console.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") & " " & strProgress)
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

    Private Sub WriteData(ByRef srOutFile As System.IO.StreamWriter, ByRef strData() As String, ByVal intMaxCharsPerColumn As Integer)
        Const MINIMUM_MAX_CHARS_PER_COLUMN As Integer = 5

        Dim intIndex As Integer

        If intMaxCharsPerColumn > 0 Then
            If intMaxCharsPerColumn < MINIMUM_MAX_CHARS_PER_COLUMN Then
                intMaxCharsPerColumn = MINIMUM_MAX_CHARS_PER_COLUMN
            End If

            For intIndex = 0 To strData.Length - 1
                If Not strData(intIndex) Is Nothing AndAlso strData(intIndex).Length > intMaxCharsPerColumn Then
                    strData(intIndex) = strData(intIndex).Substring(0, intMaxCharsPerColumn - 3) + "..."
                End If
            Next intIndex
        End If

        srOutFile.WriteLine(FlattenArray(strData))
    End Sub

    Private Sub WriteDelimitedFileHeader(ByRef srOutFile As System.IO.StreamWriter, _
                                         ByRef intAddnlColumnCount As Integer, _
                                         ByRef strAddnlColumns() As String, _
                                         ByRef intMaxCharsPerColumn As Integer)

        Dim intIndex As Integer
        Dim strData() As String

        ' Write the header line
        ReDim strData(eTargetColumn.AddnlColumnStart + intAddnlColumnCount)

        strData(eTargetColumn.IPI) = "IPI"
        strData(eTargetColumn.Accession) = "Accession"
        strData(eTargetColumn.Description) = "Description"
        strData(eTargetColumn.REFSEQ_XP_or_NP) = "REFSEQ_XP_or_NP"
        strData(eTargetColumn.GI) = "GI"
        strData(eTargetColumn.TREMBL) = "TREMBL"
        strData(eTargetColumn.ENSEMBL) = "ENSEMBL"
        strData(eTargetColumn.EntrezGeneID) = "Entrez_GeneID"
        strData(eTargetColumn.EntrezGeneName) = "Entrez_GeneName"
        strData(eTargetColumn.SwissProt) = "SwissProt"
        strData(eTargetColumn.SwissProtName) = "SwissProt_Name"
        strData(eTargetColumn.Chromosome) = "Chromosome"
        strData(eTargetColumn.AACount) = "Sequence_AA_Count"
        strData(eTargetColumn.MW) = "MW"
        strData(eTargetColumn.IPI1) = "IPI1"
        strData(eTargetColumn.IPI2) = "IPI2"
        strData(eTargetColumn.IPI3) = "IPI3"
        strData(eTargetColumn.IPI4) = "IPI4"

        If mIncludeOrganismAndPhylogeny Then
            strData(eTargetColumn.Organism) = "Organism"
            strData(eTargetColumn.Phylogeny) = "Phylogeny"
        Else
            strData(eTargetColumn.Organism) = SKIP_COLUMN_FLAG
            strData(eTargetColumn.Phylogeny) = SKIP_COLUMN_FLAG
        End If

        If mIncludeProteinSequence Then
            strData(eTargetColumn.Sequence) = "Sequence"
        Else
            strData(eTargetColumn.Sequence) = SKIP_COLUMN_FLAG
        End If

        ' Append the additional column names
        For intIndex = 0 To intAddnlColumnCount - 1
            strData(eTargetColumn.AddnlColumnStart + intIndex) = strAddnlColumns(intIndex)
        Next intIndex
        WriteData(srOutFile, strData, intMaxCharsPerColumn)

    End Sub

    Private Sub WriteFastaFileEntry(ByRef srOutFile As System.IO.StreamWriter, ByRef strData() As String)

        Const RESIDUES_PER_LINE As Integer = 60

        Dim strProteinName As String
        Dim strProteinDescription As String

        Dim intStartIndex As Integer
        Dim intLength As Integer

        If Not strData(eTargetColumn.IPI) Is Nothing AndAlso Not strData(eTargetColumn.Sequence) Is Nothing Then
            strProteinName = strData(eTargetColumn.IPI)

            If strData(eTargetColumn.Description) Is Nothing Then
                strProteinDescription = String.Empty
            Else
                strProteinDescription = TrimEnd(strData(eTargetColumn.Description), ".")
            End If

            If Not strData(eTargetColumn.Organism) Is Nothing Then
                strProteinDescription &= " [" & strData(eTargetColumn.Organism) & "]"
            End If

            srOutFile.WriteLine(">" & strProteinName & " " & strProteinDescription)

            ' Now write out the residues
            intStartIndex = 0
            intLength = strData(eTargetColumn.Sequence).Length
            Do While intStartIndex < intLength
                If intStartIndex + RESIDUES_PER_LINE <= intLength Then
                    srOutFile.WriteLine(strData(eTargetColumn.Sequence).Substring(intStartIndex, RESIDUES_PER_LINE))
                Else
                    srOutFile.WriteLine(strData(eTargetColumn.Sequence).Substring(intStartIndex))
                End If
                intStartIndex += RESIDUES_PER_LINE
            Loop

        End If


    End Sub
End Class
