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

## Importing data from Excel (Import Excel tab)

1. Click **Retrieve Table** to connect to the database first.
2. Open the **📥 Import Excel** tab and click **Browse Excel** to choose a `.xlsx` / `.xls` file. The first row of the sheet must contain the column headers.
3. Choose the **Sheet**. The data appears in the **Preview** grid; the "Row" column shows the Excel row number.
4. **Target table**: the app tries to detect it from the sheet or file name. For example, a sheet named "หน่วยนับ" (unit of measure) is matched to `SMUNITMS`. You can change the selection.
5. The app reads the target table's schema (PK, Identity, Data Type, NOT NULL) and maps columns automatically:
   - `รหัสหน่วยนับ` (unit code) → `Code`, `ชื่อหน่วยนับ` (unit name) → `Name`, `รายละเอียด` (description) → `Descr`
   - The `No.` / `ลำดับ` (sequence) column is skipped automatically.
   - **Identity** columns are left empty so the database generates the value.
   - A numeric `ID` column that is not Identity (common in ERP tables such as `SMUNITMS`) is generated as **MAX(ID) + 1** inside the transaction.
   - Rows highlighted in red are NOT NULL columns that still have no value. Choose an Excel column for them or enter a **constant value**.
   - **Duplicate-check key**: the app uses the PK first, then a Unique column, then `Code`. You can tick or untick columns yourself.
6. Choose **When duplicate data is found**: update existing data / skip / warn and cancel.
7. Click **Import data**. The app shows a summary of new rows and existing rows for confirmation, then imports while a progress bar shows progress.

All rows are imported inside a single **SqlTransaction**. If any row fails, the whole import is rolled back, and the message tells you which Excel row caused the problem.

## Project structure

```
ExcelToSqlApp/
├── Program.cs
├── MainForm.cs / MainForm.Designer.cs   # connection screen + tabs
├── Controls/ImportView.cs               # Import Excel tab
├── UiTheme.cs                           # shared colors/styles
├── Models/                              # ClientProfile, TableInfo, ColumnSchema, ColumnMapping
├── Services/ClientRepository.cs         # reads clients.json
├── Services/DatabaseService.cs          # connection string, table list, error messages
├── Services/ExcelService.cs             # reads Excel (ExcelDataReader)
├── Services/SchemaService.cs            # reads the table schema (PK / Identity / NOT NULL)
├── Services/ColumnMapper.cs             # auto-maps columns + detects the target table
├── Services/ValueConverter.cs           # converts Excel values to SQL types
├── Services/ImportService.cs            # insert/update inside a transaction
└── clients.json                         # mock customer profiles
ExcelToSqlApp.Tests/                     # unit + SQL Server integration tests
```

## Tests

```bash
dotnet run --project ExcelToSqlApp.Tests
```

The SQL Server tests run only when `TEST_SQL_CONNECTION` is set. GitHub Actions starts SQL Server 2022 in a container and runs every test before building the `.exe`.
