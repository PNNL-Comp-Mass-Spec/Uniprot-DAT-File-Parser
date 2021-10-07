# __<span style="color:#D57500">Uniprot DAT File Parser</span>__
The Uniprot DAT File Parser can read a Uniprot .Dat file and parse out the information for each entry, creating a series of tab delimited text files or creating a FASTA file.

### Description
Uniprot .Dat files can be obtained from EBI's FTP site. Useful folders include:

>  https://ftp.ebi.ac.uk/pub/databases/uniprot/current_release/knowledgebase/taxonomic_divisions <br>
https://ftp.ebi.ac.uk/pub/databases/uniprot/current_release/knowledgebase/complete

See also https://web.expasy.org/docs/userman.html for a list of frequently asked questions concerning Uniprot files.

The tab-delimited files created are:
* Primary data file, with one line per protein entry in the .DAT file, for example:

  | Protein_Name | Accession | Description | Sequence_AA_Count | MW | Organism | Phylogeny | Accession1 |
  |---|---|---|---|---|---|---|---|
  | 12AH_CLOS4 | P21215; | RecName: Full=12-alpha-hydroxysteroid dehydrogenase; EC=1.1.1.176; Flags: Fragment; | 29 | 2900 | Clostridium sp. (strain ATCC 29733 / VPI C48-50) | Bacteria; Firmicutes; Clostridia; Clostridiales; Clostridiaceae; Clostridium. | P21215 |
  | 12KD_MYCSM | P80438; | RecName: Full=12 kDa protein; Flags: Fragment; | 24 | 2766 | Mycobacterium smegmatis | Bacteria; Actinobacteria; Actinobacteridae; Actinomycetales; Corynebacterineae; Mycobacteriaceae; Mycobacterium. | P80438 |
  | 12S_PROFR | Q8GBW6; Q05617; | RecName: Full=Methylmalonyl-CoA carboxyltransferase 12S subunit; EC=2.1.3.1; AltName: Full=Transcarboxylase 12S subunit; | 611 | 65927 | Propionibacterium freudenreichii subsp. shermanii | Bacteria; Actinobacteria; Actinobacteridae; Actinomycetales; Propionibacterineae; Propionibacteriaceae; Propionibacterium. | Q8GBW6 |

* Organism Summary file

  | Organism | Proteins |
  |---|---|
  | Acidianus ambivalens | 105 |
  | Acidianus brierleyi | 11 |
  | Acidianus hospitalis | 2368 |
  | Acidianus infernus | 3 |

* Organism Map file

  | Protein |	Organism | Strain |
  |---|---|---|
  | F7PIW9_9EURY | Halorhabdus tiamatea SARL4B | |
  | F7PNT1_9EURY | Halorhabdus tiamatea SARL4B | |
  | Q2FP08_METHJ | Methanospirillum hungatei JF-1 | strain ATCC 27890 / DSM 864 / NBRC 100397 / JF-1 |
  | U1PGF8_9EURY | Haloquadratum walsbyi J07HQW1 | |

### Downloads
* [Latest version](https://github.com/PNNL-Comp-Mass-Spec/Uniprot-DAT-File-Parser/releases/latest)
* [Source code on GitHub](https://github.com/PNNL-Comp-Mass-Spec/Uniprot-DAT-File-Parser)

#### Software Instructions
Run UniprotDATFileParser_Installer.exe to install the application.  This is a command-line (console) application and thus does not have a GUI.   For more information on using command line applications, see the [Command Line Application help](https://pnnl-comp-mass-spec.github.io/CmdLineHelp) page.

### Acknowledgment

All publications that utilize this software should provide appropriate acknowledgement to PNNL and the Uniprot-DAT-File-Parser GitHub repository. However, if the software is extended or modified, then any subsequent publications should include a more extensive statement, as shown in the Readme file for the given application or on the website that more fully describes the application.

### Disclaimer

These programs are primarily designed to run on Windows machines. Please use them at your own risk. This material was prepared as an account of work sponsored by an agency of the United States Government. Neither the United States Government nor the United States Department of Energy, nor Battelle, nor any of their employees, makes any warranty, express or implied, or assumes any legal liability or responsibility for the accuracy, completeness, or usefulness or any information, apparatus, product, or process disclosed, or represents that its use would not infringe privately owned rights.

Portions of this research were supported by the NIH National Center for Research Resources (Grant RR018522), the W.R. Wiley Environmental Molecular Science Laboratory (a national scientific user facility sponsored by the U.S. Department of Energy's Office of Biological and Environmental Research and located at PNNL), and the National Institute of Allergy and Infectious Diseases (NIH/DHHS through interagency agreement Y1-AI-4894-01). PNNL is operated by Battelle Memorial Institute for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

We would like your feedback about the usefulness of the tools and information provided by the Resource. Your suggestions on how to increase their value to you will be appreciated. Please e-mail any comments to proteomics@pnl.gov
