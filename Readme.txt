This program can read a Uniprot (IPI) .Dat file and parse out the 
information for each entry, creating a tab delimited text file.  
Uniprot .Dat files can be obtained from EBI at:
 ftp://ftp.ebi.ac.uk/pub/databases/IPI/current/ and 
 ftp://ftp.ebi.ac.uk/pub/databases/IPI/old/

See also http://www.ebi.ac.uk/IPI/FAQs.html for a list of frequently
asked questions concerning Uniprot files.

This program must be run from the command line.  To use, first
download the .Dat file from EBI. If the filename ends in .gz then you
need to first unzip the file (use 7-Zip, WinZip, or WinRar).  Next, 
place theUniprot_DAT_File_Parser.exe file in the same folder as the .Dat 
file.  Now go to the command prompt, change the directory to the folder
with the files, and enter the command:
  Uniprot_DAT_File_Parser.exe Inputfile.Dat

You can optionally limit the maximum characters to retain for each column 
using the /M switch, like this:
  Uniprot_DAT_File_Parser.exe Inputfile.Dat /M:200

Use /S to include the protein sequence in the output file.  
Use /O to include the organism name and phylogeny information.

Use /F to specify that a .Fasta file be created for the proteins.

Use /Species:FilterText to only write entries to the fasta file if the Species tag contains FilterText
Use /SpeciesRegEx:"RegEx" to only write entries to the fasta file if the Species tag matches the 
specified regular expression

-------------------------------------------------------------------------------
Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
Copyright 2007, Battelle Memorial Institute.  All Rights Reserved.

E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com
Website: http://omics.pnl.gov/ or http://www.sysbio.org/resources/staff/
-------------------------------------------------------------------------------

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0

All publications that result from the use of this software should include 
the following acknowledgment statement:
 Portions of this research were supported by the W.R. Wiley Environmental 
 Molecular Science Laboratory, a national scientific user facility sponsored 
 by the U.S. Department of Energy's Office of Biological and Environmental 
 Research and located at PNNL.  PNNL is operated by Battelle Memorial Institute 
 for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

Notice: This computer software was prepared by Battelle Memorial Institute, 
hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
Department of Energy (DOE).  All rights in the computer software are reserved 
by DOE on behalf of the United States Government and the Contractor as 
provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
SOFTWARE.  This notice including this sentence must appear on any copies of 
this computer software.