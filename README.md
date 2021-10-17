## About
An automated program to utilize the storage space of website images used on the server

The program parses a `MYSQL` database. targeting a specific field in a specific table. The program then compares the found values with a specified directory in order to  remove unused files from this directory.

### Usage
0. edit `ServerCleanUpImages.exe.config`
    - set MYSQL password
    - set log directory
1. copy files from `\ServerCleanUpImages\bin\Debug` into the production directory
2. create a batch file to use `ServerCleanUpImages.exe` in this folder
3. create cmd command in the following format:
    - ServerCleanUpImages.exe PATH_TO_IMGS DB_NAME.TABLE COLUMN_NAME
    - example in `ServerCleanUpImages_beautifier.bat`
4. run the batch file as administrator

### Limitations
The program will fail with a DB design were multiple tables are using the same directory