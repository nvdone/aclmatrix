# aclmatrix
Small utility to analyse folder tree permissions and display them as a matrix in a Microsoft Excel worksheet. Quite useful for investigating permissions to a shared folder.\
\
![Sample](Manual/sample.png)\
\
Usage: ACLMatrix.exe root_path output_file.xlsx [ShowAccountNames] [BypassACL] [CheckFiles]\
\
Caveats:
1. You may use CheckFiles option to investigate permissions on a file granularity level, but keep in mind, that the file count is limited by a maximum number of rows in Microsoft Excel.
2. You may run it for a shared folder under an account, which has access to the subject directory tree and permissions to read relevant active directory data. However, performance will be higher if you run this tool on a file server. If you run it on a file server under an account with administrative permissions, you may use BypassACL option, so that you can retrieve data for folders, you are restricted to access due to NTFS permissions.

