# Import Excel to SQL

Windows Forms app (.NET 8) for connecting to a customer's SQL Server database and listing all of its tables. This is the first step before importing Excel data.

## Requirements

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (optional)

## Running the app

```bash
dotnet run --project ExcelToSqlApp
```

You can also open `ExcelToSqlApp.sln` in Visual Studio and press F5.

## Customer profiles (`clients.json`)

Customer connection profiles are stored in `ExcelToSqlApp/clients.json`, which is copied next to the program at build time:

```json
[
  {
    "ClientName": "ABC Manufacturing Co., Ltd.",
    "Server": "192.168.1.10",
    "Database": "ABC_ERP",
    "User": "erp_reader",
    "Password": "abc@1234"
  }
]
```

> ⚠️ The file stores passwords in plain text. Do not commit real customer passwords to the repository.

## How to use

1. Choose a customer from **Customer name**. Server, Database, User, and Password are filled in automatically.
2. Click **Retrieve Table** (or press Enter). The app connects and runs
   `SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'`.
3. Use the search box to filter tables by name.
4. Click **Clear Data** to clear everything on the screen.

If something goes wrong, a MessageBox explains the problem in plain language, for example a wrong user/password, a server that cannot be reached, a database that is not found, or a connection timeout.

## Project structure

```
ExcelToSqlApp/
├── Program.cs
├── MainForm.cs / MainForm.Designer.cs   # UI and event handlers
├── Models/ClientProfile.cs, TableInfo.cs
├── Services/ClientRepository.cs         # reads clients.json
├── Services/DatabaseService.cs          # connection string, table query, error mapping
└── clients.json                         # mock customer profiles
```
