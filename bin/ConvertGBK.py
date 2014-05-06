#
# From http://www2.warwick.ac.uk/fac/sci/moac/people/students/peter_cock/python/genbank2fasta/
# Modified by Matthew Monroe on 9/19/2012
# Extended by Matthew Monroe on 6/19/2013 to support command line arguments (based on code from Sam Payne)
#

UsageInfo="""
Command line switches:
 -i [FilePath] Input file name (or path); required
 -o [FilePath] Output file name (or path); optional

"""

import os
import sys
import getopt

from Bio.Seq import Seq
from Bio.SeqRecord import SeqRecord
from Bio.Alphabet import IUPAC
from Bio import SeqIO

class ParserClass:
    def __init__ (self):
        #input params
        self.InputFilePath = None
        self.OutputFilePath = ""

    def Main(self):

        if self.OutputFilePath == "" :
            self.OutputFilePath = self.InputFilePath + ".fasta"

        self.ParseGenbank(self.InputFilePath, self.OutputFilePath)

    def ParseGenbank(self, gbk_filename, faa_filename):
            
        input_handle  = open(gbk_filename, "r")
        output_handle = open(faa_filename, "w")
        
        processedCount = 0
        for seq_record in SeqIO.parse(input_handle, "genbank") :
        #    print "Dealing with GenBank record %s" % seq_record.id
            for seq_feature in seq_record.features :
                if seq_feature.type=="source" :
                    record_organism = seq_feature.qualifiers['organism'][0]
                if seq_feature.type=="CDS" :
                    processedCount = processedCount + 1
                    if processedCount % 1000 == 0 :
                        print "CDS entry %d: %s from %s" % (processedCount, seq_feature.qualifiers['locus_tag'][0], seq_record.id)
        
                    assert len(seq_feature.qualifiers['translation'])==1
        
                    # Note: Could use this to write out a standard protein fasta file entry
                    # currentProteinRecord = SeqRecord(Seq(seq_feature.qualifiers['translation'][0], IUPAC.protein),
                    #        id=seq_feature.qualifiers['locus_tag'][0], name="UnknownGene",
                    #        description=seq_feature.qualifiers['product'][0])
                    # SeqIO.write(currentProteinRecord, output_handle, "fasta")
        
        
                    # Instead, we use this code so that we can include the organism name
                    # Locus_tag is useful for Mycobacterium tuberculosis .gbk files, which have Rv numbers, for example Rv0905
                    # db_xref provides the gi number, for example gi|15608045
                    # Note: could include Locus using seq_record.name
                    output_handle.write(">%s %s; %s|%s; [%s]\n" % (
                        seq_feature.qualifiers['locus_tag'][0],
                        seq_feature.qualifiers['product'][0],
                        seq_feature.qualifiers['db_xref'][0].replace("GI:", "gi|"),
                        seq_feature.qualifiers['protein_id'][0],
                        record_organism
                        ))
        
                    residues = seq_feature.qualifiers['translation'][0]
                    
                    i = 0
                    chunkSize = 60
                    while (i < len(residues)):
                        output_handle.write("%s\n" % (
                               residues[i:i+chunkSize]))
                        i += chunkSize
        
        output_handle.close()
        input_handle.close()


    def ParseCommandLine(self,Arguments):
        (Options, Args) = getopt.getopt(Arguments, "i:o:")
        OptionsSeen = {}
        for (Option, Value) in Options:
            OptionsSeen[Option] = 1
            if Option == "-i":
                if not os.path.exists(Value):
                    print "\n** Error: Input file not found '%s'\n"%Value
                    print UsageInfo
                    sys.exit(1)
                self.InputFilePath = Value
            elif Option == "-o":               
                self.OutputFilePath = Value
           
            else:
                print "\n** Error: Option %s not recognized\n"%Option
                print UsageInfo
                sys.exit(1)

        if not OptionsSeen.has_key("-i"):
            print "\n** Error: Missing required parameter -i\n"
            print UsageInfo
            sys.exit(1)

if __name__ == "__main__":
    DoStuff = ParserClass()
    DoStuff.ParseCommandLine(sys.argv[1:])
    DoStuff.Main()
