# TLiveQuery – Delphi TDataSet Uyumlu .NET Veri Motoru

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.6-blue)]()
[![Platform](https://img.shields.io/badge/Platform-WinForms-green)]()
[![Database](https://img.shields.io/badge/Database-MSSQL-orange)]()
[![License](https://img.shields.io/badge/License-MIT-lightgrey)]()

TLiveQuery, Delphi’nin `TDataSet / TQuery` davranışını .NET Framework üzerinde birebir modelleyen
bir veri bileşeni kütüphanesidir. ORM kullanmadan, tamamen DataTable tabanlı çalışan ve Inline SQL ile
Insert/Edit/Delete/Post süreçlerini yöneten canlı bir dataset motorudur.

## 🚀 Özellikler
- TDataSet davranışı: Append, Edit, Post, Cancel, Delete
- Field API: `FieldByName("Name").AsString`
- Locate: CaseInsensitive + PartialKey
- Gelişmiş Filter Motoru (auto LIKE, BETWEEN, IN)
- Range desteği (SetRange / CancelRange)
- OrderBy (çoklu alan + ASC/DESC)
- CalcFields desteği
- MSSQL Identity otomatik alma
- Primary Key yoksa “OldValues ile güvenli UPDATE”
- DataGridView ile iki yönlü canlı binding
- Bookmark desteği
- RecNo & RecordCount

## 📦 Kurulum
1. Visual Studio’da solution’u açın.
2. Form1.cs içinde bağlantı cümlenizi düzenleyin:
   ```csharp
   _conn = new SqlConnection("Server=.;Database=TestDB;Trusted_Connection=True;");
   ```
3. Demo projesini çalıştırın.

## 📝 Örnek Kod
```csharp
TLiveQuery q = new TLiveQuery(
    "SELECT Id, FirstName, LastName FROM Employees", conn);

q.OnCalcFields += row =>
{
    row["FullName"] = row["FirstName"] + " " + row["LastName"];
};

q.Open();

q.Append();
q.FieldByName("FirstName").AsString = "Kazım";
q.FieldByName("LastName").AsString = "Çetin";
q.Post();
```

## 🔍 Locate
```csharp
q.Locate("FirstName", "kaz",
    LocateOptions.CaseInsensitive | LocateOptions.PartialKey);
```

## 🔎 Filter
```csharp
q.SetFilter("FirstName contains 'az' AND Age > 30");
```

## 🔄 OrderBy
```csharp
q.OrderBy("FirstName DESC, Age ASC");
```

## 📄 Lisans
MIT License
