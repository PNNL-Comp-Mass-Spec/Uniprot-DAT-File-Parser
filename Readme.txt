== Overview ==

This program can read a Uniprot (IPI) .Dat file and parse out the 
information for each entry, creating a tab delimited text file.  
Uniprot .Dat files can be obtained from EBI at:
 ftp://ftp.ebi.ac.uk/pub/databases/IPI/current/ and 
 ftp://ftp.ebi.ac.uk/pub/databases/IPI/old/

See also http://www.ebi.ac.uk/IPI/FAQs.html for a list of frequently
asked questions concerning Uniprot files.

== Usage ==

This program must be run from the command line.  To use, first
download the .Dat file from EBI. If the filename ends in .gz then you
need to first unzip the file (use 7-Zip, WinZip, or WinRar).  Next, 
place the Uniprot_DAT_File_Parser.exe file in the same folder as the .Dat 
file.  Now go to the command prompt, change the directory to the folder
with the files, and enter the command:
  Uniprot_DAT_File_Parser.exe Inputfile.Dat

== Command Line Options ==

Use /M to specify the maximum number of characters to retain for each column.
For example, limit to 200 characters using
  Uniprot_DAT_File_Parser.exe Inputfile.Dat /M:200

Use /S to include the protein sequence in the output file (ignored if using /F)

Use /O to include the organism name and phylogeny information (ignored if using /F)

Use /F to specify that a .Fasta file be created for the proteins.

Use /Species:FilterText to only write entries to the fasta file if the Species tag contains FilterText

Use /SpeciesRegEx:"RegEx" to only write entries to the fasta file if the Species tag matches the 
specified regular expression

Use /OrgFile:FilePath to specify the path to a file containing organism names to filter on 
(one name per line).  The organism names must be exact matches to the organism names listed 
in the _OrganismSummary.txt file that is created by this program.

-------------------------------------------------------------------------------
Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
Copyright 2007, Battelle Memorial Institute.  All Rights Reserved.

E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com
Website: http://omics.pnl.gov/ or http://www.sysbio.org/resources/staff/
-------------------------------------------------------------------------------

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0
