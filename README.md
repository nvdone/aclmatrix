# aclmatrix
Small utility which analyses folder tree permissions and displays them as a matrix in a Microsoft Excel worksheet. Quite useful for investigating permissions to a shared folder.\
\
![Sample](Manual/sample.png)\
\
Usage: ACLMatrix.exe root_path output_file.xlsx [ShowAccountNames]\
\
Caveats:
1. This tool retrieves folder permissions. If you grant access on a file level, you are on your own to update this according to your needs. However, it might be worth to reconsider approach to access rights' distribution
2. Run it under an account, which has access to the subject directory tree and permissions to read relevant active directory data
3. Run it on a file server for better performance
