== Overview ==

This program can read a Uniprot .Dat file and parse out the 
information for each entry, creating a series of tab delimited 
text files or creating a FASTA file.  Uniprot .Dat files can 
be obtained from EBI at:
  ftp.ebi.ac.uk/pub/databases/uniprot/current_release/knowledgebase/taxonomic_divisions
  ftp.ebi.ac.uk/pub/databases/uniprot/current_release/knowledgebase/complete

See also http://web.expasy.org/docs/userman.html for a list 
of frequently asked questions concerning Uniprot files.

The tab-delimited files created are:
* Primary data file, with one line per protein entry in the .DAT file, for example:

Protein_Name	Accession	Description	Sequence_AA_Count	MW	Organism	Phylogeny	Accession1
12AH_CLOS4	P21215;	RecName: Full=12-alpha-hydroxysteroid dehydrogenase; EC=1.1.1.176; Flags: Fragment;	29	2900	Clostridium sp. (strain ATCC 29733 / VPI C48-50)	Bacteria; Firmicutes; Clostridia; Clostridiales; Clostridiaceae; Clostridium.	P21215
12KD_MYCSM	P80438;	RecName: Full=12 kDa protein; Flags: Fragment;	24	2766	Mycobacterium smegmatis	Bacteria; Actinobacteria; Actinobacteridae; Actinomycetales; Corynebacterineae; Mycobacteriaceae; Mycobacterium.	P80438
12S_PROFR	Q8GBW6; Q05617;	RecName: Full=Methylmalonyl-CoA carboxyltransferase 12S subunit; EC=2.1.3.1; AltName: Full=Transcarboxylase 12S subunit;	611	65927	Propionibacterium freudenreichii subsp. shermanii	Bacteria; Actinobacteria; Actinobacteridae; Actinomycetales; Propionibacterineae; Propionibacteriaceae; Propionibacterium.	Q8GBW6

* Organism Summary file

Organism	Proteins
Acidianus ambivalens	105
Acidianus brierleyi	11
Acidianus hospitalis	2368
Acidianus infernus	3
 
* Organism Map file

Protein	Organism	Strain	Additional_Info
F7PIW9_9EURY	Halorhabdus tiamatea SARL4B		
F7PNT1_9EURY	Halorhabdus tiamatea SARL4B		
Q2FP08_METHJ	Methanospirillum hungatei JF-1	strain ATCC 27890 / DSM 864 / NBRC 100397 / JF-1	
U1PGF8_9EURY	Haloquadratum walsbyi J07HQW1		

== Usage ==

This program must be run from the command line.  To use, first
download the .Dat file from EBI. If the filename ends in .gz then you
need to first unzip the file (use 7-Zip, WinZip, WinRar, or similar).  
Next, place the Uniprot_DAT_File_Parser.exe file in the same folder 
as the .Dat file.  Now go to the command prompt, change the directory 
to the folder with the files, and enter the command:
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
