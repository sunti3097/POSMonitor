---
title: POS Monitor Deployment Checklist
description: Step-by-step guide for preparing, configuring, and deploying the POS Monitor server and agent.
---

## 1. เตรียมเครื่องมือและ repository
1. ติดตั้ง .NET 8 SDK (8.0.418+) และ .NET Hosting Bundle บนเครื่อง Server/Agent
2. ติดตั้ง SQL Server (สำหรับ Server) และ SQL Express/LocalDB (บนทุก POS)
3. ติดตั้ง EF CLI (ครั้งแรกเท่านั้น)
   ```powershell
   dotnet tool install --global dotnet-ef
   ```
4. Clone repository
   ```powershell
   git clone https://github.com/sunti3097/POSMonitor.git
   cd POSMonitor
   ```

## 2. ตั้งค่า Server
1. คัดลอก `src/Server/appsettings.Template.json` เป็น `appsettings.json`
2. ปรับค่าต่อไปนี้ให้ตรงกับระบบจริง
   - `ConnectionStrings:Default`
   - `Monitoring.*`
   - `Notifications.Email` / `Notifications.Teams`
   - `AgentAuthentication.ApiKey`
3. รัน migration เพื่อสร้างฐานข้อมูล
   ```powershell
   cd src/Server
   dotnet ef database update
   ```
4. Publish + Deploy IIS (ใช้สคริปต์หรือ manual)
   ```powershell
   cd ../../
   ./scripts/Deploy-Server.ps1 `
       -ProjectPath .\src\Server `
       -PublishPath .\publish\Server `
       -SiteName POSMonitor `
       -AppPoolName POSMonitorPool
   ```
5. ตรวจสอบ
   - เปิด IIS ดูว่า site ขึ้น 200/Swagger
   - เรียก `GET /health`

## 3. ตั้งค่า Agent บนเครื่อง POS แต่ละเครื่อง
1. ติดตั้ง .NET Hosting Bundle + SQL Express (ถ้ายังไม่มี)
2. คัดลอก `src/Agent/appsettings.Template.json` เป็น `appsettings.json`
3. ปรับค่า
   - `Agent.DeviceId` (GUID ไม่ซ้ำ)
   - `Agent.ApiBaseUrl` (URL ของ Server)
   - `Agent.ApiKey` ต้องตรงกับ server
   - `Agent.SqlExpressConnectionString`
   - `MonitoringTargets.Services/Processes/PingTargets`
4. Publish และติดตั้ง Windows Service
   ```powershell
   cd src/Agent
   dotnet publish -c Release -o ..\..\publish\Agent
   sc create POSMonitorAgent binPath= "C:\POSMonitor\publish\Agent\POSMonitor.Agent.exe"
   sc start POSMonitorAgent
   ```
5. ตั้งค่า Service Recovery ให้ restart อัตโนมัติเมื่อ fail

## 4. การตรวจสอบหลัง deploy
1. Server: ตรวจ Log ใน IIS / ASP.NET, endpoint `/api/devices` ต้องทำงาน
2. Agent: ตรวจ Event Viewer (Application) และ DB Queue ว่ามี heartbeat insert
3. ทดสอบ workflow
   - Agent ส่ง heartbeat -> Dashboard แสดงสถานะ
   - สร้าง command ผ่าน API/แดชบอร์ด -> Agent รับและส่งผลกลับ
4. ตรวจสอบการแจ้งเตือน (Email/Teams) โดยปิด heartbeat ของเครื่องทดสอบสักระยะ

## 5. เอกสารประกอบ
- README: ขั้นตอนละเอียด + script
- `scripts/Deploy-Server.ps1`, `scripts/Deploy-Agent.ps1`: ใช้ deploy ซ้ำ ๆ
- `src/Server/appsettings.Template.json`, `src/Agent/appsettings.Template.json`: แม่แบบ config
