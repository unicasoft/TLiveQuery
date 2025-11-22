
# TLiveQuery

Delphi TDataSet / TQuery davranışına benzeyen basit bir .NET Framework 4.6 veri kümesi kütüphanesi.

Bu solution şu projelerden oluşur:

- **LiveQueryLib**: TLiveQuery sınıfını içeren class library (DLL).
- **LiveQueryDemo**: WinForms demo (DataGridView + Append/Edit/Post/Delete/Locate/Filter/OrderBy).

## .NET Sürümü

- .NET Framework 4.6
- Visual Studio 2015+ ile uyumlu (VS 2022 dahil)

## Kullanim

1. `TLiveQuerySolution.sln` dosyasını Visual Studio ile açın.
2. `Form1.cs` içindeki bağlantı cümlesini kendi MSSQL ayarlarınıza göre güncelleyin.
3. `Employees` tablosunda en az şu alanlar olmalı:
   - `Id` (int, identity, primary key)
   - `FirstName` (nvarchar)
   - `LastName` (nvarchar)
   - `Title` (nvarchar)
4. Çözümü build edin ve `LiveQueryDemo` projesini çalıştırın.

