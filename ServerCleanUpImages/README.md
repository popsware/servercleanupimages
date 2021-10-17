## About
An automated program to utilize the storage space of website images used on the server

The program parses a `MYSQL` database. targeting a specific field in a specific table. The program then compares the found values with a specified directory in order to  remove unused files from this directory.

### Limitations
The program will fail with a DB design were multiple tables are using the same directory