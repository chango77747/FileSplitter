# FileSplitter

File splitter application. Good use case is when exfiling large files over C2 or other means.


Author: Evan MFing Pena AKA Chango77747


Usage:

Split a file: FileSplitter.exe -f {pathToFile} -s -n 50 -o {pathToOutputFolder}


Merge a files: FileSplitter.exe -c -d {DirectoryWithTempFiles}


copy folder to another: FileSplitter.exe -e -l {sourceFolder} -r {destinationFolder}
